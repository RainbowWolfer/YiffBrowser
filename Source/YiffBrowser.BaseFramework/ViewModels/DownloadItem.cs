using DevExpress.Mvvm;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using YiffBrowser.BaseFramework.Enums;

namespace YiffBrowser.BaseFramework.ViewModels;

public class DownloadItem : BindableBase {
	private readonly HttpClient httpClient;
	private readonly SemaphoreSlim semaphore;
	private CancellationTokenSource? cts;

	// 新增：用于区分是“暂停”引发的取消，还是真正的“彻底取消”
	private bool isCanceling = false;

	public string FileUrl { get; }
	public string DestinationPath { get; }

	public LoadingStatus Status { get; } = new LoadingStatus();

	public DownloadItemState State {
		get => GetProperty(() => State);
		set {
			SetProperty(() => State, value);
			PauseCommand.RaiseCanExecuteChanged();
			ResumeCommand.RaiseCanExecuteChanged();
			RetryCommand.RaiseCanExecuteChanged();
			CancelCommand.RaiseCanExecuteChanged();
		}
	}

	public DelegateCommand PauseCommand => field ??= new DelegateCommand(Pause, CanPause);
	public AsyncCommand ResumeCommand => field ??= new AsyncCommand(ResumeAsync, CanResume);
	public AsyncCommand RetryCommand => field ??= new AsyncCommand(RetryAsync, CanRetry);
	public DelegateCommand CancelCommand => field ??= new DelegateCommand(Cancel, CanCancel);

	public DownloadItem(string fileUrl, string destinationPath, HttpClient httpClient, SemaphoreSlim semaphore) {
		FileUrl = fileUrl;
		DestinationPath = destinationPath;
		this.httpClient = httpClient;
		this.semaphore = semaphore;

		State = DownloadItemState.Pending;
		Status.Initialize("Pending in Queue");
	}

	public async Task StartDownloadAsync() {
		cts = new CancellationTokenSource();
		CancellationToken token = cts.Token;
		bool isSemaphoreAcquired = false;

		// 每次重新开始时，重置取消标记
		isCanceling = false;

		try {
			State = DownloadItemState.Downloading;
			Status.Initialize("Waiting for available slot...");

			await semaphore.WaitAsync(token);
			isSemaphoreAcquired = true;

			Status.Initialize("Downloading...");

			using HttpRequestMessage request = new(HttpMethod.Get, FileUrl);

			using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
			response.EnsureSuccessStatusCode();

			long? totalBytes = response.Content.Headers.ContentLength;

			// 注意这里的 using，它们会在发生异常跳出 try 块时立即关闭释放文件流
			using Stream contentStream = await response.Content.ReadAsStreamAsync(token);
			using FileStream fileStream = new(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

			byte[] buffer = new byte[8192];
			long totalRead = 0;
			int readBytes;

			while ((readBytes = await contentStream.ReadAsync(buffer, token)) > 0) {
				await fileStream.WriteAsync(buffer.AsMemory(0, readBytes), token);
				totalRead += readBytes;

				if (totalBytes.HasValue) {
					double progress = (double)totalRead / totalBytes.Value * 100;
					Status.SetProgress(progress, $"{totalRead / 1024} KB / {totalBytes.Value / 1024} KB");
				}
			}

			State = DownloadItemState.Completed;
			Status.Done();
		} catch (OperationCanceledException) {
			// 重点修改：在这里判断到底是“暂停”还是“取消”
			if (isCanceling) {
				State = DownloadItemState.Canceled;
				Status.ErrorClose("Canceled");

				// 此时因为上面已经跳出了 try 块，流已经被 using 自动关闭，可以安全删除文件
				DeleteIncompleteFile();
			} else {
				State = DownloadItemState.Paused;
				Status.ErrorClose("Paused");
			}
		} catch (Exception ex) {
			State = DownloadItemState.Error;
			Status.Error(ex.Message);
		} finally {
			if (isSemaphoreAcquired) {
				semaphore.Release();
			}
		}
	}

	private void Pause() => cts?.Cancel();
	private bool CanPause() => State is DownloadItemState.Downloading or DownloadItemState.Pending;

	private async Task ResumeAsync() => await StartDownloadAsync();
	private bool CanResume() => State == DownloadItemState.Paused;

	private async Task RetryAsync() => await StartDownloadAsync();
	private bool CanRetry() => State == DownloadItemState.Error;

	// --- Cancel Implementation ---

	private void Cancel() {
		isCanceling = true;

		if (State is DownloadItemState.Pending or DownloadItemState.Downloading) {
			// 如果正在排队或下载，触发 CancellationToken
			// 这会引发 OperationCanceledException，在 catch 中去收尾和删文件
			cts?.Cancel();
		} else {
			// 如果已经是 Paused 或 Error 状态，直接改状态并删文件即可
			State = DownloadItemState.Canceled;
			Status.ErrorClose("Canceled");
			DeleteIncompleteFile();
		}
	}
	// 只有在完成状态下才不能取消
	private bool CanCancel() => State is not DownloadItemState.Completed and not DownloadItemState.Canceled;

	private void DeleteIncompleteFile() {
		try {
			if (File.Exists(DestinationPath)) {
				File.Delete(DestinationPath);
			}
		} catch (Exception ex) {
			// 有时如果文件被杀毒软件锁住可能会删除失败，此处记录日志或忽略
			Debug.WriteLine($"Failed to delete cancelled file: {ex.Message}");
		}
	}
}
