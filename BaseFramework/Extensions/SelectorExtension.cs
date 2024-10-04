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

	}
}
