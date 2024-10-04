using BaseFramework.Enums;
using BaseFramework.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace BaseFramework.Controls {

	public class UniformSpacingPanel : Panel {

		static UniformSpacingPanel() {
			FocusableProperty.OverrideMetadata(typeof(UniformSpacingPanel), new UIPropertyMetadata(false));
		}

		private Orientation orientation = Orientation.Horizontal;

		public static readonly DependencyProperty OrientationProperty =
			StackPanel.OrientationProperty.AddOwner(typeof(UniformSpacingPanel),
				new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure,
					OnOrientationChanged));

		private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			var p = (UniformSpacingPanel)d;
			p.orientation = (Orientation)e.NewValue;
		}

		public Orientation Orientation {
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
		}

		public static readonly DependencyProperty ChildWrappingProperty = DependencyProperty.Register(
			nameof(ChildWrapping), typeof(VisualWrapping), typeof(UniformSpacingPanel),
			new FrameworkPropertyMetadata(default(VisualWrapping), FrameworkPropertyMetadataOptions.AffectsMeasure));

		public VisualWrapping ChildWrapping {
			get => (VisualWrapping)GetValue(ChildWrappingProperty);
			set => SetValue(ChildWrappingProperty, value);
		}

		public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
			nameof(Spacing), typeof(double), typeof(UniformSpacingPanel),
			new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsSpacingValid);

		public double Spacing {
			get => (double)GetValue(SpacingProperty);
			set => SetValue(SpacingProperty, value);
		}

		public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(
			nameof(HorizontalSpacing), typeof(double), typeof(UniformSpacingPanel),
			new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsSpacingValid);

		public double HorizontalSpacing {
			get => (double)GetValue(HorizontalSpacingProperty);
			set => SetValue(HorizontalSpacingProperty, value);
		}

		public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(
			nameof(VerticalSpacing), typeof(double), typeof(UniformSpacingPanel),
			new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), IsSpacingValid);

		public double VerticalSpacing {
			get => (double)GetValue(VerticalSpacingProperty);
			set => SetValue(VerticalSpacingProperty, value);
		}

		public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
			nameof(ItemWidth), typeof(double), typeof(UniformSpacingPanel),
			new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure),
			IsWidthHeightValid);

		[TypeConverter(typeof(LengthConverter))]
		public double ItemWidth {
			get => (double)GetValue(ItemWidthProperty);
			set => SetValue(ItemWidthProperty, value);
		}

		public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
			nameof(ItemHeight), typeof(double), typeof(UniformSpacingPanel),
			new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure),
			IsWidthHeightValid);

		[TypeConverter(typeof(LengthConverter))]
		public double ItemHeight {
			get => (double)GetValue(ItemHeightProperty);
			set => SetValue(ItemHeightProperty, value);
		}

		public static readonly DependencyProperty ItemHorizontalAlignmentProperty = DependencyProperty.Register(
			nameof(ItemHorizontalAlignment), typeof(HorizontalAlignment?), typeof(UniformSpacingPanel),
			new FrameworkPropertyMetadata(HorizontalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure));

		public HorizontalAlignment? ItemHorizontalAlignment {
			get => (HorizontalAlignment?)GetValue(ItemHorizontalAlignmentProperty);
			set => SetValue(ItemHorizontalAlignmentProperty, value);
		}

		public static readonly DependencyProperty ItemVerticalAlignmentProperty = DependencyProperty.Register(
			nameof(ItemVerticalAlignment), typeof(VerticalAlignment?), typeof(UniformSpacingPanel),
			new FrameworkPropertyMetadata(VerticalAlignment.Stretch, FrameworkPropertyMetadataOptions.AffectsMeasure));

		public VerticalAlignment? ItemVerticalAlignment {
			get => (VerticalAlignment?)GetValue(ItemVerticalAlignmentProperty);
			set => SetValue(ItemVerticalAlignmentProperty, value);
		}

		private static bool IsWidthHeightValid(object value) {
			var v = (double)value;
			return double.IsNaN(v) || v >= 0.0d && !double.IsPositiveInfinity(v);
		}

		private static bool IsSpacingValid(object value) {
			if (value is double spacing) {
				return double.IsNaN(spacing) || spacing > 0;
			}

			return false;
		}

		private void ArrangeWrapLine(double v, double lineV, int start, int end, bool useItemU, double itemU,
			double spacing) {
			double u = 0;
			var isHorizontal = orientation == Orientation.Horizontal;

			var children = InternalChildren;
			for (var i = start; i < end; i++) {
				var child = children[i];
				if (child == null) {
					continue;
				}

				var childSize = new PanelUvSize(orientation, child.DesiredSize);
				var layoutSlotU = useItemU ? itemU : childSize.U;

				child.Arrange(isHorizontal ? new Rect(u, v, layoutSlotU, lineV) : new Rect(v, u, lineV, layoutSlotU));

				if (layoutSlotU > 0) {
					u += layoutSlotU + spacing;
				}
			}
		}

		private void ArrangeLine(double lineV, bool useItemU, double itemU, double spacing) {
			double u = 0;
			var isHorizontal = orientation == Orientation.Horizontal;

			var children = InternalChildren;
			for (var i = 0; i < children.Count; i++) {
				var child = children[i];
				if (child == null) {
					continue;
				}

				var childSize = new PanelUvSize(orientation, child.DesiredSize);
				var layoutSlotU = useItemU ? itemU : childSize.U;

				child.Arrange(isHorizontal ? new Rect(u, 0, layoutSlotU, lineV) : new Rect(0, u, lineV, layoutSlotU));

				if (layoutSlotU > 0) {
					u += layoutSlotU + spacing;
				}
			}
		}

		protected override Size MeasureOverride(Size constraint) {
			var curLineSize = new PanelUvSize(orientation);
			var panelSize = new PanelUvSize(orientation);
			var uvConstraint = new PanelUvSize(orientation, constraint);
			var itemWidth = ItemWidth;
			var itemHeight = ItemHeight;
			var itemWidthSet = !double.IsNaN(itemWidth);
			var itemHeightSet = !double.IsNaN(itemHeight);
			var childWrapping = ChildWrapping;
			var itemHorizontalAlignment = ItemHorizontalAlignment;
			var itemVerticalAlignment = ItemVerticalAlignment;
			var itemHorizontalAlignmentSet = itemHorizontalAlignment != null;
			var itemVerticalAlignmentSet = itemVerticalAlignment != null;
			var spacingSize = GetSpacingSize();

			var childConstraint = new Size(
				itemWidthSet ? itemWidth : constraint.Width,
				itemHeightSet ? itemHeight : constraint.Height);

			var children = InternalChildren;
			var isFirst = true;

			if (childWrapping == VisualWrapping.NoWrap) {
				var layoutSlotSize = constraint;

				if (orientation == Orientation.Horizontal) {
					layoutSlotSize.Width = double.PositiveInfinity;
				} else {
					layoutSlotSize.Height = double.PositiveInfinity;
				}

				for (int i = 0, count = children.Count; i < count; ++i) {
					var child = children[i];
					if (child == null) {
						continue;
					}

					if (itemHorizontalAlignmentSet) {
						child.SetCurrentValue(HorizontalAlignmentProperty, itemHorizontalAlignment);
					}

					if (itemVerticalAlignmentSet) {
						child.SetCurrentValue(VerticalAlignmentProperty, itemVerticalAlignment);
					}

					child.Measure(layoutSlotSize);

					var sz = new PanelUvSize(
						orientation,
						itemWidthSet ? itemWidth : child.DesiredSize.Width,
						itemHeightSet ? itemHeight : child.DesiredSize.Height);

					curLineSize.U += isFirst ? sz.U : sz.U + spacingSize.U;
					curLineSize.V = Math.Max(sz.V, curLineSize.V);

					isFirst = false;
				}
			} else {
				for (int i = 0, count = children.Count; i < count; i++) {
					var child = children[i];
					if (child == null) {
						continue;
					}

					if (itemHorizontalAlignmentSet) {
						child.SetCurrentValue(HorizontalAlignmentProperty, itemHorizontalAlignment);
					}

					if (itemVerticalAlignmentSet) {
						child.SetCurrentValue(VerticalAlignmentProperty, itemVerticalAlignment);
					}

					child.Measure(childConstraint);

					var sz = new PanelUvSize(
						orientation,
						itemWidthSet ? itemWidth : child.DesiredSize.Width,
						itemHeightSet ? itemHeight : child.DesiredSize.Height);

					if (!isFirst && MathHelper.GreaterThan(curLineSize.U + sz.U + spacingSize.U, uvConstraint.U)) {
						panelSize.U = Math.Max(curLineSize.U, panelSize.U);
						panelSize.V += curLineSize.V + spacingSize.V;
						curLineSize = sz;
					} else {
						curLineSize.U += isFirst ? sz.U : sz.U + spacingSize.U;
						curLineSize.V = Math.Max(sz.V, curLineSize.V);
					}

					isFirst = false;
				}
			}

			panelSize.U = Math.Max(curLineSize.U, panelSize.U);
			panelSize.V += curLineSize.V;

			return new Size(panelSize.Width, panelSize.Height);
		}

		private PanelUvSize GetSpacingSize() {
			var spacing = Spacing;

			if (!double.IsNaN(spacing)) {
				return new PanelUvSize(orientation, spacing, spacing);
			}

			var horizontalSpacing = HorizontalSpacing;
			if (double.IsNaN(horizontalSpacing)) {
				horizontalSpacing = 0;
			}

			var verticalSpacing = VerticalSpacing;
			if (double.IsNaN(verticalSpacing)) {
				verticalSpacing = 0;
			}

			return new PanelUvSize(orientation, horizontalSpacing, verticalSpacing);
		}

		protected override Size ArrangeOverride(Size finalSize) {
			var firstInLine = 0;
			var itemWidth = ItemWidth;
			var itemHeight = ItemHeight;
			double accumulatedV = 0;
			var itemU = orientation == Orientation.Horizontal ? itemWidth : itemHeight;
			var curLineSize = new PanelUvSize(orientation);
			var uvFinalSize = new PanelUvSize(orientation, finalSize);
			var itemWidthSet = !double.IsNaN(itemWidth);
			var itemHeightSet = !double.IsNaN(itemHeight);
			var useItemU = orientation == Orientation.Horizontal ? itemWidthSet : itemHeightSet;
			var childWrapping = ChildWrapping;
			var spacingSize = GetSpacingSize();

			var children = InternalChildren;
			var isFirst = true;

			if (childWrapping == VisualWrapping.NoWrap) {
				ArrangeLine(uvFinalSize.V, useItemU, itemU, spacingSize.U);
			} else {
				for (int i = 0, count = children.Count; i < count; i++) {
					var child = children[i];
					if (child == null) {
						continue;
					}

					var sz = new PanelUvSize(
						orientation,
						itemWidthSet ? itemWidth : child.DesiredSize.Width,
						itemHeightSet ? itemHeight : child.DesiredSize.Height);

					if (!isFirst && MathHelper.GreaterThan(curLineSize.U + sz.U + spacingSize.U, uvFinalSize.U)) {
						ArrangeWrapLine(accumulatedV, curLineSize.V, firstInLine, i, useItemU, itemU, spacingSize.U);

						accumulatedV += curLineSize.V + spacingSize.V;
						curLineSize = sz;

						firstInLine = i;
					} else {
						curLineSize.U += isFirst ? sz.U : sz.U + spacingSize.U;
						curLineSize.V = Math.Max(sz.V, curLineSize.V);
					}

					isFirst = false;
				}

				if (firstInLine < children.Count) {
					ArrangeWrapLine(accumulatedV, curLineSize.V, firstInLine, children.Count, useItemU, itemU,
						spacingSize.U);
				}
			}

			return finalSize;
		}


		internal static class MathHelper {
			internal const double DBL_EPSILON = 2.2204460492503131e-016;

			public static bool AreClose(double value1, double value2) =>
				// ReSharper disable once CompareOfFloatsByEqualityOperator
				value1 == value2 || IsVerySmall(value1 - value2);

			public static double Lerp(double x, double y, double alpha) => x * (1.0 - alpha) + y * alpha;

			public static bool IsVerySmall(double value) => Math.Abs(value) < 1E-06;

			public static bool IsZero(double value) => Math.Abs(value) < 10.0 * DBL_EPSILON;

			public static bool IsFiniteDouble(double x) => !double.IsInfinity(x) && !double.IsNaN(x);

			public static double DoubleFromMantissaAndExponent(double x, int exp) => x * Math.Pow(2.0, exp);

			public static bool GreaterThan(double value1, double value2) => value1 > value2 && !AreClose(value1, value2);

			public static bool GreaterThanOrClose(double value1, double value2) {
				if (value1 <= value2) {
					return AreClose(value1, value2);
				}
				return true;
			}

			public static double Hypotenuse(double x, double y) => Math.Sqrt(x * x + y * y);

			public static bool LessThan(double value1, double value2) => value1 < value2 && !AreClose(value1, value2);

			public static bool LessThanOrClose(double value1, double value2) {
				if (value1 >= value2) {
					return AreClose(value1, value2);
				}
				return true;
			}

			public static double EnsureRange(double value, double? min, double? max) {
				if (min.HasValue && value < min.Value) {
					return min.Value;
				}
				if (max.HasValue && value > max.Value) {
					return max.Value;
				}
				return value;
			}

			public static double SafeDivide(double lhs, double rhs, double fallback) {
				if (!IsVerySmall(rhs)) {
					return lhs / rhs;
				}
				return fallback;
			}
		}

	}
}
