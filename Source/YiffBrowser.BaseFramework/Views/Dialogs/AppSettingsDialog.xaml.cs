using DevExpress.Mvvm;
using MapsterMapper;
using RW.Base.WPF.Events;
using RW.Base.WPF.Extensions;
using RW.Base.WPF.Interfaces;
using RW.Base.WPF.Services;
using RW.Common.Helpers;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Events;
using YiffBrowser.BaseFramework.Resources;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.Utilities;
using YiffBrowser.BaseFramework.ViewModels;
using ProxyModeType = YiffBrowser.BaseFramework.Enums.ProxyMode;

namespace YiffBrowser.BaseFramework.Views.Dialogs;

public partial class AppSettingsDialog : UserControl {
	public AppSettingsDialog() {
		InitializeComponent();
	}

	private void InsertFileNameMacro_Click(object sender, RoutedEventArgs e) {
		if (sender is not Button { Content: string macro } || DataContext is not AppSettingsDialogViewModel viewModel) {
			return;
		}

		TextBox textBox = FileNameTemplateTextBox;
		int selectionStart = textBox.SelectionStart;
		int selectionLength = textBox.SelectionLength;
		string current = viewModel.FileNameTemplate ?? string.Empty;

		if (selectionStart < 0) {
			selectionStart = current.Length;
		}
		if (selectionStart > current.Length) {
			selectionStart = current.Length;
		}
		if (selectionStart + selectionLength > current.Length) {
			selectionLength = current.Length - selectionStart;
		}

		string updated = current
			.Remove(selectionStart, selectionLength)
			.Insert(selectionStart, macro);

		viewModel.FileNameTemplate = updated;
		textBox.Focus();
		textBox.CaretIndex = selectionStart + macro.Length;
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

	public IReadOnlyList<string> AvailableFileNameMacros => NameTemplateHandler.AvailableMacros;

	public IReadOnlyList<string> AvailableFileNamePresets => NameTemplateHandler.AvailablePresets;

	public AppSettingsModel Model {
		get => GetProperty(() => Model);
		set => SetProperty(() => Model, value);
	}

	public string FileNameTemplate {
		get => GetProperty(() => FileNameTemplate);
		set {
			if (SetProperty(() => FileNameTemplate, value)) {
				if (Model is not null) {
					Model.FileNameTemplate = value;
				}
				RefreshFileNameTemplateState();
			}
		}
	}

	public string FileNameTemplateError {
		get => GetProperty(() => FileNameTemplateError);
		private set => SetProperty(() => FileNameTemplateError, value);
	}

	public string FileNameTemplatePreview {
		get => GetProperty(() => FileNameTemplatePreview);
		private set => SetProperty(() => FileNameTemplatePreview, value);
	}

	public bool HasFileNameTemplateError {
		get => GetProperty(() => HasFileNameTemplateError);
		private set => SetProperty(() => HasFileNameTemplateError, value);
	}

	private DelegateCommand<string>? applyFileNamePresetCommand;
	public IDelegateCommand ApplyFileNamePresetCommand => applyFileNamePresetCommand ??= new(ApplyFileNamePreset);
	private void ApplyFileNamePreset(string? preset) {
		if (preset.IsNotBlank()) {
			FileNameTemplate = preset;
		}
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
		FileNameTemplate = Model.FileNameTemplate.IsBlank() ? "<id>" : Model.FileNameTemplate;
	}

	protected override bool Validate(out string message) {
		if (ProxyMode == ProxyModeType.Custom && string.IsNullOrWhiteSpace(Model.ProxyHost)) {
			message = "Proxy host is required for custom proxy.";
			return false;
		}

		if (!NameTemplateHandler.ValidateTemplate(FileNameTemplate, out string templateError)) {
			message = templateError;
			return false;
		}

		return base.Validate(out message);
	}

	protected override bool OnConfirmed() {
		try {
			Model.ProxyMode = ProxyMode;
			Model.FileNameTemplate = FileNameTemplate;
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

	private void RefreshFileNameTemplateState() {
		bool isValid = NameTemplateHandler.ValidateTemplate(FileNameTemplate, out string errorMessage);
		HasFileNameTemplateError = !isValid;
		FileNameTemplateError = isValid ? string.Empty : errorMessage;
		FileNameTemplatePreview = isValid
			? NameTemplateHandler.GeneratePreviewFilename(FileNameTemplate)
			: string.Empty;
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
