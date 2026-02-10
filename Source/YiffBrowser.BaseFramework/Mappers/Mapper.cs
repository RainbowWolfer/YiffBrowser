using AutoMapper;
using YiffBrowser.BaseFramework.Services;

namespace YiffBrowser.BaseFramework.Mappers;

internal class Mapper : Profile {
	public Mapper() {
		CreateMap<AppSettingsModel, AppSettingsModel>();
		CreateMap<AppProfileModel, AppProfileModel>();
	}
}
