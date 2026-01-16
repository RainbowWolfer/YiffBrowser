using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace BaseFramework.Controls;

public class WindowBase : CustomWindow {


	public bool SpecialNoResize {
		get => (bool)GetValue(SpecialNoResizeProperty);
		set => SetValue(SpecialNoResizeProperty, value);
	}


	public static readonly DependencyProperty SpecialNoResizeProperty = DependencyProperty.Register(
		nameof(SpecialNoResize),
		typeof(bool),
		typeof(WindowBase),
		new PropertyMetadata(false)
	);



	public Window CenterToOwner {
		get => (Window)GetValue(CenterToOwnerProperty);
		set => SetValue(CenterToOwnerProperty, value);
	}

	public static readonly DependencyProperty CenterToOwnerProperty = DependencyProperty.Register(
		nameof(CenterToOwner),
		typeof(Window),
		typeof(WindowBase),
		new PropertyMetadata(null)
	);



	public WindowBase() {

	}

	protected override void OnSourceInitialized(EventArgs e) {
		base.OnSourceInitialized(e);

		if (SpecialNoResize) {
			nint hwnd = new WindowInteropHelper(this).Handle;
			int style = GetWindowLong(hwnd, GWL_STYLE);

			// 去掉最大化和最小化按钮
			style &= ~WS_MAXIMIZEBOX;
			style &= ~WS_MINIMIZEBOX;
			// 去掉可调整大小的边框
			style &= ~WS_THICKFRAME;

			SetWindowLong(hwnd, GWL_STYLE, style);
		}

		if (CenterToOwner != null) {
			double ownerLeft = CenterToOwner.Left;
			double ownerTop = CenterToOwner.Top;
			double ownerWidth = CenterToOwner.ActualWidth;
			double ownerHeight = CenterToOwner.ActualHeight;

			Left = ownerLeft + ((ownerWidth - ActualWidth) / 2);
			Top = ownerTop + ((ownerHeight - ActualHeight) / 2);
		}
	}


	private const int GWL_STYLE = -16;
	private const int WS_MAXIMIZEBOX = 0x00010000;
	private const int WS_MINIMIZEBOX = 0x00020000;
	private const int WS_THICKFRAME = 0x00040000;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetWindowLong(nint hWnd, int nIndex);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);
}
