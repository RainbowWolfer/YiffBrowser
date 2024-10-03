using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;

namespace BaseFramework.Interfaces {
	public interface IViewBase {
		event RoutedEventHandler Loaded;
		event DragEventHandler PreviewDragLeave;
		event StylusEventHandler PreviewStylusMove;
		event StylusEventHandler PreviewStylusInRange;
		event StylusEventHandler PreviewStylusInAirMove;
		event StylusDownEventHandler PreviewStylusDown;
		event StylusButtonEventHandler PreviewStylusButtonUp;
		event StylusButtonEventHandler PreviewStylusButtonDown;
		event QueryContinueDragEventHandler PreviewQueryContinueDrag;
		event MouseWheelEventHandler PreviewMouseWheel;
		event MouseButtonEventHandler PreviewMouseUp;
		event MouseButtonEventHandler PreviewMouseRightButtonUp;
		event MouseButtonEventHandler PreviewMouseRightButtonDown;
		event MouseEventHandler PreviewMouseMove;
		event MouseButtonEventHandler PreviewMouseLeftButtonUp;
		event MouseButtonEventHandler PreviewMouseLeftButtonDown;
		event MouseButtonEventHandler PreviewMouseDown;
		event KeyboardFocusChangedEventHandler PreviewLostKeyboardFocus;
		event KeyEventHandler PreviewKeyUp;
		event KeyEventHandler PreviewKeyDown;
		event StylusEventHandler PreviewStylusOutOfRange;
		event StylusSystemGestureEventHandler PreviewStylusSystemGesture;
		event TextCompositionEventHandler PreviewTextInput;
		event KeyboardFocusChangedEventHandler PreviewGotKeyboardFocus;
		event EventHandler<TouchEventArgs> TouchLeave;
		event EventHandler<TouchEventArgs> TouchEnter;
		event EventHandler<TouchEventArgs> TouchDown;
		event TextCompositionEventHandler TextInput;
		event StylusEventHandler StylusUp;
		event StylusSystemGestureEventHandler StylusSystemGesture;
		event StylusEventHandler StylusOutOfRange;
		event StylusEventHandler StylusMove;
		event StylusEventHandler StylusLeave;
		event StylusEventHandler StylusInRange;
		event StylusEventHandler StylusInAirMove;
		event StylusEventHandler StylusEnter;
		event StylusButtonEventHandler StylusButtonUp;
		event StylusButtonEventHandler StylusButtonDown;
		event QueryCursorEventHandler QueryCursor;
		event QueryContinueDragEventHandler QueryContinueDrag;
		event EventHandler<TouchEventArgs> PreviewTouchUp;
		event EventHandler<TouchEventArgs> PreviewTouchMove;
		event EventHandler<TouchEventArgs> PreviewTouchDown;
		event StylusEventHandler PreviewStylusUp;
		event GiveFeedbackEventHandler PreviewGiveFeedback;
		event DragEventHandler PreviewDrop;
		event DragEventHandler DragEnter;
		event KeyboardFocusChangedEventHandler LostKeyboardFocus;
		event MouseEventHandler LostMouseCapture;
		event StylusEventHandler LostStylusCapture;
		event EventHandler<TouchEventArgs> LostTouchCapture;
		event EventHandler<ManipulationBoundaryFeedbackEventArgs> ManipulationBoundaryFeedback;
		event EventHandler<ManipulationCompletedEventArgs> ManipulationCompleted;
		event EventHandler<ManipulationDeltaEventArgs> ManipulationDelta;
		event EventHandler<ManipulationInertiaStartingEventArgs> ManipulationInertiaStarting;
		event EventHandler<ManipulationStartedEventArgs> ManipulationStarted;
		event EventHandler<ManipulationStartingEventArgs> ManipulationStarting;
		event MouseButtonEventHandler MouseDown;
		event MouseEventHandler MouseEnter;
		event MouseEventHandler MouseLeave;
		event MouseButtonEventHandler MouseLeftButtonDown;
		event MouseButtonEventHandler MouseLeftButtonUp;
		event MouseEventHandler MouseMove;
		event MouseButtonEventHandler MouseRightButtonDown;
		event MouseButtonEventHandler MouseRightButtonUp;
		event MouseButtonEventHandler MouseUp;
		event MouseWheelEventHandler MouseWheel;
		event DragEventHandler PreviewDragEnter;
		event RoutedEventHandler LostFocus;
		event DragEventHandler PreviewDragOver;
		event EventHandler LayoutUpdated;
		event KeyEventHandler KeyDown;
		event DragEventHandler DragLeave;
		event DragEventHandler DragOver;
		event DragEventHandler Drop;
		event DependencyPropertyChangedEventHandler FocusableChanged;
		event GiveFeedbackEventHandler GiveFeedback;
		event RoutedEventHandler GotFocus;
		event KeyboardFocusChangedEventHandler GotKeyboardFocus;
		event MouseEventHandler GotMouseCapture;
		event StylusEventHandler GotStylusCapture;
		event EventHandler<TouchEventArgs> GotTouchCapture;
		event DependencyPropertyChangedEventHandler IsEnabledChanged;
		event DependencyPropertyChangedEventHandler IsHitTestVisibleChanged;
		event DependencyPropertyChangedEventHandler IsKeyboardFocusedChanged;
		event DependencyPropertyChangedEventHandler IsKeyboardFocusWithinChanged;
		event DependencyPropertyChangedEventHandler IsMouseCapturedChanged;
		event DependencyPropertyChangedEventHandler IsMouseCaptureWithinChanged;
		event DependencyPropertyChangedEventHandler IsMouseDirectlyOverChanged;
		event DependencyPropertyChangedEventHandler IsStylusCapturedChanged;
		event DependencyPropertyChangedEventHandler IsStylusCaptureWithinChanged;
		event DependencyPropertyChangedEventHandler IsStylusDirectlyOverChanged;
		event DependencyPropertyChangedEventHandler IsVisibleChanged;
		event KeyEventHandler KeyUp;
		event StylusDownEventHandler StylusDown;
		event EventHandler<TouchEventArgs> TouchUp;
		event EventHandler<TouchEventArgs> TouchMove;

