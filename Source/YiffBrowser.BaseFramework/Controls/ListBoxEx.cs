using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YiffBrowser.BaseFramework.Controls;

public class ListBoxEx : ListBox {



	public bool EnableKeyDown {
		get => (bool)GetValue(EnableKeyDownProperty);
		set => SetValue(EnableKeyDownProperty, value);
	}

	public static readonly DependencyProperty EnableKeyDownProperty = DependencyProperty.Register(
		nameof(EnableKeyDown),
		typeof(bool),
		typeof(ListBoxEx),
		new PropertyMetadata(true)
	);



	public bool MouseUpSelect {
		get => (bool)GetValue(MouseUpSelectProperty);
		set => SetValue(MouseUpSelectProperty, value);
	}

	public static readonly DependencyProperty MouseUpSelectProperty = DependencyProperty.Register(
		nameof(MouseUpSelect),
		typeof(bool),
		typeof(ListBoxEx),
		new PropertyMetadata(false)
	);



	public bool RightButtonSelect {
		get => (bool)GetValue(RightButtonSelectProperty);
		set => SetValue(RightButtonSelectProperty, value);
	}

	public static readonly DependencyProperty RightButtonSelectProperty = DependencyProperty.Register(
		nameof(RightButtonSelect),
		typeof(bool),
		typeof(ListBoxEx),
		new PropertyMetadata(true)
	);



	public bool RightButtonScroll {
		get => (bool)GetValue(RightButtonScrollProperty);
		set => SetValue(RightButtonScrollProperty, value);
	}

	public static readonly DependencyProperty RightButtonScrollProperty = DependencyProperty.Register(
		nameof(RightButtonScroll),
		typeof(bool),
		typeof(ListBoxEx),
		new PropertyMetadata(false)
	);


	private ScrollViewer? _scrollViewer;

	private Point _lastMousePosition;
	private bool _isRightMouseButtonDragging;
	public bool IsRightButtonScrolling => _isRightMouseButtonDragging;

	private Window? associatedWindow;

	public ListBoxEx() {
		Loaded += ListBoxEx_Loaded;
		Unloaded += ListBoxEx_Unloaded;
	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		_scrollViewer = (ScrollViewer?)GetTemplateChild("PART_ScrollViewer");
	}

	private void ListBoxEx_Loaded(object sender, RoutedEventArgs e) {
		associatedWindow = Window.GetWindow(this);
		if (associatedWindow != null) {
			associatedWindow.Deactivated += Window_Deactivated;
			associatedWindow.Activated += Window_Activated;
		}
	}

	private void ListBoxEx_Unloaded(object sender, RoutedEventArgs e) {
		if (associatedWindow != null) {
			associatedWindow.Deactivated -= Window_Deactivated;
			associatedWindow.Activated -= Window_Activated;
		}
	}

	private void Window_Deactivated(object? sender, EventArgs e) => CancelRightButtonDrag();
	private void Window_Activated(object? sender, EventArgs e) => CancelRightButtonDrag();

	private void CancelRightButtonDrag() {
		if (_isRightMouseButtonDragging) {
			_isRightMouseButtonDragging = false;
			ReleaseMouseCapture();

			Cursor = null;
		}
	}

	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
		if (RightButtonScroll) {
			Focus();
			_isRightMouseButtonDragging = CaptureMouse(); // 记录是否成功捕获
			_lastMousePosition = e.GetPosition(this);
			e.Handled = true;

			Cursor = Cursors.ScrollWE;

			return;
		}
		base.OnMouseRightButtonDown(e);
	}

	protected override void OnMouseRightButtonUp(MouseButtonEventArgs e) {
		if (_isRightMouseButtonDragging) {
			_isRightMouseButtonDragging = false;
			ReleaseMouseCapture();
			e.Handled = true;

			Cursor = null;

			return;
		}
		base.OnMouseRightButtonUp(e);
	}

	protected override void OnMouseMove(MouseEventArgs e) {
		if (RightButtonScroll) {
			if (_isRightMouseButtonDragging && _scrollViewer != null) {
				// 即使鼠标在窗口外，GetPosition(this) 依然会返回相对于 ListBox 的坐标
				Point currentPosition = e.GetPosition(this);
				Vector delta = _lastMousePosition - currentPosition;

				_scrollViewer.ScrollToHorizontalOffset(_scrollViewer.HorizontalOffset + delta.X);
				_scrollViewer.ScrollToVerticalOffset(_scrollViewer.VerticalOffset + delta.Y);
				_lastMousePosition = currentPosition;

				// 标记为已处理，防止 ListBoxItem 响应鼠标进入/离开的视觉效果
				e.Handled = true;
			}
		} else {
			base.OnMouseMove(e);
		}
	}

	protected override void OnLostMouseCapture(MouseEventArgs e) {
		_isRightMouseButtonDragging = false;
		base.OnLostMouseCapture(e);
	}

	protected override void OnLostFocus(RoutedEventArgs e) {
		_isRightMouseButtonDragging = false;
		base.OnLostFocus(e);
	}

	protected override void OnKeyDown(KeyEventArgs e) {
		if (EnableKeyDown) {
			base.OnKeyDown(e);
		}
	}

	protected override DependencyObject GetContainerForItemOverride() {
		return new ListBoxItemEx();
	}

	protected override bool IsItemItsOwnContainerOverride(object item) {
		return item is ListBoxItemEx;
	}

}

