using LiteDB;
using RW.Base.WPF.DependencyInjections;
using RW.Base.WPF.Interfaces;
using RW.Common.Helpers;
using System.Diagnostics;
using System.IO;
using YiffBrowser.BaseFramework.Models;

namespace YiffBrowser.BaseFramework.Services;

public interface IDownloadIndexService {
	/// <summary>Raised after the in-memory index changes (upsert, root switch, prune).</summary>
	event EventHandler? IndexChanged;

	string? CurrentRootFolder { get; }

	/// <summary>Opens or switches the index database under the given download root (settings folder).</summary>
	void EnsureRoot(string? downloadRootFolder);

	bool IsDownloaded(string site, string itemId);

	/// <summary>
	/// Returns whether the item is indexed and the file still exists under the current root.
	/// Missing files are pruned from the index.
	/// </summary>
	bool IsDownloaded(string site, string itemId, out string? absolutePath);

	/// <summary>
	/// Records a completed download under the batch destination root.
	/// Only updates the in-memory UI cache when that root is the current settings root.
	/// </summary>
	void Upsert(string downloadRootFolder, string site, string itemId, string? md5, string absoluteFilePath);
}

/// <summary>
/// Per-download-folder LiteDB index at {root}/.yiffbrowser/index.db.
/// Keeps a memory set for fast UI lookups against the settings download folder.
/// </summary>
public class DownloadIndexService : IDownloadIndexService, ISingletonDependency, IAppInitializeAsync {
	private static DownloadIndexService? instance;
	public static DownloadIndexService Instance => instance!;

	private const string IndexFolderName = ".yiffbrowser";
	private const string IndexFileName = "index.db";
	private const string CollectionName = "downloaded_files";

	private readonly IAppSettingsService appSettingsService;
	private readonly object gate = new();

	private LiteDatabase? database;
	private ILiteCollection<DownloadedFileRecord>? collection;
	private string? currentRootFolder;

	/// <summary>Keys are DownloadedFileRecord.MakeId(site, itemId).</summary>
	private readonly HashSet<string> downloadedKeys = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>Key → relative path under current root.</summary>
	private readonly Dictionary<string, string> relativePaths = new(StringComparer.OrdinalIgnoreCase);

	public event EventHandler? IndexChanged;

	public string? CurrentRootFolder {
		get {
			lock (gate) {
				return currentRootFolder;
			}
		}
	}

	string IAppInitializeAsync.Description => "Initializing Download Index";
	int IPriority.Priority => IntPriority.Normal;

