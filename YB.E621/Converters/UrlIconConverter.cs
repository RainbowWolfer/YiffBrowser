using BaseFramework.Helpers;
using System.Globalization;
using System.Windows.Data;

namespace YB.E621.Converters {
	public class UrlIconConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value is string url) {
				if (url.IsBlank()) {
					return string.Empty;
				}

				//todo : twitter just change its name to x

				string _url = url;
				if (_url.StartsWith("https://")) {
					_url = _url[8..];
				} else if (_url.StartsWith("http://")) {
					_url = _url[7..];
				}

				string path = string.Empty;
				if (_url.Contains("tumblr")) {//something.tumblr.com
					path = GetResourcesString("Icons/tumblr-icon.png");
				}
				if (_url.StartsWith("twitter") || _url.StartsWith("www.twitter") || _url.StartsWith("pbs.twimg")) {
					path = GetResourcesString("Icons/Twitter-icon.png");
				} else if (_url.StartsWith("www.furaffinity") || _url.StartsWith("furaffinity") || _url.StartsWith("d.furaffinity")) {
					path = GetResourcesString("Icons/Furaffinity-icon.png");
				} else if (_url.StartsWith("www.deviantart") || _url.StartsWith("deviantart")) {
					path = GetResourcesString("Icons/DeviantArt-icon.png");
				} else if (_url.StartsWith("www.inkbunny") || _url.StartsWith("inkbunny")) {
					path = GetResourcesString("Icons/InkBunny-icon.png");
				} else if (_url.StartsWith("www.weasyl.com") || _url.StartsWith("weasyl.com")) {
					path = GetResourcesString("Icons/weasyl-icon.png");
				} else if (_url.StartsWith("www.pixiv") || _url.StartsWith("pixiv")) {
					path = GetResourcesString("Icons/Pixiv-icon.png");
				} else if (_url.StartsWith("www.instagram") || _url.StartsWith("instagram")) {
					path = GetResourcesString("Icons/Instagram-icon.png");
				} else if (_url.StartsWith("www.patreon") || _url.StartsWith("patreon")) {
					path = GetResourcesString("Icons/Patreon-icon.png");
				} else if (_url.StartsWith("www.subscribestar") || _url.StartsWith("subscribestar")) {
					path = GetResourcesString("Icons/SubscribeStar-icon.png");
				} else if (_url.StartsWith("mega")) {
					path = GetResourcesString("Icons/Mega-icon.png");
				} else if (_url.StartsWith("furrynetwork")) {
					path = GetResourcesString("Icons/FurryNetwork-icon.png");
				} else if (_url.StartsWith("t.me")) {
					path = GetResourcesString("Icons/Telegram-icon.png");
				} else if (_url.StartsWith("newgrounds") || _url.StartsWith("www.newgrounds")) {
					path = GetResourcesString("Icons/NewGrounds-icon.png");
				}

				return path;
			} else {
				return string.Empty;
			}
		}

		//"/BaseFramework;component/Resources/Icons/DeviantArt-icon.png";
		private static string GetResourcesString(string path) {
			return @$"/BaseFramework;component/Resources/{path}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotSupportedException();
		}
	}
}
