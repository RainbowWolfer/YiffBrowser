using BaseFramework.Enums;
using BaseFramework.ViewModels;
using DevExpress.Mvvm;
using RW.Base.WPF.ViewModelServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Models.E621;

namespace YB.E621.Views;

public partial class PostDetailView : UserControl {
	public PostDetailView() {
		InitializeComponent();
	}
}

public class PostDetailViewModel : ViewModelBase {
	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<UserControl> UserControlService => GetService<ITypedUIObjectService>(nameof(UserControlService)).As<UserControl>();

	public bool HasPost => Post != null;


	public E621Post? Post {
		get => GetProperty(() => Post);
		set {
			SetProperty(() => Post, value);
			RaisePropertyChanged(nameof(HasPost));
			PostDetailDockViewModel.Post = value;
		}
	}

	public ModuleType ModuleType { get; }

	public GridDefinitionModel LeftSideGrid { get; } = new(true, 150, new GridLength(220, GridUnitType.Pixel));
	public GridDefinitionModel RightSideGrid { get; } = new(false, 150, new GridLength(300, GridUnitType.Pixel));

	public PostDetailDockViewModel PostDetailDockViewModel { get; } = new();

	public PostDetailViewModel(ModuleType moduleType) {
		ModuleType = moduleType;
		Post = null;
	}

	public void Focus() {
		DispatcherService.Dispatcher.Invoke(() => {
			UserControlService.Object.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}, DispatcherPriority.Loaded);
	}


	private DelegateCommand<KeyEventArgs>? keyDownCommand;
	public IDelegateCommand KeyDownCommand => keyDownCommand ??= new(KeyDown);
	private void KeyDown(KeyEventArgs args) {
		if (args.Key == Key.Escape) {
			Back();
			args.Handled = true;
		}
	}

	public ICommand BackCommand => new DelegateCommand(Back);
	public void Back() {
		Post = null;
	}

	public ICommand NextCommand => new DelegateCommand(Next);
	public ICommand PreviousCommand => new DelegateCommand(Previous);

	private void Next() {

	}

	private void Previous() {

	}

}
