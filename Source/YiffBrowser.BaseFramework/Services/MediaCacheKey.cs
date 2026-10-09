namespace YiffBrowser.BaseFramework.Services;

/// <summary>
/// Identifies one media file on disk. The post id alone is not enough: a post has several
/// variants, and a replacement keeps the id while changing the hash.
/// </summary>
public sealed record MediaCacheKey(
	string Site,
	int PostId,
	string Variant,
	string? ContentHash,
	string Extension
);
