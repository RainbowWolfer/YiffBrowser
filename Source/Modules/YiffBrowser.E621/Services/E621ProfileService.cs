using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;
using YiffBrowser.BaseFramework;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models;

namespace YiffBrowser.E621.Services;

public interface IE621ProfileService : ISettingsServiceBase<E621ProfileModel> {
	(string? username, string? apiKey) GetUser(ModuleType moduleType);
	void SetUser(ModuleType moduleType, string? username, string? apiKey);
}

internal class E621ProfileService(
	AppFolderConfig appFolderConfig
) : JsonSettingsServiceBase<E621ProfileModel>, ISingletonDependency, IE621ProfileService {

	public override string FilePath => appFolderConfig.E621ProfileFilePath;
	public override E621ProfileModel GetDefaultModel() => new();
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
