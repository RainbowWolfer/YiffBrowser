using RW.Base.WPF.ViewModelServices;

namespace BaseFramework.ViewModelServices;

public class MessageBoxServiceEx : MessageBoxService, IMessageBoxServiceEx {
	public MessageBoxServiceEx() {
		MessageTitle = AppConfig.AppName;
	}
}
