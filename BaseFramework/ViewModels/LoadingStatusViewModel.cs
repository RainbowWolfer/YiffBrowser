using DevExpress.Mvvm;

namespace BaseFramework.ViewModels;

public class LoadingStatusViewModel : BindableBase {

	public bool ShowLoading {
		get => GetProperty(() => ShowLoading);
		set => SetProperty(() => ShowLoading, value);
	}

	public string DownloadInfo {
		get => GetProperty(() => DownloadInfo);
		set => SetProperty(() => DownloadInfo, value);
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

	public TimeSpan EllapsedTimeSpan {
		get => GetProperty(() => EllapsedTimeSpan);
		set => SetProperty(() => EllapsedTimeSpan, value);
	}

	public void InitialLoading() {
		ShowLoading = true;
		Progress = null;
		ErrorMessage = string.Empty;
		DownloadInfo = "Initializing";
		StartDateTime = DateTime.Now;
		ToolTip =
			"Start Downloading\n" +
			$"Start DateTime: {StartDateTime}";
	}

	public void DoneLoading() {
		ShowLoading = false;
		DownloadInfo = "Done";
		EndDateTime = DateTime.Now;
		ToolTip =
			"Download Finished\n" +
			$"Start DateTime: {StartDateTime}\n" +
			$"End DateTime: {EndDateTime}";
	}

	public void LoadingError(string errorMessage) {
		ShowLoading = true;
		ErrorMessage = errorMessage;
		EndDateTime = DateTime.Now;
		ToolTip =
			"Download Error\n" +
			$"Start DateTime: {StartDateTime}\n" +
			$"End DateTime: {EndDateTime}";
	}

	public void SetProgress(double? progress, string downloadInfo) {
		ShowLoading = true;
		Progress = progress;
		DownloadInfo = downloadInfo;
		EllapsedTimeSpan = DateTime.Now - StartDateTime;
		ToolTip =
			"Downloading\n" +
			$"Start DateTime: {StartDateTime}\n" +
			$"Ellapsed TimeSpan: {EllapsedTimeSpan}";
	}
}
