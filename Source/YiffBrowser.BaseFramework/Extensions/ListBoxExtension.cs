using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

			if (newValue) {
				listBox.SelectionChanged += ListBox_SelectionChanged;

				if (listBox.SelectedItem != null) {
					listBox.ScrollIntoView(listBox.SelectedItem);
				}
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
}
