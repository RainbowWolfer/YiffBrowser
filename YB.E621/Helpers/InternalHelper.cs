using BaseFramework.Enums;
using BaseFramework.Helpers;
using System.Windows.Media;
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

		public static Color GetRatingColor(this E621Rating rating) {
			//bool isDark = YiffApp.IsDarkTheme();
			bool isDark = false;
			return rating switch {
				E621Rating.Safe => (isDark ? "#008000" : "#36973E").HexToColor(),
				E621Rating.Questionable => (isDark ? "#FFFF00" : "#EFC50C").HexToColor(),
				E621Rating.Explicit => (isDark ? "#FF0000" : "#C92A2D").HexToColor(),
				_ => (isDark ? "#FFF" : "#000").HexToColor(),
			};
		}

		public static string GetRatingIcon(this E621Rating rating) {
			return rating switch {
				E621Rating.Safe => "\uF78C",
				E621Rating.Questionable => "\uE897",
				E621Rating.Explicit => "\uE814",
				_ => "\uE8BB",
			};
		}

	}
}
