using BaseFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BaseFramework.Views {
	public class WindowBase : Window, IWindowBase {
		Window IWindowBase.Window => this;

		public WindowBase() {
			
		}

	}
}
