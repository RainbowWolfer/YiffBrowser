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

		public BitmapImage? Image { get; private set; }

		public bool HasError { get; private set; } = false;
		public bool HasCompleted { get; private set; } = false;

		public void Initialize() {
			if (Image != null || Uri is null) {
				return;
			}

			Updated?.Invoke(this, new BitmapLoadingModel(false, false, false, 0));
			Image = new BitmapImage(Uri);
			Image.DownloadCompleted += Image_DownloadCompleted;
			Image.DownloadFailed += Image_DownloadFailed;
			Image.DownloadProgress += Image_DownloadProgress;
		}

		private void Image_DownloadProgress(object? sender, DownloadProgressEventArgs e) {
			Updated?.Invoke(this, new BitmapLoadingModel(true, false, false, e.Progress));
		}

		private void Image_DownloadFailed(object? sender, ExceptionEventArgs e) {
			HasError = true;
			Updated?.Invoke(this, new BitmapLoadingModel(true, true, false, 0, e.ErrorException));
		}

		private void Image_DownloadCompleted(object? sender, EventArgs e) {
			HasCompleted = true;
			Updated?.Invoke(this, new BitmapLoadingModel(true, false, true, 100));
		}

		public static BitmapCacheItem Null => new(null);
	}
}
