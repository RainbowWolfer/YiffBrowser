using BaseFramework.ViewModels;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using RW.Base.WPF.ViewModelServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Models.E621;
using YB.E621.ViewModels;

namespace YB.E621.Views;

public partial class PostDetailView : UserControl {
	public PostDetailView() {
		InitializeComponent();
	}
}

internal class PostDetailViewModel() : ViewModelBase {
	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<UserControl> UserControlService => GetService<ITypedUIObjectService>(nameof(UserControlService)).As<UserControl>();

	public E621Post? Post {
		get => GetProperty(() => Post);
		private set {
			SetProperty(() => Post, value);
			PostDetailDockViewModel.Post = value;
		}
	}

	public GridDefinitionModel LeftSideGrid { get; } = new(true, 150, new GridLength(220, GridUnitType.Pixel));
	public GridDefinitionModel RightSideGrid { get; } = new(false, 150, new GridLength(300, GridUnitType.Pixel));

	public PostDetailDockViewModel PostDetailDockViewModel { get; } = new();

	public PostsViewModel ParentViewModel {
		get => GetProperty(() => ParentViewModel);
		private set => SetProperty(() => ParentViewModel, value);
	}

	protected override void OnParentViewModelChanged(object parentViewModel) {
		base.OnParentViewModelChanged(parentViewModel);

		Post = null;

		ParentViewModel = (PostsViewModel)parentViewModel;
		ParentViewModel.CurrentPostChanged += PostsViewModel_CurrentPostChanged;

	}

	private void PostsViewModel_CurrentPostChanged(PostsViewModel sender, E621Post? args) {
		Post = args;
		Focus();
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
		ParentViewModel.QuitPostDetailView();
	}

	public ICommand NextCommand => new DelegateCommand(Next);
	public ICommand PreviousCommand => new DelegateCommand(Previous);

	private void Next() {

	}

	private void Previous() {

	}

}
