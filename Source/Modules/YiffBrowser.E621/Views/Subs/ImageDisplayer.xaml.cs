using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Views.Subs;

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

	public LoadingStatusViewModel LoadingStatus {
		get => (LoadingStatusViewModel)GetValue(LoadingStatusProperty);
		private set => SetValue(LoadingStatusPropertyKey, value);
	}

	private static readonly DependencyPropertyKey LoadingStatusPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(LoadingStatus),
		typeof(LoadingStatusViewModel),
		typeof(ImageDisplayer),
		new PropertyMetadata(new LoadingStatusViewModel())
	);

	public static readonly DependencyProperty LoadingStatusProperty = LoadingStatusPropertyKey.DependencyProperty;





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
		if (post is null || !post.GetFileType().IsImage()) {
			Dispatcher.Invoke(ImageViewer.Clear, DispatcherPriority.Loaded);
			return;
		}

		LoadingStatus.InitialLoading();

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
				LoadingStatus.DoneLoading();
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
			LoadingStatus.DoneLoading();
			if (sender.Image != null) {
				SetImageContent(sender);
				//ImageViewer.SetBitmapImage(sender.Image);
				IsFileReady = true;
			}
		} else if (args.HasError) {
			LoadingStatus.LoadingError($"Loading Error : {args.Exception?.Message}");
		} else {
			double progress = args.Progress / 100d;
			long downloaded = (long)(fileSize * (args.Progress / 100d));
			string downloadInfo = $"{downloaded.FileSizeToKB()} / {fileSize.FileSizeToKB()}";

			LoadingStatus.SetProgress(progress, downloadInfo);
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


	private DelegateCommand? reloadCommand;
	public IDelegateCommand ReloadCommand => reloadCommand ??= new(Reload);
	private void Reload() {
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
