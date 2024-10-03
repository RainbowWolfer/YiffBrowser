using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace BaseFramework.Controls {
	public class VariableSizedWrapGrid : Panel, IScrollInfo {



		public double? VerticalScrollAmount {
			get => (double?)GetValue(VerticalScrollAmountProperty);
			set => SetValue(VerticalScrollAmountProperty, value);
		}

		public static readonly DependencyProperty VerticalScrollAmountProperty = DependencyProperty.Register(
			nameof(VerticalScrollAmount),
			typeof(double?),
			typeof(VariableSizedWrapGrid),
			new PropertyMetadata(null)
		);



		#region HorizontalAlignment HorizontalChildrenAlignment
		public HorizontalAlignment HorizontalChildrenAlignment {
			get { return (HorizontalAlignment)GetValue(HorizontalChildrenAlignmentProperty); }
			set { SetValue(HorizontalChildrenAlignmentProperty, value); }
		}

		public static readonly DependencyProperty HorizontalChildrenAlignmentProperty =
			DependencyProperty.Register("HorizontalChildrenAlignment", typeof(HorizontalAlignment), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));
		#endregion

		#region double ItemHeight
		public double ItemHeight {
			get { return (double)GetValue(ItemHeightProperty); }
			set { SetValue(ItemHeightProperty, value); }
		}

		public static readonly DependencyProperty ItemHeightProperty =
			DependencyProperty.Register("ItemHeight", typeof(double), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
		#endregion

		#region double ItemWidth
		public double ItemWidth {
			get { return (double)GetValue(ItemWidthProperty); }
			set { SetValue(ItemWidthProperty, value); }
		}

		public static readonly DependencyProperty ItemWidthProperty =
			DependencyProperty.Register("ItemWidth", typeof(double), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
		#endregion

		#region bool LatchIemSize
		public bool LatchItemSize {
			get { return (bool)GetValue(LatchItemSizeProperty); }
			set { SetValue(LatchItemSizeProperty, value); }
		}

		public static readonly DependencyProperty LatchItemSizeProperty =
			DependencyProperty.Register("LatchItemSize", typeof(bool), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
		#endregion

		#region int MaximumRowsOrColumns
		public int MaximumRowsOrColumns {
			get { return (int)GetValue(MaximumRowsOrColumnsProperty); }
			set { SetValue(MaximumRowsOrColumnsProperty, value); }
		}

		public static readonly DependencyProperty MaximumRowsOrColumnsProperty =
			DependencyProperty.Register("MaximumRowsOrColumns", typeof(int), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
		#endregion

		#region Orientation Orientation
		public Orientation Orientation {
			get { return (Orientation)GetValue(OrientationProperty); }
			set { SetValue(OrientationProperty, value); }
		}

		public static readonly DependencyProperty OrientationProperty =
			DependencyProperty.Register("Orientation", typeof(Orientation), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
		#endregion

		#region bool StrictItemOrder
		public bool StrictItemOrder {
			get { return (bool)GetValue(StrictItemOrderProperty); }
			set { SetValue(StrictItemOrderProperty, value); }
		}

		public static readonly DependencyProperty StrictItemOrderProperty =
			DependencyProperty.Register("StrictItemOrder", typeof(bool), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));
		#endregion

		#region VerticalAlignment VerticalChildrenAlignment
		public VerticalAlignment VerticalChildrenAlignment {
			get { return (VerticalAlignment)GetValue(VerticalChildrenAlignmentProperty); }
			set { SetValue(VerticalChildrenAlignmentProperty, value); }
		}

		public static readonly DependencyProperty VerticalChildrenAlignmentProperty =
			DependencyProperty.Register("VerticalChildrenAlignment", typeof(VerticalAlignment), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(VerticalAlignment.Top, FrameworkPropertyMetadataOptions.AffectsArrange));
		#endregion

		#region int ColumnSpan
		public static int GetColumnSpan(DependencyObject obj) {
			return (int)obj.GetValue(ColumnSpanProperty);
		}

		public static void SetColumnSpan(DependencyObject obj, int value) {
			obj.SetValue(ColumnSpanProperty, value);
		}

		public static readonly DependencyProperty ColumnSpanProperty =
			DependencyProperty.RegisterAttached("ColumnSpan", typeof(int), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure));
		#endregion

		#region int RowSpan
		public static int GetRowSpan(DependencyObject obj) {
			return (int)obj.GetValue(RowSpanProperty);
		}

		public static void SetRowSpan(DependencyObject obj, int value) {
			obj.SetValue(RowSpanProperty, value);
		}

		public static readonly DependencyProperty RowSpanProperty =
			DependencyProperty.RegisterAttached("RowSpan", typeof(int), typeof(VariableSizedWrapGrid),
				new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure));
		#endregion

		private class PlotSorterVertical : IComparer<Rect> {
			public int Compare(Rect x, Rect y) {
				if (x.Left < y.Left) {
					return -1;
				}

				if (x.Left > y.Left) {
					return 1;
				}

				if (x.Top < y.Top) {
					return -1;
				}

				if (x.Top > y.Top) {
					return 1;
				}

				return 0;
			}
		}

		private class PlotSorterHorizontal : IComparer<Rect> {
			public int Compare(Rect x, Rect y) {
				if (x.Top < y.Top) {
					return -1;
				}

				if (x.Top > y.Top) {
					return 1;
				}

				if (x.Left < y.Left) {
					return -1;
				}

				if (x.Left > y.Left) {
					return 1;
				}

				return 0;
			}
		}

		private static IEnumerable<Rect> ReserveAcreage(Rect acreage, Rect plot) {
			if (acreage.IntersectsWith(plot)) {
				// Above?
				if (plot.Top < acreage.Top) {
					Rect rest = new(plot.Location, new Size(plot.Width, acreage.Top - plot.Top));
					yield return rest;
				}

				// Below?
				if (plot.Bottom > acreage.Bottom) {
					Rect rest = new(new Point(plot.Left, acreage.Bottom), new Size(plot.Width, plot.Bottom - acreage.Bottom));
					yield return rest;
				}

				// Left?
				if (plot.Left < acreage.Left) {
					Rect rest = new(plot.Location, new Size(acreage.Left - plot.Left, plot.Height));
					yield return rest;
				}

				// Right?
				if (plot.Right > acreage.Right) {
					Rect rest = new(new Point(acreage.Right, plot.Top), new Size(plot.Right - acreage.Right, plot.Height));
					yield return rest;
				}
			} else {
				yield return plot;
			}
		}

		private Point PlaceElement(Size requiredSize, ref List<Rect> plots,
			double itemWidth, double itemHeight) {
			Point location = new();

			foreach (Rect plot in plots) {
				if ((plot.Height >= requiredSize.Height) && (plot.Width >= requiredSize.Width)) {
					Rect acreage = new(plot.Location, requiredSize);

					Rect innerRect;
					Rect outerRect;
					IComparer<Rect> plotSorter;

					if (Orientation == Orientation.Vertical) {
						innerRect = new Rect(0, 0, acreage.X + itemWidth, acreage.Y);
						outerRect = new Rect(0, 0, acreage.X, double.MaxValue);
						plotSorter = new PlotSorterVertical();
					} else {
						innerRect = new Rect(0, 0, acreage.X, acreage.Y + itemHeight);
						outerRect = new Rect(0, 0, double.MaxValue, acreage.Y);
						plotSorter = new PlotSorterHorizontal();
					}

					List<Rect> localPlots;

					if (StrictItemOrder) {
						localPlots = plots.SelectMany(p => ReserveAcreage(acreage, p))
							.SelectMany(p => ReserveAcreage(outerRect, p))
							.SelectMany(p => ReserveAcreage(innerRect, p)).Distinct().ToList();
					} else {
						localPlots = plots.SelectMany(p => ReserveAcreage(acreage, p)).Distinct().ToList();
					}

					localPlots.RemoveAll(x => localPlots.Any(y => y.Contains(x) && !y.Equals(x)));
					localPlots.Sort(plotSorter);
					plots = localPlots;

					location = acreage.Location;
					break;
				}
			}

			return location;
		}

		private Rect ArrangeElement(Rect acreage, Size desiredSize, Vector offset) {
			Rect rect = acreage;

			// Adjust horizontal location and size for alignment
			switch (HorizontalChildrenAlignment) {
				case HorizontalAlignment.Center:
					rect.X += Math.Max(0, (acreage.Width - desiredSize.Width) / 2);
					rect.Width = desiredSize.Width;
					break;
				case HorizontalAlignment.Left:
					rect.Width = desiredSize.Width;
					break;
				case HorizontalAlignment.Right:
					rect.X += Math.Max(0, acreage.Width - desiredSize.Width);
					rect.Width = desiredSize.Width;
					break;
				case HorizontalAlignment.Stretch:
				default:
					break;
			}

			// Adjust vertical location and size for alignment
			switch (VerticalChildrenAlignment) {
				case VerticalAlignment.Bottom:
					rect.Y += Math.Max(0, acreage.Height - desiredSize.Height);
					rect.Height = desiredSize.Height;
					break;
				case VerticalAlignment.Center:
					rect.Y += Math.Max(0, (acreage.Height - desiredSize.Height) / 2);
					rect.Height = desiredSize.Height;
					break;
				case VerticalAlignment.Top:
					rect.Height = desiredSize.Height;
					break;
				case VerticalAlignment.Stretch:
				default:
					break;
			}

			// Adjust location for scrolling offset
			rect.Location -= offset;

			return rect;
		}

		private double _itemHeight;
		private double _itemWidth;

		private ScrollViewer? _owner;
		private Size _extent;
		private Size _viewport = new();
		private Vector _offset = new();

		private void SetViewport(Size size) {
			if (_viewport != size) {
				_viewport = size;
				_owner?.InvalidateScrollInfo();
			}
		}

		private void SetExtent(Size size) {
			if (_extent != size) {
				_extent = size;
				_owner?.InvalidateScrollInfo();
			}
		}

		private IList<Rect>? _finalRects;

		protected override Size MeasureOverride(Size availableSize) {
			Size desiredSizeMin = new();
			List<Size> elementSizes = new(InternalChildren.Count);

			_itemHeight = ItemHeight;
			_itemWidth = ItemWidth;

			foreach (UIElement element in InternalChildren) {
				Size elementSize = LatchItemSize ?
					new Size(double.IsNaN(_itemWidth) ? double.MaxValue : _itemWidth * GetColumnSpan(element), double.IsNaN(_itemHeight) ? double.MaxValue : _itemHeight * GetRowSpan(element)) :
					new Size(double.IsNaN(ItemWidth) ? double.MaxValue : _itemWidth * GetColumnSpan(element), double.IsNaN(ItemHeight) ? double.MaxValue : _itemHeight * GetRowSpan(element));

				// Measure each element providing allocated plot size.
				element.Measure(elementSize);

				// Use the elements desired size as item size in the undefined dimension(s)
				if (double.IsNaN(_itemHeight) || (!LatchItemSize && double.IsNaN(ItemHeight))) {
					elementSize.Height = element.DesiredSize.Height;
				}

				if (double.IsNaN(_itemWidth) || (!LatchItemSize && double.IsNaN(ItemWidth))) {
					elementSize.Width = element.DesiredSize.Width;
				}

				if (double.IsNaN(_itemHeight)) {
					_itemHeight = element.DesiredSize.Height / GetRowSpan(element);
				}

				if (double.IsNaN(_itemWidth)) {
					_itemWidth = element.DesiredSize.Width / GetColumnSpan(element);
				}

				// The minimum size of the panel is equal to the largest element in each dimension.
				desiredSizeMin.Height = Math.Max(desiredSizeMin.Height, elementSize.Height);
				desiredSizeMin.Width = Math.Max(desiredSizeMin.Width, elementSize.Width);

				elementSizes.Add(elementSize);
			}

			// Always use at least the available size for the panel unless infinite.
			Size desiredSize = new() {
				Height = double.IsPositiveInfinity(availableSize.Height) ? 0 : availableSize.Height,
				Width = double.IsPositiveInfinity(availableSize.Width) ? 0 : availableSize.Width
			};

			// Available plots on the panel real estate
			List<Rect> plots = [];

			// Calculate maximum size
			Size maxSize = (MaximumRowsOrColumns > 0) ?
				new Size(_itemWidth * MaximumRowsOrColumns, _itemHeight * MaximumRowsOrColumns) :
				new Size(double.MaxValue, double.MaxValue);

			// Add the first plot covering the entire estate.
			Rect bigPlot = new(new Point(0, 0), (Orientation == Orientation.Vertical) ?
				new Size(double.MaxValue, Math.Max(Math.Min(availableSize.Height, maxSize.Height), desiredSizeMin.Height)) :
				new Size(Math.Max(Math.Min(availableSize.Width, maxSize.Width), desiredSizeMin.Width), double.MaxValue));

			plots.Add(bigPlot);

			_finalRects = new List<Rect>(InternalChildren.Count);

			using (List<Size>.Enumerator sizeEnumerator = elementSizes.GetEnumerator()) {
				foreach (UIElement element in InternalChildren) {
					sizeEnumerator.MoveNext();
					Size elementSize = sizeEnumerator.Current;

					// Find a plot able to hold this element.
					Rect acreage = new(PlaceElement(elementSize, ref plots, _itemWidth, _itemHeight), elementSize);

					_finalRects.Add(acreage);

					// Keep track of panel size...
					desiredSize.Height = Math.Max(desiredSize.Height, acreage.Bottom);
					desiredSize.Width = Math.Max(desiredSize.Width, acreage.Right);
				}
			}

			SetViewport(availableSize);
			SetExtent(desiredSize);

			return desiredSize;
		}

		protected override Size ArrangeOverride(Size finalSize) {
			Size actualSize = new() {
				Height = double.IsPositiveInfinity(finalSize.Height) ? 0 : finalSize.Height,
				Width = double.IsPositiveInfinity(finalSize.Width) ? 0 : finalSize.Width
			};

			if (_finalRects != null) {
				using IEnumerator<Rect> rectEnumerator = _finalRects.GetEnumerator();
				foreach (UIElement element in InternalChildren) {
					rectEnumerator.MoveNext();
					Rect acreage = rectEnumerator.Current;

					// Keep track of panel size...
					actualSize.Height = Math.Max(actualSize.Height, acreage.Bottom);
					actualSize.Width = Math.Max(actualSize.Width, acreage.Right);

					// Arrange each element using allocated plot location and size.
					element.Arrange(ArrangeElement(acreage, element.DesiredSize, _offset));
				}
			}

			// Adjust offset when the viewport size changes
			SetHorizontalOffset(Math.Max(0, Math.Min(HorizontalOffset, ExtentWidth - ViewportWidth)));
			SetVerticalOffset(Math.Max(0, Math.Min(VerticalOffset, ExtentHeight - ViewportHeight)));

			return actualSize;
		}

		#region IScrollInfo
		// This property is not intended for use in your code. It is exposed publicly to fulfill an interface contract (IScrollInfo). Setting this property has no effect.
		public bool CanVerticallyScroll {
			get { return false; }
			set { }
		}

		// This property is not intended for use in your code. It is exposed publicly to fulfill an interface contract (IScrollInfo). Setting this property has no effect.
		public bool CanHorizontallyScroll {
			get { return false; }
			set { }
		}

		public double ExtentWidth {
			get { return _extent.Width; }
		}

		public double ExtentHeight {
			get { return _extent.Height; }
		}

		public double ViewportWidth {
			get { return _viewport.Width; }
		}

		public double ViewportHeight {
			get { return _viewport.Height; }
		}

		public double HorizontalOffset {
			get { return _offset.X; }
		}

		public double VerticalOffset {
			get { return _offset.Y; }
		}

		public ScrollViewer? ScrollOwner {
			get { return _owner; }
			set { _owner = value; }
		}

		public void LineUp() {
			double amount = VerticalScrollAmount ?? _itemHeight;
			SetVerticalOffset(VerticalOffset - amount);
		}

		public void LineDown() {
			double amount = VerticalScrollAmount ?? _itemHeight;
			SetVerticalOffset(VerticalOffset + amount);
		}

		public void LineLeft() {
			SetHorizontalOffset(HorizontalOffset - _itemWidth);
		}

		public void LineRight() {
			SetHorizontalOffset(HorizontalOffset + _itemWidth);
		}

		public void PageUp() {
			SetVerticalOffset(VerticalOffset - Math.Max(_itemHeight, Math.Max(0, _viewport.Height - _itemHeight)));
		}

		public void PageDown() {
			SetVerticalOffset(VerticalOffset + Math.Max(_itemHeight, Math.Max(0, _viewport.Height - _itemHeight)));
		}

		public void PageLeft() {
			SetHorizontalOffset(HorizontalOffset - Math.Max(_itemWidth, Math.Max(0, _viewport.Width - _itemWidth)));
		}

		public void PageRight() {
			SetHorizontalOffset(HorizontalOffset + Math.Max(_itemWidth, Math.Max(0, _viewport.Width - _itemWidth)));
		}

		public void MouseWheelUp() {
			LineUp();
		}

		public void MouseWheelDown() {
			LineDown();
		}

		public void MouseWheelLeft() {
			LineLeft();
		}

		public void MouseWheelRight() {
			LineRight();
		}

		public void SetHorizontalOffset(double offset) {
			offset = Math.Max(0, Math.Min(offset, ExtentWidth - ViewportWidth));
			if (offset != _offset.X) {
				_offset.X = offset;
				_owner?.InvalidateScrollInfo();
				InvalidateArrange();
			}
		}

		public void SetVerticalOffset(double offset) {
			offset = Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));
			if (offset != _offset.Y) {
				_offset.Y = offset;
				_owner?.InvalidateScrollInfo();
				InvalidateArrange();
			}
		}

		public Rect MakeVisible(Visual visual, Rect rectangle) {
			if (rectangle.IsEmpty || visual == null || visual == this || !IsAncestorOf(visual)) {
				return Rect.Empty;
			}

			rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);

			Rect viewRect = new(HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight);

			// Horizontal
			if (rectangle.Right + HorizontalOffset > viewRect.Right) {
				viewRect.X = viewRect.X + rectangle.Right + HorizontalOffset - viewRect.Right;
			}
			if (rectangle.Left + HorizontalOffset < viewRect.Left) {
				viewRect.X -= (viewRect.Left - (rectangle.Left + HorizontalOffset));
			}

			// Vertical
			if (rectangle.Bottom + VerticalOffset > viewRect.Bottom) {
				viewRect.Y = viewRect.Y + rectangle.Bottom + VerticalOffset - viewRect.Bottom;
			}
			if (rectangle.Top + VerticalOffset < viewRect.Top) {
				viewRect.Y -= (viewRect.Top - (rectangle.Top + VerticalOffset));
			}

			SetHorizontalOffset(viewRect.X);
			SetVerticalOffset(viewRect.Y);

			rectangle.Intersect(viewRect);

			return rectangle;
		}
		#endregion
	}
}
