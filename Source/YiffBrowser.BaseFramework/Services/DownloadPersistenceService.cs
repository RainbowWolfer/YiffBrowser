using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.BaseFramework.Models;

namespace YiffBrowser.BaseFramework.Services;

public interface IDownloadPersistenceService : ISettingsServiceBase<DownloadSessionModel> {
	void ScheduleSave();
	void FlushSave();
}

public class DownloadPersistenceService : JsonSettingsServiceBase<DownloadSessionModel>, ISingletonDependency, IDownloadPersistenceService {
	private readonly AppFolderConfig appFolderConfig;
	private readonly DebouncedAction debouncedSave;

	public DownloadPersistenceService(AppFolderConfig appFolderConfig) {
		this.appFolderConfig = appFolderConfig;
		debouncedSave = new DebouncedAction(TimeSpan.FromMilliseconds(500), () => {
			try {
				SaveSettings();
			} catch {
				// Best-effort persistence; ignore IO failures.
			}
		});
	}

	public override string FilePath => appFolderConfig.DownloadsFilePath;
	public override DownloadSessionModel GetDefaultModel() => new();

	public void ScheduleSave() => debouncedSave.Schedule();

	public void FlushSave() {
		try {
			debouncedSave.Flush();
		} catch {
			// Best-effort persistence; ignore IO failures.
		}
	}
}
