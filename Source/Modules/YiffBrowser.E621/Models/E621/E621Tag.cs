using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.BaseFramework.Services;
using Newtonsoft.Json;
using RW.Base.WPF.Extensions;
using System.Collections.Concurrent;
using System.Windows.Media;

namespace YiffBrowser.E621.Models.E621;

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

	public static ConcurrentDictionary<string, E621Tag> Pool { get; } = [];

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
		bool isDarkTheme = IoC.GetService<IThemeManager>().IsDarkTheme();
		return category switch {
			// Align with e621 API category ids 0–8
			E621TagCategory.General => (isDarkTheme ? "#B4C7D9" : "#0B7EE2").HexToColor(),
			E621TagCategory.Artists => (isDarkTheme ? "#F2AC08" : "#E39B00").HexToColor(),
			E621TagCategory.Contributor => (isDarkTheme ? "#00AABB" : "#0088AA").HexToColor(),
			E621TagCategory.Copyrights => (isDarkTheme ? "#DD00DD" : "#DD00DD").HexToColor(),
			E621TagCategory.Characters => (isDarkTheme ? "#00AA00" : "#00AA00").HexToColor(),
			E621TagCategory.Species => (isDarkTheme ? "#ED5D1F" : "#ED5D1F").HexToColor(),
			E621TagCategory.Invalid => (isDarkTheme ? "#FF3D3D" : "#FF3D3D").HexToColor(),
			E621TagCategory.Meta => (isDarkTheme ? "#FFFFFF" : "#000000").HexToColor(),
			E621TagCategory.Lore => (isDarkTheme ? "#228822" : "#228822").HexToColor(),
			// e6ai display-only groups share artist / copyright colors
			E621TagCategory.Director => (isDarkTheme ? "#F2AC08" : "#E39B00").HexToColor(),
			E621TagCategory.Franchise => (isDarkTheme ? "#DD00DD" : "#DD00DD").HexToColor(),
			E621TagCategory.NotFound => (isDarkTheme ? "#B85277" : "#B40249").HexToColor(),
			_ => (isDarkTheme ? "#FFFFFF" : "#000000").HexToColor(),
		};
	}

	public static Color GetCategoryColor(int category) {
		if (Enum.IsDefined(typeof(E621TagCategory), category)) {
			return GetCategoryColor((E621TagCategory)category);
		}
		return GetCategoryColor(E621TagCategory.NotFound);
	}
}

/// <summary>
/// e621 API category ids 0–8. Director/Franchise are e6ai JSON-key display groups only
/// (API still reports them as Artists=1 / Copyrights=3).
/// </summary>
public enum E621TagCategory {
	NotFound = -1,
	General = 0,
	Artists = 1,
	Contributor = 2,
	Copyrights = 3,
	Characters = 4,
	Species = 5,
	Invalid = 6,
	Meta = 7,
	Lore = 8,
	Director = 101,
	Franchise = 103,
}
