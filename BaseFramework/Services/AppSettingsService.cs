using Newtonsoft.Json;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;

namespace BaseFramework.Services;


public interface IAppSettingsService : ISettingsServiceBase<AppSettingsModel> {

}

public class AppSettingsService(
	AppFolderConfig appFolderConfig
) : JsonSettingsServiceBase<AppSettingsModel>, ISingletonDependency, IAppSettingsService {
	public override string FilePath => appFolderConfig.AppSettingsFilePath;
	public override AppSettingsModel GetDefaultModel() => new();
}



/// <summary> map to self </summary>
[JsonObject]
public class AppSettingsModel {

}