public class ListBoxItemEx : ListBoxItem {
	private static readonly MethodInfo _HandleMouseButtonDown = typeof(ListBoxItem).GetMethod("HandleMouseButtonDown", BindingFlags.NonPublic | BindingFlags.Instance)!;
	private static readonly PropertyInfo _ParentListBox = typeof(ListBoxItem).GetProperty("ParentListBox", BindingFlags.Instance | BindingFlags.NonPublic)!;

	public ListBoxEx GetParentListBox() => (ListBoxEx)_ParentListBox.GetValue(this)!;
	public void InvokeHandleMouseButtonDown(MouseButton mouseButton) => _HandleMouseButtonDown.Invoke(this, [mouseButton]);

	// 记录哪一个按键被按下了，防止左键按下右键抬起的误操作
	private MouseButton? _pressedButton;

	private Point _startPoint;

	protected override void OnMouseMove(MouseEventArgs e) {
		ListBoxEx listBox = GetParentListBox();
		if (!listBox.RightButtonScroll) {
			base.OnMouseMove(e);
		}
	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
		if (GetParentListBox().MouseUpSelect) {
			// 开启捕获，确保 MouseUp 即使在移出边界后也能触发（用于清理状态）
			_pressedButton = MouseButton.Left;
			CaptureMouse();
			e.Handled = true;
		} else {
			base.OnMouseLeftButtonDown(e);
		}
	}

	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
		ListBoxEx listBox = GetParentListBox();

		// 如果开启了右键滚动，Item 不处理 Down 事件，让它冒泡给 ListBoxEx
		if (listBox.RightButtonScroll) {
			return;
		}

		if (!listBox.RightButtonSelect) {
			return;
		}

		if (listBox.MouseUpSelect) {
			_pressedButton = MouseButton.Right;
			CaptureMouse();
			e.Handled = true;
		} else {
			base.OnMouseRightButtonDown(e);
		}
	}

	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
		HandleMouseUp(MouseButton.Left, e);
	}

	protected override void OnMouseRightButtonUp(MouseButtonEventArgs e) {
		ListBoxEx listBox = GetParentListBox();

		// 核心逻辑：如果是通过右键滚动完成的抬起，不触发选中
		if (listBox.RightButtonScroll && listBox.IsRightButtonScrolling) {
			return;
		}

		if (!listBox.RightButtonSelect) {
			return;
		}

		HandleMouseUp(MouseButton.Right, e);
	}

	private void HandleMouseUp(MouseButton button, MouseButtonEventArgs e) {
		if (GetParentListBox().MouseUpSelect) {
			// 只有当之前确实在这个元素上按下，且现在是同一个键抬起时才逻辑生效
			if (_pressedButton == button) {
				_pressedButton = null;
				ReleaseMouseCapture();

				// 核心判断：抬起时鼠标必须还在当前 Item 范围内
				if (IsMouseOver) {
					if (!e.Handled) {
						e.Handled = true;
						InvokeHandleMouseButtonDown(button);
					}
				}
			}
		} else {
			base.OnMouseUp(e);
		}
	}

	// 异常情况处理：如果捕获丢失（比如弹窗抢走焦点），重置状态
	protected override void OnLostMouseCapture(MouseEventArgs e) {
		base.OnLostMouseCapture(e);
		_pressedButton = null;
	}

}