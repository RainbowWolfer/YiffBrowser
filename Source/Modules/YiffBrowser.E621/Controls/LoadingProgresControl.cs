using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace YiffBrowser.E621.Controls;

public class LoadingProgresControl : Control {


	public string ErrorMessage {
		get => (string)GetValue(ErrorMessageProperty);
		set => SetValue(ErrorMessageProperty, value);
	}

	public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
		nameof(ErrorMessage),
		typeof(string),
		typeof(LoadingProgresControl),
		new PropertyMetadata(string.Empty)
	);

	public string DownloadInfo {
		get => (string)GetValue(DownloadInfoProperty);
		set => SetValue(DownloadInfoProperty, value);
	}

	public static readonly DependencyProperty DownloadInfoProperty = DependencyProperty.Register(
		nameof(DownloadInfo),
		typeof(string),
		typeof(LoadingProgresControl),
		new PropertyMetadata(string.Empty)
	);



	public double? Progress {
		get => (double?)GetValue(ProgressProperty);
		set => SetValue(ProgressProperty, value);
	}

	public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
		nameof(Progress),
		typeof(double?),
		typeof(LoadingProgresControl),
		new PropertyMetadata(null)
	);

	public ICommand ReloadCommand {
		get => (ICommand)GetValue(ReloadCommandProperty);
		set => SetValue(ReloadCommandProperty, value);
	}

	public static readonly DependencyProperty ReloadCommandProperty = DependencyProperty.Register(
		nameof(ReloadCommand),
		typeof(ICommand),
		typeof(LoadingProgresControl),
		new PropertyMetadata(null)
	);




	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (GetTemplateChild("ReloadButton") is ButtonBase reloadButton) {
			reloadButton.Click += ReloadButton_Click;
		}
	}

	private void ReloadButton_Click(object sender, RoutedEventArgs e) {
		if (ReloadCommand != null && ReloadCommand.CanExecute(e)) {
			ReloadCommand.Execute(e);
		}
	}
}
