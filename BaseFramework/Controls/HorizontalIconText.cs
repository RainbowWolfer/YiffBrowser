using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BaseFramework.Controls {
	public class HorizontalIconText : Control {

		public string Glyph {
			get => (string)GetValue(GlyphProperty);
			set => SetValue(GlyphProperty, value);
		}

		public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
			nameof(Glyph),
			typeof(string),
			typeof(HorizontalIconText),
			new PropertyMetadata(string.Empty)
		);



		public string Text {
			get => (string)GetValue(TextProperty);
			set => SetValue(TextProperty, value);
		}

		public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
			nameof(Text),
			typeof(string),
			typeof(HorizontalIconText),
			new PropertyMetadata(string.Empty)
		);




		public double IconSize {
			get => (double)GetValue(IconSizeProperty);
			set => SetValue(IconSizeProperty, value);
		}

		public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
			nameof(IconSize),
			typeof(double),
			typeof(HorizontalIconText),
			new PropertyMetadata(16d)
		);



		public Brush IconForeground {
			get => (Brush)GetValue(IconForegroundProperty);
			set => SetValue(IconForegroundProperty, value);
		}

		public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(
			nameof(IconForeground),
			typeof(Brush),
			typeof(HorizontalIconText),
			new PropertyMetadata(Brushes.Black)
		);




	}
}
