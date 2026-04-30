using HandyControl.Controls;
using RW.Common.WPF.Extensions;
using System.Windows;

namespace YiffBrowser;

public static class CustomInitialize {
	public static void Initialize() {
		ProgressBarExtension.OnProgressChangedHandler += ProgressBarExtension_OnProgressChangedHandler;
	}

	private static void ProgressBarExtension_OnProgressChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is CircleProgressBar circleProgressBar) {
			if (e.NewValue is double value) {
				circleProgressBar.Value = value;
				circleProgressBar.IsIndeterminate = false;
			} else {
				circleProgressBar.Value = 0;
				circleProgressBar.IsIndeterminate = true;
			}
		}
	}
}
