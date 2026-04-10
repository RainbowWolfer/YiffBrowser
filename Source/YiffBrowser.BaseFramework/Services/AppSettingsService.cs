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
}

