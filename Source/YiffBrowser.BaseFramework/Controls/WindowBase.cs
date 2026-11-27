using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using YiffBrowser.BaseFramework.Extensions;
using YiffBrowser.BaseFramework.ViewModels;

namespace YiffBrowser.BaseFramework.Controls;

public class WindowBase : Window {

	public nint WindowHandle { get; private set; }
	public HwndSource? HwndSource { get; private set; }

	public Window? CenterToOwner {
		get => (Window?)GetValue(CenterToOwnerProperty);
		set => SetValue(CenterToOwnerProperty, value);
	}

	public static readonly DependencyProperty CenterToOwnerProperty = DependencyProperty.Register(
		nameof(CenterToOwner),
		typeof(Window),
		typeof(WindowBase),
		new PropertyMetadata(null)
	);

	public ResizeMode? MyResizeMode { get; set; }

	public DialogCaptionButtons CaptionButtons { get; set; }

	protected override void OnMouseDown(MouseButtonEventArgs e) {
		base.OnMouseDown(e);

		if (Content is FrameworkElement frameworkElement) {
			frameworkElement.Focus();
		}
	}

	protected override void OnSourceInitialized(EventArgs e) {
		base.OnSourceInitialized(e);

		HwndSource = PresentationSource.FromVisual(this) as HwndSource;
		WindowHandle = HwndSource?.Handle ?? new WindowInteropHelper(this).EnsureHandle();
		WindowFrameTheme.Apply(WindowHandle, WindowFrameTheme.IsDark());

		if (CenterToOwner != null) {
			double ownerLeft = CenterToOwner.Left;
			double ownerTop = CenterToOwner.Top;
			double ownerWidth = CenterToOwner.ActualWidth;
			double ownerHeight = CenterToOwner.ActualHeight;

			Left = ownerLeft + ((ownerWidth - ActualWidth) / 2);
			Top = ownerTop + ((ownerHeight - ActualHeight) / 2);
		}

		if (MyResizeMode != null) {
			ResizeMode = MyResizeMode.Value;
		}

		// Setting ResizeMode puts the minimize and maximize buttons back.
		switch (CaptionButtons) {
			case DialogCaptionButtons.HideMinimize:
				this.HideMinimizeButton();
				break;
			case DialogCaptionButtons.HideMinimizeAndMaximize:
				this.HideMinimizeAndMaximizeButtons();
				break;
		}
	}

}
