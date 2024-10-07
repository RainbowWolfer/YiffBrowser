using BaseFramework.Enums;
using BaseFramework.Helpers;
using BaseFramework.Models.Apps;
using BaseFramework.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Text;
using YB.E621.Models.E621;

namespace YB.E621.Services {
	public class E621API(ModuleType moduleType) {

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

		public ModuleType ModuleType { get; } = moduleType;

		public string GetHost() {
			return GetHost(ModuleType);
		}

		public int GetPostsPerPageCount() =>/* Local.Settings?.E621PageLimitCount ??*/ 75;

		#region API

		private (string username, string apiKey) GetUser() {
			return ModuleType switch {
				ModuleType.E621 => (AppProfile.Instance.E621_Username ?? string.Empty, AppProfile.Instance.E621_ApiKey ?? string.Empty),
				ModuleType.E6AI => (AppProfile.Instance.E6AI_Username ?? string.Empty, AppProfile.Instance.E6AI_ApiKey ?? string.Empty),
				ModuleType.E926 => (AppProfile.Instance.E926_Username ?? string.Empty, AppProfile.Instance.E926_ApiKey ?? string.Empty),
				_ => throw new NotImplementedException(),
			};
		}

		public async Task<HttpResult<string>> ReadURLAsync(string url, string username, string api, CancellationToken? token = null) {
			return await NetCode.ReadURLAsync(url, username, api, token);
		}

		public async Task<HttpResult<string>> ReadURLAsync(string url, CancellationToken? token = null) {
			(string username, string apiKey) = GetUser();
			return await NetCode.ReadURLAsync(url, username, apiKey, token);
		}

		public async Task<HttpResult<string>> PutRequestAsync(string url, KeyValuePair<string, string> pair, CancellationToken? token = null) {
			(string username, string apiKey) = GetUser();
			return await NetCode.PutRequestAsync(url, pair, username, apiKey, token);
		}

		public async Task<HttpResult<string>> PostRequestAsync(string url, List<KeyValuePair<string, string>> pairs, CancellationToken? token = null) {
			(string username, string apiKey) = GetUser();
			return await NetCode.PostRequestAsync(url, pairs, username, apiKey, token);
		}

		public async Task<HttpResult<string>> DeleteRequestAsync(string url, CancellationToken? token = null) {
			(string username, string apiKey) = GetUser();
			return await NetCode.DeleteRequestAsync(url, username, apiKey, token);
		}


		#endregion


		#region Posts
		public async ValueTask<E621Post[]> GetPostsByTagsAsync(E621PostParameters parameters, CancellationToken? token = null) {
			if (parameters.Page <= 0) {
				parameters.Page = 1;
			}
			string url = $"https://{GetHost()}/posts.json?page={parameters.Page}{(parameters.UsePageLimit ? $"&limit={GetPostsPerPageCount()}" : "")}";

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


		public async ValueTask<DataResult<E621Paginator>> GetPaginatorAsync(string[] tags, int page = 1, CancellationToken? token = null) {
			string tag = string.Join("+", tags).Trim().ToLower();

			string url = $"https://{GetHost()}/posts?tags={tag}&page={page}";
			HttpResult<string> result = await ReadURLAsync(url, token: token);
			if (result.Result != HttpResultType.Success) {
				return new DataResult<E621Paginator>(result.Result, null);
			}

			if (result.Content.IsBlank()) {
				return new DataResult<E621Paginator>(result.Result, null);
			}

			try {
				string data = result.Content;
				int startIndex = data.IndexOf("paginator");

				int currentPageIndex = data.IndexOf("current-page", startIndex);

				StringBuilder currentPageString = new();
				for (int j = currentPageIndex + 1; j < data.IndexOf("</li>", currentPageIndex); j++) {
					if (char.IsDigit(data[j])) {
						currentPageString.Append(data[j]);
						for (int k = 1; k <= 4; k++) {
							if (char.IsDigit(data[j + k])) {
								currentPageString.Append(data[j + k]);
							} else {
								break;
							}
						}
						break;
					}
				}

				List<string> pagesString = [];
				int i = data.IndexOf("numbered-page", startIndex);
				while (i != -1) {
					StringBuilder pageString = new();
					for (int j = i + 1; j < data.IndexOf("</li>", i); j++) {
						if (char.IsDigit(data[j])) {
							pageString.Append(data[j]);
							for (int k = 1; k <= 4; k++) {
								if (char.IsDigit(data[j + k])) {
									pageString.Append(data[j + k]);
								} else {
									break;
								}
							}
							pagesString.Add(pageString.ToString());
							break;
						}
					}
					i = data.IndexOf("numbered-page", i + 10);
				}

				int currentPage = int.Parse(currentPageString.ToString());
				List<int> pages = [currentPage];
				foreach (string s in pagesString) {
					pages.Add(int.Parse(s));
				}
				return new DataResult<E621Paginator>(HttpResultType.Success, new E621Paginator() {
					CurrentPage = currentPage,
					Pages = [.. pages],
				});
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				//return 0 length paginator
				return new DataResult<E621Paginator>(HttpResultType.Success, new E621Paginator() {
					CurrentPage = 1,
					Pages = [0],
				});
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
		public bool UsePageLimit { get; set; } = true;


		//public bool InputPosts { get; set; }
		//public E621Post[]? Posts { get; set; }

		//public E621Pool? Pool { get; set; }

	}
}
