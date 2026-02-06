using RW.Common.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace YB.E621.Controls;

internal class TabControlEx : TabControl {

	public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
		nameof(SelectedItems),
		typeof(IEnumerable<object>),
		typeof(TabControlEx),
		new PropertyMetadata(null)
	);

	public IEnumerable<object> SelectedItems {
		get => (IEnumerable<object>)GetValue(SelectedItemsProperty);
		set => SetValue(SelectedItemsProperty, value);
	}

	public ICommand TabsManageCommand {
		get => (ICommand)GetValue(TabsManageCommandProperty);
		set => SetValue(TabsManageCommandProperty, value);
	}

	public static readonly DependencyProperty TabsManageCommandProperty = DependencyProperty.Register(
		nameof(TabsManageCommand),
		typeof(ICommand),
		typeof(TabControlEx),
		new PropertyMetadata(null)
	);



	public ContextMenu HeaderContextMenu {
		get => (ContextMenu)GetValue(HeaderContextMenuProperty);
		set => SetValue(HeaderContextMenuProperty, value);
	}

	public static readonly DependencyProperty HeaderContextMenuProperty = DependencyProperty.Register(
		nameof(HeaderContextMenu),
		typeof(ContextMenu),
		typeof(TabControlEx),
		new PropertyMetadata(null)
	);




	public DataTemplate TabListItemTemplate {
		get => (DataTemplate)GetValue(TabListItemTemplateProperty);
		set => SetValue(TabListItemTemplateProperty, value);
	}

	public static readonly DependencyProperty TabListItemTemplateProperty = DependencyProperty.Register(
		nameof(TabListItemTemplate),
		typeof(DataTemplate),
		typeof(TabControlEx),
		new PropertyMetadata(null)
	);

	private TabItemEx? _lastClickedItem;
	private bool _isHandlingInternalSelection; // 逻辑锁

	private ScrollViewerEx? scrollViewer;

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		scrollViewer = GetTemplateChild("tabHeaderScrollViewer") as ScrollViewerEx;

		if (GetTemplateChild("tabItemsList") is ListBox listBox) {

		}

		if (GetTemplateChild("headerPanel") is FrameworkElement headerPanelBorder) {
			headerPanelBorder.MouseDown += HeaderPanelBorder_MouseDown;
		}
	}

	private void HeaderPanelBorder_MouseDown(object sender, MouseButtonEventArgs e) {
		if (e.ChangedButton is MouseButton.Left) {
			ClearAllSecondarySelection();
			UpdateSelectedItems();
		}
	}

	internal void UpdateSelectedItems() {
		List<object> currentSelected = [];
		foreach (object? item in Items) {
			if (ItemContainerGenerator.ContainerFromItem(item) is TabItemEx container && container.IsSelected2) {
				currentSelected.Add(item);
			}
		}

		SelectedItems = currentSelected;
	}

	internal void HandleMultiSelect(TabItemEx currentItem, MultiSelectMode mode) {
		// 在开始处理多选逻辑前，加上锁
		_isHandlingInternalSelection = true;
		try {
			ReadOnlyCollection<object> items = ItemContainerGenerator.Items;
			int currentIndex = ItemContainerGenerator.IndexFromContainer(currentItem);

			if (mode == MultiSelectMode.None) {
				ClearAllSecondarySelection();
				currentItem.IsSelected2 = true;
				_lastClickedItem = currentItem;
			} else if (mode == MultiSelectMode.Ctrl) {
				currentItem.IsSelected2 = !currentItem.IsSelected2;
				_lastClickedItem = currentItem;
			} else if (mode == MultiSelectMode.Shift) {
				if (_lastClickedItem == null) {
					currentItem.IsSelected2 = true;
					_lastClickedItem = currentItem;
				} else {
					int anchorIndex = ItemContainerGenerator.IndexFromContainer(_lastClickedItem);
					int start = Math.Min(anchorIndex, currentIndex);
					int end = Math.Max(anchorIndex, currentIndex);

					ClearAllSecondarySelection();
					for (int i = start; i <= end; i++) {
						if (ItemContainerGenerator.ContainerFromIndex(i) is TabItemEx container) {
							container.IsSelected2 = true;
						}
					}
				}
			}
		} finally {
			// 这里不解锁，因为接下来的 base.OnMouseLeftButtonDown 
			// 引起的 SelectionChanged 还需要用到这个锁
			UpdateSelectedItems();
		}
	}

	private void ClearAllSecondarySelection() {
		foreach (object? item in Items) {
			if (ItemContainerGenerator.ContainerFromItem(item) is TabItemEx container) {
				container.IsSelected2 = false;
			}
		}
		// 这里不需要单独调 UpdateSelectedItems，因为通常调用它的地方后面会统一调一次
	}

	protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
		base.OnSelectionChanged(e);

		// 如果是多选逻辑触发的切换，跳过清空逻辑
		if (_isHandlingInternalSelection) {
			_isHandlingInternalSelection = false; // 用完即解锁
		} else {
			// 说明是键盘切换、代码切换或普通单击切换
			if (SelectedItem != null) {
				ClearAllSecondarySelection();
				if (ItemContainerGenerator.ContainerFromItem(SelectedItem) is TabItemEx container) {
					container.IsSelected2 = true;
					_lastClickedItem = container;
				}
			}
			UpdateSelectedItems();
		}

		Dispatcher.BeginInvoke(new Action(() => {
			if (ItemContainerGenerator.ContainerFromItem(SelectedItem) is FrameworkElement container) {
				scrollViewer?.ScrollToElement(container, 10, TabStripPlacement);
			}
		}), DispatcherPriority.Loaded);
	}

	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e) {
		base.OnItemsChanged(e);
		// 当标签页被关闭/删除时，同步更新多选列表
		UpdateSelectedItems();
	}

	protected override void OnPreviewKeyDown(KeyEventArgs e) {
		base.OnPreviewKeyDown(e);

		// 逻辑：如果按下 Ctrl + Tab (正向) 或 Ctrl + Shift + Tab (反向)
		if (e.Key == Key.Tab && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
			int count = Items.Count;
			if (count <= 1) {
				return;
			}

			int index = SelectedIndex;

			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
				// 反向切换：Ctrl + Shift + Tab
				index = (index - 1 + count) % count;
			} else {
				// 正向切换：Ctrl + Tab
				index = (index + 1) % count;
			}

			SelectedIndex = index;
			e.Handled = true; // 拦截事件，不让焦点跳出内容区
			return;
		}
	}

	protected override bool IsItemItsOwnContainerOverride(object item) {
		return item is TabItemEx;
	}

	protected override DependencyObject GetContainerForItemOverride() {
		return new TabItemEx();
	}
}

