using YiffBrowser.BaseFramework.Views.Dialogs;

namespace YiffBrowser.BaseFramework.ViewModelServices;

public class AppSettingsDialogService : DialogService {
	public AppSettingsDialogService() {
		Name = "AppSettingsDialog";
		ContentType = typeof(AppSettingsDialog);
		ShowDialogWindow = false;
		SingleInstance = true;
	}
}
