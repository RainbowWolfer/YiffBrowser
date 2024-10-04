using BaseFramework.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using YB.E621.Models.E621;

namespace YB.E621.Controls {
	public class SearchTagItemControl : Control {

		public E621AutoComplete AutoComplete {
			get => (E621AutoComplete)GetValue(AutoCompleteProperty);
			set => SetValue(AutoCompleteProperty, value);
		}

		public static readonly DependencyProperty AutoCompleteProperty = DependencyProperty.Register(
			nameof(AutoComplete),
			typeof(E621AutoComplete),
			typeof(SearchTagItemControl),
			new PropertyMetadata(null, OnAutoCompleteChanged)
		);

		private static void OnAutoCompleteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SearchTagItemControl view && e.NewValue is E621AutoComplete autoComplete) {
				Color color = E621Tag.GetCategoryColor(autoComplete.Category);
				view.MainBrush = new SolidColorBrush(color);

				view.CountString = autoComplete.PostCount.NumberToK();
				if (autoComplete.AntecedentName.IsBlank()) {
					view.FromName = autoComplete.Name ?? string.Empty;
					view.ToName = string.Empty;
				} else {
					view.FromName = autoComplete.AntecedentName;
					view.ToName = autoComplete.Name ?? string.Empty;
				}

			}
		}

		public bool IsSelected {
			get => (bool)GetValue(IsSelectedProperty);
			set => SetValue(IsSelectedProperty, value);
		}

		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
			nameof(IsSelected),
			typeof(bool),
			typeof(SearchTagItemControl),
			new PropertyMetadata(false, OnIsSelectedChanged)
		);

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			if (d is SearchTagItemControl view && e.NewValue is bool b) {
				//view.ViewModel.IsSelected = (bool)e.NewValue;

				//view.RectangleWidthAnimation.From = view.CategoryRectangle.Width;
				//view.RectangleWidthAnimation.To = view.ViewModel.IsSelected ? 8 : 4;
				//view.SelectionStoryboard.Begin();
			}
		}

		public override void OnApplyTemplate() {
			base.OnApplyTemplate();

		}


		#region Readonly

		public Brush MainBrush {
			get => (Brush)GetValue(MainBrushProperty);
			private set => SetValue(MainBrushPropertyKey, value);
		}

		public static readonly DependencyPropertyKey MainBrushPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(MainBrush),
			typeof(Brush),
			typeof(SearchTagItemControl),
			new PropertyMetadata(Brushes.Black)
		);

		public static readonly DependencyProperty MainBrushProperty = MainBrushPropertyKey.DependencyProperty;


		public string FromName {
			get => (string)GetValue(FromNameProperty);
			private set => SetValue(FromNamePropertyKey, value);
		}

		public static readonly DependencyPropertyKey FromNamePropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(FromName),
			typeof(string),
			typeof(SearchTagItemControl),
			new PropertyMetadata(string.Empty)
		);

		public static readonly DependencyProperty FromNameProperty = FromNamePropertyKey.DependencyProperty;


		public string ToName {
			get => (string)GetValue(ToNameProperty);
			private set => SetValue(ToNamePropertyKey, value);
		}

		public static readonly DependencyPropertyKey ToNamePropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(ToName),
			typeof(string),
			typeof(SearchTagItemControl),
			new PropertyMetadata(string.Empty)
		);

		public static readonly DependencyProperty ToNameProperty = ToNamePropertyKey.DependencyProperty;


		public string CountString {
			get => (string)GetValue(CountStringProperty);
			private set => SetValue(CountStringPropertyKey, value);
		}

		public static readonly DependencyPropertyKey CountStringPropertyKey = DependencyProperty.RegisterReadOnly(
			nameof(CountString),
			typeof(string),
			typeof(SearchTagItemControl),
			new PropertyMetadata(string.Empty)
		);

		public static readonly DependencyProperty CountStringProperty = CountStringPropertyKey.DependencyProperty;


		#endregion




	}
}
