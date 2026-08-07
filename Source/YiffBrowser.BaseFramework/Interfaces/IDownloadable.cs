namespace YiffBrowser.BaseFramework.Interfaces;

public interface IDownloadable {
	string DownloadUrl { get; }
	string TargetFileName { get; }
	string? PreviewUrl { get; }

	/// <summary>Optional subdirectory under the destination folder (e.g. grouping by tags/authors).</summary>
	string? SubDirectory => null;

	/// <summary>Site host used by the download-folder index (e.g. e621.net).</summary>
	string? IndexSite => null;

	/// <summary>Stable item id for the download-folder index (e.g. post id).</summary>
	string? IndexItemId => null;

	/// <summary>Optional content hash for the download-folder index.</summary>
	string? IndexMd5 => null;
}
