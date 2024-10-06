using System.Windows;
using System.Windows.Input;

namespace BaseFramework.Models {
	public class GridDefinitionModel : BindableBase {
		private bool isExpanded = true;
		private double min;
		private GridLength lastSize;

		public bool IsExpanded {
			get => isExpanded;
			set => SetProperty(ref isExpanded, value);
		}

		public double Min {
			get => min;
			set => SetProperty(ref min, value);
		}

		public GridLength LastSize {
			get => lastSize;
			set => SetProperty(ref lastSize, value);
		}

		public ICommand ToggleIsExpandedCommand => new DelegateCommand(() => {
			IsExpanded = !IsExpanded;
		});

		public GridDefinitionModel() {

		}

		public GridDefinitionModel(bool isExpanded, double min, GridLength lastSize) {
			IsExpanded = isExpanded;
			Min = min;
			LastSize = lastSize;
		}

	}
}
