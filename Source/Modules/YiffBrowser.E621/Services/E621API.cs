using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RW.Base.WPF.Extensions;
using RW.Common.Helpers;
using System.Diagnostics;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Services;

public class E621API(ModuleType moduleType) {

	public static int DefaultPageLimit { get; } = 75;

	private static E621API API_E621 { get; } = new(ModuleType.E621);
	private static E621API API_E926 { get; } = new(ModuleType.E926);
	private static E621API API_E6AI { get; } = new(ModuleType.E6AI);

	public static E621API GetAPI(ModuleType moduleType) {
		return moduleType switch {
			ModuleType.E621 => API_E621,
			ModuleType.E6AI => API_E6AI,
			ModuleType.E926 => API_E926,
			_ => throw new NotImplementedException(),
		};
	}

	public static string GetHost(ModuleType moduleType) {
		return moduleType switch {
			ModuleType.E621 => "e621.net",
			ModuleType.E6AI => "e6ai.net",
			ModuleType.E926 => "e926.net",
			_ => throw new NotSupportedException(),
		};
	}

	private readonly IE621ProfileService profileService = IoC.GetService<IE621ProfileService>();

	public ModuleType ModuleType { get; } = moduleType;

	public string GetHost() {
		return GetHost(ModuleType);
	}

	#region API

	public async Task<HttpResult<string>> ReadURLAsync(string url, string username, string api, CancellationToken? token = null) {
		return await NetCode.ReadURLAsync(url, username, api, token);
	}

	public async Task<HttpResult<string>> ReadURLAsync(string url, CancellationToken? token = null) {
		(string? username, string? apiKey) = profileService.GetUser(ModuleType);
		return await NetCode.ReadURLAsync(url, username, apiKey, token);
	}

	public async Task<HttpResult<string>> PutRequestAsync(string url, KeyValuePair<string, string> pair, CancellationToken? token = null) {
		(string? username, string? apiKey) = profileService.GetUser(ModuleType);
		return await NetCode.PutRequestAsync(url, pair, username, apiKey, token);
	}

	public async Task<HttpResult<string>> PostRequestAsync(string url, List<KeyValuePair<string, string>> pairs, CancellationToken? token = null) {
		(string? username, string? apiKey) = profileService.GetUser(ModuleType);
		return await NetCode.PostRequestAsync(url, pairs, username, apiKey, token);
	}

	public async Task<HttpResult<string>> DeleteRequestAsync(string url, CancellationToken? token = null) {
		(string? username, string? apiKey) = profileService.GetUser(ModuleType);
		return await NetCode.DeleteRequestAsync(url, username, apiKey, token);
	}


	#endregion


	#region Posts
	public async ValueTask<E621Post[]> GetPostsByTagsAsync(E621PostParameters parameters, CancellationToken? token = null) {
		if (parameters.Page <= 0) {
			parameters.Page = 1;
		}
		string url = $"https://{GetHost()}/posts.json?page={parameters.Page}{($"&limit={parameters.PageLimit}")}";

		IEnumerable<string> tags = parameters.Tags.Where(x => x.IsNotBlank());
		if (tags.IsNotEmpty()) {
			url += "&tags=";
			url += string.Join("+", tags);
		}

		HttpResult<string> result = await ReadURLAsync(url, token: token);

		if (result.Result == HttpResultType.Success) {
			return JsonDeserialize<E621PostsRoot>(result.Content)?.Posts?.ToArray() ?? [];
		} else {
			return [];
		}
	}

	public async ValueTask<E621Post?> GetPostAsync(int? postID, CancellationToken? token = null) {
		if (postID == null) {
			return null;
		}
		string url = $"https://{GetHost()}/posts/{postID.Value}.json";
		HttpResult<string> result = await ReadURLAsync(url, token: token);
		if (result.Result == HttpResultType.Success) {
			E621Post? post = JsonDeserialize<E621PostsRoot?>(result.Content)?.Post;
			return post;
		} else {
			return null;
		}
	}

