using BaseFramework.Events;

namespace BaseFramework.Models {
	public sealed class BindObject<T> : BindableBase {
		public event TypedEventHandler<BindObject<T>, PropertyValueUpdatedEventArgs<T?>>? ValueChanged;

		private T? value;

		public T? Value {
			get => value;
			set {
				T? oldValue = this.value;
				SetProperty(ref this.value, value);
				ValueChanged?.Invoke(this, new PropertyValueUpdatedEventArgs<T?>(oldValue, value));
			}
		}

		public BindObject(T? defaultValue = default) {
			Value = defaultValue;
		}

		public void Set(T value) {
			Value = value;
		}

		public static implicit operator BindObject<T>(T value) {
			return new BindObject<T>(value);
		}

		public static implicit operator T(BindObject<T> value) {
			return value.Value;
		}



	}
}
