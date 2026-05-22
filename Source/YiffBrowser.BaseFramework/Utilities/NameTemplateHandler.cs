using System.IO;
using System.Text.RegularExpressions;
using YiffBrowser.BaseFramework.Interfaces;

namespace YiffBrowser.BaseFramework.Utilities;

public static partial class NameTemplateHandler {

	// 忽略大小写的标签（针对文件本身的属性）
	private static readonly string[] CaseInsensitiveTags =
	[
		"id", "md5", "authors"
	];

	// 严格区分大小写的标签（针对日期和时间）
	private static readonly string[] CaseSensitiveTags =
	[
		"yyyy", "yy", "MM", "dd", "HH", "hh", "mm", "ss", "fff"
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

		string safeExtension = (post.Extension ?? "bin").TrimStart('.');
		return $"{result}.{safeExtension}";
	}

	/// <summary>
	/// 核心替换逻辑
	/// </summary>
	private static string GenerateRawFilename(INameTemplateItem post, string template) {
		string result = template;

		string safeAuthors = SanitizeFilename(string.Join(", ", post.Authors ?? ["unknown_author"]));
		string safeMd5 = SanitizeFilename(post.Md5 ?? "unknown_md5");

		// 第一部分：实体属性（忽略大小写，支持 <ID> 或 <id>）
		result = result.Replace("<id>", post.Id.ToString(), StringComparison.OrdinalIgnoreCase);
		result = result.Replace("<md5>", safeMd5, StringComparison.OrdinalIgnoreCase);
		result = result.Replace("<authors>", safeAuthors, StringComparison.OrdinalIgnoreCase);

		// 第二部分：日期时间（严格区分大小写，StringComparison.Ordinal）
		DateTime now = DateTime.Now;
		result = result.Replace("<yyyy>", now.ToString("yyyy"), StringComparison.Ordinal); // 4位年份: 2026
		result = result.Replace("<yy>", now.ToString("yy"), StringComparison.Ordinal);     // 2位年份: 26
		result = result.Replace("<MM>", now.ToString("MM"), StringComparison.Ordinal);     // 补零月份: 01-12
		result = result.Replace("<dd>", now.ToString("dd"), StringComparison.Ordinal);     // 补零日期: 01-31
		result = result.Replace("<HH>", now.ToString("HH"), StringComparison.Ordinal);     // 24小时制: 00-23
		result = result.Replace("<hh>", now.ToString("hh"), StringComparison.Ordinal);     // 12小时制: 01-12
		result = result.Replace("<mm>", now.ToString("mm"), StringComparison.Ordinal);     // 分钟: 00-59
		result = result.Replace("<ss>", now.ToString("ss"), StringComparison.Ordinal);     // 秒钟: 00-59
		result = result.Replace("<f>", now.ToString("f"), StringComparison.Ordinal);   // 毫秒: 000-999
		result = result.Replace("<ff>", now.ToString("ff"), StringComparison.Ordinal);   // 毫秒: 000-999
		result = result.Replace("<fff>", now.ToString("fff"), StringComparison.Ordinal);   // 毫秒: 000-999
		result = result.Replace("<ffff>", now.ToString("ffff"), StringComparison.Ordinal);   // 毫秒: 000-999

		return result;
	}

	private static string SanitizeFilename(string filename) {
		char[] invalidChars = Path.GetInvalidFileNameChars();
		string safeFilename = filename;

		foreach (char invalidChar in invalidChars) {
			safeFilename = safeFilename.Replace(invalidChar.ToString(), "_");
		}

		return safeFilename;
	}

	private class MockPost : INameTemplateItem {
		public string Id => "123456";
		public string Md5 => "a1b2c3d4e5f6g7h8";
		public IEnumerable<string> Authors => ["SampleAuthor"];
		public string Extension => "png";
	}

	[GeneratedRegex(@"<([^>]*)>")]
	private static partial Regex PlaceholderRegex();
}
