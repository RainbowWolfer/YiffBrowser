using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;

namespace YiffBrowser.BaseFramework.Views.Dialogs;

public partial class DownloadDialog : UserControl {
	public DownloadDialog() {
		InitializeComponent();
	}
}

internal class DownloadDialogViewModel(IDownloadService downloadService) : DialogViewModel<object> {
	// Separate ListCollectionView instances are required — CollectionViewSource shares the
	// default view for a given source, so two filters would overwrite each other.
	private readonly ListCollectionView activeDownloads = CreateFilteredView(downloadService.DownloadItems, static item => item.IsActive);
	private readonly ListCollectionView completedDownloads = CreateFilteredView(downloadService.DownloadItems, static item => item.IsCompleted);
	private readonly ListCollectionView failedDownloads = CreateFilteredView(downloadService.DownloadItems, static item => item.IsFailed);

	public IDownloadService DownloadService { get; } = downloadService;

	public LoadingStatus OverallStatus => DownloadService.OverallStatus;

	public ICollectionView ActiveDownloads => activeDownloads;
	public ICollectionView CompletedDownloads => completedDownloads;
	public ICollectionView FailedDownloads => failedDownloads;

	public bool HasActiveItems {
		get => GetProperty(() => HasActiveItems);
		set => SetProperty(() => HasActiveItems, value);
	}

	public bool HasCompletedItems {
		get => GetProperty(() => HasCompletedItems);
		set => SetProperty(() => HasCompletedItems, value);
	}

	public bool HasFailedItems {
		get => GetProperty(() => HasFailedItems);
		set => SetProperty(() => HasFailedItems, value);
	}

	public bool HasPausableItems {
		get => GetProperty(() => HasPausableItems);
		set => SetProperty(() => HasPausableItems, value);
	}

	public bool HasResumableItems {
		get => GetProperty(() => HasResumableItems);
		set => SetProperty(() => HasResumableItems, value);
	}

	public string ActiveTabHeader {
		get => GetProperty(() => ActiveTabHeader);
		set => SetProperty(() => ActiveTabHeader, value);
	}

	public string CompletedTabHeader {
		get => GetProperty(() => CompletedTabHeader);
		set => SetProperty(() => CompletedTabHeader, value);
	}

	public string FailedTabHeader {
		get => GetProperty(() => FailedTabHeader);
		set => SetProperty(() => FailedTabHeader, value);
	}

	public DelegateCommand ClearCompletedCommand => field ??= new DelegateCommand(ClearCompleted, () => HasCompletedItems);
	public DelegateCommand ClearFailedCommand => field ??= new DelegateCommand(ClearFailed, () => HasFailedItems);
	public DelegateCommand RetryFailedCommand => field ??= new DelegateCommand(RetryFailed, () => HasFailedItems);
	public DelegateCommand CancelActiveCommand => field ??= new DelegateCommand(CancelActive, () => HasActiveItems);
	public DelegateCommand PauseAllCommand => field ??= new DelegateCommand(PauseAll, () => HasPausableItems);
	public DelegateCommand ResumeAllCommand => field ??= new DelegateCommand(ResumeAll, () => HasResumableItems);

	private static ListCollectionView CreateFilteredView(
		ObservableCollection<DownloadItem> source,
		Predicate<DownloadItem> predicate) {
		return new ListCollectionView(source) {
			Filter = obj => obj is DownloadItem item && predicate(item),
		};
	}

	protected override void OnInitialized() {
		base.OnInitialized();
		DialogTitle = $"{AppConfig.DisplayAppName} - Downloads";

		DownloadService.DownloadItems.CollectionChanged += OnDownloadItemsChanged;
		foreach (DownloadItem item in DownloadService.DownloadItems) {
			item.PropertyChanged += OnDownloadItemPropertyChanged;
		}

		RefreshViews();
		UpdateSummary();
	}

	public override DialogWindowParameter DialogWindowParameter => base.DialogWindowParameter with {
		ResizeMode = ResizeMode.CanResizeWithGrip,
		SizeToContent = SizeToContent.Manual,
	};

	public override void OnWindowInitialized(Window window) {
		base.OnWindowInitialized(window);

		window.MinHeight = 360;
		window.MinWidth = 640;
		window.Width = 760;
		window.Height = 480;
	}

	private void OnDownloadItemsChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		if (e.OldItems != null) {
			foreach (DownloadItem item in e.OldItems) {
				item.PropertyChanged -= OnDownloadItemPropertyChanged;
			}
		}

		if (e.NewItems != null) {
			foreach (DownloadItem item in e.NewItems) {
				item.PropertyChanged += OnDownloadItemPropertyChanged;
			}
		}

		RefreshViews();
		UpdateSummary();
	}

	private void OnDownloadItemPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName is nameof(DownloadItem.State)
			or nameof(DownloadItem.IsActive)
			or nameof(DownloadItem.IsCompleted)
			or nameof(DownloadItem.IsFailed)
			or nameof(DownloadItem.IsFinished)) {
			RefreshViews();
			UpdateSummary();
		}
	}

	private void RefreshViews() {
		void Refresh() {
			activeDownloads.Refresh();
			completedDownloads.Refresh();
			failedDownloads.Refresh();
		}

		if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess()) {
			dispatcher.Invoke(Refresh);
		} else {
			Refresh();
		}
	}

	private void UpdateSummary() {
		int active = DownloadService.DownloadItems.Count(i => i.IsActive);
		int completed = DownloadService.DownloadItems.Count(i => i.IsCompleted);
		int failed = DownloadService.DownloadItems.Count(i => i.IsFailed);
		int pausable = DownloadService.DownloadItems.Count(i => i.State is DownloadItemState.Pending or DownloadItemState.Downloading);
		int resumable = DownloadService.DownloadItems.Count(i => i.State == DownloadItemState.Paused);

		HasActiveItems = active > 0;
		HasCompletedItems = completed > 0;
		HasFailedItems = failed > 0;
		HasPausableItems = pausable > 0;
		HasResumableItems = resumable > 0;
		ActiveTabHeader = $"Downloading ({active})";
		CompletedTabHeader = $"Completed ({completed})";
		FailedTabHeader = $"Failed ({failed})";

		ClearCompletedCommand.RaiseCanExecuteChanged();
		ClearFailedCommand.RaiseCanExecuteChanged();
		RetryFailedCommand.RaiseCanExecuteChanged();
		CancelActiveCommand.RaiseCanExecuteChanged();
		PauseAllCommand.RaiseCanExecuteChanged();
		ResumeAllCommand.RaiseCanExecuteChanged();
	}

	private void ClearCompleted() {
		DownloadService.ClearCompleted();
		RefreshViews();
		UpdateSummary();
	}

	private void ClearFailed() {
		DownloadService.ClearFailed();
		RefreshViews();
		UpdateSummary();
	}

	private void RetryFailed() {
		DownloadService.RetryFailed();
		RefreshViews();
		UpdateSummary();
	}

	private void CancelActive() {
		DownloadService.CancelActive();
		RefreshViews();
		UpdateSummary();
	}

	private void PauseAll() {
		DownloadService.PauseActive();
		RefreshViews();
		UpdateSummary();
	}

	private void ResumeAll() {
		DownloadService.ResumePaused();
		RefreshViews();
		UpdateSummary();
	}
}
