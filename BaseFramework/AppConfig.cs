namespace BaseFramework;

public static class AppConfig {

	public const string AppName = "YiffBrowser";
	public const string DisplayAppName = "Yiff Browser";

	public const string NotifyIconToken = "RainbowWolfer.YiffBrowser.WPF";

	public const string ProjectRepositoryURL = @"https://github.com/RainbowWolfer/YiffBrowser";
	public const string GithubURL = @"https://github.com/RainbowWolfer";

	public static bool IsRelease {
		get {
#if RELEASE
			return true;
#else
			return false;
#endif
		}
	}


}
