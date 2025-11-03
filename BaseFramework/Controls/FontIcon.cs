using System.Windows;
using System.Windows.Controls;

namespace BaseFramework.Controls;
public class FontIcon : TextBlock {
	static FontIcon() {
		DefaultStyleKeyProperty.OverrideMetadata(typeof(FontIcon), new FrameworkPropertyMetadata(typeof(FontIcon)));
	}
}