		event MouseButtonEventHandler PreviewMouseDoubleClick;
		event MouseButtonEventHandler MouseDoubleClick;
		event DependencyPropertyChangedEventHandler DataContextChanged;
		event EventHandler Initialized;
		event ContextMenuEventHandler ContextMenuClosing;
		event SizeChangedEventHandler SizeChanged;
		event EventHandler<DataTransferEventArgs> SourceUpdated;
		event EventHandler<DataTransferEventArgs> TargetUpdated;
		event ToolTipEventHandler ToolTipClosing;
		event ContextMenuEventHandler ContextMenuOpening;
		event RequestBringIntoViewEventHandler RequestBringIntoView;
		event RoutedEventHandler Unloaded;
		event ToolTipEventHandler ToolTipOpening;

		object DataContext { get; set; }
		bool IsLoaded { get; }
		bool IsInitialized { get; }
	}

	public interface IUserControlBase : IViewBase {
		UserControl UserControl { get; }
	}

	public interface IWindowBase : IViewBase {
		event EventHandler Activated;
		event CancelEventHandler Closing;
		event EventHandler ContentRendered;
		event EventHandler Deactivated;
		event DpiChangedEventHandler DpiChanged;
		event EventHandler LocationChanged;
		event EventHandler Closed;
		event EventHandler StateChanged;
		event EventHandler SourceInitialized;
		SizeToContent SizeToContent { get; set; }
		bool ShowInTaskbar { get; set; }
		bool ShowActivated { get; set; }
		Rect RestoreBounds { get; }
		ResizeMode ResizeMode { get; set; }
		bool AllowsTransparency { get; set; }
		WindowCollection OwnedWindows { get; }
		double Left { get; set; }
		bool? DialogResult { get; set; }
		TaskbarItemInfo TaskbarItemInfo { get; set; }
		Window Owner { get; set; }
		string Title { get; set; }
		bool IsActive { get; }
		bool Topmost { get; set; }
		WindowStartupLocation WindowStartupLocation { get; set; }
		WindowState WindowState { get; set; }
		WindowStyle WindowStyle { get; set; }
		ImageSource Icon { get; set; }
		double Top { get; set; }

		Window Window { get; }

		bool Activate();
		void Close();
		void DragMove();
		void Hide();
		void Show();
		bool? ShowDialog();
	}

}
