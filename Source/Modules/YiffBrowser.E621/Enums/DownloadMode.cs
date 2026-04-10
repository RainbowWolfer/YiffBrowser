using System.ComponentModel;

namespace YiffBrowser.E621.Enums;

internal enum DownloadMode {
	[Description("None")] None,
	[Description("Download Selected Posts")] DownloadSelected,
	[Description("Download Current Page")] DownloadCurrentPage,
	[Description("Custom Download")] CustomDownload,
}
