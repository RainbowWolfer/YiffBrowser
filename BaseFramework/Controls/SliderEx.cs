using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace BaseFramework.Controls;

public class SliderEx : Slider {
	public event MouseButtonEventHandler? PreviewMouseLeftButtonDown2;

	private Track? Track { get; set; }

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		Track = GetTemplateChild("PART_Track") as Track;
	}

	protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
		base.OnPreviewMouseLeftButtonDown(e);

		if (Track != null) {
			if (e.OriginalSource is FrameworkElement frameworkElement) {
				if (frameworkElement.Name is "AnywhereCover") {
					Point position = e.MouseDevice.GetPosition(Track);
					double num = Track.ValueFromPoint(position);
					if (IsDoubleFinite(num)) {
						//MethodInfo? method = typeof(Slider).GetMethod("UpdateValue", BindingFlags.Instance | BindingFlags.NonPublic);
						//method?.Invoke(this, [num]);
						Value = num;
						//SetCurrentValue(ValueProperty, num);

						Dispatcher.BeginInvoke(() => {
							MethodInfo? m2 = typeof(Thumb).GetMethod("OnMouseLeftButtonDown", BindingFlags.Instance | BindingFlags.NonPublic);
							m2?.Invoke(Track.Thumb, [e]);
						}, DispatcherPriority.Render);

					}
					e.Handled = true;
				}
			}
		}

		PreviewMouseLeftButtonDown2?.Invoke(this, e);

	}
	internal static bool IsDoubleFinite(object o) {
		double d = (double)o;
		if (!double.IsInfinity(d)) {
			return !double.IsNaN(d);
		}
		return false;
	}

}
