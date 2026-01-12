using BaseFramework.Controls;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
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

		Window window = CreateWindow();

		dialogViewModel.InitializeDialogCommands();

		DialogWindowWrapper dialogWindowWrapper = new() {
			Window = window,
			Content = frameworkElement,
			DialogViewModel = dialogViewModel,
		};

		window.Content = dialogWindowWrapper;

		if (SetOwner) {
			if (parameter is IDialogOwnerSetter dialogOwnerSetter) {
				window.Owner = dialogOwnerSetter.Owner;
			} else {
				window.Owner = Window.GetWindow(AssociatedObject);
			}
		}

		window.SetBinding(Window.TitleProperty, new Binding(nameof(dialogViewModel.DialogTitle)) { Source = dialogViewModel });
		window.SetBinding(Window.IconProperty, new Binding(nameof(dialogViewModel.DialogIcon)) { Source = dialogViewModel });

		dialogViewModel.DialogWindowParameter.Do(it => {
			window.ResizeMode = it.ResizeMode;
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
	}

	protected virtual Window CreateWindow() {
		return new WindowBase();
	}

}
