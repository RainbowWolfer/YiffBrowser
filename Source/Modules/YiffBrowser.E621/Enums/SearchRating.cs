using System.ComponentModel;

namespace YiffBrowser.E621.Enums;

public enum SearchRating {
	[Description("Rating: All")] All,
	[Description("Rating: Safe")] Safe,
	[Description("Rating: Questionable")] Questionable,
	[Description("Rating: Explicit")] Explicit,
}

public enum MetaCompare {
	[Description("Any")] None,
	[Description("=")] Equal,
	[Description("≥")] GreaterOrEqual,
	[Description("≤")] LessOrEqual,
	[Description("[ ... ]")] Range,
}

public static class SearchRatingExtension {
	public static string GetMeta(this SearchRating rating) {
		return rating switch {
			SearchRating.Safe => "rating:safe",
			SearchRating.Questionable => "rating:questionable",
			SearchRating.Explicit => "rating:explicit",
			_ => "",
		};
	}

	public static SearchRating GetSearchRating(this IEnumerable<string> tags) {
		foreach (string tag in tags) {
			switch (tag) {
				case "rating:safe":
				case "rating:s":
					return SearchRating.Safe;
				case "rating:questionable":
				case "rating:q":
					return SearchRating.Questionable;
				case "rating:explicit":
				case "rating:e":
					return SearchRating.Explicit;
			}
		}
		return SearchRating.All;
	}
}

public static class CountTag {
	public static string Format(string root, MetaCompare compare, int from, int to) {
		return compare switch {
			MetaCompare.Equal => $"{root}{from}",
			MetaCompare.GreaterOrEqual => $"{root}>={from}",
			MetaCompare.LessOrEqual => $"{root}<={from}",
			MetaCompare.Range => $"{root}{Math.Min(from, to)}..{Math.Max(from, to)}",
			_ => "",
		};
	}

	public static bool TryParse(string token, string root, out MetaCompare compare, out int from, out int to) {
		compare = MetaCompare.None;
		from = 0;
		to = 0;
		if (!token.StartsWith(root, StringComparison.Ordinal)) {
			return false;
		}

		string rest = token[root.Length..];
		int dots = rest.IndexOf("..", StringComparison.Ordinal);
		if (dots > 0 && int.TryParse(rest[..dots], out from) && int.TryParse(rest[(dots + 2)..], out to)) {
			compare = MetaCompare.Range;
			return true;
		}
		if (rest.StartsWith(">=") && int.TryParse(rest[2..], out from)) {
			compare = MetaCompare.GreaterOrEqual;
			return true;
		}
		if (rest.StartsWith('>') && int.TryParse(rest[1..], out from)) {
			compare = MetaCompare.GreaterOrEqual;
			return true;
		}
		if (rest.StartsWith("<=") && int.TryParse(rest[2..], out from)) {
			compare = MetaCompare.LessOrEqual;
			return true;
		}
		if (rest.StartsWith('<') && int.TryParse(rest[1..], out from)) {
			compare = MetaCompare.LessOrEqual;
			return true;
		}
		if (int.TryParse(rest, out from)) {
			compare = MetaCompare.Equal;
			return true;
		}
		return false;
	}
}
