using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.Enums;

public enum E621FileType {
	Unknown,
	PNG,
	JPG,
	GIF,
	WEBM,
	ANIM,
}

public static class E621FileTypeExtension {

	public static E621FileType? GetFileType(this IEnumerable<string> set) {
		if (set.Contains("type:png")) {
			return E621FileType.PNG;
		} else if (set.Contains("type:jpg")) {
			return E621FileType.JPG;
		} else if (set.Contains("type:gif")) {
			return E621FileType.GIF;
		} else if (set.Contains("type:webm")) {
			return E621FileType.WEBM;
		} else if (set.Contains("type:anim")) {
			return E621FileType.ANIM;
		} else {
			return null;
		}
	}

	public static string GetMeta(this E621FileType? sortBy) {
		return sortBy switch {
			null => "",
			E621FileType.Unknown => "",
			E621FileType.PNG => "type:png",
			E621FileType.JPG => "type:jpg",
			E621FileType.GIF => "type:gif",
			E621FileType.WEBM => "type:webm",
			E621FileType.ANIM => "type:anim",
			_ => "",
		};
	}

	public static PostDisplayType GetPostDisplayType(this E621FileType fileType) {
		return fileType switch {
			E621FileType.Unknown => PostDisplayType.Unknown,
			E621FileType.ANIM => PostDisplayType.NotSupported,
			E621FileType.PNG or E621FileType.JPG or E621FileType.GIF => PostDisplayType.Image,
			E621FileType.WEBM => PostDisplayType.Video,
			_ => PostDisplayType.Unknown,
		};
	}

	public static string GetIconText(this E621FileType fileType) {
		return fileType switch {
			E621FileType.PNG or E621FileType.JPG => "\uEB9F",
			E621FileType.GIF => "\uF4A9",
			E621FileType.WEBM => "\uE714",
			_ => "\uE9CE",
		};
	}

	public static E621FileType GetFileType(this E621Post post) {
		if (post.File is null || post.File.Ext is null) {
			return E621FileType.Unknown;
		}
		return post.File.Ext.ToLower().Trim() switch {
			"jpg" => E621FileType.JPG,
			"png" => E621FileType.PNG,
			"gif" => E621FileType.GIF,
			"anim" or "swf" => E621FileType.ANIM,
			"webm" => E621FileType.WEBM,
			_ => E621FileType.Unknown,
		};
	}

	public static bool IsGif(this E621FileType fileType) => fileType is E621FileType.GIF;
	public static bool IsImage(this E621FileType fileType) => fileType is E621FileType.PNG or E621FileType.JPG or E621FileType.GIF;

	public static bool IsVideo(this E621FileType fileType) => fileType is E621FileType.WEBM;

}