using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using YiffBrowser.BaseFramework.Interfaces;

namespace YiffBrowser.BaseFramework.Utilities;

public static partial class NameTemplateHandler {

	// 忽略大小写的标签（针对文件本身的属性）
	private static readonly string[] CaseInsensitiveTags =
	[
		"id", "site", "md5", "authors"
	];

	// 严格区分大小写的标签（针对日期和时间）
	private static readonly string[] CaseSensitiveTags =
	[
		"yyyy", "yy", "MM", "dd", "HH", "hh", "mm", "ss", "fff", "tt", "t"
	];

	/// <summary>Available macros for the settings UI (angle-bracket form).</summary>
	public static IReadOnlyList<string> AvailableMacros { get; } =
	[
		"_", " - ",
		"<id>", "<site>", "<md5>", "<authors>",
		"<yyyy>", "<yy>", "<MM>", "<dd>",
		"<HH>", "<hh>", "<mm>", "<ss>", "<fff>", "<tt>", "<t>"
	];

	/// <summary>Common filename templates for the settings presets dropdown.</summary>
	public static IReadOnlyList<string> AvailablePresets { get; } =
	[
		"<id>",
		"<id> - <authors>",
		"<authors> - <id>",
		"<site> - <id> - <authors>",
		"<site>_<id>_<authors>",
		"<site>_<id>",
		"<id>_<md5>",
		"<yyyy>-<MM>-<dd> <id>"
	];

	/// <summary>
	/// 检查用户输入的模板是否合法
	/// </summary>
	public static bool ValidateTemplate(string template, out string errorMessage) {
		if (string.IsNullOrWhiteSpace(template)) {
			errorMessage = "File name template cannot be empty.";
			return false;
		}

		// 1. 严格的括号嵌套和匹配检查 (Depth Check)
		int depth = 0;
		for (int i = 0; i < template.Length; i++) {
			char c = template[i];
			if (c == '<') {
				depth++;
			} else if (c == '>') {
				depth--;
			}

			if (depth < 0) {
				errorMessage = $"Unexpected '>' at position {i}.";
				return false;
			}
			if (depth > 1) {
				errorMessage = $"Nested or consecutive '<' found at position {i} (e.g., <<id>> is not allowed).";
				return false;
			}
		}
		if (depth != 0) {
			errorMessage = "Missing closing '>'.";
			return false;
		}

		// 2. 检查是否包含未知的 Flag
		MatchCollection matches = PlaceholderRegex().Matches(template);
		foreach (Match match in matches) {
			string innerTag = match.Groups[1].Value;

			bool isValidEntityTag = CaseInsensitiveTags.Contains(innerTag, StringComparer.OrdinalIgnoreCase);
			bool isValidDateTag = CaseSensitiveTags.Contains(innerTag, StringComparer.Ordinal);

			if (!isValidEntityTag && !isValidDateTag) {
				errorMessage = $"Unsupported flag: <{innerTag}>";
				return false;
			}
		}

		// 3. 必须包含防覆盖的主键
		string lowerTemplate = template.ToLower();
		if (!lowerTemplate.Contains("<id>") && !lowerTemplate.Contains("<md5>")) {
			errorMessage = "Template must contain at least <id> or <md5> to prevent file overwriting.";
			return false;
		}

		// 4. Sample 试运行
		MockPost sampleData = new();
		string rawResult = GenerateRawFilename(sampleData, template);

		char[] invalidChars = Path.GetInvalidFileNameChars();
		int invalidIndex = rawResult.IndexOfAny(invalidChars);
		if (invalidIndex >= 0) {
			errorMessage = $"Template contains invalid file name character: '{rawResult[invalidIndex]}'";
			return false;
		}

		errorMessage = string.Empty;
		return true;
	}

	/// <summary>
	/// 提供给外层的正式生成方法
	/// </summary>
	public static string GenerateFilename(INameTemplateItem post, string template) {
		if (string.IsNullOrWhiteSpace(template)) {
			template = "<id>";
		}

		string result = GenerateRawFilename(post, template);
		result = SanitizeFilename(result);

		string safeExtension = post.Extension.TrimStart('.');
		return $"{result}.{safeExtension}";
	}

	/// <summary>Generates a sample filename for settings preview using mock post data.</summary>
	public static string GeneratePreviewFilename(string template) =>
		GenerateFilename(new MockPost(), template);

	/// <summary>
	/// 核心替换逻辑
	/// </summary>
	private static string GenerateRawFilename(INameTemplateItem post, string template) {
		string result = template;

		string safeAuthors = SanitizeFilename(string.Join(", ", post.Authors));
		string safeMd5 = SanitizeFilename(post.Md5);

		// 第一部分：实体属性（忽略大小写，支持 <ID> 或 <id>）
		result = result.Replace("<site>", post.Site, StringComparison.OrdinalIgnoreCase);
		result = result.Replace("<id>", post.Id, StringComparison.OrdinalIgnoreCase);
		result = result.Replace("<md5>", safeMd5, StringComparison.OrdinalIgnoreCase);
		result = result.Replace("<authors>", safeAuthors, StringComparison.OrdinalIgnoreCase);

		// 第二部分：日期时间（严格区分大小写，StringComparison.Ordinal）
		DateTime now = DateTime.Now;

		// 建议引入 Globalization 来使用 InvariantCulture
		CultureInfo culture = CultureInfo.InvariantCulture;

		result = result.Replace("<yyyy>", now.ToString("yyyy", culture), StringComparison.Ordinal);
		result = result.Replace("<yy>", now.ToString("yy", culture), StringComparison.Ordinal);
		result = result.Replace("<MM>", now.ToString("MM", culture), StringComparison.Ordinal);
		result = result.Replace("<dd>", now.ToString("dd", culture), StringComparison.Ordinal);
		result = result.Replace("<HH>", now.ToString("HH", culture), StringComparison.Ordinal);
		result = result.Replace("<hh>", now.ToString("hh", culture), StringComparison.Ordinal);
		result = result.Replace("<mm>", now.ToString("mm", culture), StringComparison.Ordinal);
		result = result.Replace("<ss>", now.ToString("ss", culture), StringComparison.Ordinal);
		result = result.Replace("<fff>", now.ToString("fff", culture), StringComparison.Ordinal);

		// 新增：AM/PM 标识符
		result = result.Replace("<tt>", now.ToString("tt", culture), StringComparison.Ordinal); // 输出 AM 或 PM
		result = result.Replace("<t>", now.ToString("t", culture), StringComparison.Ordinal);   // 输出 A 或 P

		return result;
	}

	/// <summary>Replaces characters illegal in Windows file/folder names with '_'.</summary>
	public static string SanitizePathSegment(string segment) {
		if (string.IsNullOrEmpty(segment)) {
			return string.Empty;
		}

		char[] invalidChars = Path.GetInvalidFileNameChars();
		string safe = segment;

		foreach (char invalidChar in invalidChars) {
			safe = safe.Replace(invalidChar.ToString(), "_");
		}

		return safe.Trim();
	}

	private static string SanitizeFilename(string filename) => SanitizePathSegment(filename);

	private class MockPost : INameTemplateItem {
		public string Site => "e621";
		public string Id => "73312";
		public string Md5 => "8f3a2c9e1b4d7a60c5e8f1a2b3c4d5e6";
		public IEnumerable<string> Authors => ["zonkpunch", "ruaidri"];
		public string Extension => "png";
	}

	[GeneratedRegex(@"<([^>]*)>")]
	private static partial Regex PlaceholderRegex();
}
