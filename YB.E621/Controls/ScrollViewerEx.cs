using BaseFramework.Extensions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace YB.E621.Controls;

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

	public void ScrollToElement(FrameworkElement element, double margin = 10) {
		if (element == null) {
			return;
		}

		// 获取元素相对于 ScrollViewer (this) 的位置
		GeneralTransform transform = element.TransformToVisual(this);
		Rect elementRect = new(transform.Transform(new Point(0, 0)), new Size(element.ActualWidth, element.ActualHeight));

		double targetOffset = HorizontalOffset;

		// 考虑 margin 后的判定区域
		if (elementRect.Right + margin > ViewportWidth) {
			// 元素的右边界 + margin 如果超过了视口宽度 -> 说明右侧被遮挡或太靠边了
			targetOffset += elementRect.Right + margin - ViewportWidth;
		} else if (elementRect.Left - margin < 0) {
			// 元素的左边界 - margin 如果小于 0 -> 说明左侧被遮挡或太靠边了
			targetOffset += elementRect.Left - margin;
		} else {
			// 如果已经在视口内（且满足边距条件），直接返回
			return;
		}

		// 执行丝滑滚动
		AnimateScroll(targetOffset);
	}

	public void ScrollToElementCenter(FrameworkElement element) {
		if (element == null) {
			return;
		}

		// 计算目标元素相对于 ScrollContentPresenter 的位置
		GeneralTransform transform = element.TransformToVisual(this);
		Point relativePoint = transform.Transform(new Point(0, 0));

		// 计算目标偏移量：当前偏移量 + 元素相对位置 - (视口宽度 / 2 - 元素宽度 / 2)
		// 这样可以让选中的标签尽量居中显示
		double targetOffset = HorizontalOffset + relativePoint.X - (ViewportWidth / 2) + (element.ActualWidth / 2);

		// 限制范围
		targetOffset = Math.Max(0, Math.Min(ScrollableWidth, targetOffset));

		// 执行平滑动画
		AnimateScroll(targetOffset);
	}

	private void AnimateScroll(double targetOffset) {
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

}
