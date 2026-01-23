using BaseFramework.Controls;
using BaseFramework.ViewModels;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using RW.Common.WPF.Extensions;
using RW.Common.WPF.Helpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;

namespace BaseFramework.ViewModelServices;

public interface IDialogServiceEx {
	bool ShowOKCancel(object parentViewModel, object? parameter);
	DialogResult ShowDialog(object parentViewModel, object? parameter);
}

public record class DialogResult(bool? DialogResultFlag, DialogCommand? ResultDialogCommand);

public class DialogService : ServiceBase, IDialogServiceEx {

	private static readonly Dictionary<Type, DialogWindowWrapper> windowPool = [];

	public bool SingleInstance {
		get => (bool)GetValue(SingleInstanceProperty);
		set => SetValue(SingleInstanceProperty, value);
	}

	public static readonly DependencyProperty SingleInstanceProperty = DependencyProperty.Register(
		nameof(SingleInstance),
		typeof(bool),
		typeof(DialogService),
		new PropertyMetadata(false)
	);



	public bool SetOwner {
		get => (bool)GetValue(SetOwnerProperty);
		set => SetValue(SetOwnerProperty, value);
	}

	public static readonly DependencyProperty SetOwnerProperty = DependencyProperty.Register(
		nameof(SetOwner),
		typeof(bool),
		typeof(DialogService),
		new PropertyMetadata(false)
	);



	public bool ShowDialogWindow {
		get => (bool)GetValue(ShowDialogWindowProperty);
		set => SetValue(ShowDialogWindowProperty, value);
	}

	public static readonly DependencyProperty ShowDialogWindowProperty = DependencyProperty.Register(
		nameof(ShowDialogWindow),
		typeof(bool),
		typeof(DialogService),
		new PropertyMetadata(true)
	);



	public Type ContentType {
		get => (Type)GetValue(ContentTypeProperty);
		set => SetValue(ContentTypeProperty, value);
	}

	public static readonly DependencyProperty ContentTypeProperty = DependencyProperty.Register(
		nameof(ContentType),
		typeof(Type),
		typeof(DialogService),
		new PropertyMetadata(null, OnContentTypeChanged)
	);


	public DataTemplate ViewTemplate {
		get => (DataTemplate)GetValue(ViewTemplateProperty);
		set => SetValue(ViewTemplateProperty, value);
	}

	public static readonly DependencyProperty ViewTemplateProperty = DependencyProperty.Register(
		nameof(ViewTemplate),
		typeof(DataTemplate),
		typeof(DialogService),
		new PropertyMetadata(null, OnViewTemplateChanged)
	);

	private static void OnContentTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is DialogService self && e.NewValue is Type type) {
			try {
				self.ViewTemplate = new DataTemplate() {
					VisualTree = new FrameworkElementFactory(type),
				};
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				Debugger.Break();
			}
		}
	}

	private static void OnViewTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

	}

	public bool ShowOKCancel(object parentViewModel, object? parameter) {
		DialogResult result = ShowDialog(parentViewModel, parameter);
		return result.DialogResultFlag is true;
	}

	public DialogResult ShowDialog(object parentViewModel, object? parameter) {
		if (Activator.CreateInstance(ContentType) is not FrameworkElement frameworkElement) {
			throw new Exception("ContentType is not FrameworkElement");
		}

		if (frameworkElement.DataContext is not IDialogViewModel dialogViewModel) {
			throw new Exception("DataContext is not IDialogViewModel");
		}

		if (SingleInstance && !ShowDialogWindow) {
			if (windowPool.TryGetValue(ContentType, out DialogWindowWrapper? _dialogWindowWrapper)) {
				_dialogWindowWrapper.Window.ShowAndActivate();
				return new DialogResult(null, null);
			}
		}

		Window window = CreateWindow();

		dialogViewModel.InitializeDialogCommands();

		DialogWindowWrapper dialogWindowWrapper = new() {
			Window = window,
			Content = frameworkElement,
			DialogViewModel = dialogViewModel,
			ShowDialogWindow = ShowDialogWindow,
		};

		windowPool[ContentType] = dialogWindowWrapper;

		window.Closed += (s, e) => {
			windowPool.Remove(ContentType);
		};

		try {
			window.Content = dialogWindowWrapper;

			Window? owner;
			if (parameter is IDialogOwnerSetter dialogOwnerSetter) {
				owner = dialogOwnerSetter.Owner;
			} else {
				owner = Window.GetWindow(AssociatedObject);
			}

			if (SetOwner) {
				window.Owner = owner;
			} else {
				if (owner != null && window is WindowBase windowBase) {
					windowBase.CenterToOwner = owner;
				}
			}

			window.SetBinding(Window.TitleProperty, new Binding(nameof(dialogViewModel.DialogTitle)) { Source = dialogViewModel });
			if (dialogViewModel.CustomDialogIcon) {
				window.SetBinding(Window.IconProperty, new Binding(nameof(dialogViewModel.DialogIcon)) { Source = dialogViewModel });
			}

			dialogViewModel.DialogWindowParameter.Do(it => {
				if (window is WindowBase windowBase) {
					windowBase.MyResizeMode = it.ResizeMode;// to make ResizeMode working properly
				} else {
					window.ResizeMode = it.ResizeMode;
				}
				window.WindowStyle = it.WindowStyle;
				window.SizeToContent = it.SizeToContent;
				window.WindowStartupLocation = it.WindowStartupLocation;
				window.ShowInTaskbar = it.ShowInTaskbar;

				window.AllowsTransparency = it.AllowsTransparency;
				window.AllowDrop = it.AllowDrop;
				window.Topmost = it.TopMost;
			});

			if (dialogViewModel.DialogWindowParameter.EscapeToClose) {
				WindowHotKeyActionsHelper.PopupWindowEscape(window);
			}

			ViewModelExtensions.SetParentViewModel(frameworkElement, parentViewModel ?? new object());// to avoid null
			ViewModelExtensions.SetParameter(frameworkElement, parameter);

			dialogViewModel.OnWindowInitialized(window);

			if (ShowDialogWindow) {
				bool? resultFlag = window.ShowDialog();
				DialogCommand resultCommand = dialogWindowWrapper.ResultDialogCommand;
				return new DialogResult(resultFlag, resultCommand);
			} else {
				window.Show();
				return new DialogResult(null, null);
			}
		} finally {

		}
	}

	protected virtual Window CreateWindow() {
		WindowBase window = new() {
			//Style = (Style)Application.Current.TryFindResource("WindowBaseStyle")
		};
		return window;
	}

}
