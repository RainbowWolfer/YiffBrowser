using BaseFramework.Enums;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YB.E621.Models;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.Views {
	public partial class E621MainWindow : WindowBase {
		public E621MainWindow() {
			InitializeComponent();
		}

	}

	public class E621MainWindowViewModel : WindowViewModel<E621MainWindow> {

		public ModuleType SiteType { get; }

		public PostsViewModel PostsViewModel { get; }

		public E621MainWindowViewModel(ModuleType siteType) {
			SiteType = siteType;
			PostsViewModel = new(siteType);
			View.Title = $"{siteType}";
		}
	}

}
