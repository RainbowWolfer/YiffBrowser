using Newtonsoft.Json;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Services;
using YiffBrowser.BaseFramework.Helpers;

namespace YiffBrowser.BaseFramework.Services;

public interface IAppProfileService : ISettingsServiceBase<AppProfileModel> {
	void ScheduleSave();
	void FlushSave();
}

public class AppProfileService : JsonSettingsServiceBase<AppProfileModel>, ISingletonDependency, IAppProfileService {
	private readonly AppFolderConfig appFolderConfig;
	private readonly DebouncedAction debouncedSave;

	public AppProfileService(AppFolderConfig appFolderConfig) {
		this.appFolderConfig = appFolderConfig;
		debouncedSave = new DebouncedAction(TimeSpan.FromMilliseconds(500), () => {
			try {
				SaveSettings();
			} catch {
				// Best-effort persistence; ignore IO failures.
			}
		});
	}

	public override string FilePath => appFolderConfig.AppProfileFilePath;
	public override AppProfileModel GetDefaultModel() => new();

	public void ScheduleSave() => debouncedSave.Schedule();

	public void FlushSave() {
		try {
			debouncedSave.Flush();
		} catch {
			// Best-effort persistence; ignore IO failures.
		}
	}
}

/// <summary> map to self </summary>
[JsonObject]
public class AppProfileModel {
	/// <summary>Module name of the last focused window (e.g. "E621", "E6AI", "E926").</summary>
	public string? LastFocusedModule { get; set; }

	public List<ModuleWindowState> Windows { get; set; } = [];

	public List<ModuleSessionState> Modules { get; set; } = [];

	/// <summary>Per-module tab bookmark trees (tags + page favorites).</summary>
	public List<ModuleBookmarksState> ModuleBookmarks { get; set; } = [];

	/// <summary>Per-module recently closed tab batches.</summary>
	public List<ModuleClosedTabsState> ModuleClosedTabs { get; set; } = [];
}

[JsonObject]
public class ModuleWindowState {
	public string Module { get; set; } = string.Empty;
	public bool IsOpen { get; set; }
	public double Left { get; set; }
	public double Top { get; set; }
	public double Width { get; set; } = 1270;
	public double Height { get; set; } = 800;
	/// <summary>Normal, Minimized, or Maximized.</summary>
	public string WindowState { get; set; } = nameof(System.Windows.WindowState.Normal);
	public double RestoreLeft { get; set; }
	public double RestoreTop { get; set; }
	public double RestoreWidth { get; set; } = 1270;
	public double RestoreHeight { get; set; } = 800;
}

[JsonObject]
public class ModuleSessionState {
	public string Module { get; set; } = string.Empty;
	public int SelectedTabIndex { get; set; }
	public List<TabSessionState> Tabs { get; set; } = [];
}

[JsonObject]
public class TabSessionState {
	public string[] Tags { get; set; } = [];
	public int CurrentPage { get; set; } = 1;
}

[JsonObject]
public class ModuleBookmarksState {
	public string Module { get; set; } = string.Empty;
	public List<BookmarkNodeState> Roots { get; set; } = [];
}

[JsonObject]
public class BookmarkNodeState {
	public string Name { get; set; } = string.Empty;
	/// <summary>Null/empty means this node is a folder.</summary>
	public string[]? Tags { get; set; }
	public int Page { get; set; } = 1;
	public List<BookmarkNodeState> Children { get; set; } = [];
}

[JsonObject]
public class ModuleClosedTabsState {
	public string Module { get; set; } = string.Empty;
	public List<ClosedTabBatchState> Batches { get; set; } = [];
}

[JsonObject]
public class ClosedTabBatchState {
	public Guid Id { get; set; }
	public DateTime ClosedAt { get; set; }
	public List<ClosedTabRecordState> Tabs { get; set; } = [];
}

[JsonObject]
public class ClosedTabRecordState {
	public Guid Id { get; set; }
	public string[] Tags { get; set; } = [];
	public int Page { get; set; } = 1;
}
