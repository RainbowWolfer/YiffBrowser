using BaseFramework.Models;
using System.Windows;
using System.Windows.Controls;

namespace BaseFramework.Extensions {

	public static class GridDefinitionExtension {

		public static GridDefinitionModel GetModel(DependencyObject obj) {
			return (GridDefinitionModel)obj.GetValue(ModelProperty);
		}

		public static void SetModel(DependencyObject obj, GridDefinitionModel value) {
			obj.SetValue(ModelProperty, value);
		}

		public static readonly DependencyProperty ModelProperty = DependencyProperty.RegisterAttached(
			"Model",
			typeof(GridDefinitionModel),
			typeof(GridDefinitionExtension),
			new PropertyMetadata(null, OnModelChanged)
		);

		private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			GridDefinitionModel model = (GridDefinitionModel)e.NewValue;
			model.PropertyChanged += (s, e) => {
				if (e.PropertyName != nameof(GridDefinitionModel.IsExpanded)) {
					return;
				}
				Update(d, model);
			};
			Update(d, model);
		}

		private static void Update(DependencyObject d, GridDefinitionModel model) {
			if (d is ColumnDefinition columnDefinition) {
				if (model.IsExpanded) {
					columnDefinition.MinWidth = model.Min;
					columnDefinition.Width = model.LastSize;
				} else {
					model.Min = columnDefinition.MinWidth;
					model.LastSize = columnDefinition.Width;
					columnDefinition.MinWidth = 0;
					columnDefinition.Width = new GridLength(0, GridUnitType.Pixel);
				}
			} else if (d is RowDefinition rowDefinition) {
				//todo 
			}
		}
	}
}
