using RW.Base.WPF.ViewModels;

namespace BaseFramework;

public class AppManagerEx : AppManager {

	private static AppManagerEx? instance;
	public static AppManagerEx Instance => instance!;

	public AppManagerEx() {
		instance = this;
	}

	public override string AppName => AppConfig.AppName;
	public override string BuildMode => AppConfig.IsRelease ? "Release" : "Debug";
	public override bool IsRelease => AppConfig.IsRelease;

	public TimeSpan AppStartupTimeSpan {
		get => GetProperty(() => AppStartupTimeSpan);
		set => SetProperty(() => AppStartupTimeSpan, value);
	}
}
