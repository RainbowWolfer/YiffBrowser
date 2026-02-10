using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YiffBrowser.BaseFramework.Controls;

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



	public double Spacing {
		get => (double)GetValue(SpacingProperty);
		set => SetValue(SpacingProperty, value);
	}

	public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
		nameof(Spacing),
		typeof(double),
		typeof(HorizontalIconText),
		new PropertyMetadata(4d)
	);




	public TextAlignment TextAlignment {
		get => (TextAlignment)GetValue(TextAlignmentProperty);
		set => SetValue(TextAlignmentProperty, value);
	}

	public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register(
		nameof(TextAlignment),
		typeof(TextAlignment),
		typeof(HorizontalIconText),
		new PropertyMetadata(TextAlignment.Left)
	);



}
