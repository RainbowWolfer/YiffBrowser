using BaseFramework.Interfaces;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace BaseFramework.Controls {
	public class VariableSizedWrapGridView : ListBox {
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
			if (item is IVariableSizedGridItem model) {
				try {
					element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, Math.Clamp(model.ColSpan, 1, int.MaxValue));
					element.SetValue(VariableSizedWrapGrid.RowSpanProperty, Math.Clamp(model.RowSpan, 1, int.MaxValue));
				} catch (Exception ex) {
					Debug.WriteLine(ex);
					element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 1);
					element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 1);
				} finally {
					//element.SetValue(VerticalAlignmentProperty, VerticalAlignment.Stretch);
					//element.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
					element.SetValue(VerticalContentAlignmentProperty, VerticalAlignment.Stretch);
					element.SetValue(HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
					base.PrepareContainerForItemOverride(element, item);
				}
			}
		}
	}
}
