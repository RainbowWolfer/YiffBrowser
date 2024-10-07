using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using BaseFramework.Events;
using YB.E621.Models.E621;
using BaseFramework.Helpers;
using BaseFramework.Services;
using BaseFramework.Models;

namespace YB.E621.Models {
	public class PostImageLoader {

		public event TypedEventHandler<PostImageLoader, BitmapImage?>? ImageChanged;
		public event TypedEventHandler<PostImageLoader, GifImage?>? ImageGifChanged;

		public event TypedEventHandler<BitmapCacheItem, BitmapLoadingModel>? Progress;

		public BitmapCacheItem Preview { get; }
		public BitmapCacheItem Sample { get; }
		//public BitmapCacheItem File { get; }

		public bool GettingFile { get; private set; }

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

		private void Preview_Updated(BitmapCacheItem sender, BitmapLoadingModel args) {
			Progress?.Invoke(sender, args);
			if (args.HasCompleted) {
				RaiseImageChanged(sender);
				Sample.Initialize();
			}
		}

		private void Sample_Updated(BitmapCacheItem sender, BitmapLoadingModel args) {
			Progress?.Invoke(sender, args);
			if (args.HasCompleted) {
				RaiseImageChanged(sender);
			}
		}

		public void Initialize() {
			Preview.Initialize();
			if (Sample.HasCompleted) {
				Progress?.Invoke(Sample, new BitmapLoadingModel(true, false, true, 100));
				RaiseImageChanged(Sample);
			} else if (Preview.HasCompleted) {
				Progress?.Invoke(Preview, new BitmapLoadingModel(true, false, true, 100));
				RaiseImageChanged(Preview);
			}
		}

		private void RaiseImageChanged(BitmapCacheItem item) {
			if (item.IsGif) {
				ImageGifChanged?.Invoke(this, item.GifImage);
			} else {
				ImageChanged?.Invoke(this, item.Image);
			}
		}

	}
}
