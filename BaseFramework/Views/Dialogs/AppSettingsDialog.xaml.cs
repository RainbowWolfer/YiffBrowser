using DevExpress.Mvvm;
using System.Windows;

namespace BaseFramework.Views.Dialogs;

public partial class AppSettingsDialog : WindowBase {
    public AppSettingsDialog() {
        InitializeComponent();
    }
}

public class AppSettingsDialogViewModel : ViewModelBase {

    public static void ShowDialog(Window owner) {
        //AppSettingsDialogViewModel viewModel = new();
        //viewModel.View.Owner = owner;
        //viewModel.View.ShowDialog();
    }

    public AppSettingsDialogViewModel() {

    }

}
