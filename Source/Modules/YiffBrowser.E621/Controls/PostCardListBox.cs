using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YiffBrowser.BaseFramework.Controls;

namespace YiffBrowser.E621.Controls;

public class PostCardListBox : VariableSizedWrapGridView {

	public bool AllowSelection {
		get => (bool)GetValue(AllowSelectionProperty);
		set => SetValue(AllowSelectionProperty, value);
	}

	public static readonly DependencyProperty AllowSelectionProperty = DependencyProperty.Register(
		nameof(AllowSelection),
		typeof(bool),
		typeof(PostCardListBox),
		new PropertyMetadata(false)
	);

	private ScrollViewer? scrollViewer;

	public PostCardListBox() {
		SizeChanged += PostCardListBox_SizeChanged;

	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		scrollViewer = (ScrollViewer?)GetTemplateChild("PART_ScrollViewer");
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
		base.OnRenderSizeChanged(sizeInfo);

		//if (scrollViewer != null) {
		//	scrollViewer.InvalidateMeasure();
		//	scrollViewer.InvalidateArrange();
		//	scrollViewer.UpdateLayout();
		//}
	}

	private void PostCardListBox_SizeChanged(object sender, SizeChangedEventArgs e) {
		if (scrollViewer != null) {
			//scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - 1);
			//scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + 1);
		}
	}

	protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
		base.PrepareContainerForItemOverride(element, item);
	}

	protected override DependencyObject GetContainerForItemOverride() => new PostCardListBoxItem(this);

	protected override bool IsItemItsOwnContainerOverride(object item) => item is PostCardListBoxItem;


}

public class PostCardListBoxItem(PostCardListBox parentListBox) : ListBoxItem {
	public PostCardListBox ParentListBox { get; } = parentListBox;

	protected override void OnSelected(RoutedEventArgs e) {
		base.OnSelected(e);
	}

	protected override void OnUnselected(RoutedEventArgs e) {
		base.OnUnselected(e);
	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
		base.OnMouseLeftButtonDown(e);
	}

	protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
		base.OnMouseRightButtonDown(e);
	}

}
