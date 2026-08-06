using RW.Common.Helpers;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.Utilities;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Helpers;

internal static class DownloadGroupingHelper {

	/// <summary>
	/// Resolves an optional subdirectory under the download root based on settings.
	/// Author grouping wins if both flags are somehow set (UI keeps them mutually exclusive).
	/// </summary>
	public static string? ResolveSubDirectory(
		AppSettingsModel settings,
		IEnumerable<string>? searchedTags,
		E621Post post) {
		if (settings.IsGroupByAuthorTagOnly) {
			return FormatFolderName(GetAuthorTags(post), emptyFallback: "unknown");
		}

		if (settings.IsGroupBySearchedTags) {
			return FormatFolderName(searchedTags, emptyFallback: null);
		}

		return null;
	}

	private static IEnumerable<string> GetAuthorTags(E621Post post) {
		Tags? tags = post.Tags;
		if (tags is null) {
			yield break;
		}

		if (tags.Artist is not null) {
			foreach (string artist in tags.Artist) {
				yield return artist;
			}
		}

		if (tags.Director is not null) {
			foreach (string director in tags.Director) {
				yield return director;
			}
		}
	}

	private static string? FormatFolderName(IEnumerable<string>? tags, string? emptyFallback) {
		string[] parts = (tags ?? [])
			.Where(t => t.IsNotBlank())
			.Select(NameTemplateHandler.SanitizePathSegment)
			.Where(t => t.IsNotBlank())
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
			.ToArray();

		if (parts.Length == 0) {
			return emptyFallback;
		}

		return string.Join(", ", parts);
	}
}
