using HandyControl.Controls;
using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.Services;

namespace YiffBrowser.E621.Controls;

internal class SlidePanelImageControl : Control {
	static SlidePanelImageControl() {
		DefaultStyleKeyProperty.OverrideMetadata(typeof(SlidePanelImageControl), new FrameworkPropertyMetadata(typeof(SlidePanelImageControl)));
	}

	public string Url {
		get => (string)GetValue(UrlProperty);
		set => SetValue(UrlProperty, value);
	}

	public static readonly DependencyProperty UrlProperty = DependencyProperty.Register(
		nameof(Url),
		typeof(string),
		typeof(SlidePanelImageControl),
		new PropertyMetadata(string.Empty, OnUrlChanged)
	);

	private static void OnUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		SlidePanelImageControl control = (SlidePanelImageControl)d;
		//control.LoadImage();
	}


	//private Image? _image;
	//private CircleProgressBar? _progress;
	//private FrameworkElement? _failed;

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		//_image = GetTemplateChild("PART_Image") as Image;
		//_progress = GetTemplateChild("PART_Progress") as CircleProgressBar;
		//_failed = GetTemplateChild("PART_Failed") as FrameworkElement;

		//LoadImage();
	}

	//private void LoadImage() {
	//	if (_image == null || _progress == null || _failed == null) {
	//		return;
	//	}

	//	if (Url.IsBlank()) {
	//		_progress.Visibility = Visibility.Collapsed;
	//		_failed.Visibility = Visibility.Visible;
	//		ToolTip = "URL is blank";
	//		return;
	//	}

	//	_failed.Visibility = Visibility.Collapsed;
	//	_image.Visibility = Visibility.Collapsed;

	//	_progress.Visibility = Visibility.Visible;
	//	_progress.Value = 0;
	//	_progress.IsIndeterminate = true;

	//	BitmapCacheItem item = BitmapCacheService.Get(Url);

	//	if (item.HasCompleted) {
	//		_image.Source = item.Image;
	//	} else {
	//		// todo : event leak here. think of something.
	//		item.Updated += Item_Updated;
	//	}

	//}

	//private void Item_Updated(BitmapCacheItem sender, BitmapLoadingModel args) {
	//	if (_image == null || _progress == null || _failed == null) {
	//		return;
	//	}

	//	if (args.HasCompleted) {
	//		_progress.Visibility = Visibility.Collapsed;
	//		_image.Visibility = Visibility.Visible;
	//		if (sender.Image != null) {
	//			_image.Source = sender.Image;
	//		}
	//	} else if (args.HasError) {
	//		string error = $"Loading Error : {args.Exception?.Message}";
	//		_progress.Visibility = Visibility.Collapsed;
	//		_failed.Visibility = Visibility.Visible;
	//		ToolTip = error;
	//	} else {
	//		double progress = args.Progress / 100d;
	//		_progress.IsIndeterminate = false;
	//		_progress.Value = progress;
	//	}
	//}
}
