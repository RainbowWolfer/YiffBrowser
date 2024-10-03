using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BaseFramework.Interfaces {
	public interface IViewBase {
		event RoutedEventHandler Loaded;

		object DataContext { get; set; }
		bool IsLoaded { get; }
		bool IsInitialized { get; }
	}

	public interface IUserControlBase : IViewBase {
		UserControl UserControl { get; }
	}

	public interface IWindowBase : IViewBase {
		Window Window { get; }
	}

}
