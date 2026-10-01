using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using YiffBrowser.BaseFramework.Helpers;

namespace YiffBrowser.BaseFramework.Services;

/// <summary>
/// Shared download path for post media (previews, samples, files, gifs).
/// <para>
/// WPF's own <c>BitmapImage(uri)</c> downloader goes through the legacy WebRequest stack, which
/// we cannot configure: no user agent, no cancellation, no proxy settings of ours and a
/// connection limit we do not control. Everything media related is fetched here instead so the
/// concurrency stays bounded and progress is real.
/// </para>
/// </summary>
public static class MediaDownloadService {

	/// <summary>e621 serves media from a CDN, so this is about being a good client, not a limit.</summary>
	private const int MaxConcurrentDownloads = 6;

	private static readonly SemaphoreSlim gate = new(MaxConcurrentDownloads, MaxConcurrentDownloads);

	private static readonly Lock @lock = new();
	private static HttpClient? client;

	/// <summary>Call after proxy settings changed so the next download picks them up.</summary>
	public static void ResetClient() {
		lock (@lock) {
			// Requests in flight keep their own reference to the handler.
			client = null;
		}
	}

	private static HttpClient GetClient() {
		lock (@lock) {
			return client ??= CreateClient();
		}
	}

	private static HttpClient CreateClient() {
		SocketsHttpHandler handler = new() {
			AutomaticDecompression = DecompressionMethods.All,
			MaxConnectionsPerServer = MaxConcurrentDownloads,
			PooledConnectionLifetime = TimeSpan.FromMinutes(5),
			PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
			ConnectTimeout = TimeSpan.FromSeconds(15),
			UseCookies = false,
		};
		ProxySettingsHelper.Configure(handler, AppSettingsService.Instance.Model);

		return new HttpClient(handler) {
			Timeout = TimeSpan.FromMinutes(5),
		};
	}

	/// <param name="progress">Reports 0-100, only when the content length is known.</param>
	public static async Task<byte[]> DownloadAsync(Uri uri, Action<int>? progress = null, CancellationToken token = default) {
		await gate.WaitAsync(token).ConfigureAwait(false);
		try {
			using HttpRequestMessage request = new(HttpMethod.Get, uri);
			request.Headers.UserAgent.ParseAdd(NetCode.UserAgent);

			using HttpResponseMessage response = await GetClient()
				.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token)
				.ConfigureAwait(false);

			response.EnsureSuccessStatusCode();

			long length = response.Content.Headers.ContentLength ?? 0;
			using Stream stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
			using MemoryStream buffer = length > 0 ? new((int)length) : new MemoryStream();

			byte[] chunk = ArrayPool<byte>.Shared.Rent(81920);
			try {
				long total = 0;
				int read;
				while ((read = await stream.ReadAsync(chunk, token).ConfigureAwait(false)) > 0) {
					await buffer.WriteAsync(chunk.AsMemory(0, read), token).ConfigureAwait(false);
					total += read;

					if (progress != null && length > 0) {
						progress((int)(total * 100 / length));
					}
				}
			} finally {
				ArrayPool<byte>.Shared.Return(chunk);
			}

			return buffer.ToArray();
		} finally {
			gate.Release();
		}
	}

}
