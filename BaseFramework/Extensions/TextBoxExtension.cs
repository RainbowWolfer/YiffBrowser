using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BaseFramework.Extensions;

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

	public static readonly DependencyProperty ReplaceNewlineOnPasteProperty = DependencyProperty.RegisterAttached(
		"ReplaceNewlineOnPaste",
		typeof(bool),
		typeof(TextBoxExtension),
		new PropertyMetadata(false, OnChanged)
	);

	public static void SetReplaceNewlineOnPaste(DependencyObject obj, bool value) => obj.SetValue(ReplaceNewlineOnPasteProperty, value);
	public static bool GetReplaceNewlineOnPaste(DependencyObject obj) => (bool)obj.GetValue(ReplaceNewlineOnPasteProperty);

	private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is TextBox tb) {
			if ((bool)e.NewValue) {
				DataObject.AddPastingHandler(tb, OnPasting);
			} else {
				DataObject.RemovePastingHandler(tb, OnPasting);
			}
		}
	}

	private static void OnPasting(object sender, DataObjectPastingEventArgs e) {
		if (e.DataObject.GetDataPresent(DataFormats.Text)) {
			string text = (string)e.DataObject.GetData(DataFormats.Text);
			string cleanedText = text.ReplaceLineEndings(" ");
			DataObject newObject = new();
			newObject.SetText(cleanedText);
			e.DataObject = newObject;
		}
	}
}
