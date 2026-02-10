using LiteDB;
using RW.Base.WPF.Interfaces;
using System.Diagnostics;

namespace YiffBrowser.BaseFramework.Services;

public interface IDatabaseService {
	ILiteDatabase Database { get; }
}

internal class DatabaseService(AppFolderConfig appFolderConfig) : IDatabaseService, IAppInitializeAsync {
	string IAppInitializeAsync.Description => "Initializing Database";
	int IPriority.Priority => IntPriority.Higher;

	private LiteDatabase? database;
	ILiteDatabase IDatabaseService.Database => database!;

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) {
		string filePath = appFolderConfig.AppDatabaseFilePath;
		Debug.WriteLine(filePath);
		database = new LiteDatabase(filePath, new _BsonMapper());
	}

	private class _BsonMapper : BsonMapper {

	}
}
