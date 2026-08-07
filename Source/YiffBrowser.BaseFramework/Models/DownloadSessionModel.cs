using Newtonsoft.Json;
using YiffBrowser.BaseFramework.Enums;

namespace YiffBrowser.BaseFramework.Models;

[JsonObject]
public class DownloadSessionModel {
	public List<DownloadItemSnapshot> Items { get; set; } = [];
}

[JsonObject]
public class DownloadItemSnapshot {
	public string FileUrl { get; set; } = string.Empty;
	public string DestinationPath { get; set; } = string.Empty;
	public string? PreviewUrl { get; set; }
	public DownloadItemState State { get; set; }
	public double? Progress { get; set; }
	public string? DownloadInfo { get; set; }
	public string? ErrorMessage { get; set; }
	public string? CompletionSummary { get; set; }
	public string? CompletionReason { get; set; }
	public string? FileSizeText { get; set; }
	public FileCollisionBehaviorType CollisionBehavior { get; set; } = FileCollisionBehaviorType.SkipIfSameSize;
	public string? IndexRootFolder { get; set; }
	public string? IndexSite { get; set; }
	public string? IndexItemId { get; set; }
	public string? IndexMd5 { get; set; }

	/// <summary>When the item finished (completed, failed, or canceled).</summary>
	public DateTime? FinishedAt { get; set; }
}
