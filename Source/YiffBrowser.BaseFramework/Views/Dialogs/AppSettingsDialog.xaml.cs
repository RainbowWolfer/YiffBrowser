using DevExpress.Mvvm;
using MapsterMapper;
using RW.Base.WPF.Events;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.Services;
using RW.Common.Helpers;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Events;
using YiffBrowser.BaseFramework.Resources;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;
using ProxyModeType = YiffBrowser.BaseFramework.Enums.ProxyMode;

namespace YiffBrowser.BaseFramework.Views.Dialogs;

public partial class AppSettingsDialog : UserControl {
	public AppSettingsDialog() {
		InitializeComponent();
	}
}

public class AppSettingsDialogViewModel(
	IEventAggregator eventAggregator,
	IApplication application,
	IAppSettingsService appSettingsService,
	IDownloadService downloadService,
	IMapper mapper,
	AppManagerEx appManager,
	AppFolderConfig appFolderConfig
) : DialogViewModelOkCancel<object> {

	public AppManagerEx AppManager { get; } = appManager;
	public AppFolderConfig AppFolderConfig { get; } = appFolderConfig;

	public ISaveFileDialogService SaveFileDialogService => GetService<ISaveFileDialogService>();

	public AppSettingsModel Model {
		get => GetProperty(() => Model);
		set => SetProperty(() => Model, value);
	}

	public ProxyModeType ProxyMode {
		get => GetProperty(() => ProxyMode);
		set {
			if (SetProperty(() => ProxyMode, value)) {
				if (Model is not null) {
					Model.ProxyMode = value;
				}
				RaisePropertyChanged(nameof(IsCustomProxyEnabled));
			}
		}
	}

	public bool IsCustomProxyEnabled => ProxyMode == ProxyModeType.Custom;

	public bool IsGeneratingDiagnosticsFile {
		get => GetProperty(() => IsGeneratingDiagnosticsFile);
		set => SetProperty(() => IsGeneratingDiagnosticsFile, value);
	}

	protected override void OnInitialized() {
		base.OnInitialized();

		DialogTitle = $"{AppConfig.DisplayAppName} - Settings";

		Model = mapper.Map<AppSettingsModel>(appSettingsService.Model);
		ProxyMode = Model.ProxyMode;
	}

	protected override bool Validate(out string message) {
		if (ProxyMode == ProxyModeType.Custom && string.IsNullOrWhiteSpace(Model.ProxyHost)) {
			message = "Proxy host is required for custom proxy.";
			return false;
		}

		return base.Validate(out message);
	}

	protected override bool OnConfirmed() {
		try {
			Model.ProxyMode = ProxyMode;
			mapper.Map(Model, appSettingsService.Model);
			appSettingsService.SaveSettings();
			eventAggregator.GetEvent<AppSettingsChangedEvent>().Publish(new AppSettingsChangedEventArgs(appSettingsService.Model));

			NetCode.CreateNewClient();
			downloadService.RecreateHttpClient();

			return true;
		} catch (Exception ex) {
			DebugLog.LogHandledException(ex);
			MessageBoxService.ShowError("Saving settings error", ex);
			return false;
		}
	}



	private AsyncCommand? generateDiagnosticsFileCommand;
	public IDelegateCommand GenerateDiagnosticsFileCommand => generateDiagnosticsFileCommand ??= new(GenerateDiagnosticsFile, CanGenerateDiagnosticsFile);
	private async Task GenerateDiagnosticsFile() {
		if (CanGenerateDiagnosticsFile()) {
			SaveFileDialogService.Title = "Generate Diagnostics File";
			SaveFileDialogService.DefaultExt = "zip";
			SaveFileDialogService.DefaultFileName = $"Diagnostics Info - {AppConfig.DisplayAppName} - {DateTime.Now:yyyy_MM_dd HH_mm_ss}.zip";
			SaveFileDialogService.Filter = StaticStrings.FileFilter_ZipFile;

			if (!SaveFileDialogService.ShowDialog()) {
				return;
			}

			string filePath = SaveFileDialogService.GetFullFileName();

			IsGeneratingDiagnosticsFile = true;
			Stopwatch stopwatch = Stopwatch.StartNew();
			try {
				await Task.Run(() => {
					DiagnosticsService.CreateDiagnosticsZip(application, stopwatch, new DiagnosticsService.Parameter(
						ZipPath: filePath,
						Folders: [
							new DiagnosticsService.Folder(AppFolderConfig.LoggingFolder, SearchOption:
							SearchOption.AllDirectories, CutOffTimeSpan: TimeSpan.FromDays(30)),
							new DiagnosticsService.Folder(AppFolderConfig.DebugFolder, SearchOption:
							SearchOption.AllDirectories, CutOffTimeSpan: TimeSpan.FromDays(30)),
							new DiagnosticsService.Folder(AppFolderConfig.DataFolder, SearchOption:
							SearchOption.AllDirectories),
						]
					));
				});

				filePath.OpenPathInSystemDefault();
			} catch (Exception ex) {
				MessageBoxService.ShowError("Failed to generate diagnostics file", ex);
				DebugLog.LogHandledException(ex);
			} finally {
				stopwatch.Stop();
				IsGeneratingDiagnosticsFile = false;
			}
		}
	}
	private bool CanGenerateDiagnosticsFile() => !IsGeneratingDiagnosticsFile;


}
