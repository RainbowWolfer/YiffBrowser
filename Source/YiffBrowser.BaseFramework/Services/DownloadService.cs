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
using YiffBrowser.BaseFramework.Interfaces;
using YiffBrowser.BaseFramework.ViewModels;

namespace YiffBrowser.BaseFramework.Services;

public interface IDownloadService {
	ObservableCollection<DownloadItem> DownloadItems { get; }
	LoadingStatus OverallStatus { get; }

	void StartDownloads(IEnumerable<IDownloadable> items, string destinationFolder);
	void ClearCompleted();
	void ClearFailed();
	void RemoveItem(DownloadItem item);
	void CancelActive();
	void PauseActive();
	void ResumePaused();
	void RetryFailed();
}

internal class DownloadService(IAppSettingsService appSettingsService) : IDownloadService, IAppInitializeAsync {
	string IAppInitializeAsync.Description { get; } = "Initialzing Download Service";
	int IPriority.Priority { get; } = IntPriority.Normal;

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) {

	}

	private static HttpClient CreateHttpClient() {
		HttpClientHandler handler = new() {
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			UseCookies = true,
		};

		return new HttpClient(handler) {
			Timeout = TimeSpan.FromMinutes(5)
		};
	}

	private static int GetMaxConcurrentDownloads(IAppSettingsService settingsService) =>
		Math.Clamp(settingsService.Model.MaxConcurrentDownloads, 1, 10);

	private readonly HttpClient httpClient = CreateHttpClient();
	private readonly SemaphoreSlim globalConcurrencySemaphore = new(GetMaxConcurrentDownloads(appSettingsService));

	public ObservableCollection<DownloadItem> DownloadItems { get; } = [];

	public LoadingStatus OverallStatus { get; } = new() { ShowLoading = false };

	public void StartDownloads(IEnumerable<IDownloadable> items, string destinationFolder) {
		if (!Directory.Exists(destinationFolder)) {
			Directory.CreateDirectory(destinationFolder);
		}

		OverallStatus.Initialize("Starting Batch Download");
		_ = ProcessBatchInternalAsync(items, destinationFolder);
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
			DownloadItems.Remove(item);
		}
		UpdateOverallStatus();
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
			item.RetryCommand.Execute(null);
		}
	}

	private async Task ProcessBatchInternalAsync(IEnumerable<IDownloadable> items, string destinationFolder) {
		try {
			List<Task> downloadTasks = [];

			foreach (IDownloadable i in items) {
				string fileName = i.TargetFileName;
				string destPath = Path.Combine(destinationFolder, fileName);

				DownloadItem item = new(i.DownloadUrl, destPath, httpClient, globalConcurrencySemaphore, i.PreviewUrl);
				item.PropertyChanged += OnItemPropertyChanged;
				item.Status.PropertyChanged += OnItemStatusPropertyChanged;
				item.RemoveRequested += OnItemRemoveRequested;

				Application.Current.Dispatcher.Invoke(() => DownloadItems.Add(item));

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
	}

	private void OnItemStatusPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName is nameof(LoadingStatus.BytesPerSecond) or nameof(LoadingStatus.BytesRemaining) or nameof(LoadingStatus.Progress)) {
			UpdateOverallStatus();
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

			int downloadingCount = activeItems.Count(i => i.State == DownloadItemState.Downloading);
			int pendingCount = activeItems.Count(i => i.State == DownloadItemState.Pending);
			int pausedCount = activeItems.Count(i => i.State == DownloadItemState.Paused);

			IEnumerable<DownloadItem> transferring = activeItems.Where(i => i.State == DownloadItemState.Downloading);
			double totalBps = transferring.Sum(i => i.Status.BytesPerSecond);
			long totalRemaining = transferring.Sum(i => i.Status.BytesRemaining);
			string speedText = FormatSpeed(totalBps);
			string etaText = LoadingStatus.FormatEtaText(totalRemaining, totalBps);

			// Overall progress is based only on active items (pending / downloading / paused).
			double? globalProgress = transferring.Any(i => i.Status.Progress is null)
				? null
				: activeItems.Average(i => i.Status.Progress ?? 0);

			string info = $"{activeCount} active";
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
				info = string.Join(" · ", parts);
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