	public DownloadIndexService(IAppSettingsService appSettingsService) {
		instance = this;
		this.appSettingsService = appSettingsService;
	}

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport statusReport) {
		EnsureRoot(appSettingsService.Model.DownloadFolderPath);
	}

	public void EnsureRoot(string? downloadRootFolder) {
		string? normalized = NormalizeRoot(downloadRootFolder);
		bool raise;

		lock (gate) {
			if (string.Equals(currentRootFolder, normalized, StringComparison.OrdinalIgnoreCase)
				&& database != null) {
				return;
			}

			raise = OpenRoot_NoLock(normalized);
		}

		if (raise) {
			RaiseIndexChanged();
		}
	}

	public bool IsDownloaded(string site, string itemId) => IsDownloaded(site, itemId, out _);

	public bool IsDownloaded(string site, string itemId, out string? absolutePath) {
		absolutePath = null;
		if (site.IsBlank() || itemId.IsBlank()) {
			return false;
		}

		string key = DownloadedFileRecord.MakeId(site, itemId);
		bool pruned = false;

		lock (gate) {
			if (currentRootFolder.IsBlank() || !downloadedKeys.Contains(key)) {
				return false;
			}

			if (!relativePaths.TryGetValue(key, out string? relative) || relative.IsBlank()) {
				return true;
			}

			string fullPath = Path.GetFullPath(Path.Combine(currentRootFolder, relative));
			if (!IsPathUnderRoot(currentRootFolder, fullPath)) {
				return true;
			}

			if (File.Exists(fullPath)) {
				absolutePath = fullPath;
				return true;
			}

			downloadedKeys.Remove(key);
			relativePaths.Remove(key);
			pruned = true;
			try {
				collection?.Delete(key);
			} catch (Exception ex) {
				Debug.WriteLine($"Failed to prune missing download index entry '{key}': {ex.Message}");
			}
		}

		if (pruned) {
			RaiseIndexChanged();
		}

		return false;
	}

	public void Upsert(string downloadRootFolder, string site, string itemId, string? md5, string absoluteFilePath) {
		if (downloadRootFolder.IsBlank() || site.IsBlank() || itemId.IsBlank() || absoluteFilePath.IsBlank()) {
			return;
		}

		string? normalizedRoot = NormalizeRoot(downloadRootFolder);
		if (normalizedRoot.IsBlank()) {
			return;
		}

		string fullPath = Path.GetFullPath(absoluteFilePath);
		if (!IsPathUnderRoot(normalizedRoot, fullPath)) {
			return;
		}

		string relative = Path.GetRelativePath(normalizedRoot, fullPath);
		string key = DownloadedFileRecord.MakeId(site, itemId);
		DownloadedFileRecord record = new() {
			Id = key,
			Site = site,
			ItemId = itemId,
			Md5 = md5.IsBlank() ? null : md5,
			FileName = Path.GetFileName(fullPath),
			RelativePath = relative,
			DownloadedAt = DateTime.Now,
		};

		bool changed = false;

		lock (gate) {
			bool isCurrentRoot = string.Equals(currentRootFolder, normalizedRoot, StringComparison.OrdinalIgnoreCase);

			if (isCurrentRoot) {
				if (collection == null) {
					OpenRoot_NoLock(normalizedRoot);
				}

				if (collection == null) {
					return;
				}

				try {
					collection.Upsert(record);
					changed = downloadedKeys.Add(key)
						|| !string.Equals(relativePaths.GetValueOrDefault(key), relative, StringComparison.OrdinalIgnoreCase);
					relativePaths[key] = relative;
				} catch (Exception ex) {
					Debug.WriteLine($"Failed to upsert download index entry '{key}': {ex}");
				}
			} else {
				// Custom destination: persist into that folder's DB without switching the UI root.
				try {
					UpsertToRootFile(normalizedRoot, record);
				} catch (Exception ex) {
					Debug.WriteLine($"Failed to upsert download index entry '{key}' to '{normalizedRoot}': {ex}");
				}
			}
		}

		if (changed) {
			RaiseIndexChanged();
		}
	}

	private bool OpenRoot_NoLock(string? normalized) {
		CloseDatabase_NoLock();
		downloadedKeys.Clear();
		relativePaths.Clear();
		currentRootFolder = normalized;

		if (normalized.IsBlank()) {
			return true;
		}

		try {
			if (!Directory.Exists(normalized)) {
				Directory.CreateDirectory(normalized);
			}

			string indexDir = Path.Combine(normalized, IndexFolderName);
			Directory.CreateDirectory(indexDir);
			TryHideDirectory(indexDir);

			string dbPath = Path.Combine(indexDir, IndexFileName);
			database = new LiteDatabase(dbPath);
			collection = database.GetCollection<DownloadedFileRecord>(CollectionName);
			collection.EnsureIndex(x => x.Site);
			collection.EnsureIndex(x => x.ItemId);
			collection.EnsureIndex(x => x.Md5);

			foreach (DownloadedFileRecord record in collection.FindAll()) {
				if (record.Id.IsBlank() || record.Site.IsBlank() || record.ItemId.IsBlank()) {
					continue;
				}

				downloadedKeys.Add(record.Id);
				if (record.RelativePath.IsNotBlank()) {
					relativePaths[record.Id] = record.RelativePath;
				}
			}

			return true;
		} catch (Exception ex) {
			Debug.WriteLine($"Failed to open download index at '{normalized}': {ex}");
			CloseDatabase_NoLock();
			downloadedKeys.Clear();
			relativePaths.Clear();
			return true;
		}
	}

	private static void UpsertToRootFile(string normalizedRoot, DownloadedFileRecord record) {
		if (!Directory.Exists(normalizedRoot)) {
			Directory.CreateDirectory(normalizedRoot);
		}

		string indexDir = Path.Combine(normalizedRoot, IndexFolderName);
		Directory.CreateDirectory(indexDir);
		TryHideDirectory(indexDir);

		string dbPath = Path.Combine(indexDir, IndexFileName);
		using LiteDatabase db = new(dbPath);
		ILiteCollection<DownloadedFileRecord> col = db.GetCollection<DownloadedFileRecord>(CollectionName);
		col.EnsureIndex(x => x.Site);
		col.EnsureIndex(x => x.ItemId);
		col.EnsureIndex(x => x.Md5);
		col.Upsert(record);
	}

	private void CloseDatabase_NoLock() {
		try {
			database?.Dispose();
		} catch (Exception ex) {
			Debug.WriteLine($"Failed to dispose download index database: {ex.Message}");
		}

		database = null;
		collection = null;
	}

	private void RaiseIndexChanged() => IndexChanged?.Invoke(this, EventArgs.Empty);

	private static string? NormalizeRoot(string? path) {
		if (path.IsBlank()) {
			return null;
		}

		try {
			return Path.GetFullPath(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		} catch {
			return null;
		}
	}

	private static bool IsPathUnderRoot(string root, string fullPath) {
		string rootFull = Path.GetFullPath(root)
			.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
			+ Path.DirectorySeparatorChar;
		string pathFull = Path.GetFullPath(fullPath);
		return pathFull.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase)
			|| string.Equals(
				pathFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
				rootFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
				StringComparison.OrdinalIgnoreCase);
	}

	private static void TryHideDirectory(string directoryPath) {
		try {
			DirectoryInfo info = new(directoryPath);
			if (!info.Attributes.HasFlag(FileAttributes.Hidden)) {
				info.Attributes |= FileAttributes.Hidden;
			}
		} catch (Exception ex) {
			Debug.WriteLine($"Failed to hide download index folder: {ex.Message}");
		}
	}
}
