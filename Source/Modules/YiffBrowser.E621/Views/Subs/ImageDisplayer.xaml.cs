using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Views.Subs;

public partial class ImageDisplayer : UserControl, IPostDisplayer {

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

	public LoadingStatus LoadingStatus {
		get => (LoadingStatus)GetValue(LoadingStatusProperty);
		private set => SetValue(LoadingStatusPropertyKey, value);
	}

	private static readonly DependencyPropertyKey LoadingStatusPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(LoadingStatus),
		typeof(LoadingStatus),
		typeof(ImageDisplayer),
		new PropertyMetadata(new LoadingStatus())
	);

	public static readonly DependencyProperty LoadingStatusProperty = LoadingStatusPropertyKey.DependencyProperty;

	public ImageDisplayer() {
		InitializeComponent();
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
		base.OnRenderSizeChanged(sizeInfo);
		ImageViewer.Initialize();
	}

	private void Update() {
		Update(Post);
	}

	private long fileSize = 0;

	private BitmapCacheItem? preview;
	private BitmapCacheItem? sample;
	private BitmapCacheItem? file;

	private static bool LoadSample =>
		AppSettingsService.Instance.Model.PreviewQuality == PreviewQuality.Sample;

	public void Update(E621Post? post) {
		Unbind();

		if (post is null || !post.GetFileType().IsImage()) {
			preview = sample = file = null;
			ImageViewer.Clear();
			return;
		}

		LoadingStatus.Initialize();

		IsFileReady = false;
		fileSize = post.File?.Size ?? 0;

		preview = GetCache(post.Preview?.URL);
		sample = LoadSample ? GetCache(post.Sample?.URL) : null;
		file = GetCache(post.File?.URL);

		Bind();

		// Show preview/sample already cached from the post card while the full file loads.
		if (!ShowBestAvailable()) {
			ImageViewer.Clear();
		}

		if (HasContent(file)) {
			LoadingStatus.Done();
			IsFileReady = true;
			return;
		}

		if (preview != null && !preview.HasCompleted) {
			preview.Initialize();
			return;
		}

		ContinueAfterPreview();
	}

	private static BitmapCacheItem? GetCache(string? url) {
		if (url.IsBlank()) {
			return null;
		}
		BitmapCacheItem item = BitmapCacheService.Get(url);
		return item.IsNull ? null : item;
	}

	private static bool HasContent(BitmapCacheItem? item) =>
		item != null && (item.Image != null || item.GifImage != null);

	private void Unbind() {
		if (preview != null) {
			preview.Updated -= Preview_Updated;
		}
		if (sample != null) {
			sample.Updated -= Sample_Updated;
		}
		if (file != null) {
			file.Updated -= File_Updated;
		}
	}

	private void Bind() {
		if (preview != null) {
			preview.Updated += Preview_Updated;
		}
		if (sample != null) {
			sample.Updated += Sample_Updated;
		}
		if (file != null) {
			file.Updated += File_Updated;
		}
	}

	private bool ShowBestAvailable() {
		if (HasContent(file)) {
			SetImageContent(file!);
			return true;
		}
		if (LoadSample && HasContent(sample)) {
			SetImageContent(sample!);
			return true;
		}
		if (HasContent(preview)) {
			SetImageContent(preview!);
			return true;
		}
		return false;
	}

	private void ContinueAfterPreview() {
		if (sample != null && !sample.HasCompleted) {
			sample.Initialize();
			return;
		}

		file?.Initialize();
	}

	private void Preview_Updated(BitmapCacheItem sender, CacheLoadingModel args) {
		if (!CheckAccess()) {
			Dispatcher.Invoke(Preview_Updated, DispatcherPriority.Normal, sender, args);
			return;
		}
		if (!args.HasCompleted) {
			return;
		}

		if (!HasContent(sample) && !HasContent(file)) {
			SetImageContent(sender);
		}

		ContinueAfterPreview();
	}

	private void Sample_Updated(BitmapCacheItem sender, CacheLoadingModel args) {
		if (!CheckAccess()) {
			Dispatcher.Invoke(Sample_Updated, DispatcherPriority.Normal, sender, args);
			return;
		}
		if (!args.HasCompleted) {
			return;
		}

		if (!HasContent(file)) {
			SetImageContent(sender);
		}

		file?.Initialize();

		if (HasContent(file)) {
			SetImageContent(file!);
			LoadingStatus.Done();
			IsFileReady = true;
		}
	}

	private void File_Updated(BitmapCacheItem sender, CacheLoadingModel args) {
		if (!CheckAccess()) {
			Dispatcher.Invoke(File_Updated, DispatcherPriority.Normal, sender, args);
			return;
		}
		if (args.HasCompleted) {
			LoadingStatus.Done();
			if (HasContent(sender)) {
				SetImageContent(sender);
				IsFileReady = true;
			}
		} else if (args.HasError) {
			LoadingStatus.Error($"Loading Error : {args.Exception?.Message}");
		} else {
			double progress = args.Progress / 100d;
			long downloaded = (long)(fileSize * (args.Progress / 100d));
			string downloadInfo = $"{downloaded.FileSizeToKB()} / {fileSize.FileSizeToKB()}";

			LoadingStatus.SetProgress(progress, downloadInfo);
		}
	}

	private void SetImageContent(BitmapCacheItem item) {
		void Apply() {
			if (item.IsGif && item.GifImage != null) {
				ImageViewer.SetGifImage(item.GifImage);
			} else if (item.Image != null) {
				ImageViewer.SetBitmapImage(item.Image);
			}
		}

		if (CheckAccess()) {
			Apply();
		} else {
			Dispatcher.Invoke(Apply);
		}
	}

	private DelegateCommand? reloadCommand;
	public IDelegateCommand ReloadCommand => reloadCommand ??= new(Reload);
	private void Reload() {
		file?.Clear();
		Update();
	}

	private DelegateCommand? fitToWindowCommand;
	public IDelegateCommand FitToWindowCommand => fitToWindowCommand ??= new(ImageViewer.Initialize);

	private DelegateCommand? actualSizeCommand;
	public IDelegateCommand ActualSizeCommand => actualSizeCommand ??= new(ImageViewer.Actual);

	private DelegateCommand? copyImageCommand;
	public IDelegateCommand CopyImageCommand => copyImageCommand ??= new(CopyImage, CanCopyImage);
	private void CopyImage() {
		if (ImageViewer.BitmapImage is { } bitmap) {
			Clipboard.SetImage(bitmap);
		}
	}
	// Gifs are played through GifImage and have no single BitmapSource to hand over.
	private bool CanCopyImage() => ImageViewer.BitmapImage != null;

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
