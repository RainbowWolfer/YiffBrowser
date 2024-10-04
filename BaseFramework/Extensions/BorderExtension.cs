using System.Windows;

namespace BaseFramework.Extensions {
	public static class BorderExtension {


		public static CornerRadius GetCornerRadius(DependencyObject obj) {
			return (CornerRadius)obj.GetValue(CornerRadiusProperty);
		}

		public static void SetCornerRadius(DependencyObject obj, CornerRadius value) {
			obj.SetValue(CornerRadiusProperty, value);
		}

		public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
			"CornerRadius",
			typeof(CornerRadius),
			typeof(BorderExtension),
			new PropertyMetadata(new CornerRadius())
		);


	}
}
