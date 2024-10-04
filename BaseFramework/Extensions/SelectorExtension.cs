using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BaseFramework.Extensions {
	public static class SelectorExtension {

		public static ICommand GetSelectionChangedCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(SelectionChangedCommandProperty);
		}

		public static void SetSelectionChangedCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(SelectionChangedCommandProperty, value);
		}

		public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.RegisterAttached(
			"SelectionChangedCommand",
			typeof(ICommand),
			typeof(SelectorExtension),
			new PropertyMetadata(null, OnSelectionChangedCommandChanged)
		);

		private static void OnSelectionChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Selector selector) {
				if (e.OldValue is ICommand) {
					selector.SelectionChanged -= Selector_SelectionChanged;
				}
				if (e.NewValue is ICommand) {
					selector.SelectionChanged += Selector_SelectionChanged;
				}
			}
		}

		private static void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			GetSelectionChangedCommand((DependencyObject)sender)?.Execute(sender);
		}




		public static IList GetSelectedItems(DependencyObject obj) {
			return (IList)obj.GetValue(SelectedItemsProperty);
		}

		public static void SetSelectedItems(DependencyObject obj, IList value) {
			obj.SetValue(SelectedItemsProperty, value);
		}

		public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.RegisterAttached(
			"SelectedItems",
			typeof(IList),
			typeof(SelectorExtension),
			new PropertyMetadata(null, OnSelectedItemsChanged)
		);

		private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Selector selector) {
				if (e.OldValue is IList) {
					selector.SelectionChanged -= Selector_SelectionChanged1;
				}
				if (e.NewValue is IList) {
					selector.SelectionChanged += Selector_SelectionChanged1;
				}
			}
		}

		private static void Selector_SelectionChanged1(object sender, SelectionChangedEventArgs e) {
			IList list = GetSelectedItems((DependencyObject)sender);
			if (sender is MultiSelector multiSelector) {
				list.Clear();
				foreach (object? item in multiSelector.Items) {
					list.Add(item);
				}
			} else if (sender is Selector selector) {
				list.Clear();
				list.Add(selector.SelectedItem);
			}
		}
	}
}
