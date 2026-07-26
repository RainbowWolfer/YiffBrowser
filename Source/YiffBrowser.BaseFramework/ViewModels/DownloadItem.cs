using DevExpress.Mvvm;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Models;

namespace YiffBrowser.BaseFramework.ViewModels;

public class DownloadItem : BindableBase {
	private readonly HttpClient httpClient;
	private readonly SemaphoreSlim semaphore;
	private readonly FileCollisionBehaviorType collisionBehavior;
	private CancellationTokenSource? cts;
	private readonly Stopwatch downloadStopwatch = new();
	private long lastUiUpdateMs;

	// Distinguishes a hard cancel from a pause-triggered cancel.
	private bool isCanceling = false;

	public string FileUrl { get; }
	public string DestinationPath { get; private set; }
	public string FileName { get; private set; }
	public string DirectoryPath { get; private set; }
	public string? PreviewUrl { get; }
	public FileCollisionBehaviorType CollisionBehavior => collisionBehavior;

	public string FileSizeText {
		get => GetProperty(() => FileSizeText);
		private set => SetProperty(() => FileSizeText, value);
	}

	/// <summary>Short label on Completed items: "Downloaded" or "Skipped".</summary>
	public string CompletionSummary {
		get => GetProperty(() => CompletionSummary);
		private set => SetProperty(() => CompletionSummary, value);
	}

	/// <summary>Full completion detail for tooltip (e.g. skip reason).</summary>
	public string CompletionReason {
		get => GetProperty(() => CompletionReason);
		private set => SetProperty(() => CompletionReason, value);
	}

	public LoadingStatus Status { get; } = new LoadingStatus();

	public bool IsActive => State is DownloadItemState.Pending or DownloadItemState.Downloading or DownloadItemState.Paused;
	public bool IsCompleted => State == DownloadItemState.Completed;
	public bool IsFailed => State is DownloadItemState.Error or DownloadItemState.Canceled;
	public bool IsFinished => IsCompleted || IsFailed;

	public DownloadItemState State {
		get => GetProperty(() => State);
		set {
			SetProperty(() => State, value);
			RaisePropertyChanged(() => IsActive);
			RaisePropertyChanged(() => IsCompleted);
			RaisePropertyChanged(() => IsFailed);
			RaisePropertyChanged(() => IsFinished);
			PauseCommand.RaiseCanExecuteChanged();
			ResumeCommand.RaiseCanExecuteChanged();
			RetryCommand.RaiseCanExecuteChanged();
			CancelCommand.RaiseCanExecuteChanged();
			RemoveFromListCommand.RaiseCanExecuteChanged();
			OpenFolderCommand.RaiseCanExecuteChanged();
		}
	}

	public DelegateCommand PauseCommand => field ??= new DelegateCommand(Pause, CanPause);
	public AsyncCommand ResumeCommand => field ??= new AsyncCommand(ResumeAsync, CanResume);
	public AsyncCommand RetryCommand => field ??= new AsyncCommand(RetryAsync, CanRetry);
	public DelegateCommand CancelCommand => field ??= new DelegateCommand(Cancel, CanCancel);
	public DelegateCommand OpenFolderCommand => field ??= new DelegateCommand(OpenFolder, CanOpenFolder);
	public DelegateCommand RemoveFromListCommand => field ??= new DelegateCommand(RemoveFromList, () => IsFinished);
	public DelegateCommand CopyUrlCommand => field ??= new DelegateCommand(CopyUrl, () => FileUrl.IsNotBlank());
	public DelegateCommand CopyFilePathCommand => field ??= new DelegateCommand(CopyFilePath, () => DestinationPath.IsNotBlank());
	public DelegateCommand CopyFileNameCommand => field ??= new DelegateCommand(CopyFileName, () => FileName.IsNotBlank());

	public event EventHandler? RemoveRequested;

