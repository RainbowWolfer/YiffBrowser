using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace BaseFramework.Helpers {
	public static class VisualHelper {
		internal static VisualStateGroup? TryGetVisualStateGroup(DependencyObject d, string groupName) {
			FrameworkElement? root = GetImplementationRoot(d);
			if (root == null) {
				return null;
			}

			return VisualStateManager
				.GetVisualStateGroups(root)?
				.OfType<VisualStateGroup>()
				.FirstOrDefault(group => string.CompareOrdinal(groupName, group.Name) == 0);
		}

		internal static FrameworkElement? GetImplementationRoot(DependencyObject d) {
			if (1 == VisualTreeHelper.GetChildrenCount(d)) {
				return VisualTreeHelper.GetChild(d, 0) as FrameworkElement;
			} else {
				return null;
			}
		}

		public static T? GetChild<T>(DependencyObject d) where T : DependencyObject {
			if (d == null) {
				return default;
			}

			if (d is T t) {
				return t;
			}

			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++) {
				DependencyObject child = VisualTreeHelper.GetChild(d, i);

				T? result = GetChild<T>(child);
				if (result != null) {
					return result;
				}
			}

			return default;
		}

		public static T? GetParent<T>(DependencyObject d) where T : DependencyObject {
			return d switch {
				null => default,
				T t => t,
				Window _ => null,
				_ => GetParent<T>(VisualTreeHelper.GetParent(d))
			};
		}

		public static IntPtr GetHandle(this Visual visual) => (PresentationSource.FromVisual(visual) as HwndSource)?.Handle ?? IntPtr.Zero;

		internal static void HitTestVisibleElements(Visual visual, HitTestResultCallback resultCallback, HitTestParameters parameters) =>
			VisualTreeHelper.HitTest(visual, ExcludeNonVisualElements, resultCallback, parameters);

		private static HitTestFilterBehavior ExcludeNonVisualElements(DependencyObject potentialHitTestTarget) {
			if (potentialHitTestTarget is not Visual) {
				return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
			}

			if (potentialHitTestTarget is not UIElement uIElement || uIElement.IsVisible && uIElement.IsEnabled) {
				return HitTestFilterBehavior.Continue;
			}

			return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
		}

	}
}
