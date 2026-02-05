using System.Windows;
using System.Windows.Controls;

namespace BaseFramework.Extensions;

public class ScrollViewerBehavior {
   
	public static readonly DependencyProperty HorizontalOffsetProperty =
		DependencyProperty.RegisterAttached("HorizontalOffset", typeof(double), typeof(ScrollViewerBehavior),
			new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

	private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ScrollViewer sv) sv.ScrollToHorizontalOffset((double)e.NewValue);
	}
}
