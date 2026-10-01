using RW.Common;
using RW.Common.Helpers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Models;

namespace YiffBrowser.BaseFramework.Services;

public static class BitmapCacheService {
	private static ConcurrentDictionary<string, BitmapCacheItem> Pool { get; } = [];

	public static BitmapCacheItem Get(string? url) {
		if (url.IsBlank()) {
			return BitmapCacheItem.Null;
		}
		// GetOrAdd keeps every view on the same item; two items for one url would download twice
		// and each would only notify its own subscribers.
		return Pool.GetOrAdd(url, static key => new BitmapCacheItem(key));
	}

}

public class BitmapCacheItem(string? url) {
	public event TypedEventHandler<BitmapCacheItem, CacheLoadingModel>? Updated;

	public bool IsNull => UrlString is null;
	public string? UrlString { get; } = url;
	public Guid ID { get; } = Guid.NewGuid();
	public Uri? Uri { get; } = url.IsBlank() ? null : new Uri(url);

	public bool IsGif { get; } = url != null && url.EndsWith(".gif");

	public BitmapImage? Image { get; private set; }
	public GifImage? GifImage { get; private set; }

	public bool HasError { get; private set; } = false;
	public bool HasCompleted { get; private set; } = false;
	public bool IsLoading { get; private set; } = false;

	public Exception? Error { get; private set; }

	private int lastProgress = 0;

	/// <summary>Bumped by <see cref="Clear"/> so a download started before it is discarded.</summary>
	private int generation = 0;

	/// <summary>
	/// Items are shared per url, so this can be called by any number of views at any point of the
	/// download. Every call reports the current state back, otherwise late callers keep waiting
	/// for an event that already happened.
	/// </summary>
	public void Initialize() {
		if (Uri is null) {
			return;
		}

		if (HasCompleted) {
			RaiseUpdated(new CacheLoadingModel(true, false, true, 100));
			return;
		}

		if (IsLoading) {
			RaiseUpdated(new CacheLoadingModel(true, false, false, lastProgress));
			return;
		}

		// A failed item is reset here so that reopening a post retries it.
		if (HasError) {
			Clear();
		}

		IsLoading = true;
		lastProgress = 0;
		RaiseUpdated(new CacheLoadingModel(false, false, false, 0));

		_ = LoadAsync(Uri, generation);
	}

	private async Task LoadAsync(Uri uri, int startedGeneration) {
		try {
			byte[] data = await MediaDownloadService.DownloadAsync(uri, OnProgress).ConfigureAwait(false);

			if (startedGeneration != generation) {
				return;
			}

			if (IsGif) {
				GifImage gifImage = new(uri);
				gifImage.Load(data);
				GifImage = gifImage;
			} else {
				Image = CreateFrozenImage(data);
			}

			Complete();
		} catch (Exception ex) {
			Debug.WriteLine(ex);
			if (startedGeneration == generation) {
				Fail(ex);
			}
		}
	}

	/// <summary>
	/// Decoded on the calling (background) thread and frozen, so it can be handed to the UI
	/// thread afterwards.
	/// </summary>
	private static BitmapImage CreateFrozenImage(byte[] data) {
		using MemoryStream stream = new(data, writable: false);

		BitmapImage image = new();
		image.BeginInit();
		// OnLoad decodes right here so the stream can be released and the image frozen.
		image.CacheOption = BitmapCacheOption.OnLoad;
		image.StreamSource = stream;
		image.EndInit();
		image.Freeze();

		return image;
	}

	private void OnProgress(int percent) {
		// Every report crosses to the UI thread, so only forward meaningful steps.
		if (percent < 100 && percent - lastProgress < 5) {
			return;
		}

		lastProgress = percent;
		RaiseUpdated(new CacheLoadingModel(true, false, false, percent));
	}

	private void Complete() {
		IsLoading = false;
		HasError = false;
		Error = null;
		HasCompleted = true;
		lastProgress = 100;

		RaiseUpdated(new CacheLoadingModel(true, false, true, 100));
	}

	private void Fail(Exception exception) {
		IsLoading = false;
		HasError = true;
		Error = exception;

		RaiseUpdated(new CacheLoadingModel(true, true, false, 0, exception));
	}

	/// <summary>
	/// Downloads run on background threads while every consumer updates WPF state, so events are
	/// always delivered on the UI thread. State is set before raising, so handlers see it either
	/// way.
	/// </summary>
	private void RaiseUpdated(CacheLoadingModel model) {
		if (Updated is null) {
			return;
		}

		Dispatcher? dispatcher = Application.Current?.Dispatcher;
		if (dispatcher is null || dispatcher.CheckAccess()) {
			Updated?.Invoke(this, model);
		} else {
			_ = dispatcher.BeginInvoke(() => Updated?.Invoke(this, model));
		}
	}

	// Clear 可能会有很多的问题，二次载入相关的问题。
	public void Clear() {
		generation++;
		Image = null;
		GifImage = null;
		HasCompleted = false;
		HasError = false;
		IsLoading = false;
		Error = null;
		lastProgress = 0;
	}

	public static BitmapCacheItem Null => new(null);
}
