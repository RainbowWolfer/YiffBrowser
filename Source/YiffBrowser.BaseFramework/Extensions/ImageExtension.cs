using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using XamlAnimatedGif;
using YiffBrowser.BaseFramework.Enums;

namespace YiffBrowser.BaseFramework.Extensions;

public static class ImageExtension {

	private static readonly Dictionary<ScrollViewer, List<WeakReference<Image>>> TrackedImages = [];

	public static GifAutoPlayType GetGifAutoPlayType(DependencyObject obj) {
		return (GifAutoPlayType)obj.GetValue(GifAutoPlayTypeProperty);
	}

	public static void SetGifAutoPlayType(DependencyObject obj, GifAutoPlayType value) {
		obj.SetValue(GifAutoPlayTypeProperty, value);
	}

	public static readonly DependencyProperty GifAutoPlayTypeProperty = DependencyProperty.RegisterAttached(
		"GifAutoPlayType",
		typeof(GifAutoPlayType),
		typeof(ImageExtension),
		new PropertyMetadata(GifAutoPlayType.WhenMouseOver, OnGifAutoPlayTypeChanged)
	);

	private static readonly DependencyProperty ViewportScrollViewerProperty = DependencyProperty.RegisterAttached(
		"ViewportScrollViewer",
		typeof(ScrollViewer),
		typeof(ImageExtension),
		new PropertyMetadata(null)
	);

