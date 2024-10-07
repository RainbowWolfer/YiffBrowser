using System.Diagnostics;
using System.IO;

namespace BaseFramework.Services {
	public static class LoggingService {

		public static void Log(string message) {
			try {
				string filePath = Path.Combine(FolderConfig.LogFolder, $"Log - {DateTime.Today:yyyy-MM-dd}");
				using StreamWriter writer = new(filePath, append: true);

				writer.WriteLine($"{DateTime.Now} - {message}");

			} catch (Exception ex) {
				Debug.WriteLine(ex);
			}
		}
	}
}
