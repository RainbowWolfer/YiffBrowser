using BaseFramework.Helpers;
using Newtonsoft.Json;
using System.Windows.Media;

namespace YB.E621.Models.E621 {
	public class E621Tag {
		[JsonProperty("id")]
		public int ID { get; set; }
		[JsonProperty("name")]
		public string? Name { get; set; }
		[JsonProperty("post_count")]
		public int PostCount { get; set; }
		[JsonProperty("related_tags")]
		public string? RelatedTags { get; set; }
		[JsonProperty("related_tags_updated_at")]
		public DateTime RelatedTagsUpdatedAt { get; set; }
		[JsonProperty("category")]
		public int Category { get; set; }
		[JsonProperty("is_locked")]
		public bool IsLocked { get; set; }
		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }
		[JsonProperty("updated_at")]
		public DateTime UpdatedAt { get; set; }

		public string PostCountInK => PostCount.NumberToK();

		public static Dictionary<string, E621Tag> Pool { get; } = [];

		public override string ToString() {
			return $"E621Tags:({ID})({Name})({RelatedTags})({PostCount})({Category})";
		}

		public static string GetCategory(E621TagCategory category) {
			return category.ToString();
		}

		public static string GetCategory(int category) {
			return GetCategory((E621TagCategory)category);
		}

		public static Color GetCategoryColor(E621TagCategory category) {
			//bool isDarkTheme = YiffApp.IsDarkTheme();
			bool isDarkTheme = false;
			return category switch {
				E621TagCategory.Artists => (isDarkTheme ? "#F2AC08" : "#E39B00").HexToColor(),
				E621TagCategory.Copyrights => (isDarkTheme ? "#DD00DD" : "#DD00DD").HexToColor(),
				E621TagCategory.Species => (isDarkTheme ? "#ED5D1F" : "#ED5D1F").HexToColor(),
				E621TagCategory.Director => (isDarkTheme ? "#00AA00" : "#00AA00").HexToColor(),
				E621TagCategory.Characters => (isDarkTheme ? "#00AA00" : "#00AA00").HexToColor(),
				E621TagCategory.General => (isDarkTheme ? "#B4C7D9" : "#0B7EE2").HexToColor(),
				E621TagCategory.Meta => (isDarkTheme ? "#FFFFFF" : "#000000").HexToColor(),
				E621TagCategory.Invalid => (isDarkTheme ? "#FF3D3D" : "#FF3D3D").HexToColor(),
				E621TagCategory.Lore => (isDarkTheme ? "#228822" : "#228822").HexToColor(),
				E621TagCategory.NotFound => (isDarkTheme ? "#B85277" : "#B40249").HexToColor(),
				E621TagCategory.UnKnown => (isDarkTheme ? "#CCCBF9" : "#050507").HexToColor(),
				_ => (isDarkTheme ? "#FFFFFF" : "#000000").HexToColor(),
			};
		}

		public static Color GetCategoryColor(int category) {
			return GetCategoryColor((E621TagCategory)category);
		}
	}

	public enum E621TagCategory {
		NotFound = -1,
		UnKnown = 0,
		General = 1,
		Artists = 2,
		Director = 3,
		Characters = 4,
		Copyrights = 5,
		Species = 6,
		Invalid = 7,
		Meta = 8,
		Lore = 9,
	}
}