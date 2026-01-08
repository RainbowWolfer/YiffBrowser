using RW.Base.WPF.Configs;
using RW.Base.WPF.Interfaces;
using System.IO;

namespace BaseFramework;

public class AppFolderConfig(IAppManager appManager) : FolderConfig(appManager) {

	public string AppSettingsFilePath => Path.Combine(DataFolder, "AppSettings.json");

}
