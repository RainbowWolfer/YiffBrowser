using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace YiffBrowser.BaseFramework.Helpers;

/// <summary>
/// Mitigates .NET 6+ ToolTip "safe area" sticking when the mouse moves between
/// adjacent tooltip owners (typical in ListBox rows / toolbars). Moving toward the
/// tip (usually down/right with default Placement=Mouse) keeps the old tip open;
/// this closes it when entering another owner so the new tip can show.
/// </summary>
public static class ToolTipServiceFix {
	private static bool applied;

	public static void Apply() {
		if (applied) {
			return;
		}

		applied = true;

		// Keep a short between-show window so the next tip appears immediately after
		// we force-close the previous one while scanning a list.
		ToolTipService.BetweenShowDelayProperty.OverrideMetadata(
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(200));

		// Prefer Right over Mouse so vertical list scanning is not aimed into the tip.
		// Explicit Placement on controls (e.g. Bottom on horizontal icon bars) still wins.
		ToolTipService.PlacementProperty.OverrideMetadata(
			typeof(FrameworkElement),
			new FrameworkPropertyMetadata(PlacementMode.Right));

		EventManager.RegisterClassHandler(
			typeof(FrameworkElement),
			UIElement.MouseLeaveEvent,
			new MouseEventHandler(OnMouseLeave),
			handledEventsToo: false);
	}

	private static void OnMouseLeave(object sender, MouseEventArgs e) {
		if (sender is not FrameworkElement leaving || leaving.ToolTip is null) {
			return;
		}

		// Only dismiss when moving onto another tooltip owner. Leaving into empty
		// space or onto the tip popup itself is left to the normal safe-area logic.
		FrameworkElement? entering = FindNearestToolTipOwner(Mouse.DirectlyOver as DependencyObject);
		if (entering is null || ReferenceEquals(entering, leaving)) {
			return;
		}

		Dismiss(leaving);
	}

	private static FrameworkElement? FindNearestToolTipOwner(DependencyObject? start) {
		for (DependencyObject? current = start; current != null;) {
			if (current is FrameworkElement element && element.ToolTip is not null) {
				return element;
			}

			current = GetParent(current);
		}

		return null;
	}

	private static DependencyObject? GetParent(DependencyObject current) {
		if (current is Visual) {
			return VisualTreeHelper.GetParent(current);
		}

		return LogicalTreeHelper.GetParent(current);
	}

	private static void Dismiss(FrameworkElement element) {
		if (element.ToolTip is ToolTip toolTip) {
			toolTip.IsOpen = false;
			return;
		}

		// String / other content is hosted by ToolTipService — toggle IsEnabled to close.
		if (!ToolTipService.GetIsEnabled(element)) {
			return;
		}

		ToolTipService.SetIsEnabled(element, false);
		ToolTipService.SetIsEnabled(element, true);
	}
}
