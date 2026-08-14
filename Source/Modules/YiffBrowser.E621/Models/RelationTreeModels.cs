using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Models;

public sealed class RelationTreeNode {
	public required E621Post Post { get; init; }
	public List<RelationTreeNode> Children { get; } = [];
	public int Depth { get; set; }
	public bool IsSeed { get; set; }
}

public sealed class RelationTreeBuildResult(
	RelationTreeNode? Root,
	IReadOnlyList<E621Post> FlatOrdered,
	IReadOnlyList<E621Post> AllPosts
) {
	public RelationTreeNode? Root { get; } = Root;
	public IReadOnlyList<E621Post> FlatOrdered { get; } = FlatOrdered;
	public IReadOnlyList<E621Post> AllPosts { get; } = AllPosts;
}
