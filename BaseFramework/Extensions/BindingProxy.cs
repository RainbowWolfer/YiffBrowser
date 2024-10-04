using System.Windows;

namespace BaseFramework.Extensions {
	public class BindingProxy : Freezable {
		protected override Freezable CreateInstanceCore() {
			return new BindingProxy();
		}

		public object Data {
			get => GetValue(DataProperty);
			set => SetValue(DataProperty, value);
		}

		public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
			nameof(Data),
			typeof(object),
			typeof(BindingProxy),
			new UIPropertyMetadata(null)
		);

	}

	public class BooleanProxy : Freezable {
		protected override Freezable CreateInstanceCore() {
			return new BindingProxy();
		}

		public bool Data {
			get => (bool)GetValue(DataProperty);
			set => SetValue(DataProperty, value);
		}

		public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
			nameof(Data),
			typeof(bool),
			typeof(BooleanProxy),
			new UIPropertyMetadata(false)
		);

	}
}
