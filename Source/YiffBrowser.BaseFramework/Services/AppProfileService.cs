using YiffBrowser.BaseFramework.Enums;
using Newtonsoft.Json;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;

namespace YiffBrowser.BaseFramework.Services;

public interface IAppProfileService : ISettingsServiceBase<AppProfileModel> {
	(string? username, string? apiKey) GetUser(ModuleType moduleType);
	void SetUser(ModuleType moduleType, string? username, string? apiKey);
}

public class AppProfileService(
	AppFolderConfig appFolderConfig
) : JsonSettingsServiceBase<AppProfileModel>, ISingletonDependency, IAppProfileService {
	public override string FilePath => appFolderConfig.AppProfileFilePath;
	public override AppProfileModel GetDefaultModel() => new();


	public (string? username, string? apiKey) GetUser(ModuleType moduleType) {
		return moduleType switch {
			ModuleType.E621 => (Model.E621_Username ?? string.Empty, Model.E621_ApiKey ?? string.Empty),
			ModuleType.E6AI => (Model.E6AI_Username ?? string.Empty, Model.E6AI_ApiKey ?? string.Empty),
			ModuleType.E926 => (Model.E926_Username ?? string.Empty, Model.E926_ApiKey ?? string.Empty),
			_ => throw new NotImplementedException(),
		};
	}

	public void SetUser(ModuleType moduleType, string? username, string? apiKey) {
		switch (moduleType) {
			case ModuleType.E621:
				Model.E621_Username = username;
				Model.E621_ApiKey = apiKey;
				break;
			case ModuleType.E6AI:
				Model.E6AI_Username = username;
				Model.E6AI_ApiKey = apiKey;
				break;
			case ModuleType.E926:
				Model.E926_Username = username;
				Model.E926_ApiKey = apiKey;
				break;
			default:
				throw new NotImplementedException();
		}
	}

}

/// <summary> map to self </summary>
[JsonObject]
public class AppProfileModel {

	public string? E621_Username { get; set; }
	public string? E621_ApiKey { get; set; }

	public string? E6AI_Username { get; set; }
	public string? E6AI_ApiKey { get; set; }

	public string? E926_Username { get; set; }
	public string? E926_ApiKey { get; set; }


}

