using AutoMapper;
using BaseFramework.Services;
using BaseFramework.ViewModels;
using RW.Base.WPF.Extensions;
using System.Windows.Controls;

namespace BaseFramework.Views.Dialogs;

public partial class AppSettingsDialog : UserControl {
	public AppSettingsDialog() {
		InitializeComponent();
	}
}

public class AppSettingsDialogViewModel(
	IAppSettingsService appSettingsService,
	IMapper mapper
) : DialogViewModelOkCancel<object> {

	public AppSettingsModel Model {
		get => GetProperty(() => Model);
		set => SetProperty(() => Model, value);
	}

	protected override void OnInitialized() {
		base.OnInitialized();

		DialogTitle = $"{AppConfig.DisplayAppName} - Settings";

		Model = mapper.Map<AppSettingsModel>(appSettingsService.Model);

	}

	protected override bool Validate(out string message) {
		return base.Validate(out message);
	}

	protected override bool OnConfirmed() {
		try {
			mapper.Map(Model, appSettingsService.Model);
			appSettingsService.SaveSettings();
			return true;
		} catch (Exception ex) {
			DebugLoggerManager.LogHandledException(ex);
			MessageBoxService.ShowError("Saving settings error", ex);
			return false;
		}
	}

}
