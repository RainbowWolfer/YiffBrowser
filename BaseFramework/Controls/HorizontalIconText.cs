using System.Windows;
using System.Windows.Controls;

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



	}
}
