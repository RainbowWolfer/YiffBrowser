using BaseFramework.Events;
using BaseFramework.Helpers;
using BaseFramework.Models;
using System.Collections.Concurrent;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BaseFramework.Services {
	public static class BitmapCacheService {
		private static ConcurrentDictionary<string, BitmapCacheItem> Pool { get; } = [];

		public static BitmapCacheItem Get(string? url) {
			if (url.IsBlank()) {
				return BitmapCacheItem.Null;
			}
			if (Pool.TryGetValue(url, out BitmapCacheItem? found)) {
				return found;
			} else {
				BitmapCacheItem item = new(url);
				return Pool[url] = item;
			}
		}

	}

	public class BitmapCacheItem(string? url) {
		public event TypedEventHandler<BitmapCacheItem, BitmapLoadingModel>? Updated;

		public string? UrlString { get; } = url;
		public Guid ID { get; } = Guid.NewGuid();
		public Uri? Uri { get; } = url.IsBlank() ? null : new Uri(url);

		public bool IsGif { get; } = url != null && url.EndsWith(".gif");

		public BitmapImage? Image { get; private set; }
		public GifImage? GifImage { get; private set; }

		public bool HasError { get; private set; } = false;
		public bool HasCompleted { get; private set; } = false;

		public void Initialize() {
			if (Image != null || GifImage != null) {
				return;
			}

			if (Uri is null) {
				return;
			}

			Updated?.Invoke(this, new BitmapLoadingModel(false, false, false, 0));

			if (!IsGif) {
				Image = new BitmapImage(Uri);
				Image.DownloadCompleted += DownloadCompleted;
				Image.DownloadFailed += DownloadFailed;
				Image.DownloadProgress += DownloadProgress;
			} else {
				GifImage = new GifImage(Uri);
				GifImage.Initialize();
				GifImage.DownloadCompleted += DownloadCompleted;
				GifImage.DownloadFailed += GifImage_DownloadFailed;
				GifImage.DownloadProgress += GifImage_DownloadProgress;
				//Updated?.Invoke(this, new BitmapLoadingModel(true, false, true, 100));
			}
		}

		private void DownloadProgress(object? sender, DownloadProgressEventArgs e) {
			Updated?.Invoke(this, new BitmapLoadingModel(true, false, false, e.Progress));
		}

		private void GifImage_DownloadProgress(GifImage sender, int args) {
			Updated?.Invoke(this, new BitmapLoadingModel(true, false, false, args));
		}

		private void DownloadFailed(object? sender, ExceptionEventArgs e) {
			HasError = true;
			Updated?.Invoke(this, new BitmapLoadingModel(true, true, false, 0, e.ErrorException));
		}

		private void GifImage_DownloadFailed(GifImage sender, Exception args) {
			HasError = true;
			Updated?.Invoke(this, new BitmapLoadingModel(true, true, false, 0, args));
		}

		private void DownloadCompleted(object? sender, EventArgs e) {
			HasCompleted = true;
			Updated?.Invoke(this, new BitmapLoadingModel(true, false, true, 100));

			Image?.Freeze();
		}

		public void Clear() {
			if (Image != null) {
				Image.DownloadCompleted -= DownloadCompleted;
				Image.DownloadFailed -= DownloadFailed;
				Image.DownloadProgress -= DownloadProgress;
			}
			if (GifImage != null) {
				GifImage.DownloadCompleted -= DownloadCompleted;
				GifImage.DownloadFailed -= GifImage_DownloadFailed;
				GifImage.DownloadProgress -= GifImage_DownloadProgress;
			}
			Image = null;
			GifImage = null;
			//Updated?.Invoke(this, new BitmapLoadingModel(false, false, false, 0));
		}

		public static BitmapCacheItem Null => new(null);
	}
}
