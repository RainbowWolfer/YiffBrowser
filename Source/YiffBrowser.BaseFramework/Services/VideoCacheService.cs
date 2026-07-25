using RW.Common;
using RW.Common.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.BaseFramework.Models;

namespace YiffBrowser.BaseFramework.Services;

public static class VideoCacheService {
	private static ConcurrentDictionary<string, VideoCacheItem> Pool { get; } = [];

	public static VideoCacheItem Get(string? url, long fileSize) {
		if (url.IsBlank()) {
			return VideoCacheItem.Null;
		}
		Debug.WriteLine(url);
		if (Pool.TryGetValue(url, out VideoCacheItem? found)) {
			return found;
		} else {
			VideoCacheItem item = new(url, (int)fileSize);
			return Pool[url] = item;
		}
	}


}

public class VideoCacheItem(string? url, int fileSize) {
	public event TypedEventHandler<VideoCacheItem, CacheLoadingModel>? Updated;


	public bool IsNull => UrlString is null;
	public string? UrlString { get; } = url;
	public Guid ID { get; } = Guid.NewGuid();
	public Uri? Uri { get; } = url.IsBlank() ? null : new Uri(url);


	public bool HasError { get; private set; } = false;
	public bool HasCompleted { get; private set; } = false;

	private MemoryStream? cacheStream;
	public MemoryStream? MemoryStream => cacheStream;

	private readonly CancellationTokenSource cts = new();

	public async void Initialize() {
		if (cacheStream != null) {
			return;
		}

		if (Uri is null) {
			return;
		}

		CancellationToken token = cts.Token;

		try {
			using HttpClient client = ProxySettingsHelper.CreateHttpClient();
			using HttpResponseMessage response = await client.GetAsync(UrlString, HttpCompletionOption.ResponseHeadersRead, token);
			response.EnsureSuccessStatusCode();

			long totalBytes = response.Content.Headers.ContentLength ?? fileSize;

			using Stream contentStream = await response.Content.ReadAsStreamAsync(token);

			byte[] buffer = new byte[8192];
			long totalRead = 0;
			int read;

			cacheStream = new MemoryStream(fileSize);
			while ((read = await contentStream.ReadAsync(buffer, token)) > 0) {
				await cacheStream.WriteAsync(buffer.AsMemory(0, read), token);
				totalRead += read;

				double progress = (double)totalRead / totalBytes;

				DownloadProgress((int)(progress * 100));

				token.ThrowIfCancellationRequested();
			}

			DownloadCompleted();
		} catch (OperationCanceledException) {

		} catch (Exception ex) {
			DownloadFailed(ex);
		} finally {

		}

	}



	private void DownloadProgress(int progress) {
		Updated?.Invoke(this, new CacheLoadingModel(true, false, false, progress));
	}

	private void DownloadFailed(Exception ex) {
		HasError = true;
		Updated?.Invoke(this, new CacheLoadingModel(true, true, false, 0, ex));
	}

	private void DownloadCompleted() {
		HasCompleted = true;
		Updated?.Invoke(this, new CacheLoadingModel(true, false, true, 100));
	}

	public void Clear() {
		cacheStream?.Dispose();
		cacheStream = null;
		//Updated?.Invoke(this, new CacheLoadingModel(false, false, false, 0));
	}

	public static VideoCacheItem Null => new(null, 0);
}