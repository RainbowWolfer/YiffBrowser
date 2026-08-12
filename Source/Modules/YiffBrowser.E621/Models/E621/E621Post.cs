using Newtonsoft.Json;
using RW.Common.Data;
using RW.Common.Helpers;
using System.Runtime.Serialization;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Services;

namespace YiffBrowser.E621.Models.E621;

public class E621Post {

	[JsonProperty("id")]
	public int ID { get; set; }

	[JsonProperty("created_at")]
	public DateTime? CreatedAt { get; set; }

	[JsonProperty("updated_at")]
	public DateTime? UpdatedAt { get; set; }

	[JsonProperty("file")]
	public File? File { get; set; }

	[JsonProperty("preview")]
	public Preview? Preview { get; set; }

	[JsonProperty("sample")]
	public Sample? Sample { get; set; }

	[JsonProperty("score")]
	public Score? Score { get; set; }

	[JsonProperty("tags")]
	public Tags? Tags { get; set; }

	[JsonProperty("locked_tags")]
	public List<string>? LockedTags { get; set; }

	[JsonProperty("change_seq")]
	public int ChangeSeq { get; set; }

	[JsonProperty("flags")]
	public Flags? Flags { get; set; }

	[JsonProperty("rating")]
	public E621Rating Rating { get; set; }

	[JsonProperty("fav_count")]
	public int FavCount { get; set; }

	[JsonProperty("sources")]
	public List<string>? Sources { get; set; }

	[JsonProperty("pools")]
	public List<int>? Pools { get; set; }

	[JsonProperty("relationships")]
	public Relationships? Relationships { get; set; }

	[JsonProperty("approver_id")]
	public string? ApproverId { get; set; }

	[JsonProperty("uploader_id")]
	public int UploaderId { get; set; }

	[JsonProperty("uploader_name")]
	public string? UploaderName { get; set; }

	[JsonProperty("description")]
	public string? Description { get; set; }

	[JsonProperty("comment_count")]
	public int CommentCount { get; set; }

	[JsonProperty("is_favorited")]
	public bool IsFavorited { get; set; }

	/// <summary>Current user's vote when authenticated: 1 up, -1 down, 0 none.</summary>
	[JsonProperty("vote")]
	public int Vote { get; set; }

	[JsonProperty("has_notes")]
	public bool HasNotes { get; set; }

	[JsonProperty("duration")]
	public string? Duration { get; set; }

	#region Additional

	public bool HasVotedUp => Vote > 0;
	public bool HasVotedDown => Vote < 0;

	#endregion

	public bool HasNoValidURLs() {
		if (Preview == null || Sample == null || File == null) {
			return true;
		}
		E621FileType type = this.GetFileType();
		if (type.IsGif()) {
			return Preview.URL.IsBlank() || File.URL.IsBlank();
		} else {
			return Preview.URL.IsBlank() || Sample.URL.IsBlank() || File.URL.IsBlank();
		}
	}

	public override string ToString() {
		return $"E621Post ({ID}.{File?.Ext})";
	}

	public Vector2 GetSize() {
		if (Preview != null) {
			return new Vector2(Preview.Width, Preview.Height);
		} else if (File != null) {
			return new Vector2(File.Width, File.Height);
		} else {
			return Vector2.Zero;
		}
	}

	public string GetPostLink(ModuleType type) {
		return @$"https://{E621API.GetHost(type)}/posts/{ID}";
	}
}

public class E621PostsRoot {
	[JsonProperty("posts")]
	public List<E621Post>? Posts { get; set; }

	[JsonProperty("post")]
	public E621Post? Post { get; set; }
}

public class File {
	[JsonProperty("width")]
	public int Width { get; set; }

	[JsonProperty("height")]
	public int Height { get; set; }

	[JsonProperty("ext")]
	public string? Ext { get; set; }

	[JsonProperty("size")]
	public long Size { get; set; }

	[JsonProperty("md5")]
	public string? Md5 { get; set; }

	[JsonProperty("url")]
	public string? URL { get; set; }

	[JsonIgnore]
	public string SizeInfo => $"{Width} × {Height} ({Size.FileSizeToKB()})";
}

public class Preview {
	[JsonProperty("width")]
	public int Width { get; set; }

	[JsonProperty("height")]
	public int Height { get; set; }

	[JsonProperty("url")]
	public string? URL { get; set; }

	[JsonProperty("alt")]
	public string? Alt { get; set; }
}

public class Sample {
	[JsonProperty("has")]
	public bool Has { get; set; }

	[JsonProperty("height")]
	public int Height { get; set; }

	[JsonProperty("width")]
	public int Width { get; set; }

	[JsonProperty("url")]
	public string? URL { get; set; }

	[JsonProperty("alt")]
	public string? Alt { get; set; }

