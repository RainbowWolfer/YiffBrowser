namespace YiffBrowser.E621.ViewModels;

/// <summary>Contract for tab content hosts so session/refresh work across Search/Pool/Relations.</summary>
internal interface IPostTabContent : IDisposable {
	int CurrentPage { get; }
	void RefreshPosts();
}

internal static class PostTabTitleHelper {
	public static string FromTags(string[]? tags) {
		if (tags is null || tags.Length == 0) {
			return string.Empty;
		}
		if (tags.Length == 1 && string.IsNullOrWhiteSpace(tags[0])) {
			return string.Empty;
		}
		return string.Join(" ", tags);
	}

	public static string ForPool(int poolId, string? poolName = null) {
		if (!string.IsNullOrWhiteSpace(poolName)) {
			return $"Pool: {poolName.Replace('_', ' ')}";
		}
		return $"Pool #{poolId}";
	}

	public static string ForRelations(int rootPostId) => $"Relations · #{rootPostId}";
}
