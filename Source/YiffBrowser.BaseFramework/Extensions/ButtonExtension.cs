using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace YiffBrowser.BaseFramework.Extensions;

public static class ButtonExtension {

	private static readonly MethodInfo OnClickMethod = typeof(ButtonBase).GetMethod("OnClick", BindingFlags.Instance | BindingFlags.NonPublic)!;

	static ButtonExtension() {

	}

	public static bool GetSetPreviewClick(DependencyObject obj) => (bool)obj.GetValue(SetPreviewClickProperty);

	public static void SetSetPreviewClick(DependencyObject obj, bool value) => obj.SetValue(SetPreviewClickProperty, value);

	public static readonly DependencyProperty SetPreviewClickProperty = DependencyProperty.RegisterAttached(
		"SetPreviewClick",
		typeof(bool),
		typeof(ButtonExtension),
		new PropertyMetadata(false, OnSetPreviewClickChanged)
	);

	private static void OnSetPreviewClickChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ButtonBase button) {
			button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
			button.PreviewMouseRightButtonUp += Button_PreviewMouseRightButtonUp;
		}
	}

	private static void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
		if (sender is ButtonBase button) {
			e.Handled = true;
			button.Focus();
			if (e.ButtonState == MouseButtonState.Pressed) {
				button.CaptureMouse();
				if (button.IsMouseCaptured) {
					if (e.ButtonState == MouseButtonState.Pressed) {
						if (!button.IsPressed) {
							button.SetValue(ButtonBase.IsPressedProperty, true);
						}
					} else {
						button.ReleaseMouseCapture();
					}
				}
			}
		}
	}

	private static void Button_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e) {
		e.Handled = true;
		if (sender is ButtonBase button) {
			bool num = button.IsPressed;
			if (button.IsMouseCaptured) {
				button.ReleaseMouseCapture();
			}
			if (num) {
				OnClickMethod.Invoke(button, null);
			}
		}
	}

}
