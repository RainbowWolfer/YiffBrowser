using BaseFramework.Helpers;
using BaseFramework.Models;
using BaseFramework.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Models.E621;

namespace YB.E621.Views.Subs {
	public partial class ImageDisplayer : UserControl {

		public E621Post? Post {
			get => (E621Post)GetValue(PostProperty);
			set => SetValue(PostProperty, value);
		}

		public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
			nameof(Post),
			typeof(E621Post),
			typeof(ImageDisplayer),
			new PropertyMetadata(null, OnPostChanged)
		);

		private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((ImageDisplayer)d).Update();
		}

		public bool IsFileReady {
			get => (bool)GetValue(IsFileReadyProperty);
			private set => SetValue(IsFileReadyPropertyKey, value);
		}

		public static readonly DependencyPropertyKey IsFileReadyPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(IsFileReady),
			typeof(bool),
			typeof(ImageDisplayer),
			new PropertyMetadata(false)
		);

		public static readonly DependencyProperty IsFileReadyProperty = IsFileReadyPropertyKey.DependencyProperty;



		public string DownloadInfo {
			get => (string)GetValue(DownloadInfoProperty);
			private set => SetValue(DownloadInfoPropertyKey, value);
		}

		public static readonly DependencyPropertyKey DownloadInfoPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(DownloadInfo),
			typeof(string),
			typeof(ImageDisplayer),
			new PropertyMetadata(string.Empty)
		);

		public static readonly DependencyProperty DownloadInfoProperty = DownloadInfoPropertyKey.DependencyProperty;



		public string ErrorMessage {
			get => (string)GetValue(ErrorMessageProperty);
			private set => SetValue(ErrorMessagePropertyKey, value);
		}

		public static readonly DependencyPropertyKey ErrorMessagePropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(ErrorMessage),
			typeof(string),
			typeof(ImageDisplayer),
			new PropertyMetadata(string.Empty)
		);

		public static readonly DependencyProperty ErrorMessageProperty = ErrorMessagePropertyKey.DependencyProperty;


		public ImageDisplayer() {
			InitializeComponent();
		}

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
			base.OnRenderSizeChanged(sizeInfo);
			//if (!IsFileReady) {
			ImageViewer.Initialize();
			//}
		}

		private void Update() {
			Update(Post);
		}

		private long fileSize = 0;

		private BitmapCacheItem? sample;
		private BitmapCacheItem? file;

		public void Update(E621Post? post) {
			if (post is null) {
				Dispatcher.Invoke(ImageViewer.Clear, DispatcherPriority.Loaded);
				return;
			}

			ErrorMessage = string.Empty;
			ProgressBar.IsIndeterminate = true;
			LoadingBorder.Visibility = Visibility.Visible;

			IsFileReady = false;
			fileSize = post.File?.Size ?? 0;

			if (sample != null) {
				sample.Updated -= Sample_Updated;
			}

			if (file != null) {
				file.Updated -= File_Updated;
			}

			if (post.Sample != null && post.Sample.URL != null) {
				sample = BitmapCacheService.Get(post.Sample.URL);
			}

			if (post.File != null && post.File.URL != null) {
				file = BitmapCacheService.Get(post.File.URL);
			}

			if (sample != null) {
				sample.Updated += Sample_Updated;
			}

			if (file != null) {
				file.Updated += File_Updated;
			}

			if (sample != null) {
				if (sample.HasCompleted) {
					SetImageContent(sample);
					//ImageViewer.SetBitmapImage(sample.Image);
				} else {
					sample.Initialize();
					return;
				}
			}

			if (file != null) {
				if (file.HasCompleted) {
					SetImageContent(file);
					//ImageViewer.SetBitmapImage(file.Image);
					LoadingBorder.Visibility = Visibility.Collapsed;
					IsFileReady = true;
				} else {
					file.Initialize();
					return;
				}
			}

		}

		private void Sample_Updated(BitmapCacheItem sender, BitmapLoadingModel args) {
			if (!CheckAccess()) {
				Dispatcher.Invoke(Sample_Updated, DispatcherPriority.Loaded, sender, args);
				return;
			}
			if (args.HasCompleted) {
				file?.Initialize();
				if (sender.Image != null) {
					SetImageContent(sender);
					//ImageViewer.SetBitmapImage(sender.Image);
				}
				if (file != null && file.HasCompleted) {
					SetImageContent(file);
				}
			}
		}

		private void File_Updated(BitmapCacheItem sender, BitmapLoadingModel args) {
			if (!CheckAccess()) {
				Dispatcher.Invoke(File_Updated, DispatcherPriority.Loaded, sender, args);
				return;
			}
			if (args.HasCompleted) {
				LoadingBorder.Visibility = Visibility.Collapsed;
				if (sender.Image != null) {
					SetImageContent(sender);
					//ImageViewer.SetBitmapImage(sender.Image);
					IsFileReady = true;
				}
			} else if (args.HasError) {
				LoadingBorder.Visibility = Visibility.Visible;
				ErrorMessage = $"Loading Error : {args.Exception?.Message}";
			} else {
				LoadingBorder.Visibility = Visibility.Visible;
				ProgressBar.IsIndeterminate = false;
				ProgressBar.Value = args.Progress;
				int downloaded = (int)(fileSize * (args.Progress / 100d));
				DownloadInfo = $"{downloaded.FileSizeToKB()} / {fileSize.FileSizeToKB()}";
			}
		}

		private void SetImageContent(BitmapCacheItem item) {
			Dispatcher.Invoke(() => {
				if (item.IsGif && item.GifImage != null) {
					ImageViewer.SetGifImage(item.GifImage);
				} else if (item.Image != null) {
					ImageViewer.SetBitmapImage(item.Image);
				}
			}, DispatcherPriority.Loaded);
		}

		private void ReloadButton_Click(object sender, RoutedEventArgs e) {
			file?.Clear();
			Update();
		}

		private void ImageViewer_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (Math.Abs(ImageViewer.ImageScale - 1) < 0.01) {
				ImageViewer.Initialize();
			} else {
				ImageViewer.Actual();
			}
			e.Handled = true;
		}

		private void ImageViewer_MouseUp(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton == MouseButton.Middle) {
				ImageViewer.Initialize();
				e.Handled = true;
			}
		}
	}
}
