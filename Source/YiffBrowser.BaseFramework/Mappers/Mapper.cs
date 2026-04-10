using Mapster;
using YiffBrowser.BaseFramework.Services;

namespace YiffBrowser.BaseFramework.Mappers;

internal class Mapper : IRegister {
	public void Register(TypeAdapterConfig config) {
		config.NewConfig<AppSettingsModel, AppSettingsModel>();
		config.NewConfig<AppProfileModel, AppProfileModel>();
	}
}
