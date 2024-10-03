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

			View.Loaded += View_Loaded;
		}

		private void View_Loaded(object sender, RoutedEventArgs e) {
			View.Loaded -= View_Loaded;
			Loaded(View);
		}

		protected virtual void Loaded(IViewBase viewBase) {
			
		}

	}
}
