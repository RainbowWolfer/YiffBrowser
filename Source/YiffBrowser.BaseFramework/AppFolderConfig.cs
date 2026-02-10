using RW.Base.WPF.Configs;
using RW.Base.WPF.Interfaces;
using System.IO;

namespace YiffBrowser.BaseFramework;

public class AppFolderConfig : FolderConfig {

	private static AppFolderConfig? instance;
	public static AppFolderConfig Instance => instance!;

	public AppFolderConfig(IAppManager appManager) : base(appManager) {
		instance = this;
	}

	public string AppSettingsFilePath => Path.Combine(DataFolder, "AppSettings.json");
	public string AppProfileFilePath => Path.Combine(DataFolder, "AppProfile.json");

	public string AppDatabaseFilePath => Path.Combine(DataFolder, "AppData.db");

}
