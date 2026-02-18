using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace YiffBrowser.BaseFramework.Extensions;

public static class ListBoxExtension {
	public static bool GetClickBlankToDeselect(DependencyObject obj) {
		return (bool)obj.GetValue(ClickBlankToDeselectProperty);
	}

	public static void SetClickBlankToDeselect(DependencyObject obj, bool value) {
		obj.SetValue(ClickBlankToDeselectProperty, value);
	}

	public static readonly DependencyProperty ClickBlankToDeselectProperty = DependencyProperty.RegisterAttached(
		"ClickBlankToDeselect",
		typeof(bool),
		typeof(ListBoxExtension),
		new PropertyMetadata(false, OnClickBlankToDeselectChanged)
	);

	private static void OnClickBlankToDeselectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ListBox listBox) {
			if ((bool)e.NewValue == true) {
				listBox.MouseDown += ListBox_MouseDown;
			} else {
				listBox.MouseDown -= ListBox_MouseDown;
			}
		}
	}

	private static void ListBox_MouseDown(object sender, MouseButtonEventArgs e) {
		if (sender is ListBox listBox) {
			listBox.UnselectAll();
			listBox.Focus();
			e.Handled = true;
		}
	}

	public static bool GetEscapeToDeselect(DependencyObject obj) {
		return (bool)obj.GetValue(EscapeToDeselectProperty);
	}

	public static void SetEscapeToDeselect(DependencyObject obj, bool value) {
		obj.SetValue(EscapeToDeselectProperty, value);
	}

	public static readonly DependencyProperty EscapeToDeselectProperty = DependencyProperty.RegisterAttached(
		"EscapeToDeselect",
		typeof(bool),
		typeof(ListBoxExtension),
		new PropertyMetadata(false, OnEscapeToDeselectChanged)
	);

	private static void OnEscapeToDeselectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ListBox listBox) {
			if (e.NewValue is true) {
				listBox.KeyDown += ListBox_KeyDown;
			} else {
				listBox.KeyDown -= ListBox_KeyDown;
			}
		}
	}

	private static void ListBox_KeyDown(object sender, KeyEventArgs e) {
		if (sender is ListBox listBox) {
			if (e.Key == Key.Escape) {
				if (listBox.SelectedItems.IsNotEmpty()) {
					listBox.UnselectAll();
					listBox.Focus();
					e.Handled = true;
				}
			}
		}
	}




	public static bool GetAutoScrollToSelected(DependencyObject obj) => (bool)obj.GetValue(AutoScrollToSelectedProperty);

	public static void SetAutoScrollToSelected(DependencyObject obj, bool value) => obj.SetValue(AutoScrollToSelectedProperty, value);

	public static readonly DependencyProperty AutoScrollToSelectedProperty = DependencyProperty.RegisterAttached(
		"AutoScrollToSelected",
		typeof(bool),
		typeof(ListBoxExtension),
		new PropertyMetadata(false, OnAutoScrollToSelectedChanged)
	);

	private static void OnAutoScrollToSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ListBox listBox) {
			bool newValue = (bool)e.NewValue;

			listBox.SelectionChanged -= ListBox_SelectionChanged;
			listBox.Loaded -= ListBox_Loaded;

			if (newValue) {
				listBox.SelectionChanged += ListBox_SelectionChanged;
				listBox.Loaded += ListBox_Loaded;

				if (listBox.SelectedItem != null) {
					listBox.ScrollIntoView(listBox.SelectedItem);
				}
			}
		}
	}

	private static void ListBox_Loaded(object sender, RoutedEventArgs e) {
		if (sender is ListBox listBox) {
			if (listBox.SelectedItem != null) {
				listBox.ScrollIntoView(listBox.SelectedItem);
			}
		}
	}

	private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
		if (sender is ListBox listBox) {
			if (listBox.SelectedItem != null) {
				listBox.ScrollIntoView(listBox.SelectedItem);
			}
		}
	}



	public static readonly DependencyProperty SelectOnMouseUpProperty = DependencyProperty.RegisterAttached(
		"SelectOnMouseUp",
		typeof(bool),
		typeof(ListBoxExtension),
		new PropertyMetadata(false, OnSelectOnMouseUpChanged)
	);

	public static void SetSelectOnMouseUp(DependencyObject obj, bool value) => obj.SetValue(SelectOnMouseUpProperty, value);

	public static bool GetSelectOnMouseUp(DependencyObject obj) => (bool)obj.GetValue(SelectOnMouseUpProperty);

	private static void OnSelectOnMouseUpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is not ListBox listBox) {
			return;
		}

		listBox.PreviewMouseLeftButtonDown -= OnMouseDown;
		listBox.PreviewMouseLeftButtonUp -= OnMouseUp;
		listBox.MouseLeave -= OnMouseLeave;
		listBox.LostMouseCapture -= ListBox_LostMouseCapture;
		if (e.NewValue is true) {
			listBox.PreviewMouseLeftButtonDown += OnMouseDown;
			listBox.PreviewMouseLeftButtonUp += OnMouseUp;
			listBox.MouseLeave += OnMouseLeave;
			listBox.LostMouseCapture += ListBox_LostMouseCapture;
		}

	}

	// 记录按下时的 item
	private static readonly Dictionary<ListBox, ListBoxItem?> PressedItemMap = new();

	private static void OnMouseDown(object sender, MouseButtonEventArgs e) {
		if (sender is not ListBox listBox) {
			return;
		}

		// 找到按下时的 item
		ListBoxItem? item = FindItemUnderMouse(listBox, e);

		PressedItemMap[listBox] = item;

		// 捕获鼠标（像 Button 一样）
		listBox.CaptureMouse();

		// 阻止默认的“按下即选中”
		e.Handled = true;
	}

	private static void OnMouseUp(object sender, MouseButtonEventArgs e) {
		if (sender is not ListBox listBox) {
			return;
		}

		listBox.ReleaseMouseCapture();

		ListBoxItem? pressedItem = PressedItemMap.GetValueOrDefault(listBox);
		ListBoxItem? releasedItem = FindItemUnderMouse(listBox, e);

		// 必须按下和抬起都在同一个 item 上才算点击
		if (pressedItem != null && pressedItem == releasedItem) {
			listBox.SelectedItem = pressedItem.DataContext;
			pressedItem.Focus();
		}

		PressedItemMap[listBox] = null;
		e.Handled = true;
	}

	private static void ListBox_LostMouseCapture(object sender, MouseEventArgs e) {
		if (sender is not ListBox listBox) {
			return;
		}
		PressedItemMap[listBox] = null;
	}

	private static void OnMouseLeave(object sender, MouseEventArgs e) {
		if (sender is not ListBox listBox) {
			return;
		}

		// 鼠标离开 ListBox 时取消捕获
		if (listBox.IsMouseCaptured) {
			listBox.ReleaseMouseCapture();
		}
	}

	private static ListBoxItem? FindItemUnderMouse(ListBox listBox, MouseEventArgs e) {
		DependencyObject? hit = VisualTreeHelper.HitTest(listBox, e.GetPosition(listBox))?.VisualHit;

		while (hit != null && hit is not ListBoxItem) {
			hit = VisualTreeHelper.GetParent(hit);
		}

		return hit as ListBoxItem;
	}
}
