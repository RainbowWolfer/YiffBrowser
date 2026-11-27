using System.Windows;
using System.Windows.Controls;

namespace YiffBrowser.E621.Extensions;

internal static class E621MainViewExtension {
	public static bool? GetDockPanelShow(DependencyObject obj) => (bool?)obj.GetValue(DockPanelShowProperty);
	public static void SetDockPanelShow(DependencyObject obj, bool? value) => obj.SetValue(DockPanelShowProperty, value);
	public static readonly DependencyProperty DockPanelShowProperty = DependencyProperty.RegisterAttached(
		"DockPanelShow",
		typeof(bool?),
		typeof(E621MainViewExtension),
		new PropertyMetadata(null, OnDockPanelShowChanged)
	);
	private static void OnDockPanelShowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		Update(d);
	}


	public static bool GetIsDockPanelCollapsed(DependencyObject obj) => (bool)obj.GetValue(IsDockPanelCollapsedProperty);
	public static void SetIsDockPanelCollapsed(DependencyObject obj, bool value) => obj.SetValue(IsDockPanelCollapsedProperty, value);
	public static readonly DependencyProperty IsDockPanelCollapsedProperty = DependencyProperty.RegisterAttached(
		"IsDockPanelCollapsed",
		typeof(bool),
		typeof(E621MainViewExtension),
		new PropertyMetadata(false, OnIsDockPanelCollapsedChanged)
	);
	private static void OnIsDockPanelCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		Update(d);
	}

	private static void Update(DependencyObject d) {

		if (d is RowDefinition rowDefinition) {
			if (GetDockPanelShow(d) is true) {
				if (GetIsDockPanelCollapsed(d) is true) {
					rowDefinition.MinHeight = 0;
					rowDefinition.Height = new GridLength(1, GridUnitType.Auto);
				} else {
					rowDefinition.MinHeight = 100;
					rowDefinition.Height = new GridLength(300, GridUnitType.Pixel);
				}
			} else {
				rowDefinition.MinHeight = 0;
				rowDefinition.Height = new GridLength(0, GridUnitType.Pixel);
			}
		}
	}
}
