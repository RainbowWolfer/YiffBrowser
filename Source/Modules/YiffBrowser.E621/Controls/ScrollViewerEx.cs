using YiffBrowser.BaseFramework.Extensions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YiffBrowser.E621.Controls;

internal class ScrollViewerEx : HandyControl.Controls.ScrollViewer {

	public ScrollViewerEx() {
		Loaded += ScrollViewerEx_Loaded;
	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		DisableClipping();
	}


	private void ScrollViewerEx_Loaded(object sender, RoutedEventArgs e) {
		DisableClipping();
	}

	private void DisableClipping() {
		//if (GetTemplateChild("PART_ScrollContentPresenter") is UIElement cp) {
		//	// 1. 尝试清除绑定
		//	BindingOperations.ClearBinding(cp, ClipProperty);
		//	cp.Clip = null;

		//	// 2. 关键：通过 PropertyDescriptor 监听 Clip 属性的变化
		//	// 只要系统尝试给 PART_ScrollContentPresenter 设置 Clip，我们就把它强行置 null
		//	DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(ClipProperty, typeof(UIElement));
		//	dpd?.AddValueChanged(cp, (s, e) => {
		//		if (cp.Clip != null) {
		//			cp.Clip = null;
		//		}
		//	});
		//}
	}

	private static readonly Type parentType = typeof(HandyControl.Controls.ScrollViewer);

	private static readonly FieldInfo? vDpField = parentType.GetField("CurrentVerticalOffsetProperty", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

	private static readonly FieldInfo? hDpField = parentType.GetField("CurrentHorizontalOffsetProperty", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

	private static readonly FieldInfo? runningField = parentType.GetField("_isRunning", BindingFlags.Instance | BindingFlags.NonPublic);

	public void StopScrollAnimations() {
		if (vDpField?.GetValue(null) is DependencyProperty vDp) {
			double currentV = (double)GetValue(vDp);
			BeginAnimation(vDp, null);
			SetCurrentValue(vDp, currentV);
		}

		if (hDpField?.GetValue(null) is DependencyProperty hDp) {
			double currentH = (double)GetValue(hDp);
			BeginAnimation(hDp, null);
			SetCurrentValue(hDp, currentH);
		}

		runningField?.SetValue(this, false);
	}

	public void ScrollToElement(FrameworkElement element, double margin, Dock dock) {
		if (element == null) {
			return;
		}

		StopScrollAnimations();

		// 获取元素相对于 ScrollViewer (this) 的位置
		GeneralTransform transform = element.TransformToVisual(this);
		Rect elementRect = new(transform.Transform(new Point(0, 0)), new Size(element.ActualWidth, element.ActualHeight));

		if (dock is Dock.Top or Dock.Bottom) {
			double targetOffset = HorizontalOffset;
			if (elementRect.Right + margin > ViewportWidth) {
				targetOffset += elementRect.Right + margin - ViewportWidth;
			} else if (elementRect.Left - margin < 0) {
				targetOffset += elementRect.Left - margin;
			} else {
				return;
			}

			AnimateScrollHorizontal(targetOffset);
		} else {
			double targetOffset = VerticalOffset;
			if (elementRect.Bottom + margin > ViewportHeight) {
				targetOffset += elementRect.Bottom + margin - ViewportHeight;
			} else if (elementRect.Top - margin < 0) {
				targetOffset += elementRect.Top - margin;
			} else {
				return;
			}

			AnimateScrollVertical(targetOffset);
		}
	}

	//public void ScrollToElementCenter(FrameworkElement element) {
	//	if (element == null) {
	//		return;
	//	}

	//	// 计算目标元素相对于 ScrollContentPresenter 的位置
	//	GeneralTransform transform = element.TransformToVisual(this);
	//	Point relativePoint = transform.Transform(new Point(0, 0));

	//	// 计算目标偏移量：当前偏移量 + 元素相对位置 - (视口宽度 / 2 - 元素宽度 / 2)
	//	// 这样可以让选中的标签尽量居中显示
	//	double targetOffset = HorizontalOffset + relativePoint.X - (ViewportWidth / 2) + (element.ActualWidth / 2);

	//	// 限制范围
	//	targetOffset = Math.Max(0, Math.Min(ScrollableWidth, targetOffset));

	//	// 执行平滑动画
	//	AnimateScrollHorizontal(targetOffset);
	//}

	private void AnimateScrollHorizontal(double targetOffset) {
		DoubleAnimation animation = new() {
			To = targetOffset,
			Duration = TimeSpan.FromMilliseconds(200),
			EasingFunction = new ExponentialEase {
				EasingMode = EasingMode.EaseOut,
				Exponent = 10,
			}
		};

		// 使用我们自定义的附加属性或直接操作
		BeginAnimation(ScrollViewerBehavior.HorizontalOffsetProperty, animation);
	}

	private void AnimateScrollVertical(double targetOffset) {
		DoubleAnimation animation = new() {
			To = targetOffset,
			Duration = TimeSpan.FromMilliseconds(200),
			EasingFunction = new ExponentialEase {
				EasingMode = EasingMode.EaseOut,
				Exponent = 10,
			}
		};

		// 使用我们自定义的附加属性或直接操作
		BeginAnimation(ScrollViewerBehavior.VerticalOffsetProperty, animation);
	}

}