	public DownloadItem(
		string fileUrl,
		string destinationPath,
		HttpClient httpClient,
		SemaphoreSlim semaphore,
		string? previewUrl = null,
		FileCollisionBehaviorType collisionBehavior = FileCollisionBehaviorType.SkipIfSameSize) {
		FileUrl = fileUrl;
		DestinationPath = destinationPath;
		FileName = Path.GetFileName(destinationPath);
		DirectoryPath = Path.GetDirectoryName(destinationPath) ?? string.Empty;
		PreviewUrl = previewUrl;
		this.httpClient = httpClient;
		this.semaphore = semaphore;
		this.collisionBehavior = collisionBehavior;

		State = DownloadItemState.Pending;
		Status.Initialize("Pending in Queue");
		Status.Progress = 0;
	}

	/// <summary>Applies persisted state without starting a transfer. Active states become Paused.</summary>
	public void ApplyRestoredSnapshot(DownloadItemSnapshot snapshot) {
		DownloadItemState restoredState = snapshot.State switch {
			DownloadItemState.Pending or DownloadItemState.Downloading or DownloadItemState.Paused
				=> DownloadItemState.Paused,
			_ => snapshot.State,
		};

		CompletionSummary = snapshot.CompletionSummary ?? string.Empty;
		CompletionReason = snapshot.CompletionReason ?? string.Empty;
		FileSizeText = snapshot.FileSizeText ?? string.Empty;

		State = restoredState;

		switch (restoredState) {
			case DownloadItemState.Paused:
				Status.ShowLoading = false;
				Status.Progress = snapshot.Progress ?? 0;
				Status.DownloadInfo = string.IsNullOrWhiteSpace(snapshot.DownloadInfo)
					|| snapshot.DownloadInfo is "Downloading" or "Resuming" or "Waiting for available slot" or "Pending in Queue"
					? "Paused"
					: snapshot.DownloadInfo;
				Status.SpeedText = "—";
				Status.EtaText = "—";
				Status.BytesPerSecond = 0;
				Status.BytesRemaining = 0;
				Status.ErrorMessage = string.Empty;
				break;

			case DownloadItemState.Completed:
				Status.Done(snapshot.CompletionReason.IsNotBlank() ? snapshot.CompletionReason! : (snapshot.DownloadInfo ?? "Downloaded"));
				Status.Progress = snapshot.Progress ?? 100;
				break;

			case DownloadItemState.Error:
			case DownloadItemState.Canceled:
				Status.ErrorClose(
					snapshot.ErrorMessage.IsNotBlank() ? snapshot.ErrorMessage! : (snapshot.DownloadInfo ?? restoredState.ToString()),
					snapshot.ErrorMessage);
				Status.Progress = snapshot.Progress;
				break;
		}
	}

	public DownloadItemSnapshot ToSnapshot() => new() {
		FileUrl = FileUrl,
		DestinationPath = DestinationPath,
		PreviewUrl = PreviewUrl,
		State = State,
		Progress = Status.Progress,
		DownloadInfo = Status.DownloadInfo,
		ErrorMessage = Status.ErrorMessage,
		CompletionSummary = CompletionSummary,
		CompletionReason = CompletionReason,
		FileSizeText = FileSizeText,
		CollisionBehavior = collisionBehavior,
	};

