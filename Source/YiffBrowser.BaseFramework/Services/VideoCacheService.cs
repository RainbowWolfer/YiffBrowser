using RW.Common;
using RW.Common.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using YiffBrowser.BaseFramework.Models;

namespace YiffBrowser.BaseFramework.Services;

public static class VideoCacheService {
	private static ConcurrentDictionary<string, VideoCacheItem> Pool { get; } = [];

	public static VideoCacheItem Get(string? url, long fileSize, MediaCacheKey? cacheKey = null) {
		if (url.IsBlank()) {
			return VideoCacheItem.Null;
		}

		VideoCacheItem item = Pool.GetOrAdd(url, key => new VideoCacheItem(key, fileSize));
		item.AttachKey(cacheKey);
		return item;
	}

	/// <summary>
	/// Writes media that was decoded before the disk cache was turned on.
	/// Already-open videos are copied from memory; anything else is downloaded again.
	/// </summary>
	public static void BackfillDisk() {
		if (!MediaDiskCache.IsEnabled) {
			return;
		}

		foreach (VideoCacheItem item in Pool.Values) {
			item.BackfillDisk();
		}
	}

}

public class VideoCacheItem(string? url, long fileSize) {
	public event TypedEventHandler<VideoCacheItem, CacheLoadingModel>? Updated;

	public bool IsNull => UrlString is null;
	public string? UrlString { get; } = url;
	public Guid ID { get; } = Guid.NewGuid();
	public Uri? Uri { get; } = url.IsBlank() ? null : new Uri(url);

	public bool HasError { get; private set; } = false;
	public bool HasCompleted { get; private set; } = false;
	public bool IsLoading { get; private set; } = false;

	/// <summary>Set when the bytes live in the disk cache. Prefer this over <see cref="MemoryStream"/>.</summary>
	public string? LocalPath { get; private set; }

	private MemoryStream? cacheStream;
	public MemoryStream? MemoryStream => cacheStream;

	private MediaCacheKey? cacheKey;
	private int generation;
	public long FileSize { get; } = fileSize;

	public void AttachKey(MediaCacheKey? key) {
		if (key != null && cacheKey == null) {
			cacheKey = key;
		}
	}

	public void Initialize() {
		if (Uri is null) {
			return;
		}

		if (HasCompleted) {
			Raise(new CacheLoadingModel(true, false, true, 100));
			return;
		}

		if (IsLoading) {
			Raise(new CacheLoadingModel(true, false, false, 0));
			return;
		}

		if (HasError) {
			Clear();
		}

		if (cacheKey != null && MediaDiskCache.TryGetExistingFile(cacheKey, FileSize, out string existing)) {
			LocalPath = existing;
			Complete();
			return;
		}

		IsLoading = true;
		Raise(new CacheLoadingModel(false, false, false, 0));
		_ = LoadAsync(Uri, generation);
	}

	private async Task LoadAsync(Uri uri, int startedGeneration) {
		try {
			if (cacheKey != null && MediaDiskCache.IsEnabled) {
				string? path = await MediaDiskCache.DownloadToFileAsync(cacheKey, uri, FileSize, OnProgress).ConfigureAwait(false);
				if (startedGeneration != generation) {
					return;
				}

				if (path != null) {
					LocalPath = path;
					Complete();
					return;
				}
			}

			byte[] data = await MediaDownloadService.DownloadAsync(uri, OnProgress).ConfigureAwait(false);
			if (startedGeneration != generation) {
				return;
			}

			cacheStream = new MemoryStream(data, writable: false);
			Complete();
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			if (startedGeneration == generation) {
				Fail(ex);
			}
		}
	}

	private void OnProgress(int progress) {
		Raise(new CacheLoadingModel(true, false, false, progress));
	}

	private void Fail(Exception ex) {
		IsLoading = false;
		HasError = true;
		Raise(new CacheLoadingModel(true, true, false, 0, ex));
	}

	private void Complete() {
		IsLoading = false;
		HasError = false;
		HasCompleted = true;
		Raise(new CacheLoadingModel(true, false, true, 100));
	}

	private void Raise(CacheLoadingModel model) {
		Updated?.Invoke(this, model);
	}

	/// <summary>Drops the file so the next <see cref="Initialize"/> downloads it again.</summary>
	public void DiscardDisk() {
		if (cacheKey != null) {
			MediaDiskCache.Delete(cacheKey);
		}
		Clear();
	}

	public void BackfillDisk() {
		if (cacheKey == null || Uri == null || IsNull || !MediaDiskCache.IsEnabled) {
			return;
		}

		if (MediaDiskCache.TryGetExistingFile(cacheKey, FileSize, out _)) {
			return;
		}

		if (cacheStream != null) {
			MediaDiskCache.TryStore(cacheKey, cacheStream.ToArray());
			return;
		}

		if (!HasCompleted) {
			return;
		}

		Uri uri = Uri;
		MediaCacheKey key = cacheKey;
		long size = FileSize;
		_ = Task.Run(async () => {
			try {
				await MediaDiskCache.DownloadToFileAsync(key, uri, size, null).ConfigureAwait(false);
			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}
		});
	}

	public void Clear() {
		generation++;
		cacheStream?.Dispose();
		cacheStream = null;
		LocalPath = null;
		HasCompleted = false;
		HasError = false;
		IsLoading = false;
	}

	public static VideoCacheItem Null => new(null, 0);
}
