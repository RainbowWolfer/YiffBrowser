using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Common.Helpers;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using YiffBrowser.BaseFramework.Helpers;

namespace YiffBrowser.BaseFramework.Services;

// todo: 改成不同的网站有自己的netcode实例
public static class NetCode {
	/// <summary>
	/// e621 hard limit is 2 req/s; docs recommend staying at ~1 req/s sustained.
	/// </summary>
	private static readonly TimeSpan MinRequestInterval = TimeSpan.FromMilliseconds(1000);
	private static readonly TimeSpan RateLimitBackoffBase = TimeSpan.FromSeconds(2);
	private const int MaxAttempts = 3;

	private static readonly SemaphoreSlim throttleLock = new(1, 1);
	private static DateTime lastRequestUtc = DateTime.MinValue;

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
			UseCookies = false,
			KeepAlivePingDelay = TimeSpan.FromSeconds(30),
			KeepAlivePingTimeout = TimeSpan.FromSeconds(5),
			KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
			PreAuthenticate = true,
		};
		ProxySettingsHelper.Configure(handler, model);

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
		=> SendRequestAsync(HttpMethod.Put, url, () => new FormUrlEncodedContent([pair]), username, api, token);

	public static Task<HttpResult<string>> PostRequestAsync(string url, List<KeyValuePair<string, string>> pairs, string? username, string? api, CancellationToken? token = null)
		=> SendRequestAsync(HttpMethod.Post, url, () => new FormUrlEncodedContent(pairs), username, api, token);

	public static Task<HttpResult<string>> DeleteRequestAsync(string url, string? username, string? api, CancellationToken? token = null)
		=> SendRequestAsync(HttpMethod.Delete, url, null, username, api, token);

	private static Task<HttpResult<string>> SendRequestAsync(
		HttpMethod method,
		string url,
		Func<HttpContent?>? contentFactory,
		string? username,
		string? api,
		CancellationToken? token = null
	) {
		return SendRequestAsync(method, url, contentFactory, username, api, token ?? CancellationToken.None);
	}

	private static async Task<HttpResult<string>> SendRequestAsync(
		HttpMethod method,
		string url,
		Func<HttpContent?>? contentFactory,
		string? username,
		string? api,
		CancellationToken token
	) {
		Debug.WriteLine($"{method}: {url}");

		DateTime startTime = DateTime.Now;
		Stopwatch sw = Stopwatch.StartNew();

		HttpResultType resultType = HttpResultType.Error;
		HttpStatusCode code = HttpStatusCode.BadRequest;
		string? responseContent = null;
		string helper = "";

		try {
			for (int attempt = 1; attempt <= MaxAttempts; attempt++) {
				token.ThrowIfCancellationRequested();
				await ThrottleAsync(token).ConfigureAwait(false);

				HttpContent? content = contentFactory?.Invoke();
				using HttpRequestMessage request = new(method, url) { Content = content };
				ApplyRequestHeaders(request, username, api);

				HttpResponseMessage? response = null;
				try {
					HttpClientWrapper client = GetClient();
					using (client.Use(method, url)) {
						response = await client.HttpClient.SendAsync(request, token).ConfigureAwait(false);
						responseContent = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
						code = response.StatusCode;

						if (response.IsSuccessStatusCode) {
							resultType = HttpResultType.Success;
							helper = "";
							break;
						}

						bool rateLimited = code is HttpStatusCode.TooManyRequests
							or HttpStatusCode.ServiceUnavailable;
						helper = $"Status Code: {code}";

						if (rateLimited && attempt < MaxAttempts) {
							TimeSpan backoff = GetRetryDelay(response, attempt);
							Debug.WriteLine($"Rate limited ({(int)code}), retry {attempt}/{MaxAttempts} after {backoff.TotalSeconds:0.#}s");
							await Task.Delay(backoff, token).ConfigureAwait(false);
							continue;
						}

						resultType = HttpResultType.Error;
						break;
					}
				} finally {
					response?.Dispose();
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
		}

		return new HttpResult<string>(
			Result: resultType,
			StatusCode: code,
			Content: responseContent,
			Time: sw.ElapsedMilliseconds,
			StartTime: startTime,
			Helper: helper
		);
	}

	private static async Task ThrottleAsync(CancellationToken token) {
		await throttleLock.WaitAsync(token).ConfigureAwait(false);
		try {
			TimeSpan wait = MinRequestInterval - (DateTime.UtcNow - lastRequestUtc);
			if (wait > TimeSpan.Zero) {
				await Task.Delay(wait, token).ConfigureAwait(false);
			}
			lastRequestUtc = DateTime.UtcNow;
		} finally {
			throttleLock.Release();
		}
	}

	private static TimeSpan GetRetryDelay(HttpResponseMessage response, int attempt) {
		if (response.Headers.RetryAfter?.Delta is TimeSpan retryAfter && retryAfter > TimeSpan.Zero) {
			return retryAfter;
		}
		// 2s, 4s, ...
		return TimeSpan.FromTicks(RateLimitBackoffBase.Ticks * (1L << (attempt - 1)));
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
