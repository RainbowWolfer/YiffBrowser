using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Services;

public static class E621MetaCompletions {
	private readonly record struct Entry(string Name, string Hint);

	private static readonly Entry[] Entries = [
		new("order:new", "Newest"),
		new("order:score", "Score"),
		new("order:favcount", "Favorites"),
		new("order:rank", "Rank"),
		new("order:random", "Random"),
		new("type:jpg", "JPEG"),
		new("type:png", "PNG"),
		new("type:gif", "GIF"),
		new("type:webp", "WebP"),
		new("type:webm", "WebM"),
		new("type:mp4", "MP4"),
		new("type:anim", "Flash"),
		new("type:swf", "Flash (SWF)"),
		new("rating:safe", "Safe"),
		new("rating:questionable", "Questionable"),
		new("rating:explicit", "Explicit"),
		new("rating:s", "Safe"),
		new("rating:q", "Questionable"),
		new("rating:e", "Explicit"),
	];

	public static E621AutoComplete[] Match(string? word) {
		if (string.IsNullOrWhiteSpace(word)) {
			return [];
		}

		string text = word.Trim();
		bool negated = text.StartsWith('-');
		string query = (negated ? text[1..] : text).ToLowerInvariant();
		if (query.Length < 2 && !query.Contains(':')) {
			return [];
		}

		List<E621AutoComplete> matches = [];
		foreach (Entry entry in Entries) {
			string value = entry.Name[(entry.Name.IndexOf(':') + 1)..];
			if (!entry.Name.StartsWith(query, StringComparison.Ordinal) && !value.StartsWith(query, StringComparison.Ordinal)) {
				continue;
			}

			matches.Add(new E621AutoComplete {
				Name = negated ? "-" + entry.Name : entry.Name,
				AntecedentName = entry.Hint,
				Category = (int)E621TagCategory.Meta,
				PostCount = -1,
			});
		}
		return [.. matches];
	}
}
