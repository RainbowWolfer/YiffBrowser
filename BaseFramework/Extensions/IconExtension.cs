using BaseFramework.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BaseFramework.Extensions {
	public static class IconExtension {

		public static string GetGlyph(DependencyObject obj) {
			return (string)obj.GetValue(GlyphProperty);
		}

		public static void SetGlyph(DependencyObject obj, string value) {
			obj.SetValue(GlyphProperty, value);
		}

		public static readonly DependencyProperty GlyphProperty = DependencyProperty.RegisterAttached(
			"Glyph",
			typeof(string),
			typeof(IconExtension),
			new PropertyMetadata(null, OnGlyphChanged)
		);

		private static void OnGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			FontIcon? text = GetTextBlock(d);
			if (text == null) {
				return;
			}

			text.Text = e.NewValue?.ToString();

		}

		public static Brush GetForeground(DependencyObject obj) {
			return (Brush)obj.GetValue(ForegroundProperty);
		}

		public static void SetForeground(DependencyObject obj, Brush value) {
			obj.SetValue(ForegroundProperty, value);
		}

		public static readonly DependencyProperty ForegroundProperty = DependencyProperty.RegisterAttached(
			"Foreground",
			typeof(Brush),
			typeof(IconExtension),
			new PropertyMetadata(Brushes.Black, OnForegroundChanged)
		);

		private static void OnForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			FontIcon? text = GetTextBlock(d);
			if (text == null) {
				return;
			}

			text.Foreground = (Brush)e.NewValue;
		}

		private static FontIcon? GetTextBlock(DependencyObject obj) {
			if (obj is MenuItem item) {
				if (item.Icon is FontIcon fontIcon) {
					return fontIcon;
				} else {
					FontIcon tb = new();

					item.Icon = tb;

					return tb;
				}
			}

			return null;
		}
	}
}
