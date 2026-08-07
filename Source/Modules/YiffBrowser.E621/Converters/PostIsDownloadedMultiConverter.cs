using System.Globalization;
using System.Windows.Data;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.Converters;

/// <summary>
/// values[0] = E621Post (binding DataContext),
/// values[1] = DownloadIndexVersion (refresh trigger),
/// values[2] = PostsViewModel
/// </summary>
public class PostIsDownloadedMultiConverter : IMultiValueConverter {
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
		if (values is { Length: >= 3 }
			&& values[0] is E621Post post
			&& values[2] is PostsViewModel viewModel) {
			return viewModel.IsPostDownloaded(post);
		}

		return false;
	}

	public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
		throw new NotSupportedException();
	}
}
