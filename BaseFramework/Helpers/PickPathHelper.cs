using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace BaseFramework.Helpers;

public static class PickPathHelper {

	public static bool PickFolder([NotNullWhen(true)] out string? path) {
		FolderBrowserDialog dialog = new() {
			
		};

		if (dialog.ShowDialog() is DialogResult.OK) {
			path = dialog.SelectedPath;
			return true;
		}

		path = null;
		return false;
	}
}
