using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Interfaces;

internal interface IPostViewModel {
	E621Post? Post { get; set; }
}
