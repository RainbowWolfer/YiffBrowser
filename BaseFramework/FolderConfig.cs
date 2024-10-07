using BaseFramework.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseFramework {
	public static class FolderConfig {

		public static string DocumentFolder => Environment.GetFolderPath(Environment.SpecialFolder.MyDoc‌​uments);
		public static string LocalFolder => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

		public static string DocumentAppFolder {
			get {
				string result = Path.Combine(DocumentFolder, $"RainbowWolfer", $"YiffBrowser");
				FileHelper.CreateDirectory(result);
				return result;
			}
		}

		public static string LocalAppFolder {
			get {
				string result = Path.Combine(LocalFolder, $"RainbowWolfer", $"YiffBrowser");
				FileHelper.CreateDirectory(result);
				return result;
			}
		}

		public static string LogFolder => Path.Combine(LocalAppFolder, "Logs");

		public static string AppProfileFilePath => Path.Combine(LocalAppFolder, $"AppProfile.json");



		public static void Initialize() {
			FileHelper.CreateDirectory(DocumentAppFolder);
			FileHelper.CreateDirectory(LocalAppFolder);
			FileHelper.CreateDirectory(LogFolder);


		}

	}
}