	/// <summary>
	/// Fetches posts by id list (chunked via id:a,b,c) and returns them in the requested order.
	/// Missing ids are skipped.
	/// </summary>
	public async ValueTask<E621Post[]> GetPostsByIdsAsync(IEnumerable<int> ids, CancellationToken? token = null) {
		int[] ordered = ids.Where(id => id > 0).Distinct().ToArray();
		if (ordered.Length == 0) {
			return [];
		}

		const int chunkSize = 75;
		Dictionary<int, E621Post> map = [];

		for (int i = 0; i < ordered.Length; i += chunkSize) {
			token?.ThrowIfCancellationRequested();
			int[] chunk = ordered.Skip(i).Take(chunkSize).ToArray();
			string idTag = "id:" + string.Join(",", chunk);
			E621Post[] posts = await GetPostsByTagsAsync(new E621PostParameters {
				Page = 1,
				PageLimit = chunk.Length,
				Tags = [idTag],
			}, token);

			foreach (E621Post post in posts) {
				map[post.ID] = post;
			}
		}

		return ordered.Where(map.ContainsKey).Select(id => map[id]).ToArray();
	}

	/// <summary>
	/// Walks parent chain to root, then BFS children to build the full relationship tree.
	/// </summary>
	public async ValueTask<RelationTreeBuildResult> BuildRelationTreeAsync(int seedPostId, CancellationToken? token = null) {
		Dictionary<int, E621Post> cache = [];

		async Task<E621Post?> EnsurePost(int id) {
			if (cache.TryGetValue(id, out E621Post? cached)) {
				return cached;
			}
			E621Post? post = await GetPostAsync(id, token);
			if (post != null) {
				cache[post.ID] = post;
			}
			return post;
		}

		E621Post? seed = await EnsurePost(seedPostId);
		if (seed == null) {
			return new RelationTreeBuildResult(null, [], []);
		}

		E621Post rootPost = seed;
		while (rootPost.Relationships?.ParentId is int parentId and > 0) {
			token?.ThrowIfCancellationRequested();
			E621Post? parent = await EnsurePost(parentId);
			if (parent == null) {
				break;
			}
			rootPost = parent;
		}

		Queue<int> queue = new();
		HashSet<int> visited = [];
		queue.Enqueue(rootPost.ID);
		visited.Add(rootPost.ID);

		while (queue.Count > 0) {
			token?.ThrowIfCancellationRequested();
			int currentId = queue.Dequeue();
			E621Post? current = await EnsurePost(currentId);
			if (current?.Relationships?.Children is not { Count: > 0 } children) {
				continue;
			}

			List<int> missing = [];
			foreach (int? childId in children) {
				if (childId is int cid and > 0 && visited.Add(cid)) {
					if (!cache.ContainsKey(cid)) {
						missing.Add(cid);
					}
					queue.Enqueue(cid);
				}
			}

			if (missing.Count > 0) {
				E621Post[] fetched = await GetPostsByIdsAsync(missing, token);
				foreach (E621Post post in fetched) {
					cache[post.ID] = post;
				}
			}
		}

		RelationTreeNode BuildNode(E621Post post, int depth) {
			RelationTreeNode node = new() {
				Post = post,
				Depth = depth,
				IsSeed = post.ID == seedPostId,
			};
			IEnumerable<int> childIds = (post.Relationships?.Children ?? [])
				.Where(id => id is int and > 0)
				.Select(id => id!.Value);
			foreach (int childId in childIds) {
				if (cache.TryGetValue(childId, out E621Post? childPost)) {
					node.Children.Add(BuildNode(childPost, depth + 1));
				}
			}
			return node;
		}

		RelationTreeNode root = BuildNode(rootPost, 0);
		List<E621Post> flat = [.. cache.Values.OrderBy(p => p.ID)];
		return new RelationTreeBuildResult(root, flat, cache.Values.ToList());
	}

	#endregion

	#region Tags

	public async ValueTask<E621AutoComplete[]> GetE621AutoCompleteAsync(string tag, CancellationToken? token = null) {
		HttpResult<string> result = await ReadURLAsync($"https://{GetHost()}/tags/autocomplete.json?search[name_matches]={tag}", token: token);
		if (result.Result == HttpResultType.Success) {
			return JsonDeserialize<E621AutoComplete[]>(result.Content) ?? [];
		} else {
			return [];
		}
	}

