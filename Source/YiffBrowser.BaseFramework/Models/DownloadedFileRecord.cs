using LiteDB;

namespace YiffBrowser.BaseFramework.Models;

/// <summary>One downloaded file entry stored in {downloadRoot}/.yiffbrowser/index.db.</summary>
public class DownloadedFileRecord {
	/// <summary>Composite id: "{site}|{itemId}" for stable upserts.</summary>
	[BsonId]
	public string Id { get; set; } = string.Empty;

	public string Site { get; set; } = string.Empty;

	public string ItemId { get; set; } = string.Empty;

	public string? Md5 { get; set; }

	public string FileName { get; set; } = string.Empty;

	/// <summary>Path relative to the download root folder.</summary>
	public string RelativePath { get; set; } = string.Empty;

	public DateTime DownloadedAt { get; set; }

	public static string MakeId(string site, string itemId) => $"{site}|{itemId}";
}
