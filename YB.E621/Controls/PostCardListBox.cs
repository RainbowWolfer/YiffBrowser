using BaseFramework.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YB.E621.Controls {
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

		public PostCardListBox() {

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

}
