using System.Windows;
using System.Windows.Controls;

namespace YiffBrowser.BaseFramework.Extensions;

public class ScrollViewerBehavior {

	public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.RegisterAttached(
		"HorizontalOffset",
		typeof(double),
		typeof(ScrollViewerBehavior),
		new PropertyMetadata(0d, OnHorizontalOffsetChanged)
	);

	private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ScrollViewer scrollViewer) {
			scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
		}
	}


	public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached(
		"VerticalOffset",
		typeof(double),
		typeof(ScrollViewerBehavior),
		new PropertyMetadata(0d, OnVerticalOffsetChanged)
	);

	private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ScrollViewer scrollViewer) {
			scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
		}
	}


}
