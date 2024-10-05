using System.Windows;
using System.Windows.Controls;

namespace BaseFramework.Controls {
	public class BoolContent : Control {

		public object TrueContent {
			get => GetValue(TrueContentProperty);
			set => SetValue(TrueContentProperty, value);
		}

		public object FalseContent {
			get => GetValue(FalseContentProperty);
			set => SetValue(FalseContentProperty, value);
		}

		public bool Bool {
			get => (bool)GetValue(BoolProperty);
			set => SetValue(BoolProperty, value);
		}

		public static readonly DependencyProperty TrueContentProperty = DependencyProperty.Register(
			nameof(TrueContent),
			typeof(object),
			typeof(BoolContent),
			new PropertyMetadata(null)
		);

		public static readonly DependencyProperty FalseContentProperty = DependencyProperty.Register(
			nameof(FalseContent),
			typeof(object),
			typeof(BoolContent),
			new PropertyMetadata(null)
		);

		public static readonly DependencyProperty BoolProperty = DependencyProperty.Register(
			nameof(Bool),
			typeof(bool),
			typeof(BoolContent),
			new PropertyMetadata(false)
		);




	}
}
