using BaseFramework.Enums;
using YB.E621.Models.E621;

namespace YB.E621.Helpers {
	internal static class InternalHelper {
		public static FileType GetFileType(this E621Post post) {
			if (post.File is null || post.File.Ext is null) {
				return FileType.Unknown;
			}
			return post.File.Ext.ToLower().Trim() switch {
				"jpg" => FileType.JPG,
				"png" => FileType.PNG,
				"gif" => FileType.GIF,
				"anim" or "swf" => FileType.ANIM,
				"webm" => FileType.WEBM,
				_ => FileType.Unknown,
			};
		}

	}
}
