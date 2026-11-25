using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YiffBrowser.E621.Models.E621;

/// <summary>
/// Reads both the legacy posts payload and the format made default in e621ng 26.11.18.
/// Legacy: <c>{"posts":[...]}</c> / <c>{"post":{...}}</c> with <c>file</c>, <c>preview</c>, <c>sample</c>.
/// Current: a bare array or post, with media under <c>files</c> and counts under <c>stats</c>.
/// </summary>
public static class E621PostJson {

	private static readonly JsonSerializer Serializer = JsonSerializer.Create(new JsonSerializerSettings {
		NullValueHandling = NullValueHandling.Ignore,
		Error = (_, args) => args.ErrorContext.Handled = true,
	});

	public static E621Post[] ReadMany(string? json) {
		if (string.IsNullOrWhiteSpace(json)) {
			return [];
		}

		JToken token;
		try {
			token = JToken.Parse(json);
		} catch (JsonException) {
			return [];
		}

		if (token is JArray array) {
			return ReadAll(array);
		}

		if (token is not JObject obj) {
			return [];
		}

		if (obj["posts"] is JArray posts) {
			return ReadAll(posts);
		}

		E621Post? single = ReadToken(obj["post"] ?? (obj["id"] != null ? obj : null));
		return single == null ? [] : [single];
	}

	public static E621Post? ReadOne(string? json) {
		E621Post[] posts = ReadMany(json);
		return posts.Length == 0 ? null : posts[0];
	}

	private static E621Post[] ReadAll(JArray array) {
		List<E621Post> posts = new(array.Count);
		foreach (JToken item in array) {
			E621Post? post = ReadToken(item);
			if (post != null) {
				posts.Add(post);
			}
		}
		return posts.ToArray();
	}

	private static E621Post? ReadToken(JToken? token) {
		if (token is not JObject obj || obj["id"] == null) {
			return null;
		}

		if (obj["files"] is JObject) {
			obj = ToLegacyShape(obj);
		}

		return obj.ToObject<E621Post>(Serializer);
	}

	private static JObject ToLegacyShape(JObject current) {
		JObject files = (JObject)current["files"]!;
		JObject? meta = files["meta"] as JObject;
		JObject? original = files["original"] as JObject;
		JObject? preview = files["preview"] as JObject;
		JObject? sample = files["sample"] as JObject;
		JObject? stats = current["stats"] as JObject;
		JObject? has = current["has"] as JObject;
		JObject relationships = current["relationships"] as JObject ?? [];

		JToken? previewUrl = preview?["jpg"] ?? preview?["webp"];
		JToken? sampleUrl = sample?["jpg"] ?? sample?["webp"];

		JObject legacy = new() {
			["id"] = current["id"],
			["created_at"] = current["created_at"],
			["updated_at"] = current["updated_at"],
			["change_seq"] = current["change_seq"],
			["uploader_id"] = current["uploader_id"],
			["uploader_name"] = current["uploader_name"],
			["approver_id"] = current["approver_id"],
			["description"] = current["description"],
			["rating"] = current["rating"],
			["locked_tags"] = current["locked_tags"],
			["sources"] = current["sources"],
			["pools"] = current["pools"],
			["flags"] = current["flags"],
			["file"] = new JObject {
				["width"] = original?["width"],
				["height"] = original?["height"],
				["ext"] = meta?["ext"],
				["size"] = meta?["size"],
				["md5"] = meta?["md5"],
				["url"] = original?["url"],
			},
			["preview"] = new JObject {
				["width"] = preview?["width"],
				["height"] = preview?["height"],
				["url"] = previewUrl,
				["alt"] = preview?["webp"],
			},
			["sample"] = new JObject {
				["has"] = meta?["has_sample"] ?? has?["sample"] ?? false,
				["width"] = sample?["width"],
				["height"] = sample?["height"],
				["url"] = sampleUrl,
				["alt"] = sample?["webp"],
				["alternates"] = files["video"],
			},
			["score"] = stats?["score"],
			["fav_count"] = stats?["fav_count"],
			["is_favorited"] = stats?["is_favorited"],
			["vote"] = stats?["vote"],
			["comment_count"] = stats?["comment_count"],
			["has_notes"] = has?["notes"],
			["duration"] = meta?["duration"],
			["relationships"] = new JObject {
				["parent_id"] = relationships["parent_id"],
				["children"] = relationships["children"],
				["has_children"] = has?["children"] ?? false,
				["has_active_children"] = has?["active_children"] ?? false,
			},
		};

		if (current["tags"] is JArray flat) {
			legacy["tags"] = new JObject {
				["general"] = flat,
			};
		} else {
			legacy["tags"] = current["tags"];
		}

		return legacy;
	}
}
