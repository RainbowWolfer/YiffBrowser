using System.IO;
using System.Security.Cryptography;
using System.Text;
using RW.Common.Helpers;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Helpers;

public static class E621MediaCacheKeys {
	public const string Preview = "preview";
	public const string Sample = "sample";
	public const string File = "file";

	public static MediaCacheKey? For(E621Post? post, string variant, string? url) {
		if (post == null || post.ID <= 0 || url.IsBlank()) {
			return null;
		}

		if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)) {
			return null;
		}

		return new MediaCacheKey(
			SiteFromHost(uri.Host),
			post.ID,
			variant,
			string.IsNullOrWhiteSpace(post.File?.Md5) ? HashUrl(url) : post.File!.Md5,
			ExtensionFromUrl(uri));
	}

	private static string SiteFromHost(string host) {
		host = host.ToLowerInvariant();
		if (host.Contains("e6ai")) {
			return "e6ai";
		}
		if (host.Contains("e926")) {
			return "e926";
		}
		if (host.Contains("e621")) {
			return "e621";
		}
		return "site";
	}

	private static string ExtensionFromUrl(Uri uri) {
		string ext = Path.GetExtension(uri.AbsolutePath);
		return string.IsNullOrEmpty(ext) ? "bin" : ext;
	}

	private static string HashUrl(string url) {
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url)))[..16].ToLowerInvariant();
	}
}
