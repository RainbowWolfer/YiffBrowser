using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BaseFramework.Extensions;

public static class ListBoxItemClickExtension {



	public static bool GetIsPressed(DependencyObject obj) => (bool)obj.GetValue(IsPressedProperty);

	public static void SetIsPressed(DependencyObject obj, bool value) => obj.SetValue(IsPressedProperty, value);

	public static readonly DependencyProperty IsPressedProperty = DependencyProperty.RegisterAttached(
		"IsPressed",
		typeof(bool),
		typeof(ListBoxItemClickExtension),
		new PropertyMetadata(false)
	);




	public static object GetClickCommandParameter(DependencyObject obj) => (object)obj.GetValue(ClickCommandParameterProperty);

	public static void SetClickCommandParameter(DependencyObject obj, object value) => obj.SetValue(ClickCommandParameterProperty, value);

	public static readonly DependencyProperty ClickCommandParameterProperty = DependencyProperty.RegisterAttached(
		"ClickCommandParameter",
		typeof(object),
		typeof(ListBoxItemClickExtension),
		new PropertyMetadata(null)
	);




	public static ICommand GetClickCommand(DependencyObject obj) => (ICommand)obj.GetValue(ClickCommandProperty);

	public static void SetClickCommand(DependencyObject obj, ICommand value) => obj.SetValue(ClickCommandProperty, value);

	public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.RegisterAttached(
		"ClickCommand",
		typeof(ICommand),
		typeof(ListBoxItemClickExtension),
		new PropertyMetadata(null, OnClickCommandChanged)
	);

	private static void OnClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ListBoxItem listBoxItem) {
			listBoxItem.PreviewMouseLeftButtonDown -= ListBoxItem_MouseLeftButtonDown;
			listBoxItem.PreviewMouseLeftButtonUp -= ListBoxItem_MouseLeftButtonUp;
			listBoxItem.PreviewMouseMove -= ListBoxItem_MouseMove;
			listBoxItem.LostMouseCapture -= ListBoxItem_LostMouseCapture;
			if (e.NewValue is ICommand) {
				listBoxItem.PreviewMouseLeftButtonDown += ListBoxItem_MouseLeftButtonDown;
				listBoxItem.PreviewMouseLeftButtonUp += ListBoxItem_MouseLeftButtonUp;
				listBoxItem.PreviewMouseMove += ListBoxItem_MouseMove;
				listBoxItem.LostMouseCapture += ListBoxItem_LostMouseCapture;
			}
		}
	}

	private static void ListBoxItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
		if (sender is ListBoxItem listBoxItem) {
			e.Handled = true;
			listBoxItem.Focus();
			if (e.ButtonState == MouseButtonState.Pressed) {
				listBoxItem.CaptureMouse();
				if (listBoxItem.IsMouseCaptured) {
					if (e.ButtonState == MouseButtonState.Pressed) {
						if (!GetIsPressed(listBoxItem)) {
							SetIsPressed(listBoxItem, true);
						}
					} else {
						listBoxItem.ReleaseMouseCapture();
					}
				}
			}
		}
	}

	private static void ListBoxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
		if (sender is ListBoxItem listBoxItem) {
			e.Handled = true;
			bool num = GetIsPressed(listBoxItem);
			if (listBoxItem.IsMouseCaptured) {
				listBoxItem.ReleaseMouseCapture();
			}
			if (num) {
				ICommand command = GetClickCommand(listBoxItem);
				if (command != null) {
					object parameter = GetClickCommandParameter(listBoxItem);
					if (command.CanExecute(parameter)) {
						command.Execute(parameter);
					}
				}
			}
		}
	}

	private static void ListBoxItem_LostMouseCapture(object sender, MouseEventArgs e) {
		if (sender is ListBoxItem listBoxItem) {
			if (e.OriginalSource == listBoxItem) {
				if (listBoxItem.IsKeyboardFocused && !IsInMainFocusScope(listBoxItem)) {
					Keyboard.Focus(null);
				}
				SetIsPressed(listBoxItem, false);
			}
		}
	}

	private static bool IsInMainFocusScope(DependencyObject dependencyObject) {
		if (FocusManager.GetFocusScope(dependencyObject) is Visual reference) {
			return VisualTreeHelper.GetParent(reference) == null;
		}
		return true;
	}

	private static void ListBoxItem_MouseMove(object sender, MouseEventArgs e) {
		if (sender is ListBoxItem listBoxItem) {
			if (listBoxItem.IsMouseCaptured && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed) {
				Point position = Mouse.PrimaryDevice.GetPosition(listBoxItem);
				bool isPressed = GetIsPressed(listBoxItem);
				if (position.X >= 0.0 && position.X <= listBoxItem.ActualWidth && position.Y >= 0.0 && position.Y <= listBoxItem.ActualHeight) {
					if (!isPressed) {
						SetIsPressed(listBoxItem, true);
					}
				} else if (isPressed) {
					SetIsPressed(listBoxItem, false);
				}
				e.Handled = true;
			}
		}
	}

}