	public async Task StartDownloadAsync() {
		cts = new CancellationTokenSource();
		CancellationToken token = cts.Token;
		bool isSemaphoreAcquired = false;

		isCanceling = false;

		// Keep prior progress while waiting/resuming so the bar does not jump back to 0.
		double? preservedProgress = Status.Progress is > 0 ? Status.Progress : null;
		string? preservedInfo = Status.DownloadInfo;
		long existingLength = GetExistingFileLength();
		bool isResume = existingLength > 0 && preservedProgress is > 0;

		try {
			// Stay Pending while queued for a slot; only flip to Downloading once transfer starts.
			State = DownloadItemState.Pending;
			Status.Initialize("Waiting for available slot");
			ApplyPreservedProgress(preservedProgress, preservedInfo, existingLength);

			await semaphore.WaitAsync(token);
			isSemaphoreAcquired = true;

			existingLength = GetExistingFileLength();
			isResume = existingLength > 0 && preservedProgress is > 0;

			if (!isResume && File.Exists(DestinationPath)) {
				bool handled = await TryHandleExistingFileAsync(token);
				if (handled) {
					return;
				}

				existingLength = GetExistingFileLength();
			}

			State = DownloadItemState.Downloading;
			Status.Initialize(existingLength > 0 ? "Resuming" : "Downloading");
			ApplyPreservedProgress(preservedProgress, preservedInfo, existingLength);
			downloadStopwatch.Restart();
			lastUiUpdateMs = 0;

			using HttpRequestMessage request = new(HttpMethod.Get, FileUrl);
			if (existingLength > 0) {
				request.Headers.Range = new RangeHeaderValue(existingLength, null);
			}

			using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

			long startOffset = 0;
			long? totalBytes = null;

			if (response.StatusCode == HttpStatusCode.PartialContent) {
				// Server accepted Range — continue from existing bytes.
				startOffset = existingLength;
				if (response.Content.Headers.ContentRange?.Length is long totalLength) {
					totalBytes = totalLength;
				} else if (response.Content.Headers.ContentLength is long contentLength) {
					totalBytes = existingLength + contentLength;
				}
			} else if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable) {
				// 416: local file may already be complete.
				if (existingLength > 0 && response.Content.Headers.ContentRange?.Length is long completeLength && existingLength >= completeLength) {
					MarkCompleted("Skipped: local file already complete");
					return;
				}

				response.EnsureSuccessStatusCode();
			} else {
				// 200 OK: full body. Restart from byte 0 even if a range was requested.
				response.EnsureSuccessStatusCode();
				startOffset = 0;
				totalBytes = response.Content.Headers.ContentLength;

				// Skip-if-same-size when we only learned remote size from GET.
				if (!isResume
					&& collisionBehavior == FileCollisionBehaviorType.SkipIfSameSize
					&& existingLength > 0
					&& totalBytes is long remoteSize
					&& existingLength == remoteSize) {
					MarkCompleted("Skipped: file already exists with the same size");
					return;
				}
			}

			using Stream contentStream = await response.Content.ReadAsStreamAsync(token);
			await using FileStream fileStream = CreateDestinationStream(startOffset);

			byte[] buffer = new byte[8192];
			long totalRead = startOffset;
			int readBytes;

			// Speed should reflect the current transfer, not include already-downloaded bytes.
			long sessionStartOffset = startOffset;

			// Sync bar to resumed offset as soon as total size is known.
			if (startOffset > 0) {
				UpdateProgress(totalRead, totalBytes, sessionStartOffset);
				if (Status.Progress is null && preservedProgress is > 0) {
					Status.Progress = preservedProgress;
				}
			}

			while ((readBytes = await contentStream.ReadAsync(buffer, token)) > 0) {
				await fileStream.WriteAsync(buffer.AsMemory(0, readBytes), token);
				totalRead += readBytes;
				UpdateProgress(totalRead, totalBytes, sessionStartOffset);
			}

			MarkCompleted(startOffset > 0 ? "Downloaded (resumed)" : "Downloaded");
		} catch (OperationCanceledException) {
			if (isCanceling) {
				State = DownloadItemState.Canceled;
				CompletionSummary = string.Empty;
				CompletionReason = string.Empty;
				Status.ErrorClose("Canceled", "Canceled");
				DeleteIncompleteFile();
			} else {
				State = DownloadItemState.Paused;
				Status.SpeedText = "—";
				Status.EtaText = "—";
				Status.BytesPerSecond = 0;
				Status.ShowLoading = false;
				// Keep determinate progress (0 if nothing transferred yet) — never leave indeterminate after pause.
				Status.Progress ??= 0;
				if (string.IsNullOrWhiteSpace(Status.DownloadInfo) || Status.DownloadInfo is "Downloading" or "Resuming" or "Waiting for available slot" or "Pending in Queue") {
					Status.DownloadInfo = "Paused";
				}
			}
		} catch (Exception ex) {
			State = DownloadItemState.Error;
			CompletionSummary = string.Empty;
			CompletionReason = string.Empty;
			Status.ErrorClose(ex.Message, ex.ToString());
		} finally {
			downloadStopwatch.Stop();
			if (isSemaphoreAcquired) {
				semaphore.Release();
			}
		}
	}

	/// <returns>True if the item was completed/skipped and the download should stop.</returns>
	private async Task<bool> TryHandleExistingFileAsync(CancellationToken token) {
		long localLength = GetExistingFileLength();
		if (localLength <= 0 && !File.Exists(DestinationPath)) {
			return false;
		}

		switch (collisionBehavior) {
			case FileCollisionBehaviorType.Skip:
				MarkCompleted("Skipped: file already exists");
				return true;

			case FileCollisionBehaviorType.SkipIfSameSize: {
				long? remoteSize = await TryGetRemoteContentLengthAsync(token);
				if (remoteSize is long size && localLength == size) {
					MarkCompleted("Skipped: file already exists with the same size");
					return true;
				}

				// Different size (or unknown remote size): overwrite and re-download.
				TryDeleteDestination();
				return false;
			}

			case FileCollisionBehaviorType.Overwrite:
				TryDeleteDestination();
				return false;

			case FileCollisionBehaviorType.AutoRename:
				ApplyUniqueDestinationPath();
				return false;

			default:
				return false;
		}
	}

	private async Task<long?> TryGetRemoteContentLengthAsync(CancellationToken token) {
		try {
			using HttpRequestMessage request = new(HttpMethod.Head, FileUrl);
			using HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
			if (response.IsSuccessStatusCode && response.Content.Headers.ContentLength is long length) {
				return length;
			}
		} catch (Exception ex) {
			Debug.WriteLine($"HEAD request failed for collision check: {ex.Message}");
		}

		return null;
	}

	private void ApplyUniqueDestinationPath() {
		string uniquePath = GetAvailablePath(DestinationPath);
		if (uniquePath == DestinationPath) {
			return;
		}

		DestinationPath = uniquePath;
		FileName = Path.GetFileName(uniquePath);
		DirectoryPath = Path.GetDirectoryName(uniquePath) ?? string.Empty;
		RaisePropertyChanged(nameof(DestinationPath));
		RaisePropertyChanged(nameof(FileName));
		RaisePropertyChanged(nameof(DirectoryPath));
	}

	private static string GetAvailablePath(string path) {
		if (!File.Exists(path)) {
			return path;
		}

		string? directory = Path.GetDirectoryName(path);
		string name = Path.GetFileNameWithoutExtension(path);
		string extension = Path.GetExtension(path);

		for (int i = 1; i < 10_000; i++) {
			string candidate = Path.Combine(directory ?? string.Empty, $"{name} ({i}){extension}");
			if (!File.Exists(candidate)) {
				return candidate;
			}
		}

		return Path.Combine(directory ?? string.Empty, $"{name} ({Guid.NewGuid():N}){extension}");
	}

	private void TryDeleteDestination() {
		try {
			if (File.Exists(DestinationPath)) {
				File.Delete(DestinationPath);
			}
		} catch (Exception ex) {
			Debug.WriteLine($"Failed to delete existing file before overwrite: {ex.Message}");
		}
	}

	private void MarkCompleted(string reason) {
		State = DownloadItemState.Completed;
		CompletionSummary = reason.StartsWith("Skipped", StringComparison.OrdinalIgnoreCase)
			? "Skipped"
			: "Downloaded";
		CompletionReason = reason;
		RefreshFileSizeText();
		Status.Done(reason);
	}

	private void RefreshFileSizeText() {
		try {
			if (File.Exists(DestinationPath)) {
				FileSizeText = new FileInfo(DestinationPath).Length.FileSizeToKB();
			}
		} catch (Exception ex) {
			Debug.WriteLine($"Failed to read file size: {ex.Message}");
		}
	}

	private void ApplyPreservedProgress(double? preservedProgress, string? preservedInfo, long existingLength) {
		if (preservedProgress is > 0) {
			Status.Progress = preservedProgress;
			if (existingLength > 0
				&& preservedInfo.IsNotBlank()
				&& preservedInfo is not ("Waiting for available slot" or "Resuming" or "Downloading" or "Pending in Queue" or "Paused")) {
				Status.DownloadInfo = preservedInfo;
			}
		} else {
			Status.Progress = 0;
		}
	}

	private FileStream CreateDestinationStream(long startOffset) {
		if (startOffset > 0) {
			FileStream stream = new(DestinationPath, FileMode.Open, FileAccess.Write, FileShare.None, 8192, true);
			stream.Seek(startOffset, SeekOrigin.Begin);
			return stream;
		}

		return new FileStream(DestinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
	}

	private long GetExistingFileLength() {
		try {
			if (File.Exists(DestinationPath)) {
				long length = new FileInfo(DestinationPath).Length;
				if (length > 0) {
					return length;
				}
			}
		} catch (Exception ex) {
			Debug.WriteLine($"Failed to read partial download length: {ex.Message}");
		}

		return 0;
	}

	private void UpdateProgress(long totalRead, long? totalBytes, long sessionStartOffset) {
		long elapsedMs = downloadStopwatch.ElapsedMilliseconds;
		bool isComplete = totalBytes.HasValue && totalRead >= totalBytes.Value;
		if (!isComplete && elapsedMs - lastUiUpdateMs < 200) {
			return;
		}

		lastUiUpdateMs = elapsedMs;

		double seconds = Math.Max(downloadStopwatch.Elapsed.TotalSeconds, 0.001);
		long sessionBytes = Math.Max(0, totalRead - sessionStartOffset);
		double bytesPerSecond = sessionBytes / seconds;
		string speedText = FormatSpeed(bytesPerSecond);

		if (totalBytes.HasValue) {
			long remaining = Math.Max(0, totalBytes.Value - totalRead);
			double progress = (double)totalRead / totalBytes.Value * 100;
			string info = $"{totalRead.FileSizeToKB()} / {totalBytes.Value.FileSizeToKB()}";
			string etaText = LoadingStatus.FormatEtaText(remaining, bytesPerSecond);
			Status.SetProgress(progress, info, speedText, etaText, bytesPerSecond, remaining);
		} else {
			Status.SetProgress(null, $"{totalRead.FileSizeToKB()} downloaded", speedText, "—", bytesPerSecond, 0);
		}
	}

	private static string FormatSpeed(double bytesPerSecond) {
		if (bytesPerSecond < 1) {
			return "—";
		}

		return $"{((long)bytesPerSecond).FileSizeToKB()}/s";
	}

	private void Pause() => cts?.Cancel();
	private bool CanPause() => State is DownloadItemState.Downloading or DownloadItemState.Pending;

	private async Task ResumeAsync() => await StartDownloadAsync();
	private bool CanResume() => State == DownloadItemState.Paused;

	private async Task RetryAsync() => await StartDownloadAsync();
	private bool CanRetry() => State is DownloadItemState.Error or DownloadItemState.Canceled;

	private void Cancel() {
		isCanceling = true;

		if (State is DownloadItemState.Pending or DownloadItemState.Downloading) {
			cts?.Cancel();
		} else {
			State = DownloadItemState.Canceled;
			CompletionSummary = string.Empty;
			CompletionReason = string.Empty;
			Status.ErrorClose("Canceled", "Canceled");
			DeleteIncompleteFile();
		}
	}

	private bool CanCancel() => State is not DownloadItemState.Completed and not DownloadItemState.Canceled;

	private void DeleteIncompleteFile() {
		try {
			if (File.Exists(DestinationPath)) {
				File.Delete(DestinationPath);
			}
		} catch (Exception ex) {
			Debug.WriteLine($"Failed to delete cancelled file: {ex.Message}");
		}
	}

	private void OpenFolder() {
		if (DirectoryPath.IsNotBlank()) {
			DirectoryPath.OpenPathInSystemDefault();
		}
	}

	private bool CanOpenFolder() => DirectoryPath.IsNotBlank() && (State == DownloadItemState.Completed || Directory.Exists(DirectoryPath));

	private void RemoveFromList() => RemoveRequested?.Invoke(this, EventArgs.Empty);

	private void CopyUrl() => FileUrl.CopyToClipboard();

	private void CopyFilePath() => DestinationPath.CopyToClipboard();

	private void CopyFileName() => FileName.CopyToClipboard();
}
