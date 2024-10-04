using BaseFramework.Interfaces;
using BaseFramework.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BaseFramework.ViewModels {
	public class ViewModelBase<T> : BindableBase where T : IViewBase, new() {

		public T View { get; }

		public ViewModelBase() {
			View = new() {
				DataContext = this,
			};

			View.Loaded += View_LoadedOnce;
			View.Loaded += View_Loaded;
		}

		private void View_Loaded(object sender, RoutedEventArgs e) {
			Loaded(View);
		}

		private void View_LoadedOnce(object sender, RoutedEventArgs e) {
			View.Loaded -= View_LoadedOnce;
			LoadedOnce(View);
		}

		protected virtual void LoadedOnce(IViewBase viewBase) {

		}

		protected virtual void Loaded(IViewBase viewBase) {

		}

	}
}
