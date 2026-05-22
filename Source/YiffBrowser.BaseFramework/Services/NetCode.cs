using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Common.Helpers;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace YiffBrowser.BaseFramework.Services;

// todo: 改成不同的网站有自己的netcode实例
public static class NetCode {
	private static HttpClientWrapper currentClientWrapper;
	private static readonly Lock @lock = new();

	static NetCode() {
		currentClientWrapper = new HttpClientWrapper(CreateClient());
	}

	public static void CreateNewClient() {
		lock (@lock) {
			currentClientWrapper.MarkAsDeprecated();
			currentClientWrapper = new HttpClientWrapper(CreateClient());
		}
	}

	public static HttpClientWrapper GetClient() {
		lock (@lock) {
			return currentClientWrapper;
		}
	}

	private static HttpClient CreateClient() {
		AppSettingsModel model = AppSettingsService.Instance.Model;

		SocketsHttpHandler handler = new() {
			PooledConnectionLifetime = TimeSpan.FromMinutes(2),
			PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
			MaxConnectionsPerServer = 4, // 最高并发
			AutomaticDecompression = DecompressionMethods.All,
			ConnectTimeout = TimeSpan.FromSeconds(15),
			EnableMultipleHttp2Connections = true,
			UseProxy = true,
			Proxy = HttpClient.DefaultProxy,
			UseCookies = false,
			KeepAlivePingDelay = TimeSpan.FromSeconds(30),
			KeepAlivePingTimeout = TimeSpan.FromSeconds(5),
			KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
			PreAuthenticate = true,
		};

		return new HttpClient(handler) {
			Timeout = TimeSpan.FromSeconds(60),
		};
	}

	public static string UserAgent {
		get {
			IAppManager appManager = IoC.GetService<IAppManager>();
			return $"{AppConfig.AppName}/{appManager.AppVersion.ShortVersion} (by {appManager.Author})";
		}
	}

	public static Task<HttpResult<string>> ReadURLAsync(string url, string? username, string? api, CancellationToken? token = null)
		=> SendRequestAsync(HttpMethod.Get, url, null, username, api, token);

	public static Task<HttpResult<string>> PutRequestAsync(string url, KeyValuePair<string, string> pair, string? username, string? api, CancellationToken? token = null)
		=> SendRequestAsync(HttpMethod.Put, url, new FormUrlEncodedContent([pair]), username, api, token);

	public static Task<HttpResult<string>> PostRequestAsync(string url, List<KeyValuePair<string, string>> pairs, string? username, string? api, CancellationToken? token = null)
		=> SendRequestAsync(HttpMethod.Post, url, new FormUrlEncodedContent(pairs), username, api, token);

	public static Task<HttpResult<string>> DeleteRequestAsync(string url, string? username, string? api, CancellationToken? token = null)
		=> SendRequestAsync(HttpMethod.Delete, url, null, username, api, token);

	private static async Task<HttpResult<string>> SendRequestAsync(
		HttpMethod method,
		string url,
		HttpContent? content,
		string? username,
		string? api,
		CancellationToken? token = null
	) {

		Debug.WriteLine($"{method}: {url}");

		DateTime startTime = DateTime.Now;
		Stopwatch sw = Stopwatch.StartNew();

		using HttpRequestMessage request = new(method, url) { Content = content };

		ApplyRequestHeaders(request, username, api);

		HttpResponseMessage? response = null;
		HttpResultType resultType;
		string? responseContent = null;
		string helper = "";

		try {
			HttpClientWrapper client = GetClient();
			using (client.Use(method, url)) {
				response = await client.HttpClient.SendAsync(request, token ?? CancellationToken.None);

				responseContent = await response.Content.ReadAsStringAsync();

				if (response.IsSuccessStatusCode) {
					resultType = HttpResultType.Success;
				} else {
					resultType = HttpResultType.Error;
					helper = $"Status Code: {response.StatusCode}";
				}
			}
		} catch (OperationCanceledException) {
			resultType = HttpResultType.Canceled;
		} catch (HttpRequestException e) {
			resultType = HttpResultType.Error;
			responseContent = e.Message;
			helper = e.Message;
		} finally {
			sw.Stop();
			response?.Dispose();
		}

		HttpStatusCode code = response?.StatusCode
			?? (resultType == HttpResultType.Success ? HttpStatusCode.OK : HttpStatusCode.BadRequest);

		return new HttpResult<string>(
			Result: resultType,
			StatusCode: code,
			Content: responseContent,
			Time: sw.ElapsedMilliseconds,
			StartTime: startTime,
			Helper: helper
		);
	}

	private static void ApplyRequestHeaders(HttpRequestMessage request, string? username, string? api) {
		request.Headers.UserAgent.ParseAdd(UserAgent);

		if (username.IsNotBlank() && api.IsNotBlank()) {
			string authRaw = $"{username}:{api}";
			string encoded = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(authRaw));
			request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encoded);
		}
	}

}


public class HttpClientWrapper(HttpClient httpClient) {
	public HttpClient HttpClient { get; } = httpClient;
	private int referenceCount = 0;
	private bool isDeprecated = false;

	public void MarkAsDeprecated() {
		isDeprecated = true;
		CheckAndDispose();
	}

	public IDisposable Use(HttpMethod method, string url) {
		Interlocked.Increment(ref referenceCount);
		return new ReleaseHelper(this);
	}

	private void CheckAndDispose() {
		if (isDeprecated && Volatile.Read(ref referenceCount) <= 0) {
			HttpClient.Dispose();
			Debug.WriteLine("HttpClient disposed after all tasks finished.");
		}
	}

	private class ReleaseHelper(HttpClientWrapper wrapper) : IDisposable {
		public void Dispose() {
			Interlocked.Decrement(ref wrapper.referenceCount);
			wrapper.CheckAndDispose();
		}
	}
}

public record class HttpResult<T>(
	HttpResultType Result,
	HttpStatusCode StatusCode,
	T? Content,
	long Time,
	DateTime StartTime,
	string? Helper = null
);

public record class DataResult<T>(HttpResultType ResultType, T? Data);

public class HttpResultTypeNotFoundException : Exception {
	public HttpResultTypeNotFoundException() : base("") { }
}

public enum HttpResultType {
	Success,
	Error,
	Canceled,
}
