using AutoMapper;
using YiffBrowser.E621.Models;

namespace YiffBrowser.E621.Mappers;

internal class Mapper : Profile {
	public Mapper() {
		CreateMap<E621ProfileModel, E621ProfileModel>();
	}

}
