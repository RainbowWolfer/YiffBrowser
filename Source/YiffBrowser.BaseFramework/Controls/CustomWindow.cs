using System.Windows;
using System.Windows.Input;

namespace YiffBrowser.BaseFramework.Controls;

public class CustomWindow : Window {
	static CustomWindow() {
		DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata(typeof(CustomWindow)));
	}

	public CustomWindow() {
	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

	}

}
