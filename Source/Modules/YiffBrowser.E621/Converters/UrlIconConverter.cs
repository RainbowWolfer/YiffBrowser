using RW.Common.Helpers;
using System.Globalization;
using System.Windows.Data;
using YiffBrowser.Resources.Icons;

namespace YiffBrowser.E621.Converters;

public class UrlIconConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
        if (value is string url) {
            if (url.IsBlank()) {
                return string.Empty;
            }

            //todo : twitter just change its name to x

            string _url = url.TrimmedSafeString();
            if (_url.StartsWith("https://")) {
                _url = _url[8..];
            } else if (_url.StartsWith("http://")) {
                _url = _url[7..];
            }

            string path = string.Empty;
            if (_url.Contains("tumblr")) {//something.tumblr.com
                path = IconResources.tumblr_PNG;
            }
            if (_url.StartsWith("twitter") || _url.StartsWith("www.twitter") || _url.StartsWith("pbs.twimg")) {
                path = IconResources.Twitter_PNG;
            } else if (_url.StartsWith("www.furaffinity") || _url.StartsWith("furaffinity") || _url.StartsWith("d.furaffinity")) {
                path = IconResources.Furaffinity_PNG;
            } else if (_url.StartsWith("www.deviantart") || _url.StartsWith("deviantart")) {
                path = IconResources.DeviantArt_PNG;
            } else if (_url.StartsWith("www.inkbunny") || _url.StartsWith("inkbunny")) {
                path = IconResources.InkBunny_PNG;
            } else if (_url.StartsWith("www.weasyl.com") || _url.StartsWith("weasyl.com")) {
                path = IconResources.weasyl_PNG;
            } else if (_url.StartsWith("www.pixiv") || _url.StartsWith("pixiv")) {
                path = IconResources.Pixiv_PNG;
            } else if (_url.StartsWith("www.instagram") || _url.StartsWith("instagram")) {
                path = IconResources.Instagram_PNG;
            } else if (_url.StartsWith("www.patreon") || _url.StartsWith("patreon")) {
                path = IconResources.Patreon_PNG;
            } else if (_url.StartsWith("www.subscribestar") || _url.StartsWith("subscribestar")) {
                path = IconResources.SubscribeStar_PNG;
            } else if (_url.StartsWith("mega")) {
                path = IconResources.Mega_PNG;
            } else if (_url.StartsWith("furrynetwork")) {
                path = IconResources.FurryNetwork_PNG;
            } else if (_url.StartsWith("t.me")) {
                path = IconResources.Telegram_PNG;
            } else if (_url.StartsWith("newgrounds") || _url.StartsWith("www.newgrounds")) {
                path = IconResources.NewGrounds_PNG;
            }

            return path;
        } else {
            return string.Empty;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
        throw new NotSupportedException();
    }
}
