using HandyControl.Controls;
using RW.Common.WPF.Controls;
using System.Windows;
using System.Windows.Controls;

namespace YiffBrowser.BaseFramework.Controls;

public class PaginationEx : Pagination {

	static PaginationEx() {
		DefaultStyleKeyProperty.OverrideMetadata(typeof(PaginationEx), new FrameworkPropertyMetadata(typeof(PaginationEx)));
	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (GetTemplateChild("PART_ButtonLeft") is Button button_left) {
			button_left.Content = new FontIcon("\uE96F");
			button_left.Padding = new Thickness(0);
		}

		if (GetTemplateChild("PART_ButtonRight") is Button button_right) {
			button_right.Content = new FontIcon("\uE970");
			button_right.Padding = new Thickness(0);
		}

		if(GetTemplateChild("PART_Jump") is NumericUpDown numericUpDown) {
			numericUpDown.MinWidth = 60;
		}

	}
}
