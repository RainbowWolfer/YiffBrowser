using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YiffBrowser.BaseFramework.Services;

/// <summary>
/// On-disk cache for post media. Files live at
/// <c>{root}/{site}/{postId}/{variant}-{hash}.{ext}</c>.
/// Memory caches stay in front of this; a hit here only avoids the network.
/// </summary>
public static class MediaDiskCache {

	public const long DefaultMaxBytes = 32L * 1024 * 1024 * 1024;

	public static string DefaultRoot => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		AppConfig.AppName,
		"MediaCache");

	public static bool IsEnabled => AppSettingsService.Instance.Model.EnableMediaCache;

	public static string ResolveRoot(AppSettingsModel? model = null) {
		model ??= AppSettingsService.Instance.Model;
		string folder = model.MediaCacheFolder;
		if (string.IsNullOrWhiteSpace(folder)) {
			return DefaultRoot;
		}

		return Path.GetFullPath(folder.Trim());
	}

	public static long ResolveMaxBytes(AppSettingsModel? model = null) {
		model ??= AppSettingsService.Instance.Model;
		return model.MediaCacheMaxBytes > 0 ? model.MediaCacheMaxBytes : DefaultMaxBytes;
	}

	public static long GigabytesToBytes(int gigabytes) {
		int clamped = Math.Clamp(gigabytes, 1, 512);
		return clamped * 1024L * 1024 * 1024;
	}

	public static int BytesToGigabytes(long bytes) {
		if (bytes <= 0) {
			bytes = DefaultMaxBytes;
		}
		long gigabytes = bytes / (1024L * 1024 * 1024);
		return (int)Math.Clamp(gigabytes, 1, 512);
	}

	public static bool TryRead(MediaCacheKey key, out byte[] data) {
		data = [];
		if (!IsEnabled || !TryGetExistingFile(key, 0, out string path)) {
			return false;
		}

		try {
			data = File.ReadAllBytes(path);
			if (data.Length == 0) {
				TryDeleteFile(path);
				return false;
			}

			return true;
		} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
			return false;
		}
	}

	/// <param name="expectedLength">When greater than zero, a file of any other length is deleted and treated as a miss.</param>
	public static bool TryGetExistingFile(MediaCacheKey key, long expectedLength, out string path) {
		path = "";
		if (!IsEnabled || !TryGetPath(key, out path)) {
			return false;
		}

		try {
			FileInfo info = new(path);
			if (!info.Exists || info.Length <= 0) {
				return false;
			}

			if (expectedLength > 0 && info.Length != expectedLength) {
				TryDeleteFile(path);
				return false;
			}

			Touch(path);
			return true;
		} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
			return false;
		}
	}

	public static void TryStore(MediaCacheKey key, byte[] data) {
		if (!IsEnabled || data.Length == 0 || !TryGetPath(key, out string path)) {
			return;
		}

		try {
			EnsureRoom(data.Length);
			WriteAtomically(path, data);
			Touch(path);
			NoteStored(data.Length);
		} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
			// A failed store must not break the in-memory image that already decoded.
		}
	}

	/// <summary>
	/// Streams a download straight to the cache file. Used for video so the bytes are not also
	/// held in a <see cref="MemoryStream"/>.
	/// </summary>
	public static async Task<string?> DownloadToFileAsync(MediaCacheKey key, Uri uri, long expectedLength, Action<int>? progress, CancellationToken token = default) {
		if (!IsEnabled || !TryGetPath(key, out string path)) {
			return null;
		}

		if (TryGetExistingFile(key, expectedLength, out string existing)) {
			return existing;
		}

		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		string partial = NewPartialPath(path);
		try {
			await MediaDownloadService.DownloadToFileAsync(uri, partial, progress, token).ConfigureAwait(false);
			FileInfo written = new(partial);
			if (expectedLength > 0 && written.Length != expectedLength) {
				throw new IOException($"Cached download is {written.Length} bytes, expected {expectedLength}.");
			}

			EnsureRoom(written.Length);
			File.Move(partial, path, overwrite: true);
			Touch(path);
			NoteStored(written.Length);
			return path;
		} catch {
			TryDeleteFile(partial);
			throw;
		}
	}

	public static void Delete(MediaCacheKey key) {
		if (TryGetPath(key, out string path)) {
			TryDeleteFile(path);
		}
	}

	public static bool ContainsFiles(string root) {
		if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) {
			return false;
		}

		return Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Any(path => !path.EndsWith(".part", StringComparison.OrdinalIgnoreCase));
	}

	public static bool IsNestedCachePath(string oldRoot, string newRoot) {
		if (string.IsNullOrWhiteSpace(oldRoot) || string.IsNullOrWhiteSpace(newRoot)) {
			return false;
		}

		return IsNested(oldRoot, newRoot) || IsNested(newRoot, oldRoot);
	}

	public static bool ShouldOfferMigration(string oldRoot, string newRoot) {
		if (string.IsNullOrWhiteSpace(oldRoot) || string.IsNullOrWhiteSpace(newRoot)) {
			return false;
		}

		if (PathsEqual(oldRoot, newRoot)) {
			return false;
		}

		if (IsNested(oldRoot, newRoot) || IsNested(newRoot, oldRoot)) {
			return false;
		}

		return ContainsFiles(oldRoot);
	}

	public static void Migrate(string oldRoot, string newRoot) {
		if (PathsEqual(oldRoot, newRoot) || !Directory.Exists(oldRoot)) {
			return;
		}

		Directory.CreateDirectory(newRoot);

		// Snapshot first. Moving files while the enumerator is live skips entries or throws.
		foreach (string file in Directory.EnumerateFiles(oldRoot, "*", SearchOption.AllDirectories).ToArray()) {
			string relative = Path.GetRelativePath(oldRoot, file);
			string destination = Path.Combine(newRoot, relative);
			string? directory = Path.GetDirectoryName(destination);
			if (directory != null) {
				Directory.CreateDirectory(directory);
			}

			if (File.Exists(destination)) {
				File.Delete(destination);
			}

			try {
				File.Move(file, destination);
			} catch (IOException) {
				File.Copy(file, destination, overwrite: true);
				File.Delete(file);
			}
		}

		TryDeleteEmptyDirectories(oldRoot);
	}

	public static MediaCacheClearResult Clear(string root) {
		if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root)) {
			return new MediaCacheClearResult(0, 0, null);
		}

		int deleted = 0;
		int skipped = 0;
		foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToArray()) {
			try {
				File.Delete(file);
				deleted++;
			} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
				skipped++;
			}
		}

		TryDeleteEmptyDirectories(root);
		return new MediaCacheClearResult(deleted, skipped, null);
	}

	private static bool TryGetPath(MediaCacheKey key, out string path) {
		path = "";
		if (key.PostId <= 0) {
			return false;
		}

		string variant = SanitizeToken(key.Variant, "file");
		string site = SanitizeToken(key.Site, "site");
		string hash = SanitizeHash(key.ContentHash);
		string extension = SanitizeExtension(key.Extension);

		string root = ResolveRoot();
		string candidate = Path.GetFullPath(Path.Combine(root, site, key.PostId.ToString(), $"{variant}-{hash}.{extension}"));
		if (!IsUnderRoot(root, candidate)) {
			return false;
		}

		path = candidate;
		return true;
	}

	private static string SanitizeToken(string? value, string fallback) {
		if (string.IsNullOrWhiteSpace(value)) {
			return fallback;
		}

		Span<char> buffer = stackalloc char[Math.Min(value.Length, 24)];
		int length = 0;
		foreach (char c in value) {
			if (length == buffer.Length) {
				break;
			}
			if (char.IsAsciiLetterOrDigit(c)) {
				buffer[length++] = char.ToLowerInvariant(c);
			}
		}

		return length == 0 ? fallback : new string(buffer[..length]);
	}

	private static string SanitizeHash(string? hash) {
		if (!string.IsNullOrWhiteSpace(hash)) {
			string cleaned = SanitizeToken(hash, "");
			if (cleaned.Length >= 8) {
				return cleaned.Length > 40 ? cleaned[..40] : cleaned;
			}
		}

		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hash ?? "none")))[..16].ToLowerInvariant();
	}

	private static string SanitizeExtension(string? extension) {
		string cleaned = SanitizeToken(extension, "bin");
		return cleaned.Length > 8 ? cleaned[..8] : cleaned;
	}

	private static bool IsUnderRoot(string root, string candidate) {
		string fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		string fullCandidate = Path.GetFullPath(candidate);
		return fullCandidate.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
	}

	private static bool PathsEqual(string left, string right) {
		return string.Equals(
			Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
			Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
			StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsNested(string parent, string child) {
		string fullParent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		string fullChild = Path.GetFullPath(child).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
		return fullChild.StartsWith(fullParent, StringComparison.OrdinalIgnoreCase)
			&& !PathsEqual(parent, child);
	}

	/// <summary>
	/// Last-access time is often frozen by Windows, so eviction uses last-write time, which we set ourselves.
	/// </summary>
	private static void Touch(string path) {
		try {
			File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
		} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
		}
	}

	private static string NewPartialPath(string finalPath) => finalPath + "." + Guid.NewGuid().ToString("N") + ".part";

	private static void WriteAtomically(string path, byte[] data) {
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		string partial = NewPartialPath(path);
		try {
			File.WriteAllBytes(partial, data);
			File.Move(partial, path, overwrite: true);
		} catch {
			TryDeleteFile(partial);
			throw;
		}
	}

	private const long FreeSpaceReserve = 512L * 1024 * 1024;

	private static void EnsureRoom(long incoming) {
		string root = ResolveRoot();
		if (!IsFreeSpaceLow(root, incoming)) {
			return;
		}

		Trim(root, ResolveMaxBytes());
		if (IsFreeSpaceLow(root, incoming)) {
			throw new IOException("Not enough free disk space for the media cache.");
		}
	}

	private static bool IsFreeSpaceLow(string root, long incoming) {
		try {
			string? drive = Path.GetPathRoot(Path.GetFullPath(root));
			if (string.IsNullOrEmpty(drive)) {
				return false;
			}

			DriveInfo info = new(drive);
			if (!info.IsReady) {
				return false;
			}

			return info.AvailableFreeSpace < incoming + FreeSpaceReserve;
		} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException) {
			return false;
		}
	}

	private static void TryDeleteFile(string path) {
		try {
			if (File.Exists(path)) {
				File.Delete(path);
			}
		} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
		}
	}

	private static void TryDeleteEmptyDirectories(string root) {
		if (!Directory.Exists(root)) {
			return;
		}

		foreach (string directory in Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories).OrderByDescending(path => path.Length)) {
			try {
				if (!Directory.EnumerateFileSystemEntries(directory).Any()) {
					Directory.Delete(directory);
				}
			} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
			}
		}
	}

	private const long TrimByteThreshold = 32L * 1024 * 1024;
	private const long TrimIntervalMs = 30_000;

	private static int trimming;
	private static long bytesSinceTrim;
	private static long lastTrimTick;

	private static void NoteStored(long bytes) {
		long pending = Interlocked.Add(ref bytesSinceTrim, Math.Max(0, bytes));
		long now = Environment.TickCount64;
		long last = Volatile.Read(ref lastTrimTick);
		bool due = last == 0 || now - last >= TrimIntervalMs || pending >= TrimByteThreshold;
		if (!due) {
			return;
		}

		ScheduleTrim();
	}

	private static void ScheduleTrim() {
		if (Interlocked.CompareExchange(ref trimming, 1, 0) != 0) {
			return;
		}

		_ = Task.Run(() => {
			try {
				Interlocked.Exchange(ref bytesSinceTrim, 0);
				Volatile.Write(ref lastTrimTick, Environment.TickCount64);
				Trim(ResolveRoot(), ResolveMaxBytes());
			} finally {
				Interlocked.Exchange(ref trimming, 0);
				if (Interlocked.Read(ref bytesSinceTrim) >= TrimByteThreshold) {
					ScheduleTrim();
				}
			}
		});
	}

	private static void Trim(string root, long maxBytes) {
		if (!Directory.Exists(root) || maxBytes <= 0) {
			return;
		}

		List<FileInfo> files = [];
		long total = 0;
		foreach (string path in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).ToArray()) {
			if (path.EndsWith(".part", StringComparison.OrdinalIgnoreCase)) {
				try {
					FileInfo partial = new(path);
					if (partial.Exists && partial.LastWriteTimeUtc < DateTime.UtcNow.AddHours(-1)) {
						partial.Delete();
					}
				} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
				}
				continue;
			}

			FileInfo info = new(path);
			if (!info.Exists) {
				continue;
			}

			files.Add(info);
			total += info.Length;
		}

		if (total <= maxBytes) {
			return;
		}

		foreach (FileInfo info in files.OrderBy(file => file.LastWriteTimeUtc)) {
			if (total <= maxBytes) {
				break;
			}

			try {
				long length = info.Length;
				info.Delete();
				total -= length;
			} catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) {
			}
		}
	}

}

public readonly record struct MediaCacheClearResult(int Deleted, int Skipped, string? Error);