	[JsonProperty("alternates")]
	public SampleAlternates? Alternates { get; set; }
}

public class SampleAlternates {
	[JsonProperty("has")]
	public bool Has { get; set; }

	[JsonProperty("original")]
	public MediaVariant? Original { get; set; }

	[JsonProperty("variants")]
	public Dictionary<string, MediaVariant>? Variants { get; set; }

	[JsonProperty("samples")]
	public Dictionary<string, MediaVariant>? Samples { get; set; }
}

public class MediaVariant {
	[JsonProperty("fps")]
	public double Fps { get; set; }

	[JsonProperty("codec")]
	public string? Codec { get; set; }

	[JsonProperty("size")]
	public long Size { get; set; }

	[JsonProperty("width")]
	public int Width { get; set; }

	[JsonProperty("height")]
	public int Height { get; set; }

	[JsonProperty("url")]
	public string? URL { get; set; }
}


public class Score {
	[JsonProperty("up")]
	public int Up { get; set; }

	[JsonProperty("down")]
	public int Down { get; set; }

	[JsonProperty("total")]
	public int Total { get; set; }
}

public class Tags : ICloneable {
	[JsonProperty("general")]
	public List<string>? General { get; set; }

	[JsonProperty("species")]
	public List<string>? Species { get; set; }

	/// <summary>e6ai: maps to API category 1 (same slot as e621 artist).</summary>
	[JsonProperty("director")]
	public List<string>? Director { get; set; }

	/// <summary>e621 contributor tags (category 2).</summary>
	[JsonProperty("contributor")]
	public List<string>? Contributor { get; set; }

	[JsonProperty("character")]
	public List<string>? Character { get; set; }

	[JsonProperty("copyright")]
	public List<string>? Copyright { get; set; }

	/// <summary>e6ai: maps to API category 3 (same slot as e621 copyright).</summary>
	[JsonProperty("franchise")]
	public List<string>? Franchise { get; set; }

	[JsonProperty("artist")]
	public List<string>? Artist { get; set; }

	[JsonProperty("invalid")]
	public List<string>? Invalid { get; set; }

	[JsonProperty("lore")]
	public List<string>? Lore { get; set; }

	[JsonProperty("meta")]
	public List<string>? Meta { get; set; }

	public HashSet<string> GetAllTags() {
		HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);
		General?.ForEach(x => result.Add(x));
		Species?.ForEach(x => result.Add(x));
		Director?.ForEach(x => result.Add(x));
		Contributor?.ForEach(x => result.Add(x));
		Character?.ForEach(x => result.Add(x));
		Copyright?.ForEach(x => result.Add(x));
		Franchise?.ForEach(x => result.Add(x));
		Artist?.ForEach(x => result.Add(x));
		Invalid?.ForEach(x => result.Add(x));
		Lore?.ForEach(x => result.Add(x));
		Meta?.ForEach(x => result.Add(x));
		return result;
	}

	public Tags CreateNewBySearch(string searchKey) {
		if (searchKey.IsBlank()) {
			return this;
		}

		Tags clone = new() {
			General = General?.Where(x => x.SearchFor(searchKey)).ToList(),
			Species = Species?.Where(x => x.SearchFor(searchKey)).ToList(),
			Director = Director?.Where(x => x.SearchFor(searchKey)).ToList(),
			Contributor = Contributor?.Where(x => x.SearchFor(searchKey)).ToList(),
			Character = Character?.Where(x => x.SearchFor(searchKey)).ToList(),
			Copyright = Copyright?.Where(x => x.SearchFor(searchKey)).ToList(),
			Franchise = Franchise?.Where(x => x.SearchFor(searchKey)).ToList(),
			Artist = Artist?.Where(x => x.SearchFor(searchKey)).ToList(),
			Invalid = Invalid?.Where(x => x.SearchFor(searchKey)).ToList(),
			Lore = Lore?.Where(x => x.SearchFor(searchKey)).ToList(),
			Meta = Meta?.Where(x => x.SearchFor(searchKey)).ToList(),
		};

		return clone;
	}

	public object Clone() {
		return MemberwiseClone();
	}

}

public class Flags {
	public bool? pending;
	public bool? flagged;
	public bool? note_locked;
	public bool? status_locked;
	public bool? rating_locked;
	public bool? deleted;
}

public class Relationships {
	[JsonProperty("parent_id")]
	public int? ParentId { get; set; }

	[JsonProperty("has_children")]
	public bool HasChildren { get; set; }

	[JsonProperty("has_active_children")]
	public bool HasActiveChildren { get; set; }

	[JsonProperty("children")]
	public List<int?>? Children { get; set; }
}

public enum E621Rating {
	[EnumMember(Value = "s")]
	Safe,
	[EnumMember(Value = "q")]
	Questionable,
	[EnumMember(Value = "e")]
	Explicit,
}
