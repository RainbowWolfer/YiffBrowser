using System.ComponentModel;
using System.Windows;
using YiffBrowser.BaseFramework.Interfaces;

namespace YiffBrowser.BaseFramework.Controls;

public class VariableSizedWrapGridView : ListBoxEx {
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
		base.PrepareContainerForItemOverride(element, item);

		if (item is IVariableSizedGridItem model) {
			UpdateSpans(element, model);

			model.PropertyChanged -= OnItemPropertyChanged;  
			model.PropertyChanged += OnItemPropertyChanged;

			element.SetValue(TagProperty, model);
		}
	}

	private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e) {
		if (sender is not IVariableSizedGridItem model) {
			return;
		}
		if (e.PropertyName is (nameof(IVariableSizedGridItem.ColSpan)) or (nameof(IVariableSizedGridItem.RowSpan))) {
			if (ItemContainerGenerator.ContainerFromItem(model) is UIElement container) {
				UpdateSpans(container, model);
			}
		}
	}

	private void UpdateSpans(DependencyObject container, IVariableSizedGridItem model) {
		container.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, Math.Max(1, model.ColSpan));
		container.SetValue(VariableSizedWrapGrid.RowSpanProperty, Math.Max(1, model.RowSpan));
	}
}
