using DevExpress.Mvvm;
using YiffBrowser.E621.Parameters;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.ViewModels;

internal class PostTabItem : BindableBase {

	public E621API Api { get; }

	public E621MainViewModel ParentViewModel { get; }
	public ViewParameter ViewParameter { get; }

	public string[] Tags { get; }

	public PostsView View { get; }

	public PostTabItem(E621MainViewModel parentViewModel, string[] tags) {
		ParentViewModel = parentViewModel ?? throw new ArgumentNullException(nameof(parentViewModel));
		ViewParameter = parentViewModel.ViewParameter ?? throw new ArgumentNullException(nameof(ViewParameter));

		Api = E621API.GetAPI(ViewParameter.ModuleType);

		ParentViewModel = parentViewModel;
		Tags = tags ?? [];

		View = new PostsView(this);
	}
}