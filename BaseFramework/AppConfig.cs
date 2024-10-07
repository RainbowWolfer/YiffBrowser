using BaseFramework.Models;
using BaseFramework.Services;
using System.Diagnostics;
using System.Reflection;

namespace BaseFramework {
	public static class AppConfig {
		public static Guid SessionID { get; }
		public static DateTime AppStartTime { get; }
		public static VersionStruct Version { get; }

		public static bool IsDebugging {
			get {
#if RELEASE
				return false;
#else
				return true;
#endif
			}
		}

		static AppConfig() {
			SessionID = Guid.NewGuid();
			AppStartTime = DateTime.Now;

			LoggingService.Log(Environment.ProcessPath ?? "Environment.ProcessPath is null");

			FileVersionInfo version = FileVersionInfo.GetVersionInfo(Environment.ProcessPath ?? throw new Exception("Environment.ProcessPath is null"));

			Version = new VersionStruct(version.FileMajorPart, version.ProductMinorPart, version.FileBuildPart);

			Debug.WriteLine($"Yiff Browser Version: {Version}");
		}

		public static void Initialize() {
			LoggingService.Log($"AppConfig Initialized - {SessionID} - {Version}");
		}



	}
}
