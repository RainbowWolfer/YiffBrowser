namespace YiffBrowser.BaseFramework.Interfaces;

public interface IDownloadable {
	string DownloadUrl { get; }
	string TargetFileName { get; }
	string? PreviewUrl { get; }

	/// <summary>Optional subdirectory under the destination folder (e.g. grouping by tags/authors).</summary>
	string? SubDirectory => null;
}
