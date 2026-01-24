using BaseFramework.Controls;
using System.Diagnostics;

namespace YiffBrowser;

public partial class TestWindow : WindowBase {
	public TestWindow() {
		InitializeComponent();
	}

	private void SliderEx_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e) {
		Debug.WriteLine(e.NewValue);
	}
}
