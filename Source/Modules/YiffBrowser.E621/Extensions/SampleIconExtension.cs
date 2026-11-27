using System.Windows.Markup;

namespace YiffBrowser.E621.Extensions;

public class SampleIconExtension : MarkupExtension {
	private static int next;

	private static readonly string[] Glyphs = [
		"\uE721",
		"\uE8A5",
		"\uE896",
		"\uE70D",
		"\uE70E",
		"\uE8BB",
		"\uE912",
		"\uE713",
		"\uE728",
		"\uE762",
		"\uE74D",
		"\uE8F4",
	];

	public override object ProvideValue(IServiceProvider serviceProvider) {
		int index = Interlocked.Increment(ref next);
		return Glyphs[index % Glyphs.Length];
	}
}
