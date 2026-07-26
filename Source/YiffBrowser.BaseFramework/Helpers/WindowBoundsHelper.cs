using System.Windows;

namespace YiffBrowser.BaseFramework.Helpers;

public static class WindowBoundsHelper {
	private const double MinWidth = 800;
	private const double MinHeight = 600;
	private const double TitleBarProbeHeight = 40;

	/// <summary>
	/// Clamps window bounds so the title-bar region intersects the virtual screen.
	/// Returns adjusted left/top/width/height.
	/// </summary>
	public static Rect ClampToVirtualScreen(double left, double top, double width, double height) {
		width = Math.Max(MinWidth, width);
		height = Math.Max(MinHeight, height);

		Rect virtualBounds = GetVirtualScreenBounds();
		if (virtualBounds.Width <= 0 || virtualBounds.Height <= 0) {
			return new Rect(left, top, width, height);
		}

		// Title-bar probe: a strip at the top of the window must intersect the virtual screen.
		Rect titleBar = new(left, top, width, TitleBarProbeHeight);
		if (titleBar.IntersectsWith(virtualBounds)) {
			// Also keep size from exploding beyond virtual screen.
			width = Math.Min(width, virtualBounds.Width);
			height = Math.Min(height, virtualBounds.Height);
			return new Rect(left, top, width, height);
		}

		// Move onto nearest edge of virtual screen with a small margin.
		const double margin = 40;
		double clampedLeft = Math.Clamp(left, virtualBounds.Left + margin, virtualBounds.Right - width - margin);
		double clampedTop = Math.Clamp(top, virtualBounds.Top + margin, virtualBounds.Bottom - TitleBarProbeHeight - margin);

		if (double.IsNaN(clampedLeft) || double.IsInfinity(clampedLeft)) {
			clampedLeft = virtualBounds.Left + margin;
		}

		if (double.IsNaN(clampedTop) || double.IsInfinity(clampedTop)) {
			clampedTop = virtualBounds.Top + margin;
		}

		width = Math.Min(width, virtualBounds.Width - margin * 2);
		height = Math.Min(height, virtualBounds.Height - margin * 2);
		width = Math.Max(MinWidth, width);
		height = Math.Max(MinHeight, height);

		return new Rect(clampedLeft, clampedTop, width, height);
	}

	private static Rect GetVirtualScreenBounds() {
		double left = SystemParameters.VirtualScreenLeft;
		double top = SystemParameters.VirtualScreenTop;
		double width = SystemParameters.VirtualScreenWidth;
		double height = SystemParameters.VirtualScreenHeight;
		return new Rect(left, top, width, height);
	}
}
