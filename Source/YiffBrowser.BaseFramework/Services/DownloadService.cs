using RW.Base.WPF.Interfaces;
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
}

internal class DownloadService : IDownloadService, IAppInitializeAsync {
	string IAppInitializeAsync.Description { get; } = "Initialzing Download Service";
	int IPriority.Priority { get; } = IntPriority.Normal;

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) {

	}

	private readonly HttpClient httpClient;

	// 控制全局最高并发数，比如最高4个同时下载
	private readonly SemaphoreSlim globalConcurrencySemaphore = new(4);

	// UI 可以直接绑定这个集合渲染 ItemsControl / ListBox
	public ObservableCollection<DownloadItem> DownloadItems { get; } = new ObservableCollection<DownloadItem>();

	// 全局的总体进度
	public LoadingStatus OverallStatus { get; } = new LoadingStatus();

	public DownloadService() {
		OverallStatus.ShowLoading = false; // 初始处于空闲

		HttpClientHandler handler = new() {
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
			UseCookies = true,
			//MaxConnectionsPerServer = 4,
		};

		httpClient = new HttpClient(handler) {
			Timeout = TimeSpan.FromMinutes(5)
		};
	}

	// 触发即走（Fire and Forget）
	public void StartDownloads(IEnumerable<IDownloadable> items, string destinationFolder) {
		if (!Directory.Exists(destinationFolder)) {
			Directory.CreateDirectory(destinationFolder);
		}

		OverallStatus.Initialize("Starting Batch Download...");

		// 丢到后台运行，直接放行主线程 (Fire-and-Forget)
		_ = ProcessBatchInternalAsync(items, destinationFolder);
	}

	public void ClearCompleted() {
		List<DownloadItem> completedItems = DownloadItems.Where(i => i.State == DownloadItemState.Completed).ToList();
		foreach (DownloadItem item in completedItems) {
			item.PropertyChanged -= OnItemPropertyChanged;
			DownloadItems.Remove(item);
		}
		UpdateOverallStatus();
	}

	private async Task ProcessBatchInternalAsync(IEnumerable<IDownloadable> items, string destinationFolder) {
		try {
			List<Task> downloadTasks = [];

			foreach (IDownloadable i in items) {
				string fileName = i.TargetFileName;
				string destPath = Path.Combine(destinationFolder, fileName);

				DownloadItem item = new(i.DownloadUrl, destPath, httpClient, globalConcurrencySemaphore);

				// 监听每个单独 Item 的状态变化，用来更新全局的进度条
				item.PropertyChanged += OnItemPropertyChanged;

				// 切回 UI 线程添加集合项（如果这是 WPF/DevExpress，必须这样以避免集合跨线程报错）
				Application.Current.Dispatcher.Invoke(() => DownloadItems.Add(item));

				// 启动单个任务，因为内部有 Semaphore 阻挡，所以不会一瞬间爆发全部请求
				Task downloadTask = item.StartDownloadAsync();
				downloadTasks.Add(downloadTask);
			}

			// 可以在这里 await 所有任务的完成，如果只是后台挂机，这一步可以不阻挡任何前端
			await Task.WhenAll(downloadTasks);
		} catch (Exception ex) {
			OverallStatus.Error(ex.Message);
		}
	}

	private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (e.PropertyName == nameof(DownloadItem.State)) {
			UpdateOverallStatus();
		}
	}

	private void UpdateOverallStatus() {
		// 这是一个轻量级的聚合计算。
		// 注意：如果在高并发下被频繁触发，可以使用 Dispatcher / 节流(Throttle) 来优化。
		int totalCount = DownloadItems.Count;
		if (totalCount == 0) {
			return;
		}

		int completedCount = DownloadItems.Count(i => i.State == DownloadItemState.Completed);
		int errorCount = DownloadItems.Count(i => i.State == DownloadItemState.Error);
		int finishedCount = completedCount + errorCount;

		double globalProgress = (double)finishedCount / totalCount * 100;


		Application.Current.Dispatcher.Invoke(() => {
			OverallStatus.SetProgress(globalProgress, $"Total Progress: {finishedCount} / {totalCount} (Errors: {errorCount})");

			if (finishedCount == totalCount) {
				OverallStatus.Done();
			}
		});
	}
}
