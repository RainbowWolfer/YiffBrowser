namespace YiffBrowser.BaseFramework.Interfaces;

public interface IDownloadable {
	string DownloadUrl { get; }
	string TargetFileName { get; }
	string? PreviewUrl { get; }
}
