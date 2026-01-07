namespace BaseFramework;

public static class AppConfig {

    public const string AppName = "YiffBrowser";

    public static bool IsDebugging {
        get {
#if RELEASE
			return false;
#else
            return true;
#endif
        }
    }

}
