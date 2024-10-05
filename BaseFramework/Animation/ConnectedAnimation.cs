﻿using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace BaseFramework.Animation {
	public class ConnectedAnimation {
		internal ConnectedAnimation([NotNull] string key, [NotNull] UIElement source, [NotNull] EventHandler completed) {
			Key = key ?? throw new ArgumentNullException(nameof(key));
			_source = source ?? throw new ArgumentNullException(nameof(source));
			_reportCompleted = completed ?? throw new ArgumentNullException(nameof(completed));
		}

		public string Key { get; }
		private readonly UIElement _source;
		private readonly EventHandler _reportCompleted;

		public bool TryStart([NotNull] UIElement destination) {
			return TryStart(destination, []);
		}

		public bool TryStart([NotNull] UIElement destination, [NotNull] IEnumerable<UIElement> coordinatedElements) {
			ArgumentNullException.ThrowIfNull(destination);
			ArgumentNullException.ThrowIfNull(coordinatedElements);
			if (Equals(_source, destination)) {
				return false;
			}
			// showing the animation?

			// ready to connect the animation。
			ConnectedAnimationAdorner adorner = ConnectedAnimationAdorner.FindFrom(destination);
			ConnectedVisual connectionHost = new(_source, destination);
			adorner.Children.Add(connectionHost);

			Storyboard storyboard = new();
			DoubleAnimation animation = new(0.0, 1.0, new Duration(TimeSpan.FromSeconds(10.6))) {
				EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut },
			};
			Storyboard.SetTarget(animation, connectionHost);
			Storyboard.SetTargetProperty(animation, new PropertyPath(ConnectedVisual.ProgressProperty.Name));
			storyboard.Children.Add(animation);
			storyboard.Completed += (sender, args) => {
				_reportCompleted(this, EventArgs.Empty);
				//destination.ClearValue(UIElement.VisibilityProperty);
				adorner.Children.Remove(connectionHost);
			};
			//destination.Visibility = Visibility.Hidden;
			storyboard.Begin();

			return true;
		}

		private class ConnectedVisual : DrawingVisual {
			public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
				"Progress", typeof(double), typeof(ConnectedVisual),
				new PropertyMetadata(0.0, OnProgressChanged), ValidateProgress);

			public double Progress {
				get => (double)GetValue(ProgressProperty);
				set => SetValue(ProgressProperty, value);
			}

			private static bool ValidateProgress(object value) => value is double progress && progress >= 0 && progress <= 1;

			private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
				((ConnectedVisual)d).Render((double)e.NewValue);
			}

			public ConnectedVisual([NotNull] Visual source, [NotNull] Visual destination) {
				_source = source ?? throw new ArgumentNullException(nameof(source));
				_destination = destination ?? throw new ArgumentNullException(nameof(destination));

				_sourceBrush = new VisualBrush(source) { Stretch = Stretch.Fill };
				_destinationBrush = new VisualBrush(destination) { Stretch = Stretch.Fill };
			}

			private readonly Visual _source;
			private readonly Visual _destination;
			private readonly Brush _sourceBrush;
			private readonly Brush _destinationBrush;
			private Rect _sourceBounds;
			private Rect _destinationBounds;

			protected override void OnVisualParentChanged(DependencyObject oldParent) {
				if (VisualTreeHelper.GetParent(this) == null) {
					return;
				}

				Rect sourceBounds = VisualTreeHelper.GetContentBounds(_source);
				if (sourceBounds.IsEmpty) {
					sourceBounds = VisualTreeHelper.GetDescendantBounds(_source);
				}
				_sourceBounds = new Rect(
					_source.PointToScreen(sourceBounds.TopLeft),
					_source.PointToScreen(sourceBounds.BottomRight));
				_sourceBounds = new Rect(
					PointFromScreen(_sourceBounds.TopLeft),
					PointFromScreen(_sourceBounds.BottomRight));

				Rect destinationBounds = VisualTreeHelper.GetContentBounds(_destination);
				if (destinationBounds.IsEmpty) {
					destinationBounds = VisualTreeHelper.GetDescendantBounds(_destination);
				}
				_destinationBounds = new Rect(
					_destination.PointToScreen(destinationBounds.TopLeft),
					_destination.PointToScreen(destinationBounds.BottomRight));
				_destinationBounds = new Rect(
					PointFromScreen(_destinationBounds.TopLeft),
					PointFromScreen(_destinationBounds.BottomRight));
			}

			private void Render(double progress) {
				Rect bounds = new(
					(_destinationBounds.Left - _sourceBounds.Left) * progress + _sourceBounds.Left,
					(_destinationBounds.Top - _sourceBounds.Top) * progress + _sourceBounds.Top,
					(_destinationBounds.Width - _sourceBounds.Width) * progress + _sourceBounds.Width,
					(_destinationBounds.Height - _sourceBounds.Height) * progress + _sourceBounds.Height
				);

				using DrawingContext dc = RenderOpen();
				dc.DrawRectangle(_sourceBrush, null, bounds);
				dc.PushOpacity(progress);
				dc.DrawRectangle(_destinationBrush, null, bounds);
				dc.Pop();
			}
		}

		private class ConnectedAnimationAdorner : Adorner {
			private ConnectedAnimationAdorner([NotNull] UIElement adornedElement) : base(adornedElement) {
				Children = new VisualCollection(this);
				IsHitTestVisible = false;
			}

			internal VisualCollection Children { get; }

			protected override int VisualChildrenCount => Children.Count;

			protected override Visual GetVisualChild(int index) => Children[index];

			protected override Size ArrangeOverride(Size finalSize) {
				foreach (UIElement child in Children.OfType<UIElement>()) {
					child.Arrange(new Rect(child.DesiredSize));
				}
				return finalSize;
			}

			internal static ConnectedAnimationAdorner FindFrom([NotNull] Visual visual) {
				if (Window.GetWindow(visual)?.Content is UIElement root) {
					AdornerLayer layer = AdornerLayer.GetAdornerLayer(root);
					if (layer != null) {
						ConnectedAnimationAdorner? adorner = layer.GetAdorners(root)?.OfType<ConnectedAnimationAdorner>().FirstOrDefault();
						if (adorner == null) {
							adorner = new ConnectedAnimationAdorner(root);
							layer.Add(adorner);
						}
						return adorner;
					}
				}
				throw new InvalidOperationException("指定的 Visual 尚未连接到可见的视觉树中，找不到用于承载动画的容器。");
			}

			internal static void ClearFor([NotNull] Visual visual) {
				if (Window.GetWindow(visual)?.Content is UIElement root) {
					AdornerLayer? layer = AdornerLayer.GetAdornerLayer(root);
					ConnectedAnimationAdorner? adorner = layer?.GetAdorners(root)?.OfType<ConnectedAnimationAdorner>().FirstOrDefault();
					if (adorner != null && layer != null) {
						layer.Remove(adorner);
					}
				}
			}
		}
	}
}
