using DevExpress.Mvvm;
using RW.Base.WPF.ViewModelServices;
using System.Windows;

namespace BaseFramework.ViewModelServices;

public class MessageBoxServiceEx : MessageBoxService, IMessageBoxServiceEx {
	public MessageBoxServiceEx() {
		MessageTitle = AppConfig.AppName;
	}

	public override MessageResult Show(string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult) {

		MessageBoxResult result;
		Window? window = AssociatedObject != null ? Window.GetWindow(AssociatedObject) : null;
		if (window == null) {
			result = HandyControl.Controls.MessageBox.Show(messageBoxText, caption, button.ToMessageBoxButton(), icon.ToMessageBoxImage(), defaultResult.ToMessageBoxResult());
		} else {
			result = HandyControl.Controls.MessageBox.Show(window, messageBoxText, caption, button.ToMessageBoxButton(), icon.ToMessageBoxImage(), defaultResult.ToMessageBoxResult());
		}
		return result switch {
			MessageBoxResult.None => MessageResult.None,
			MessageBoxResult.OK => MessageResult.OK,
			MessageBoxResult.Cancel => MessageResult.Cancel,
			MessageBoxResult.Abort => MessageResult.Cancel,
			MessageBoxResult.Retry => MessageResult.Cancel,
			MessageBoxResult.Ignore => MessageResult.Cancel,
			MessageBoxResult.Yes => MessageResult.Yes,
			MessageBoxResult.No => MessageResult.No,
			MessageBoxResult.TryAgain => MessageResult.Cancel,
			MessageBoxResult.Continue => MessageResult.Cancel,
			_ => MessageResult.None,
		};
	}
}
