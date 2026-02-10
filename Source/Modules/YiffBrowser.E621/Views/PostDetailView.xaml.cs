using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.ViewModels;
using DevExpress.Mvvm;
using RW.Base.WPF.ViewModelServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YiffBrowser.E621.Helpers;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Views;

public partial class PostDetailView : UserControl {
	public PostDetailView() {
		InitializeComponent();
	}
}

public enum PostDisplayType {
	Unknown,
	NotSupported,
	Image,
	Video,
}

internal class PostDetailViewModel() : ViewModelBase {
	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<UserControl> UserControlService => GetService<ITypedUIObjectService>(nameof(UserControlService)).As<UserControl>();

	public E621Post? Post {
		get => GetProperty(() => Post);
		private set {
			SetProperty(() => Post, value);
			RaisePropertyChanged(() => FileType);
			RaisePropertyChanged(() => DisplayType);
		}
	}

	public FileType FileType => Post?.GetFileType() ?? FileType.Unknown;

	public PostDisplayType DisplayType {
		get {
			return FileType switch {
				FileType.Unknown => PostDisplayType.Unknown,
				FileType.ANIM => PostDisplayType.NotSupported,
				FileType.PNG or FileType.JPG or FileType.GIF => PostDisplayType.Image,
				FileType.WEBM => PostDisplayType.Video,
				_ => PostDisplayType.Unknown,
			};
		}
	}

	public GridDefinitionModel LeftSideGrid { get; } = new(true, 150, new GridLength(220, GridUnitType.Pixel));
	public GridDefinitionModel RightSideGrid { get; } = new(false, 150, new GridLength(300, GridUnitType.Pixel));

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
		//ParentViewModel
	}

	private void Previous() {

	}

}
