using System.Windows;
using System.Windows.Controls;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;

namespace YiffBrowser.BaseFramework.Views.Dialogs;

public partial class DownloadDialog : UserControl {
	public DownloadDialog() {
		InitializeComponent();
	}

}

internal class DownloadDialogViewModel(IDownloadService downloadService) : DialogViewModel<object> {
	public IDownloadService DownloadService { get; } = downloadService;


	protected override void OnInitialized() {
		base.OnInitialized();
		DialogTitle = $"{AppConfig.DisplayAppName} - Downloads";

	}

	public override DialogWindowParameter DialogWindowParameter => base.DialogWindowParameter with {
		ResizeMode = ResizeMode.CanResizeWithGrip,
	};


	public override void OnWindowInitialized(Window window) {
		base.OnWindowInitialized(window);

		window.MinHeight = 160;
		window.MinWidth = 240;
	}

}
