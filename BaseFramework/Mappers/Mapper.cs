using AutoMapper;
using BaseFramework.Services;

namespace BaseFramework.Mappers;

internal class Mapper : Profile {
	public Mapper() {
		CreateMap<AppSettingsModel, AppSettingsModel>();
		CreateMap<AppProfileModel, AppProfileModel>();
	}
}
