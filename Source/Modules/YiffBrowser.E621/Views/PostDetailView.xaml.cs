using DevExpress.Mvvm;
using RW.Base.WPF.ViewModelServices;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.E621.Enums;
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

internal class PostDetailViewModel(IVideoControlsService videoControlsService) : ViewModelBase {
	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	public IUIObjectService<UserControl> UserControlService => GetService<ITypedUIObjectService>(nameof(UserControlService)).As<UserControl>();

	public E621Post? Post {
		get => GetProperty(() => Post);
		private set {
			SetProperty(() => Post, value);
			RaisePropertyChanged(() => FileType);
			RaisePropertyChanged(() => DisplayType);
			RaisePropertyChanged(() => IsCurrentPostSelected);
			copyPostUrlCommand?.RaiseCanExecuteChanged();
			openPostInBrowserCommand?.RaiseCanExecuteChanged();
			downloadPostCommand?.RaiseCanExecuteChanged();
		}
	}

	public bool IsCurrentPostSelected {
		get => ParentViewModel?.IsPostSelected(Post) == true;
		set {
			ParentViewModel?.SetPostSelected(Post, value);
			RaisePropertyChanged(() => IsCurrentPostSelected);
		}
	}

	public IVideoControlsService VideoControlsService { get; } = videoControlsService;


	public E621FileType FileType => Post?.GetFileType() ?? E621FileType.Unknown;

	public PostDisplayType DisplayType => FileType.GetPostDisplayType();

	public GridDefinitionModel LeftSideGrid { get; } = new(true, 150, new GridLength(220, GridUnitType.Pixel));
	public GridDefinitionModel RightSideGrid { get; } = new(false, 150, new GridLength(300, GridUnitType.Pixel));

	public bool ShowSlidePanel {
		get => GetProperty(() => ShowSlidePanel);
		set => SetProperty(() => ShowSlidePanel, value);
	}

	public PostsViewModel? ParentViewModel {
		get => GetProperty(() => ParentViewModel);
		private set => SetProperty(() => ParentViewModel, value);
	}

	protected override void OnParentViewModelChanged(object parentViewModel) {
		base.OnParentViewModelChanged(parentViewModel);

		Post = null;

		ParentViewModel = (PostsViewModel)parentViewModel;
		ParentViewModel.CurrentPostChanged += PostsViewModel_CurrentPostChanged;
		ParentViewModel.SelectionChanged += PostsViewModel_SelectionChanged;

	}

	private void PostsViewModel_CurrentPostChanged(PostsViewModel sender, E621Post? args) {
		Post = args;
		Focus();
	}

	private void PostsViewModel_SelectionChanged(PostsViewModel sender, EventArgs args) {
		RaisePropertyChanged(() => IsCurrentPostSelected);
	}

	public void Focus() {
		DispatcherService.Dispatcher.Invoke(() => {
			UserControlService.Object.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
		}, DispatcherPriority.Loaded);
	}


	private DelegateCommand<KeyEventArgs>? keyDownCommand;
	public IDelegateCommand KeyDownCommand => keyDownCommand ??= new(KeyDown);
	private void KeyDown(KeyEventArgs args) {
		if (Keyboard.FocusedElement is TextBoxBase) {
			return;
		}
		switch (args.Key) {
			case Key.Escape: {
				ParentViewModel?.QuitPostDetailView();
				args.Handled = true;
				break;
			}
			case Key.Left or Key.A: {
				ParentViewModel?.PreviousPost();
				args.Handled = true;
				break;
			}
			case Key.Right or Key.D: {
				ParentViewModel?.NextPost();
				args.Handled = true;
				break;
			}
		}
	}

	private DelegateCommand? copyPostUrlCommand;
	public IDelegateCommand CopyPostUrlCommand => copyPostUrlCommand ??= new(CopyPostUrl, CanCopyPostUrl);
	private void CopyPostUrl() {
		if (CanCopyPostUrl()) {
			Post!.GetPostLink(ParentViewModel!.ModuleType).CopyToClipboard();
		}
	}
	private bool CanCopyPostUrl() => Post != null && ParentViewModel != null;

	private DelegateCommand? openPostInBrowserCommand;
	public IDelegateCommand OpenPostInBrowserCommand => openPostInBrowserCommand ??= new(OpenPostInBrowser, CanOpenPostInBrowser);
	private void OpenPostInBrowser() {
		if (CanOpenPostInBrowser()) {
			Post!.GetPostLink(ParentViewModel!.ModuleType).OpenInBrowser();
		}
	}
	private bool CanOpenPostInBrowser() => Post != null && ParentViewModel != null;

	private DelegateCommand? downloadPostCommand;
	public IDelegateCommand DownloadPostCommand => downloadPostCommand ??= new(DownloadPost, CanDownloadPost);
	private void DownloadPost() {
		if (CanDownloadPost()) {
			ParentViewModel!.DownloadPostCommand.Execute(Post);
		}
	}
	private bool CanDownloadPost() => Post != null && ParentViewModel?.DownloadPostCommand.CanExecute(Post) == true;

	private DelegateCommand? toggleLeftPanelCommand;
	public IDelegateCommand ToggleLeftPanelCommand => toggleLeftPanelCommand ??= new(ToggleLeftPanel);
	private void ToggleLeftPanel() {
		LeftSideGrid.IsExpanded = !LeftSideGrid.IsExpanded;
	}

	private DelegateCommand? toggleRightPanelCommand;
	public IDelegateCommand ToggleRightPanelCommand => toggleRightPanelCommand ??= new(ToggleRightPanel);
	private void ToggleRightPanel() {
		RightSideGrid.IsExpanded = !RightSideGrid.IsExpanded;
	}

}
