namespace YiffBrowser.BaseFramework;

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

	/// <summary>开发用：为 true 时页面不显示帖子图片/预览/视频（不影响加载与逻辑）。</summary>
	public const bool SafeMode = true;

}
