using System.ComponentModel;

namespace YiffBrowser.E621.Enums;

public enum SortBy {
	[Description("Sort by: Default")] Default,
	[Description("Sort by: New")] New,
	[Description("Sort by: Score")] Score,
	[Description("Sort by: FavCount")] FavCount,
	[Description("Sort by: Rank")] Rank,
	[Description("Sort by: Random")] Random,
}

public static class SortByExtension {

	public static string GetMeta(this SortBy sortBy) {
		return sortBy switch {
			SortBy.Default => "",
			SortBy.New => "order:new",
			SortBy.Score => "order:score",
			SortBy.FavCount => "order:favcount",
			SortBy.Rank => "order:rank",
			SortBy.Random => "order:random",
			_ => "",
		};
	}

	public static SortBy GetSortBy(this IEnumerable<string> set) {
		if (set.Contains("order:new")) {
			return SortBy.New;
		} else if (set.Contains("order:rank")) {
			return SortBy.Rank;
		} else if (set.Contains("order:random")) {
			return SortBy.Random;
		} else if (set.Contains("order:favcount")) {
			return SortBy.FavCount;
		} else if (set.Contains("order:score")) {
			return SortBy.Score;
		} else {
			return SortBy.Default;
		}
	}
}