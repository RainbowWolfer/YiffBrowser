namespace BaseFramework.Events {
	public class PropertyValueUpdatedEventArgs(object oldValue, object newValue) : EventArgs {
		public object OldValue { get; } = oldValue;
		public object NewValue { get; } = newValue;
	}

	public class PropertyValueUpdatedEventArgs<T>(T oldValue, T newValue) : EventArgs {
		public T OldValue { get; } = oldValue;
		public T NewValue { get; } = newValue;
	}
}
