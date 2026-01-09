using BaseFramework.Enums;
using DevExpress.Mvvm;
using YB.E621.Services;

namespace YB.E621.ViewModels;

public class PostTabItem : BindableBase {

	public E621API Api { get; }

	public ModuleType SiteType { get; }
	public string[] Tags { get; }

	public PostTabItem(ModuleType moduleType, string[] tags) {
		Api = E621API.GetAPI(moduleType);

		SiteType = moduleType;
		Tags = tags;
	}
}