using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace BaseFramework.Controls {
	public class ButtonPopup : Popup {
		public bool HandleEscapeInPreviewEvent {
			get => (bool)GetValue(HandleEscapeInPreviewEventProperty);
			set => SetValue(HandleEscapeInPreviewEventProperty, value);
		}

		public static readonly DependencyProperty HandleEscapeInPreviewEventProperty = DependencyProperty.Register(
			nameof(HandleEscapeInPreviewEvent),
			typeof(bool),
			typeof(ButtonPopup),
			new PropertyMetadata(false)
		);


		public UIElement FocusObjectOnOpened {
			get => (UIElement)GetValue(FocusObjectOnOpenedProperty);
			set => SetValue(FocusObjectOnOpenedProperty, value);
		}

		public static readonly DependencyProperty FocusObjectOnOpenedProperty = DependencyProperty.Register(
			nameof(FocusObjectOnOpened),
			typeof(UIElement),
			typeof(ButtonPopup),
			new PropertyMetadata(null)
		);


		public ButtonBase SourceButton {
			get => (ButtonBase)GetValue(SourceButtonProperty);
			set => SetValue(SourceButtonProperty, value);
		}

		public static readonly DependencyProperty SourceButtonProperty = DependencyProperty.Register(
			nameof(SourceButton),
			typeof(ButtonBase),
			typeof(ButtonPopup),
			new PropertyMetadata(null, OnSourceButtonChanged)
		);

		private static void OnSourceButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((ButtonPopup)d).UpdateSourceButton((ButtonBase)e.OldValue, (ButtonBase)e.NewValue);
		}

		private void UpdateSourceButton(ButtonBase oldValue, ButtonBase newValue) {
			PlacementTarget ??= newValue;

			if (oldValue != null) {
				oldValue.Click -= Button_Click;
			}

			if (newValue != null) {
				newValue.Click += Button_Click;
			}

		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			Show();
			OnClickOpenCommand?.Execute(this);
		}

		public ICommand OnClickOpenCommand {
			get => (ICommand)GetValue(OnClickOpenCommandProperty);
			set => SetValue(OnClickOpenCommandProperty, value);
		}

		public static readonly DependencyProperty OnClickOpenCommandProperty = DependencyProperty.Register(
			nameof(OnClickOpenCommand),
			typeof(ICommand),
			typeof(ButtonPopup),
			new PropertyMetadata(null)
		);



		public ButtonPopup() {
			StaysOpen = false;
			AllowsTransparency = true;
			//Focusable = true;
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e) {
			base.OnPreviewKeyDown(e);
			if (HandleEscapeInPreviewEvent) {
				if (e.Key == Key.Escape) {
					e.Handled = true;
					Hide();
				}
			}
		}

		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown(e);
			if (!HandleEscapeInPreviewEvent) {
				if (e.Key == Key.Escape) {
					e.Handled = true;
					Hide();
				}
			}
		}

		public void Show() {
			IsOpen = true;
		}

		public void Hide() {
			IsOpen = false;
		}

		protected override void OnGotFocus(RoutedEventArgs e) {
			base.OnGotFocus(e);
		}

		protected override void OnOpened(EventArgs e) {
			base.OnOpened(e);
			//if (FocusObjectOnOpened == null) {
			//	Child.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
			//} else {
			//	FocusObjectOnOpened.Focus();
			//	Keyboard.Focus(FocusObjectOnOpened);
			//}
		}


	}
}
