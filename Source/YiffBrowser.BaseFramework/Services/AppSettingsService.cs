using Newtonsoft.Json;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;
using YiffBrowser.BaseFramework.Enums;

namespace YiffBrowser.BaseFramework.Services;


public interface IAppSettingsService : ISettingsServiceBase<AppSettingsModel> {

}

public class AppSettingsService : JsonSettingsServiceBase<AppSettingsModel>, ISingletonDependency, IAppSettingsService {
	private static AppSettingsService? instance;
	public static AppSettingsService Instance => instance!;

	private readonly AppFolderConfig appFolderConfig;

	public AppSettingsService(AppFolderConfig appFolderConfig) {
		instance = this;
		this.appFolderConfig = appFolderConfig;
	}

	public override string FilePath => appFolderConfig.AppSettingsFilePath;
	public override AppSettingsModel GetDefaultModel() => new();
}



/// <summary> map to self </summary>
[JsonObject]
public class AppSettingsModel {
	public bool EnableTrayIcon { get; set; } = true;
	public string DownloadFolderPath { get; set; } = string.Empty;

	public int MaxConcurrentDownloads { get; set; } = 3;

	public FileCollisionBehaviorType FileCollisionBehavior { get; set; } = FileCollisionBehaviorType.SkipIfSameSize;

	public bool IsGroupBySearchedTags { get; set; }
	public bool IsGroupByAuthorTagOnly { get; set; }

	public string FileNameTemplate { get; set; } = "<id>";

	public PreviewQuality PreviewQuality { get; set; } = PreviewQuality.Sample;

	/// <summary>Off by default. When on, preview/sample/file bytes are kept under <see cref="MediaCacheFolder"/>.</summary>
	public bool EnableMediaCache { get; set; }

	/// <summary>Empty uses <c>%LocalAppData%\YiffBrowser\MediaCache</c>.</summary>
	public string MediaCacheFolder { get; set; } = string.Empty;

	/// <summary>0 means the built-in default (32 GiB).</summary>
	public long MediaCacheMaxBytes { get; set; } = MediaDiskCache.DefaultMaxBytes;

	public GifAutoPlayType GifAutoPlayType { get; set; } = GifAutoPlayType.WhenMouseOver;

	public ProxyMode ProxyMode { get; set; } = ProxyMode.System;
	public string ProxyHost { get; set; } = "127.0.0.1";
	public int ProxyPort { get; set; } = 7890;
	public string ProxyUsername { get; set; } = string.Empty;
	public string ProxyPassword { get; set; } = string.Empty;
}