	public async ValueTask<E621Tag?> GetE621TagAsync(string tag, CancellationToken? token = null) {
		if (tag.IsBlank()) {
			return null;
		}
		tag = tag.ToLower().Trim();

		if (E621Tag.Pool.TryGetValue(tag, out E621Tag? e621Tag)) {
			return e621Tag;
		}

		string url = $"https://{GetHost()}/tags.json?search[name_matches]={tag}";
		HttpResult<string> result = await ReadURLAsync(url, token: token);
		if (result.Result == HttpResultType.Success && result.Content != "{\"tags\":[]}") {
			E621Tag? t = JsonDeserialize<E621Tag[]>(result.Content)?.FirstOrDefault();
			if (t != null) {
				E621Tag.Pool.TryAdd(tag, t);
			}
			return t;
		} else {
			return null;
		}
	}

	public async ValueTask<E621Wiki?> GetE621WikiAsync(string tag, CancellationToken? token = null) {
		tag = tag.ToLower().Trim();

		if (E621Wiki.wikiDictionary.TryGetValue(tag, out string? value)) {
			return new E621Wiki() {
				Body = value
			};
		} else if (tag.StartsWith("fav:")) {
			return new E621Wiki() {
				Body = $"Favorites of \"{tag[4..]}\"",
			};
		}

		if (E621Wiki.Pool.TryGetValue(tag, out E621Wiki? e621Wiki)) {
			return e621Wiki;
		}

		string url = $"https://{GetHost()}/wiki_pages.json?search[title]={tag}";
		HttpResult<string> result = await ReadURLAsync(url, token: token);
		if (result.Result == HttpResultType.Success) {
			if (result.Content == "[]") {
				return new E621Wiki();
			}
			return JsonDeserialize<E621Wiki[]>(result.Content)?.FirstOrDefault();
		} else {
			return null;
		}
	}

	public async ValueTask<bool> UploadBlacklistTags(string username, string[] tags) {
		HttpResult<string> result = await PutRequestAsync(
			$"https://{GetHost()}/users/{username}.json",
			new KeyValuePair<string, string>("user[blacklisted_tags]", string.Join("\n", tags))
		);
		return result.Result == HttpResultType.Success;
	}

	#endregion

	#region Comments
	public async ValueTask<E621Comment[]> GetCommentsAsync(int postID, CancellationToken? token = null) {
		string url = $"https://{GetHost()}/comments.json?group_by=comment&search[post_id]={postID}";
		HttpResult<string> result = await ReadURLAsync(url, token: token);
		if (result.Content == "{\"comments\":[]}") {
			return [];
		}
		if (result.Result == HttpResultType.Success) {
			return JsonDeserialize<E621Comment[]>(result.Content) ?? [];
		} else {
			return [];
		}
	}

	#endregion


	#region Pool

	public async ValueTask<E621Pool?> GetPoolAsync(string id, CancellationToken? token = null) {
		HttpResult<string> result = await ReadURLAsync($"https://{GetHost()}/pools/{id}.json", token: token);
		if (result.Result == HttpResultType.Success) {
			return JsonDeserialize<E621Pool>(result.Content);
		} else {
			return null;
		}
	}

	#endregion

	#region Users

	public async ValueTask<E621User?> GetUserAsync(string username, string apiKey, CancellationToken? token = null) {
		string url = $"https://{GetHost()}/users.json?search[name_matches]={username}";
		HttpResult<string> result = await ReadURLAsync(url, username, apiKey, token);
		if (result.Result == HttpResultType.Success) {
			return JsonDeserialize<E621User[]>(result.Content)?.FirstOrDefault();
		} else {
			return null;
		}
	}

	public async ValueTask<E621User?> GetUserAsync(int id, CancellationToken? token = null) {
		string url = $"https://{GetHost()}/users.json?search[id]={id}";
		HttpResult<string> result = await ReadURLAsync(url, token: token);
		if (result.Result == HttpResultType.Success) {
			return JsonDeserialize<E621User[]>(result.Content)?.FirstOrDefault();
		} else {
			return null;
		}
	}

