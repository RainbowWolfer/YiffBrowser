using BaseFramework.ViewModels;
using System;
using System.Collections.Generic;
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

namespace BaseFramework.Views.Dialogs {
	public partial class AppSettingsDialog : WindowBase {
		public AppSettingsDialog() {
			InitializeComponent();
		}
	}

	public class AppSettingsDialogViewModel : WindowViewModel<AppSettingsDialog> {

		public static void ShowDialog(Window owner) {
			AppSettingsDialogViewModel viewModel = new();
			viewModel.View.Owner = owner;
			viewModel.View.ShowDialog();
		}

		public AppSettingsDialogViewModel() {

		}

	}
}