	private static void OnGifAutoPlayTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is not Image image) {
			return;
		}

		DetachHoverHandlers(image);
		DetachViewportTracking(image);

		if (e.NewValue is not GifAutoPlayType type) {
			return;
		}

		switch (type) {
			case GifAutoPlayType.Never:
				AnimationBehavior.SetAutoStart(image, false);
				AnimationBehavior.GetAnimator(image)?.Pause();
				break;
			case GifAutoPlayType.WhenMouseOver:
				AnimationBehavior.SetAutoStart(image, false);
				AnimationBehavior.GetAnimator(image)?.Pause();
				image.MouseEnter += Image_MouseEnter;
				image.MouseLeave += Image_MouseLeave;
				break;
			case GifAutoPlayType.Always:
				AttachViewportTracking(image);
				UpdateAlwaysPlayback(image);
				break;
			default:
				throw new NotImplementedException();
		}
	}

	private static void DetachHoverHandlers(Image image) {
		image.MouseEnter -= Image_MouseEnter;
		image.MouseLeave -= Image_MouseLeave;
	}

	private static void Image_MouseLeave(object sender, MouseEventArgs e) {
		if (sender is Image image) {
			AnimationBehavior.GetAnimator(image)?.Pause();
		}
	}

	private static void Image_MouseEnter(object sender, MouseEventArgs e) {
		if (sender is Image image) {
			AnimationBehavior.GetAnimator(image)?.Play();
		}
	}

	private static void AttachViewportTracking(Image image) {
		image.Loaded += Image_ViewportLoaded;
		image.Unloaded += Image_ViewportUnloaded;
		image.IsVisibleChanged += Image_ViewportIsVisibleChanged;
		image.SizeChanged += Image_ViewportSizeChanged;
		TryAttachScrollViewer(image);
	}

	private static void DetachViewportTracking(Image image) {
		image.Loaded -= Image_ViewportLoaded;
		image.Unloaded -= Image_ViewportUnloaded;
		image.IsVisibleChanged -= Image_ViewportIsVisibleChanged;
		image.SizeChanged -= Image_ViewportSizeChanged;
		UnregisterFromScrollViewer(image);
	}

	private static void Image_ViewportLoaded(object sender, RoutedEventArgs e) {
		if (sender is Image image) {
			TryAttachScrollViewer(image);
			UpdateAlwaysPlayback(image);
		}
	}

	private static void Image_ViewportUnloaded(object sender, RoutedEventArgs e) {
		if (sender is Image image) {
			AnimationBehavior.GetAnimator(image)?.Pause();
			UnregisterFromScrollViewer(image);
		}
	}

	private static void Image_ViewportIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
		if (sender is Image image) {
			UpdateAlwaysPlayback(image);
		}
	}

	private static void Image_ViewportSizeChanged(object sender, SizeChangedEventArgs e) {
		if (sender is Image image) {
			UpdateAlwaysPlayback(image);
		}
	}

	private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e) {
		if (sender is not ScrollViewer scrollViewer) {
			return;
		}
		if (!TrackedImages.TryGetValue(scrollViewer, out List<WeakReference<Image>>? list)) {
			return;
		}

		for (int i = list.Count - 1; i >= 0; i--) {
			if (list[i].TryGetTarget(out Image? image)) {
				UpdateAlwaysPlayback(image);
			} else {
				list.RemoveAt(i);
			}
		}

		if (list.Count == 0) {
			scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
			TrackedImages.Remove(scrollViewer);
		}
	}

	private static void TryAttachScrollViewer(Image image) {
		ScrollViewer? found = FindParentScrollViewer(image);
		ScrollViewer? current = image.GetValue(ViewportScrollViewerProperty) as ScrollViewer;
		if (ReferenceEquals(current, found)) {
			return;
		}

		UnregisterFromScrollViewer(image);

		if (found is null) {
			return;
		}

		if (!TrackedImages.TryGetValue(found, out List<WeakReference<Image>>? list)) {
			list = [];
			TrackedImages[found] = list;
			found.ScrollChanged += ScrollViewer_ScrollChanged;
		}

		list.Add(new WeakReference<Image>(image));
		image.SetValue(ViewportScrollViewerProperty, found);
	}

	private static void UnregisterFromScrollViewer(Image image) {
		if (image.GetValue(ViewportScrollViewerProperty) is not ScrollViewer scrollViewer) {
			return;
		}

		image.ClearValue(ViewportScrollViewerProperty);

		if (!TrackedImages.TryGetValue(scrollViewer, out List<WeakReference<Image>>? list)) {
			return;
		}

		for (int i = list.Count - 1; i >= 0; i--) {
			if (!list[i].TryGetTarget(out Image? target) || ReferenceEquals(target, image)) {
				list.RemoveAt(i);
			}
		}

		if (list.Count == 0) {
			scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
			TrackedImages.Remove(scrollViewer);
		}
	}

	private static ScrollViewer? FindParentScrollViewer(DependencyObject? child) {
		while (child != null) {
			if (child is ScrollViewer scrollViewer) {
				return scrollViewer;
			}
			child = VisualTreeHelper.GetParent(child);
		}
		return null;
	}

	private static void UpdateAlwaysPlayback(Image image) {
		if (GetGifAutoPlayType(image) != GifAutoPlayType.Always) {
			return;
		}

		bool inViewport = IsInViewport(image);
		AnimationBehavior.SetAutoStart(image, inViewport);

		Animator? animator = AnimationBehavior.GetAnimator(image);
		if (animator is null) {
			return;
		}

		if (inViewport) {
			animator.Play();
		} else {
			animator.Pause();
		}
	}

	private static bool IsInViewport(FrameworkElement element) {
		if (!element.IsLoaded || !element.IsVisible || element.ActualWidth <= 0 || element.ActualHeight <= 0) {
			return false;
		}

		ScrollViewer? scrollViewer = element.GetValue(ViewportScrollViewerProperty) as ScrollViewer
			?? FindParentScrollViewer(element);
		if (scrollViewer is null) {
			return element.IsVisible;
		}

		try {
			GeneralTransform transform = element.TransformToAncestor(scrollViewer);
			Rect bounds = transform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
			Rect viewport = new(0, 0, scrollViewer.ViewportWidth, scrollViewer.ViewportHeight);
			return viewport.IntersectsWith(bounds);
		} catch (InvalidOperationException) {
			return false;
		}
	}
}
