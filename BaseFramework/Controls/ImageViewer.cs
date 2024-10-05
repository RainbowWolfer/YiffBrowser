using BaseFramework.Helpers;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BaseFramework.Controls {
	[TemplatePart(Name = ElementPanelMain, Type = typeof(Panel))]
	[TemplatePart(Name = ElementImageMain, Type = typeof(Image))]
	public class ImageViewer : Control, IDisposable {
		#region Constants

		private const string ElementPanelMain = "PART_PanelMain";

		private const string ElementImageMain = "PART_ImageMain";

		private const double ScaleInternal = 0.1;

		#endregion Constants

		#region Data

		private Panel? _panelMain;
		private Image? _imageMain;

		private bool _canMoveX;
		private bool _canMoveY;
		private Thickness _imgActualMargin;
		private double _imgActualRotate;
		private double _imgActualScale = 1;
		private Point _imgCurrentPoint;
		private bool _imgIsMouseDown;
		private Thickness _imgMouseDownMargin;
		private Point _imgMouseDownPoint;
		private double _imgWidHeiScale;
		private bool _isOblique;
		private double _scaleInternalHeight;
		private double _scaleInternalWidth;

		private DispatcherTimer? _dispatcher;

		private bool _isLoaded;
		private MouseBinding? _mouseMoveBinding;

		private bool _isDisposed;

		public static RoutedCommand MouseMoveCommand { get; } = new(nameof(MouseMove), typeof(ImageViewer));

		#endregion Data

		public ImageViewer() {
			CommandBindings.Add(new CommandBinding(MouseMoveCommand, ImageMain_OnMouseDown));
			OnMoveGestureChanged(MoveGesture);

			Loaded += (s, e) => {
				_isLoaded = true;
				Init();
			};
		}

		#region Properties

		public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
			nameof(ImageSource), typeof(BitmapImage), typeof(ImageViewer), new PropertyMetadata(default(BitmapImage), OnImageSourceChanged));

		public static readonly DependencyProperty UriProperty = DependencyProperty.Register(
			nameof(Uri), typeof(Uri), typeof(ImageViewer), new PropertyMetadata(default(Uri), OnUriChanged));

		public static readonly DependencyProperty MoveGestureProperty = DependencyProperty.Register(
			nameof(MoveGesture), typeof(MouseGesture), typeof(ImageViewer), new UIPropertyMetadata(new MouseGesture(MouseAction.LeftClick), OnMoveGestureChanged));

		internal static readonly DependencyProperty ImgPathProperty = DependencyProperty.Register(
			nameof(ImgPath), typeof(string), typeof(ImageViewer), new PropertyMetadata(default(string)));

		internal static readonly DependencyProperty ImgSizeProperty = DependencyProperty.Register(
			nameof(ImgSize), typeof(long), typeof(ImageViewer), new PropertyMetadata(-1L));

		internal static readonly DependencyProperty ImageContentProperty = DependencyProperty.Register(
			nameof(ImageContent), typeof(object), typeof(ImageViewer), new PropertyMetadata(default(object)));

		internal static readonly DependencyProperty ImageMarginProperty = DependencyProperty.Register(
			nameof(ImageMargin), typeof(Thickness), typeof(ImageViewer), new PropertyMetadata(default(Thickness)));

		internal static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(
			nameof(ImageWidth), typeof(double), typeof(ImageViewer), new PropertyMetadata(0d));

		internal static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register(
			nameof(ImageHeight), typeof(double), typeof(ImageViewer), new PropertyMetadata(0d));

		internal static readonly DependencyProperty ImageScaleProperty = DependencyProperty.Register(
			nameof(ImageScale), typeof(double), typeof(ImageViewer), new PropertyMetadata(1d, OnImageScaleChanged));

		internal static readonly DependencyProperty ScaleStrProperty = DependencyProperty.Register(
			nameof(ScaleStr), typeof(string), typeof(ImageViewer), new PropertyMetadata("100%"));

		internal static readonly DependencyProperty ImageRotateProperty = DependencyProperty.Register(
			nameof(ImageRotate), typeof(double), typeof(ImageViewer), new PropertyMetadata(0d));

		[ValueSerializer(typeof(MouseGestureValueSerializer))]
		[TypeConverter(typeof(MouseGestureConverter))]
		public MouseGesture MoveGesture {
			get => (MouseGesture)GetValue(MoveGestureProperty);
			set => SetValue(MoveGestureProperty, value);
		}

		public BitmapImage? ImageSource {
			get => (BitmapImage)GetValue(ImageSourceProperty);
			set => SetValue(ImageSourceProperty, value);
		}

		public Uri Uri {
			get => (Uri)GetValue(UriProperty);
			set => SetValue(UriProperty, value);
		}

		internal object ImageContent {
			get => GetValue(ImageContentProperty);
			set => SetValue(ImageContentProperty, value);
		}

		internal string ImgPath {
			get => (string)GetValue(ImgPathProperty);
			set => SetValue(ImgPathProperty, value);
		}

		internal long ImgSize {
			get => (long)GetValue(ImgSizeProperty);
			set => SetValue(ImgSizeProperty, value);
		}

		internal Thickness ImageMargin {
			get => (Thickness)GetValue(ImageMarginProperty);
			set => SetValue(ImageMarginProperty, value);
		}

		internal double ImageWidth {
			get => (double)GetValue(ImageWidthProperty);
			set => SetValue(ImageWidthProperty, value);
		}

		internal double ImageHeight {
			get => (double)GetValue(ImageHeightProperty);
			set => SetValue(ImageHeightProperty, value);
		}

		internal double ImageScale {
			get => (double)GetValue(ImageScaleProperty);
			set => SetValue(ImageScaleProperty, value);
		}

		internal string ScaleStr {
			get => (string)GetValue(ScaleStrProperty);
			set => SetValue(ScaleStrProperty, value);
		}

		internal double ImageRotate {
			get => (double)GetValue(ImageRotateProperty);
			set => SetValue(ImageRotateProperty, value);
		}

		private double ImageOriWidth { get; set; }

		private double ImageOriHeight { get; set; }

		#endregion

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

			_panelMain = (Panel)GetTemplateChild(ElementPanelMain);
			_imageMain = (Image)GetTemplateChild(ElementImageMain);

			if (_imageMain != null) {
				RotateTransform t = new();
				BindingOperations.SetBinding(t, RotateTransform.AngleProperty, new Binding(ImageRotateProperty.Name) { Source = this });
				_imageMain.LayoutTransform = t;
			}

		}

		private static void OnImageScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is ImageViewer imageViewer && e.NewValue is double newValue) {
				imageViewer.ImageWidth = imageViewer.ImageOriWidth * newValue;
				imageViewer.ImageHeight = imageViewer.ImageOriHeight * newValue;
				imageViewer.ScaleStr = $"{newValue * 100:#0}%";
			}
		}

		private void Init() {
			if (ImageSource == null || !_isLoaded) {
				return;
			}

			if (ImageSource.IsDownloading) {
				_dispatcher = new DispatcherTimer(DispatcherPriority.ApplicationIdle) {
					Interval = TimeSpan.FromSeconds(1)
				};
				_dispatcher.Tick += Dispatcher_Tick;
				_dispatcher.Start();

				return;
			}

			double width;
			double height;

			if (!_isOblique) {
				width = ImageSource.PixelWidth;
				height = ImageSource.PixelHeight;
			} else {
				width = ImageSource.PixelHeight;
				height = ImageSource.PixelWidth;
			}

			ImageWidth = width;
			ImageHeight = height;
			ImageOriWidth = width;
			ImageOriHeight = height;
			_scaleInternalWidth = ImageOriWidth * ScaleInternal;
			_scaleInternalHeight = ImageOriHeight * ScaleInternal;

			if (Math.Abs(height - 0) < 0.001 || Math.Abs(width - 0) < 0.001) {
				MessageBox.Show("ErrorImgSize");
				return;
			}

			_imgWidHeiScale = width / height;
			double scaleWindow = ActualWidth / ActualHeight;
			ImageScale = 1;

			if (_imgWidHeiScale > scaleWindow) {
				if (width > ActualWidth) {
					ImageScale = ActualWidth / width;
				}
			} else if (height > ActualHeight) {
				ImageScale = ActualHeight / height;
			}

			ImageMargin = new Thickness((ActualWidth - ImageWidth) / 2, (ActualHeight - ImageHeight) / 2, 0, 0);

			_imgActualScale = ImageScale;
			_imgActualMargin = ImageMargin;
		}

		private void Dispatcher_Tick(object? sender, EventArgs e) {
			if (_dispatcher == null) {
				return;
			}

			if (ImageSource == null || !_isLoaded) {
				_dispatcher.Stop();
				_dispatcher.Tick -= Dispatcher_Tick;
				_dispatcher = null;
				return;
			}

			if (!ImageSource.IsDownloading) {
				_dispatcher.Stop();
				_dispatcher.Tick -= Dispatcher_Tick;
				_dispatcher = null;
				Init();
			}
		}

		private void ButtonActual_OnClick(object sender, RoutedEventArgs e) {
			DoubleAnimation scaleAnimation = AnimationHelper.CreateAnimation(1);
			scaleAnimation.FillBehavior = FillBehavior.Stop;
			_imgActualScale = 1;
			scaleAnimation.Completed += (s, e1) => {
				ImageScale = 1;
				_canMoveX = ImageWidth > ActualWidth;
				_canMoveY = ImageHeight > ActualHeight;
			};
			Thickness thickness = new((ActualWidth - ImageOriWidth) / 2, (ActualHeight - ImageOriHeight) / 2, 0, 0);
			ThicknessAnimation marginAnimation = AnimationHelper.CreateAnimation(thickness);
			marginAnimation.FillBehavior = FillBehavior.Stop;
			_imgActualMargin = thickness;
			marginAnimation.Completed += (s, e1) => { ImageMargin = thickness; };

			BeginAnimation(ImageScaleProperty, scaleAnimation);
			BeginAnimation(ImageMarginProperty, marginAnimation);
		}

		public void Reduce(object sender, RoutedEventArgs e) => ScaleImg(false);

		public void Enlarge(object sender, RoutedEventArgs e) => ScaleImg(true);

		public void RotateLeft(object sender, RoutedEventArgs e) => RotateImg(_imgActualRotate - 90);

		public void RotateRight(object sender, RoutedEventArgs e) => RotateImg(_imgActualRotate + 90);

		protected override void OnMouseMove(MouseEventArgs e) => MoveImg();

		protected override void OnMouseWheel(MouseWheelEventArgs e) => ScaleImg(e.Delta > 0);

		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
			base.OnRenderSizeChanged(sizeInfo);

			OnRenderSizeChanged();
		}

		private void OnRenderSizeChanged() {
			if (ImageWidth < 0.001 || ImageHeight < 0.001) {
				return;
			}

			_canMoveX = true;
			_canMoveY = true;

			double marginX = ImageMargin.Left;
			double marginY = ImageMargin.Top;

			if (ImageWidth <= ActualWidth) {
				_canMoveX = false;
				marginX = (ActualWidth - ImageWidth) / 2;
			}

			if (ImageHeight <= ActualHeight) {
				_canMoveY = false;
				marginY = (ActualHeight - ImageHeight) / 2;
			}

			ImageMargin = new Thickness(marginX, marginY, 0, 0);
			_imgActualMargin = ImageMargin;
		}

		private void ImageMain_OnMouseDown(object sender, ExecutedRoutedEventArgs e) {
			_imgMouseDownPoint = Mouse.GetPosition(_panelMain);
			_imgMouseDownMargin = ImageMargin;
			_imgIsMouseDown = true;
		}

		protected override void OnPreviewMouseUp(MouseButtonEventArgs e) => _imgIsMouseDown = false;

		private void ScaleImg(bool isEnlarge) {
			if (Mouse.LeftButton == MouseButtonState.Pressed) {
				return;
			}

			double oldImageWidth = ImageWidth;
			double olgImageHeight = ImageHeight;

			double tempScale = isEnlarge ? _imgActualScale + ScaleInternal : _imgActualScale - ScaleInternal;
			if (Math.Abs(tempScale) < ScaleInternal) {
				tempScale = ScaleInternal;
			} else if (Math.Abs(tempScale) > 50) {
				tempScale = 50;
			}

			ImageScale = tempScale;

			Point posCanvas = Mouse.GetPosition(_panelMain);
			Point posImg = new(posCanvas.X - _imgActualMargin.Left, posCanvas.Y - _imgActualMargin.Top);

			double marginX = .5 * _scaleInternalWidth;
			double marginY = .5 * _scaleInternalHeight;

			if (ImageWidth > ActualWidth) {
				_canMoveX = true;
				if (ImageHeight > ActualHeight) {
					_canMoveY = true;
					marginX = posImg.X / oldImageWidth * _scaleInternalWidth;
					marginY = posImg.Y / olgImageHeight * _scaleInternalHeight;
				} else {
					_canMoveY = false;
				}
			} else {
				_canMoveY = ImageHeight > ActualHeight;
				_canMoveX = false;
			}

			Thickness thickness;
			if (isEnlarge) {
				thickness = new Thickness(_imgActualMargin.Left - marginX, _imgActualMargin.Top - marginY, 0, 0);
			} else {
				double marginActualX = _imgActualMargin.Left + marginX;
				double marginActualY = _imgActualMargin.Top + marginY;
				double subX = ImageWidth - ActualWidth;
				double subY = ImageHeight - ActualHeight;

				if (Math.Abs(ImageMargin.Left) < 0.001) {
					marginActualX = _imgActualMargin.Left * _scaleInternalWidth;
				}

				if (Math.Abs(ImageMargin.Top) < 0.001) {
					marginActualY = _imgActualMargin.Top * _scaleInternalHeight;
				}

				if (subX < 0.001) {
					marginActualX = (ActualWidth - ImageWidth) / 2;
				}

				if (subY < 0.001) {
					marginActualY = (ActualHeight - ImageHeight) / 2;
				}

				thickness = new Thickness(marginActualX, marginActualY, 0, 0);
			}

			ImageMargin = thickness;
			_imgActualScale = tempScale;
			_imgActualMargin = thickness;
		}

		private void RotateImg(double rotate) {
			_imgActualRotate = rotate;

			_isOblique = ((int)_imgActualRotate - 90) % 180 == 0;
			Init();

			DoubleAnimation animation = AnimationHelper.CreateAnimation(rotate);
			animation.Completed += (s, e1) => { ImageRotate = rotate; };
			animation.FillBehavior = FillBehavior.Stop;
			BeginAnimation(ImageRotateProperty, animation);
		}

		private MouseButtonState GetMouseButtonState() => MoveGesture.MouseAction switch {
			MouseAction.LeftClick => Mouse.LeftButton,
			MouseAction.RightClick => Mouse.RightButton,
			MouseAction.MiddleClick => Mouse.MiddleButton,
			_ => Mouse.LeftButton
		};

		private void MoveImg() {
			_imgCurrentPoint = Mouse.GetPosition(_panelMain);

			if (GetMouseButtonState() == MouseButtonState.Released) {
				return;
			}

			if (_imgIsMouseDown) {
				double subX = _imgCurrentPoint.X - _imgMouseDownPoint.X;
				double subY = _imgCurrentPoint.Y - _imgMouseDownPoint.Y;

				double marginX = _imgMouseDownMargin.Left;
				if (ImageWidth > ActualWidth) {
					marginX = _imgMouseDownMargin.Left + subX;
					if (marginX >= 0) {
						marginX = 0;
					} else if (-marginX + ActualWidth >= ImageWidth) {
						marginX = ActualWidth - ImageWidth;
					}

					_canMoveX = true;
				}

				double marginY = _imgMouseDownMargin.Top;
				if (ImageHeight > ActualHeight) {
					marginY = _imgMouseDownMargin.Top + subY;
					if (marginY >= 0) {
						marginY = 0;
					} else if (-marginY + ActualHeight >= ImageHeight) {
						marginY = ActualHeight - ImageHeight;
					}

					_canMoveY = true;
				}

				ImageMargin = new Thickness(marginX, marginY, 0, 0);
				_imgActualMargin = ImageMargin;
			}
		}

		private static void OnMoveGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((ImageViewer)d).OnMoveGestureChanged((MouseGesture)e.NewValue);
		}

		private void OnMoveGestureChanged(MouseGesture newValue) {
			InputBindings.Remove(_mouseMoveBinding);
			_mouseMoveBinding = new MouseBinding(MouseMoveCommand, newValue);
			InputBindings.Add(_mouseMoveBinding);
		}

		private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((ImageViewer)d).OnImageSourceChanged();
		}

		private void OnImageSourceChanged() {
			Init();
		}

		private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			((ImageViewer)d).OnUriChanged((Uri)e.NewValue);
		}

		private void OnUriChanged(Uri newValue) {
			ImageSource = newValue is not null ? GetBitmapImage(newValue) : null;
			if (ImageSource is not null && newValue!.IsAbsoluteUri) {
				ImgPath = newValue.AbsolutePath;
				if (File.Exists(ImgPath)) {
					ImgSize = new FileInfo(ImgPath).Length;
				}
			} else {
				ImgPath = string.Empty;
				ImgSize = 0;
			}

			static BitmapImage? GetBitmapImage(Uri source) {
				try {
					return new BitmapImage(source);
				} catch {
					return null;
				}
			}
		}

		protected virtual void Dispose(bool disposing) {
			if (!_isDisposed) {
				if (disposing) {
					ImageSource = null;
					_imageMain!.Source = null;
					_imageMain.UpdateLayout();
				}

				_isDisposed = true;
			}
		}

		public void Dispose() {
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
