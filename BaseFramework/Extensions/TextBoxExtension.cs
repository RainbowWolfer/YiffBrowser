using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BaseFramework.Extensions {
	public static class TextBoxExtension {

		public static ICommand GetSelectionChangedCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(SelectionChangedCommandProperty);
		}

		public static void SetSelectionChangedCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(SelectionChangedCommandProperty, value);
		}

		public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.RegisterAttached(
			"SelectionChangedCommand",
			typeof(ICommand),
			typeof(TextBoxExtension),
			new PropertyMetadata(null, OnSelectionChangedCommandChanged)
		);

		private static void OnSelectionChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is TextBoxBase textBoxBase) {
				if (e.OldValue is ICommand) {
					textBoxBase.SelectionChanged -= TextBoxBase_SelectionChanged;
				}
				if (e.NewValue is ICommand) {
					textBoxBase.SelectionChanged += TextBoxBase_SelectionChanged;
				}
			}
		}

		private static void TextBoxBase_SelectionChanged(object sender, RoutedEventArgs e) {
			GetSelectionChangedCommand((DependencyObject)sender)?.Execute(e);
		}

	}
}
