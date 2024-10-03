using BaseFramework.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace YB.E621.Controls {
	public class PostCardListBox : VariableSizedWrapGridView {
		public PostCardListBox() {

		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
			base.PrepareContainerForItemOverride(element, item);
		}

		protected override DependencyObject GetContainerForItemOverride() {
			return new PostCardListBoxItem();
		}

		protected override bool IsItemItsOwnContainerOverride(object item) {
			return item is PostCardListBoxItem;
		}

	}

	public class PostCardListBoxItem : ListBoxItem {

		public PostCardListBoxItem() {

		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
			base.OnMouseLeftButtonDown(e);
		}

		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
			base.OnMouseRightButtonDown(e);
		}

	}

}
