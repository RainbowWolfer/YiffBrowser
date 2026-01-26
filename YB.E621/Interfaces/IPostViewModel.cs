using YB.E621.Models.E621;

namespace YB.E621.Interfaces;

internal interface IPostViewModel {
	E621Post? Post { get; set; }
}
