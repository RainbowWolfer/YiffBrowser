using BaseFramework.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace YB.E621.Controls {
	public partial class SourceHyperLinkButton : UserControl {

		public string? URL {
			get => (string)GetValue(URLProperty);
			set => SetValue(URLProperty, value);
		}

		public static readonly DependencyProperty URLProperty = DependencyProperty.Register(
			nameof(URL),
			typeof(string),
			typeof(SourceHyperLinkButton),
			new PropertyMetadata(string.Empty)
		);

		public SourceHyperLinkButton() {
			InitializeComponent();
		}

		private void MainButton_Click(object sender, RoutedEventArgs e) {
			URL.OpenInBrowser();
		}

		private void CopyItem_Click(object sender, RoutedEventArgs e) {
			URL.CopyToClipboard();
		}
	}
}
