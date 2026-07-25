using System.ComponentModel;

namespace YiffBrowser.BaseFramework.Enums;

public enum DownloadItemState {
	[Description("Pending")] Pending,
	[Description("Downloading")] Downloading,
	[Description("Paused")] Paused,
	[Description("Completed")] Completed,
	[Description("Error")] Error,
	[Description("Canceled")] Canceled,
}