	public async ValueTask<HttpResult<string>> PostAddFavoriteAsync(int postID, CancellationToken? token = null) {
		string url = $"https://{GetHost()}/favorites.json";
		return await PostRequestAsync(url, [
			new KeyValuePair<string, string>("post_id", postID.ToString())
		], token: token);
	}

	public async ValueTask<HttpResult<string>> PostDeleteFavoriteAsync(int postID, CancellationToken? token = null) {
		string url = $"https://{GetHost()}/favorites/{postID}.json";
		return await DeleteRequestAsync(url, token: token);
	}

	public async ValueTask<DataResult<E621Vote>> VotePost(int postID, int score, bool no_unvote, CancellationToken? token = null) {
		HttpResult<string> result = await PostRequestAsync($"https://{GetHost()}/posts/{postID}/votes.json", [
			new KeyValuePair<string, string>("score", $"{score}"),
			new KeyValuePair<string, string>("no_unvote", $"{no_unvote}"),
		], token: token);
		return new DataResult<E621Vote>(result.Result, JsonDeserialize<E621Vote>(result.Content));
	}

	// no up and down
	public async ValueTask<DataResult<E621Vote>> VoteComment(int commentID, int score, bool no_unvote, CancellationToken? token = null) {
		HttpResult<string> result = await PostRequestAsync($"https://{GetHost()}/comments/{commentID}/votes.json", [
			new KeyValuePair<string, string>("score", $"{score}"),
			new KeyValuePair<string, string>("no_unvote", $"{no_unvote}"),
		], token: token);
		return new DataResult<E621Vote>(result.Result, JsonDeserialize<E621Vote>(result.Content));
	}

	#endregion

	#region Paginator


	public async ValueTask<E621Paginator?> GetPaginatorAsync(string[] tags, int pageLimit, int page = 1, CancellationToken? token = null) {
		string tag = string.Join("+", tags).Trim().ToLower();

		string url = $"https://{GetHost()}/posts?tags={tag}&page={page}&limit={pageLimit}";
		HttpResult<string> result = await ReadURLAsync(url, token: token);
		if (result.Result != HttpResultType.Success) {
			return null;
		}

		if (result.Content.IsBlank()) {
			return new E621Paginator();
		}

		try {
			string html = result.Content;

			HtmlDocument doc = new();
			doc.LoadHtml(html);

			HtmlNode? GetNodeByClasses(HtmlDocument doc, string[] targetClasses) {
				return doc.DocumentNode.Descendants().FirstOrDefault(n => {
					string[] classes = n.GetAttributeValue("class", "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
					return targetClasses.All(tc => classes.Contains(tc));
				});
			}

			HtmlNode? node = GetNodeByClasses(doc, ["page", "last"])
				?? GetNodeByClasses(doc, ["page", "current"]);

			int maxPage;
			if (node != null && NumberHelper.ConvertInt(node.InnerText, out int _maxPage)) {
				maxPage = _maxPage;
			} else {
				maxPage = 0;
			}

			return new E621Paginator() {
				MaxPage = maxPage,
			};
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			//return 0 length paginator
			return new E621Paginator() {
				MaxPage = 750,
			};
		}
	}

	#endregion


	private T? JsonDeserialize<T>(string? json) {
		if (json is null) {
			return default;
		}
		T? t = JsonConvert.DeserializeObject<T?>(json, new JsonSerializerSettings() {
			NullValueHandling = NullValueHandling.Ignore,
			Error = JsonDeserializeErrorHandler,
		});
		return t;
	}

	private void JsonDeserializeErrorHandler(object? sender, ErrorEventArgs e) {

	}
}

public class E621PostParameters {
	//public event Action<string[]> OnPreviewsUpdated;

	public int Page { get; set; } = 1;
	public string[] Tags { get; set; } = [""];
	public int PageLimit { get; set; } = E621API.DefaultPageLimit;


	//public bool InputPosts { get; set; }
	//public E621Post[]? Posts { get; set; }

	//public E621Pool? Pool { get; set; }

}
