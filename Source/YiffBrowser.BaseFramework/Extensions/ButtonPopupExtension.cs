using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using RW.Common.WPF.Controls;

namespace YiffBrowser.BaseFramework.Extensions;

public static class ButtonPopupExtension {

	public static bool GetCloseOnClick(DependencyObject obj) => (bool)obj.GetValue(CloseOnClickProperty);

	public static void SetCloseOnClick(DependencyObject obj, bool value) => obj.SetValue(CloseOnClickProperty, value);

	public static readonly DependencyProperty CloseOnClickProperty = DependencyProperty.RegisterAttached(
		"CloseOnClick",
		typeof(bool),
		typeof(ButtonPopupExtension),
		new PropertyMetadata(false, OnCloseOnClickChanged)
	);

	private static void OnCloseOnClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is not ButtonBase button) {
			return;
		}

		button.Click -= Button_Click;
		if ((bool)e.NewValue) {
			button.Click += Button_Click;
		}
	}

	private static void Button_Click(object sender, RoutedEventArgs e) {
		if (sender is not DependencyObject start) {
			return;
		}

		ButtonPopup? buttonPopup = FindAncestor<ButtonPopup>(start);
		if (buttonPopup != null) {
			buttonPopup.Hide();
			return;
		}

		// Popup content often sits in a separate visual tree; closing the host Popup is enough.
		Popup? popup = FindAncestor<Popup>(start);
		if (popup != null) {
			popup.IsOpen = false;
		}
	}

	private static T? FindAncestor<T>(DependencyObject start) where T : DependencyObject {
		DependencyObject? current = start;
		while (current != null) {
			if (current is T match) {
				return match;
			}

			DependencyObject? parent = LogicalTreeHelper.GetParent(current);
			if (parent == null && current is FrameworkElement fe) {
				parent = fe.Parent;
			}

			parent ??= VisualTreeHelper.GetParent(current);
			current = parent;
		}

		return null;
	}
}
