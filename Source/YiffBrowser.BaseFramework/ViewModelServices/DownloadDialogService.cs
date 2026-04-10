using System.Windows;
using YiffBrowser.BaseFramework.Views.Dialogs;

namespace YiffBrowser.BaseFramework.ViewModelServices;

public class DownloadDialogService : DialogService {
	public DownloadDialogService() {
		Name = "DownloadDialog";
		ContentType = typeof(DownloadDialog);
		ShowDialogWindow = false;
		SingleInstance = true;
	}
}
