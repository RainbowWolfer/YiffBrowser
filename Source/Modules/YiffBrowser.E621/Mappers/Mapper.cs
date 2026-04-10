using Mapster;
using YiffBrowser.E621.Models;

namespace YiffBrowser.E621.Mappers;

internal class Mapper : IRegister {
	public void Register(TypeAdapterConfig config) {
		config.NewConfig<E621ProfileModel, E621ProfileModel>();
	}
}
