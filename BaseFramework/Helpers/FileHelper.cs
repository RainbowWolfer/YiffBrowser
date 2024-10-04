using System.IO;

namespace BaseFramework.Helpers {
	public static class FileHelper {

		public static DirectoryInfo CreateDirectory(string dir) {
			if (Path.HasExtension(dir)) {
				dir = Path.GetDirectoryName(dir) ?? string.Empty;
			}
			if (!Directory.Exists(dir)) {
				return Directory.CreateDirectory(dir);
			}
			return new DirectoryInfo(dir);
		}

	}
}
