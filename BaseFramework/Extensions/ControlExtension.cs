using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BaseFramework.Extensions {
	public static class ControlExtension {

		public static ICommand GetLoadedOnceCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(LoadedOnceCommandProperty);
		}

		public static void SetLoadedOnceCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(LoadedOnceCommandProperty, value);
		}

		public static readonly DependencyProperty LoadedOnceCommandProperty = DependencyProperty.RegisterAttached(
			"LoadedOnceCommand",
			typeof(ICommand),
			typeof(ControlExtension),
			new PropertyMetadata(null, OnLoadedOnceCommandChanged)
		);

		private static void OnLoadedOnceCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is FrameworkElement frameworkElement) {
				if (e.NewValue is ICommand newCommand) {
					frameworkElement.Loaded += FrameworkElement_Loaded;
				}
			}
		}

		private static void FrameworkElement_Loaded(object sender, RoutedEventArgs e) {
			FrameworkElement frameworkElement = (FrameworkElement)sender;
			frameworkElement.Loaded -= FrameworkElement_Loaded;
			GetLoadedOnceCommand(frameworkElement)?.Execute(frameworkElement);
		}



		public static ICommand GetLoadedCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(LoadedCommandProperty);
		}

		public static void SetLoadedCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(LoadedCommandProperty, value);
		}

		public static readonly DependencyProperty LoadedCommandProperty = DependencyProperty.RegisterAttached(
			"LoadedCommand",
			typeof(ICommand),
			typeof(ControlExtension),
			new PropertyMetadata(null, OnLoadedCommandChanged)
		);

		private static void OnLoadedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is FrameworkElement frameworkElement) {
				if (e.OldValue is ICommand) {
					frameworkElement.Loaded -= FrameworkElement_Loaded2;
				}
				if (e.NewValue is ICommand newCommand) {
					frameworkElement.Loaded += FrameworkElement_Loaded2;
				}
			}
		}

		private static void FrameworkElement_Loaded2(object sender, RoutedEventArgs e) {
			FrameworkElement frameworkElement = (FrameworkElement)sender;
			GetLoadedCommand(frameworkElement)?.Execute(frameworkElement);
		}

		public static ICommand GetDoubleClickCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(DoubleClickCommandProperty);
		}

		public static void SetDoubleClickCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(DoubleClickCommandProperty, value);
		}

		public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.RegisterAttached(
			"DoubleClickCommand",
			typeof(ICommand),
			typeof(ControlExtension),
			new PropertyMetadata(null, OnDoubleClickCommandChanged)
		);

		private static void OnDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Control control) {
				if (e.NewValue is ICommand command) {
					control.MouseDoubleClick += Control_MouseDoubleClick;
				} else {
					control.MouseDoubleClick -= Control_MouseDoubleClick;
				}
			}
		}

		private static void Control_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton != MouseButton.Left) {
				return;
			}
			object args = GetDoubleClickCommandParameter((DependencyObject)sender) ?? e;
			GetDoubleClickCommand((DependencyObject)sender)?.Execute(args);
			e.Handled = true;
		}



		public static object GetDoubleClickCommandParameter(DependencyObject obj) {
			return obj.GetValue(DoubleClickCommandParameterProperty);
		}

		public static void SetDoubleClickCommandParameter(DependencyObject obj, object value) {
			obj.SetValue(DoubleClickCommandParameterProperty, value);
		}

		public static readonly DependencyProperty DoubleClickCommandParameterProperty = DependencyProperty.RegisterAttached(
			"DoubleClickCommandParameter",
			typeof(object),
			typeof(ControlExtension),
			new PropertyMetadata(null)
		);


		public static ICommand GetPreviewDoubleClickCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(PreviewDoubleClickCommandProperty);
		}

		public static void SetPreviewDoubleClickCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(PreviewDoubleClickCommandProperty, value);
		}

		public static readonly DependencyProperty PreviewDoubleClickCommandProperty = DependencyProperty.RegisterAttached(
			"PreviewDoubleClickCommand",
			typeof(ICommand),
			typeof(ControlExtension),
			new PropertyMetadata(null, OnPreviewDoubleClickCommandChanged)
		);

		private static void OnPreviewDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is Control control) {
				if (e.NewValue is ICommand command) {
					control.PreviewMouseDoubleClick += Control_PreviewMouseDoubleClick;
				} else {
					control.PreviewMouseDoubleClick -= Control_PreviewMouseDoubleClick;
				}
			}
		}

		private static void Control_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e) {
			if (e.ChangedButton != MouseButton.Left) {
				return;
			}
			GetPreviewDoubleClickCommand((DependencyObject)sender)?.Execute(e);
		}

		public static ICommand GetDropCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(DropCommandProperty);
		}

		public static void SetDropCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(DropCommandProperty, value);
		}

		public static readonly DependencyProperty DropCommandProperty = DependencyProperty.RegisterAttached(
			"DropCommand",
			typeof(ICommand),
			typeof(ControlExtension),
			new PropertyMetadata(null, OnDropCommandChanged)
		);

		private static void OnDropCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is UIElement control) {
				if (e.NewValue is ICommand command) {
					control.Drop += Control_Drop;
				} else {
					control.Drop -= Control_Drop;
				}
			}
		}

		private static void Control_Drop(object sender, DragEventArgs e) {
			GetDropCommand((DependencyObject)sender)?.Execute(e);
		}







		public static ICommand GetMouseDownCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(MouseDownCommandProperty);
		}

		public static void SetMouseDownCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(MouseDownCommandProperty, value);
		}

		public static readonly DependencyProperty MouseDownCommandProperty = DependencyProperty.RegisterAttached(
			"MouseDownCommand",
			typeof(ICommand),
			typeof(ControlExtension),
			new PropertyMetadata(null, OnMouseDownCommandChanged)
		);

		private static void OnMouseDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is UIElement control) {
				if (e.NewValue is ICommand command) {
					control.MouseDown += Control_MouseDown;
				} else {
					control.MouseDown -= Control_MouseDown;
				}
			}
		}

		private static void Control_MouseDown(object sender, MouseButtonEventArgs e) {
			GetMouseDownCommand((DependencyObject)sender)?.Execute(e);
		}





		public static ICommand GetPreviewKeyDownCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(PreviewKeyDownCommandProperty);
		}

		public static void SetPreviewKeyDownCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(PreviewKeyDownCommandProperty, value);
		}

		public static readonly DependencyProperty PreviewKeyDownCommandProperty = DependencyProperty.RegisterAttached(
			"PreviewKeyDownCommand",
			typeof(ICommand),
			typeof(ControlExtension),
			new PropertyMetadata(null, OnPreviewKeyDownCommandChanged)
		);

		private static void OnPreviewKeyDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is UIElement control) {
				if (e.OldValue is ICommand) {
					control.PreviewKeyDown -= Control_PreviewKeyDown;
				}
				if (e.NewValue is ICommand) {
					control.PreviewKeyDown += Control_PreviewKeyDown;
				}
			}
		}

		private static void Control_PreviewKeyDown(object sender, KeyEventArgs e) {
			GetPreviewKeyDownCommand((DependencyObject)sender)?.Execute(e);
		}

		public static ICommand GetKeyDownCommand(DependencyObject obj) {
			return (ICommand)obj.GetValue(KeyDownCommandProperty);
		}

		public static void SetKeyDownCommand(DependencyObject obj, ICommand value) {
			obj.SetValue(KeyDownCommandProperty, value);
		}

		public static readonly DependencyProperty KeyDownCommandProperty = DependencyProperty.RegisterAttached(
			"KeyDownCommand",
			typeof(ICommand),
			typeof(ControlExtension),
			new PropertyMetadata(null, OnKeyDownCommandChanged)
		);

		private static void OnKeyDownCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is UIElement control) {
				if (e.NewValue is ICommand command) {
					control.KeyDown += Control_KeyDown;
				} else {
					control.KeyDown -= Control_KeyDown;
				}
			}
		}

		private static void Control_KeyDown(object sender, KeyEventArgs e) {
			//object parameter = GetKeyDownCommandParameter((DependencyObject)sender);
			GetKeyDownCommand((DependencyObject)sender)?.Execute(/*parameter ?? */e);
		}

		//public static object GetKeyDownCommandParameter(DependencyObject obj) {
		//	return obj.GetValue(KeyDownCommandParameterProperty);
		//}

		//public static void SetKeyDownCommandParameter(DependencyObject obj, object value) {
		//	obj.SetValue(KeyDownCommandParameterProperty, value);
		//}

		//public static readonly DependencyProperty KeyDownCommandParameterProperty = DependencyProperty.RegisterAttached(
		//	"KeyDownCommandParameter",
		//	typeof(object),
		//	typeof(ControlExtension),
		//	new PropertyMetadata(null)
		//);

	}
}