public class TabItemEx : TabItem {


	public bool IsSelected2 {
		get => (bool)GetValue(IsSelected2Property);
		set => SetValue(IsSelected2Property, value);
	}

	public static readonly DependencyProperty IsSelected2Property = DependencyProperty.Register(
		nameof(IsSelected2),
		typeof(bool),
		typeof(TabItemEx),
		new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsSelected2Changed)
	);

	private static void OnIsSelected2Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) {

	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {

		if (ItemsControl.ItemsControlFromItemContainer(this) is TabControlEx tabControl) {
			if (KeyboardHelper.ShiftPressed) {
				tabControl.HandleMultiSelect(this, MultiSelectMode.Shift);
			} else if (KeyboardHelper.ControlPressed) {
				tabControl.HandleMultiSelect(this, MultiSelectMode.Ctrl);
			} else {
				// 普通点击时，不需要锁，直接让 OnSelectionChanged 处理即可
				// 或者也走 HandleMultiSelect 以保持逻辑统一
				tabControl.HandleMultiSelect(this, MultiSelectMode.None);
			}
		}

		base.OnMouseLeftButtonDown(e);
	}

	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
		if (ItemsControl.ItemsControlFromItemContainer(this) is TabControlEx tabControl) {
			// 逻辑：如果当前项已经是 IsSelected2，则保持多选状态不变
			// 如果当前项不是 IsSelected2，则清空其他，只选自己
			if (!IsSelected2) {
				tabControl.HandleMultiSelect(this, MultiSelectMode.None);
			}
		}

		base.OnMouseRightButtonDown(e);
	}

	protected override void OnContextMenuClosing(ContextMenuEventArgs e) {
		base.OnContextMenuClosing(e);
	}
}

internal enum MultiSelectMode { None, Ctrl, Shift }