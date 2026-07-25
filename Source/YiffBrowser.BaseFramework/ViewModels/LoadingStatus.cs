using DevExpress.Mvvm;

namespace YiffBrowser.BaseFramework.ViewModels;

public class LoadingStatus : BindableBase {

	public bool ShowLoading {
		get => GetProperty(() => ShowLoading);
		set => SetProperty(() => ShowLoading, value);
	}

	public string DownloadInfo {
		get => GetProperty(() => DownloadInfo);
		set => SetProperty(() => DownloadInfo, value);
	}

	public string SpeedText {
		get => GetProperty(() => SpeedText);
		set => SetProperty(() => SpeedText, value);
	}

	public string EtaText {
		get => GetProperty(() => EtaText);
		set => SetProperty(() => EtaText, value);
	}

	public double BytesPerSecond {
		get => GetProperty(() => BytesPerSecond);
		set => SetProperty(() => BytesPerSecond, value);
	}

	public long BytesRemaining {
		get => GetProperty(() => BytesRemaining);
		set => SetProperty(() => BytesRemaining, value);
	}

	public string ErrorMessage {
		get => GetProperty(() => ErrorMessage);
		set => SetProperty(() => ErrorMessage, value);
	}

	public double? Progress {
		get => GetProperty(() => Progress);
		set => SetProperty(() => Progress, value);
	}

	public string? ToolTip {
		get => GetProperty(() => ToolTip);
		set => SetProperty(() => ToolTip, value);
	}

	public DateTime StartDateTime {
		get => GetProperty(() => StartDateTime);
		set => SetProperty(() => StartDateTime, value);
	}

	public DateTime EndDateTime {
		get => GetProperty(() => EndDateTime);
		set => SetProperty(() => EndDateTime, value);
	}

	public TimeSpan ElapsedTimeSpan {
		get => GetProperty(() => ElapsedTimeSpan);
		set => SetProperty(() => ElapsedTimeSpan, value);
	}

	public void Initialize(string downloadInfo = "Initializing") {
		ShowLoading = true;
		Progress = null;
		ErrorMessage = string.Empty;
		DownloadInfo = downloadInfo;
		SpeedText = "—";
		EtaText = "—";
		BytesPerSecond = 0;
		BytesRemaining = 0;
		StartDateTime = DateTime.Now;
		ToolTip =
			"Start Downloading\n" +
			$"Start DateTime: {StartDateTime}";
	}

	public void Done(string downloadInfo = "Downloaded") {
		ShowLoading = false;
		DownloadInfo = downloadInfo;
		SpeedText = "—";
		EtaText = "—";
		BytesPerSecond = 0;
		BytesRemaining = 0;
		EndDateTime = DateTime.Now;
		ToolTip =
			$"{downloadInfo}\n" +
			$"Start DateTime: {StartDateTime}\n" +
			$"End DateTime: {EndDateTime}";
	}

	public void Error(string errorMessage, string? detail = null) {
		ShowLoading = true;
		ErrorMessage = errorMessage;
		SpeedText = "—";
		EtaText = "—";
		BytesPerSecond = 0;
		BytesRemaining = 0;
		EndDateTime = DateTime.Now;
		ToolTip = detail ?? (
			"Download Error\n" +
			$"Start DateTime: {StartDateTime}\n" +
			$"End DateTime: {EndDateTime}");
	}

	public void ErrorClose(string errorMessage, string? detail = null) {
		Error(errorMessage, detail);
		ShowLoading = false;
	}

	public void SetProgress(double? progress, string downloadInfo, string speedText = "", string etaText = "", double bytesPerSecond = 0, long bytesRemaining = 0) {
		ShowLoading = true;
		Progress = progress;
		DownloadInfo = downloadInfo;
		SpeedText = speedText;
		EtaText = etaText;
		BytesPerSecond = bytesPerSecond;
		BytesRemaining = bytesRemaining;
		ElapsedTimeSpan = DateTime.Now - StartDateTime;
		ToolTip =
			"Downloading\n" +
			$"Start DateTime: {StartDateTime}\n" +
			$"Elapsed TimeSpan: {ElapsedTimeSpan}\n" +
			$"Speed: {speedText}\n" +
			$"Time remaining: {etaText}";
	}

	/// <summary>Browser-style remaining time, e.g. "12s left", "3m left", "1h 05m left".</summary>
	public static string FormatEtaText(long bytesRemaining, double bytesPerSecond) {
		if (bytesPerSecond < 1 || bytesRemaining <= 0) {
			return "—";
		}

		TimeSpan eta = TimeSpan.FromSeconds(bytesRemaining / bytesPerSecond);
		if (eta.TotalHours >= 1) {
			return $"{(int)eta.TotalHours}h {eta.Minutes:D2}m left";
		}

		if (eta.TotalMinutes >= 1) {
			return $"{(int)eta.TotalMinutes}m left";
		}

		return $"{Math.Max(1, (int)Math.Ceiling(eta.TotalSeconds))}s left";
	}
}
