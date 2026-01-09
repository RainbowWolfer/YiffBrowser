using Newtonsoft.Json;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;

namespace BaseFramework.Services;

public interface IAppProfileService : ISettingsServiceBase<AppProfileModel> {

}

public class AppProfileService(
	AppFolderConfig appFolderConfig
) : JsonSettingsServiceBase<AppProfileModel>, ISingletonDependency, IAppProfileService {
	public override string FilePath => appFolderConfig.AppProfileFilePath;
	public override AppProfileModel GetDefaultModel() => new();
}

/// <summary> map to self </summary>
[JsonObject]
public class AppProfileModel {

}

