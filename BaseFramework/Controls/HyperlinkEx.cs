using BaseFramework.Helpers;
using System.Windows;
using System.Windows.Documents;

namespace BaseFramework.Controls;

public class HyperlinkEx : Hyperlink {

	public string NavigateString {
		get => (string)GetValue(NavigateStringProperty);
		set => SetValue(NavigateStringProperty, value);
	}

	public static readonly DependencyProperty NavigateStringProperty = DependencyProperty.Register(
		nameof(NavigateString),
		typeof(string),
		typeof(HyperlinkEx),
		new PropertyMetadata(string.Empty, OnNavigateStringChanged)
	);

	private static void OnNavigateStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is HyperlinkEx self) {
			self.ToolTip = e.NewValue;
		}
	}

	protected override void OnClick() {
		//base.OnClick();
		NavigateString.OpenInBrowser();
	}
}
