using BaseFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BaseFramework.ViewModels {
	public class WindowViewModel<T> : ViewModelBase<T> where T : IWindowBase, new() {
		public WindowViewModel() {

		}

	}
}
