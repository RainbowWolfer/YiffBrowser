using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.Services;
using RW.Common;
using System.Windows.Media.Imaging;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Models;

public class PostImageLoader {

	public event TypedEventHandler<PostImageLoader, BitmapImage?>? ImageChanged;
	public event TypedEventHandler<PostImageLoader, GifImage?>? ImageGifChanged;

	public event TypedEventHandler<BitmapCacheItem, CacheLoadingModel>? Progress;

	public BitmapCacheItem Preview { get; }
	public BitmapCacheItem Sample { get; }

	public bool GettingFile { get; private set; }

	private static bool LoadSample =>
		AppSettingsService.Instance.Model.PreviewQuality == PreviewQuality.Sample;

	public PostImageLoader(E621Post post) {
		Preview = BitmapCacheService.Get(post.Preview?.URL);
		Sample = BitmapCacheService.Get(post.Sample?.URL);

		Preview.Updated += Preview_Updated;
		Sample.Updated += Sample_Updated;
	}

	~PostImageLoader() {
		Preview.Updated -= Preview_Updated;
		Sample.Updated -= Sample_Updated;
	}

	private void Preview_Updated(BitmapCacheItem sender, CacheLoadingModel args) {
		Progress?.Invoke(sender, args);
		if (args.HasCompleted) {
			RaiseImageChanged(sender);
			if (LoadSample) {
				Sample.Initialize();
			}
		}
	}

	private void Sample_Updated(BitmapCacheItem sender, CacheLoadingModel args) {
		Progress?.Invoke(sender, args);
		if (args.HasCompleted) {
			RaiseImageChanged(sender);
		}
	}

	public void Initialize() {
		if (LoadSample && Sample.HasCompleted) {
			Progress?.Invoke(Sample, new CacheLoadingModel(true, false, true, 100));
			RaiseImageChanged(Sample);
			return;
		}

		if (Preview.HasCompleted) {
			Progress?.Invoke(Preview, new CacheLoadingModel(true, false, true, 100));
			RaiseImageChanged(Preview);
			if (LoadSample) {
				Sample.Initialize();
			}
			return;
		}

		Preview.Initialize();
	}

	private void RaiseImageChanged(BitmapCacheItem item) {
		if (item.IsGif) {
			ImageGifChanged?.Invoke(this, item.GifImage);
		} else {
			ImageChanged?.Invoke(this, item.Image);
		}
	}

}
