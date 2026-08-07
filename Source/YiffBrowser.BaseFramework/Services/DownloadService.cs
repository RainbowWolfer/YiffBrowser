using RW.Base.WPF.Interfaces;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.BaseFramework.Interfaces;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.ViewModels;

namespace YiffBrowser.BaseFramework.Services;

public interface IDownloadService {
	ObservableCollection<DownloadItem> DownloadItems { get; }
	LoadingStatus OverallStatus { get; }

	/// <summary>Raised on the UI thread after a batch is queued. Argument is the number of items added.</summary>
	event EventHandler<int>? DownloadsQueued;

	void StartDownloads(IEnumerable<IDownloadable> items, string destinationFolder);
	void ClearCompleted();
	void ClearFailed();
	void RemoveItem(DownloadItem item);
	void CancelActive();
	void PauseActive();
	void ResumePaused();
	void RetryFailed();
	void RecreateHttpClient();
	void FlushPersistence();
}

internal class DownloadService(
	IAppSettingsService appSettingsService,
	IDownloadPersistenceService downloadPersistenceService,
	IDownloadIndexService downloadIndexService
) : IDownloadService, IAppInitializeAsync {
	string IAppInitializeAsync.Description { get; } = "Initialzing Download Service";
	int IPriority.Priority { get; } = IntPriority.Normal;

	private bool isRestoring;
	private bool persistenceLoaded;

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) {
		RestoreFromPersistence();
	}

	private static HttpClient CreateHttpClient(AppSettingsModel model) {
		HttpClientHandler handler = new() {
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			UseCookies = true,
		};
		ProxySettingsHelper.Configure(handler, model);

		return new HttpClient(handler) {
			Timeout = TimeSpan.FromMinutes(5)
		};
	}

	private static int GetMaxConcurrentDownloads(IAppSettingsService settingsService) =>
		Math.Clamp(settingsService.Model.MaxConcurrentDownloads, 1, 10);

	private HttpClient httpClient = CreateHttpClient(appSettingsService.Model);
	private readonly SemaphoreSlim globalConcurrencySemaphore = new(GetMaxConcurrentDownloads(appSettingsService));

	public void RecreateHttpClient() {
		// In-flight DownloadItems keep their existing HttpClient reference.
		httpClient = CreateHttpClient(appSettingsService.Model);
	}

	/// <summary>
	/// Items belonging to the current download session.
	/// A new session starts when downloads begin while nothing is active.
	/// </summary>
	private readonly HashSet<DownloadItem> sessionItems = [];

	public ObservableCollection<DownloadItem> DownloadItems { get; } = [];

	public LoadingStatus OverallStatus { get; } = new() { ShowLoading = false };

	public event EventHandler<int>? DownloadsQueued;

	private void RaiseDownloadsQueued(int count) {
		if (count <= 0) {
			return;
		}

		void Raise() => DownloadsQueued?.Invoke(this, count);

		if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess()) {
			dispatcher.BeginInvoke(Raise);
		} else {
			Raise();
		}
	}

	private void RestoreFromPersistence() {
		if (persistenceLoaded) {
			return;
		}

		persistenceLoaded = true;
		isRestoring = true;
		try {
			downloadPersistenceService.LoadSettings();
			foreach (DownloadItemSnapshot snapshot in downloadPersistenceService.Model.Items) {
				if (snapshot.FileUrl.IsBlank() || snapshot.DestinationPath.IsBlank()) {
					continue;
				}

				DownloadItem item = new(
					snapshot.FileUrl,
					snapshot.DestinationPath,
					httpClient,
					globalConcurrencySemaphore,
					snapshot.PreviewUrl,
					snapshot.CollisionBehavior,
					snapshot.IndexRootFolder,
					snapshot.IndexSite,
					snapshot.IndexItemId,
					snapshot.IndexMd5);
				item.ApplyRestoredSnapshot(snapshot);
				item.PropertyChanged += OnItemPropertyChanged;
				item.Status.PropertyChanged += OnItemStatusPropertyChanged;
				item.RemoveRequested += OnItemRemoveRequested;
				DownloadItems.Add(item);

				if (item.IsCompleted) {
					TryIndexCompletedDownload(item);
				}

				if (item.IsActive) {
					sessionItems.Add(item);
				}
			}

			UpdateOverallStatus();
		} finally {
			isRestoring = false;
		}
	}

	private void PersistDownloads() {
		if (isRestoring) {
			return;
		}

		downloadPersistenceService.Model.Items = DownloadItems.Select(i => i.ToSnapshot()).ToList();
		downloadPersistenceService.ScheduleSave();
	}

	public void FlushPersistence() {
		if (isRestoring) {
			return;
		}

		downloadPersistenceService.Model.Items = DownloadItems.Select(i => i.ToSnapshot()).ToList();
		downloadPersistenceService.FlushSave();
	}

	public void StartDownloads(IEnumerable<IDownloadable> items, string destinationFolder) {
		List<IDownloadable> list = items as List<IDownloadable> ?? items.ToList();
		if (list.Count == 0) {
			return;
		}

		if (!Directory.Exists(destinationFolder)) {
			Directory.CreateDirectory(destinationFolder);
		}

		OverallStatus.Initialize("Starting Batch Download");
		RaiseDownloadsQueued(list.Count);
		_ = ProcessBatchInternalAsync(list, destinationFolder);
	}

	public void ClearCompleted() {
		RemoveItems(DownloadItems.Where(i => i.IsCompleted));
	}

	public void ClearFailed() {
		RemoveItems(DownloadItems.Where(i => i.IsFailed));
	}

	public void RemoveItem(DownloadItem item) {
		RemoveItems([item]);
	}

	private void RemoveItems(IEnumerable<DownloadItem> items) {
		foreach (DownloadItem item in items.ToList()) {
			item.PropertyChanged -= OnItemPropertyChanged;
			item.Status.PropertyChanged -= OnItemStatusPropertyChanged;
			item.RemoveRequested -= OnItemRemoveRequested;
			sessionItems.Remove(item);
			DownloadItems.Remove(item);
		}
		UpdateOverallStatus();
		PersistDownloads();
	}

	public void CancelActive() {
		foreach (DownloadItem item in DownloadItems.Where(i => i.State is DownloadItemState.Pending or DownloadItemState.Downloading or DownloadItemState.Paused).ToList()) {
			if (item.CancelCommand.CanExecute(null)) {
				item.CancelCommand.Execute(null);
			}
		}
	}

	public void PauseActive() {
		foreach (DownloadItem item in DownloadItems.Where(i => i.PauseCommand.CanExecute(null)).ToList()) {
			item.PauseCommand.Execute(null);
		}
	}

	public void ResumePaused() {
		foreach (DownloadItem item in DownloadItems.Where(i => i.ResumeCommand.CanExecute(null)).ToList()) {
			item.ResumeCommand.Execute(null);
		}
	}

	public void RetryFailed() {
		foreach (DownloadItem item in DownloadItems.Where(i => i.RetryCommand.CanExecute(null)).ToList()) {
			sessionItems.Add(item);
			item.RetryCommand.Execute(null);
		}
	}

	private async Task ProcessBatchInternalAsync(IEnumerable<IDownloadable> items, string destinationFolder) {
		try {
			List<Task> downloadTasks = [];

			foreach (IDownloadable i in items) {
				string fileName = i.TargetFileName;
				string itemFolder = destinationFolder;
				if (i.SubDirectory.IsNotBlank()) {
					itemFolder = Path.Combine(destinationFolder, i.SubDirectory);
				}

				if (!Directory.Exists(itemFolder)) {
					Directory.CreateDirectory(itemFolder);
				}

				string destPath = Path.Combine(itemFolder, fileName);

				DownloadItem item = new(
					i.DownloadUrl,
					destPath,
					httpClient,
					globalConcurrencySemaphore,
					i.PreviewUrl,
					appSettingsService.Model.FileCollisionBehavior,
					destinationFolder,
					i.IndexSite,
					i.IndexItemId,
					i.IndexMd5);
				item.PropertyChanged += OnItemPropertyChanged;
				item.Status.PropertyChanged += OnItemStatusPropertyChanged;
				item.RemoveRequested += OnItemRemoveRequested;

				Application.Current.Dispatcher.Invoke(() => {
					// New session when nothing is currently active (previous batch fully settled).
					if (!DownloadItems.Any(existing => existing.IsActive)) {
						sessionItems.Clear();
					}

					DownloadItems.Add(item);
					sessionItems.Add(item);
				});

				PersistDownloads();

				Task downloadTask = item.StartDownloadAsync();
				downloadTasks.Add(downloadTask);
			}

			await Task.WhenAll(downloadTasks);
		} catch (Exception ex) {
			OverallStatus.Error(ex.Message);
		}
	}

	private void OnItemRemoveRequested(object? sender, EventArgs e) {
		if (sender is DownloadItem item) {
			RemoveItem(item);
		}
	}

	private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName != nameof(DownloadItem.State) || sender is not DownloadItem item) {
			return;
		}

		if (item.IsCompleted) {
			TryIndexCompletedDownload(item);
		}

		if (item.IsCompleted || item.IsFailed) {
			void MoveToFront() {
				int index = DownloadItems.IndexOf(item);
				if (index > 0) {
					DownloadItems.Move(index, 0);
				}
			}

			if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess()) {
				dispatcher.Invoke(MoveToFront);
			} else {
				MoveToFront();
			}
		}

		UpdateOverallStatus();
		PersistDownloads();
	}

	private void TryIndexCompletedDownload(DownloadItem item) {
		if (item.IndexRootFolder.IsBlank() || item.IndexSite.IsBlank() || item.IndexItemId.IsBlank()) {
			return;
		}

		if (item.DestinationPath.IsBlank()) {
			return;
		}

		try {
			downloadIndexService.Upsert(
				item.IndexRootFolder,
				item.IndexSite,
				item.IndexItemId,
				item.IndexMd5,
				item.DestinationPath);
		} catch (Exception ex) {
			System.Diagnostics.Debug.WriteLine($"Failed to index completed download: {ex.Message}");
		}
	}

	private void OnItemStatusPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName is nameof(LoadingStatus.BytesPerSecond) or nameof(LoadingStatus.BytesRemaining) or nameof(LoadingStatus.Progress)) {
			UpdateOverallStatus();
		}

		// Persist progress occasionally so crash restore keeps approximate progress.
		if (e.PropertyName is nameof(LoadingStatus.Progress)) {
			PersistDownloads();
		}
	}

	private void UpdateOverallStatus() {
		Application.Current.Dispatcher.Invoke(() => {
			List<DownloadItem> activeItems = DownloadItems.Where(i => i.IsActive).ToList();
			int activeCount = activeItems.Count;

			if (activeCount == 0) {
				OverallStatus.ShowLoading = false;
				OverallStatus.DownloadInfo = "No active downloads";
				OverallStatus.SpeedText = "—";
				OverallStatus.EtaText = "—";
				OverallStatus.BytesPerSecond = 0;
				OverallStatus.BytesRemaining = 0;
				OverallStatus.Progress = null;
				return;
			}

			// Session progress: finished items count as 100%, active items contribute their own progress.
			// Starting a new batch while idle resets the session so the next run begins near 0%.
			List<DownloadItem> session = sessionItems.Where(DownloadItems.Contains).ToList();
			int sessionTotal = session.Count;
			if (sessionTotal == 0) {
				session = activeItems;
				sessionTotal = activeCount;
				foreach (DownloadItem item in activeItems) {
					sessionItems.Add(item);
				}
			}

			int sessionFinished = session.Count(i => i.IsFinished);
			double activeContribution = session
				.Where(i => i.IsActive)
				.Sum(i => (i.Status.Progress ?? 0) / 100.0);
			double globalProgress = (sessionFinished + activeContribution) / sessionTotal * 100;

			int downloadingCount = activeItems.Count(i => i.State == DownloadItemState.Downloading);
			int pendingCount = activeItems.Count(i => i.State == DownloadItemState.Pending);
			int pausedCount = activeItems.Count(i => i.State == DownloadItemState.Paused);

			IEnumerable<DownloadItem> transferring = activeItems.Where(i => i.State == DownloadItemState.Downloading);
			double totalBps = transferring.Sum(i => i.Status.BytesPerSecond);
			long totalRemaining = transferring.Sum(i => i.Status.BytesRemaining);
			string speedText = FormatSpeed(totalBps);
			string etaText = LoadingStatus.FormatEtaText(totalRemaining, totalBps);

			string info = $"{sessionFinished} / {sessionTotal} finished · {activeCount} active";
			List<string> parts = [];
			if (downloadingCount > 0) {
				parts.Add($"{downloadingCount} downloading");
			}
			if (pendingCount > 0) {
				parts.Add($"{pendingCount} pending");
			}
			if (pausedCount > 0) {
				parts.Add($"{pausedCount} paused");
			}
			if (parts.Count > 0) {
				info = $"{sessionFinished} / {sessionTotal} · {string.Join(" · ", parts)}";
			}

			OverallStatus.SetProgress(globalProgress, info, speedText, etaText, totalBps, totalRemaining);
		});
	}

	private static string FormatSpeed(double bytesPerSecond) {
		if (bytesPerSecond < 1) {
			return "—";
		}

		return $"{((long)bytesPerSecond).FileSizeToKB()}/s";
	}
}
