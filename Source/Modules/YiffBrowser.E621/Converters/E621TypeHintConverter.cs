using RW.Common.Helpers;
using System.Globalization;
using System.Windows.Data;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Converters;

internal class E621TypeHintConverter : IValueConverter {
	private static readonly HashSet<string> sourceArray = new(["gif", "webm", "swf"], StringComparer.OrdinalIgnoreCase);

	// todo: option: 
	// show duration
	// show all types
	// show image resolution

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
		if (value is E621Post post && post.File != null && post.File.Ext != null) {
			int mode = NumberHelper.ConvertInt(parameter);

			string ext = post.File.Ext.ToLower();

			switch (mode) {
				case 1: { // PostCardControl
					if (sourceArray.Contains(ext)) {
						string r = post.File.Ext.ToUpper();
						if (ext is "webm" && NumberHelper.ConvertDouble(post.Duration, out double duration)) {
							// duration is in seconds
							return $"{Math.Round(duration)}s";
						} else {
							return string.Empty;
						}
					}
					break;
				}
				case 2: { // PostDetailView.ListBoxEx
					string r = post.File.Ext.ToUpper();
					if (ext is "webm" && NumberHelper.ConvertDouble(post.Duration, out double duration)) {
						// duration is in seconds
						return $"{r} ({Math.Round(duration)}s)";
					} else {
						return r;
					}
				}
			}


			return string.Empty;
		}
		return string.Empty;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
		throw new NotImplementedException();
	}
}
