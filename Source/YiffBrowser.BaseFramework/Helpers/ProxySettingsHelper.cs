using System.Net;
using System.Net.Http;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Services;

namespace YiffBrowser.BaseFramework.Helpers;

public static class ProxySettingsHelper {
	public static void Configure(SocketsHttpHandler handler, AppSettingsModel model) {
		Apply(model, out bool useProxy, out IWebProxy? proxy);
		handler.UseProxy = useProxy;
		handler.Proxy = proxy;
	}

	public static void Configure(HttpClientHandler handler, AppSettingsModel model) {
		Apply(model, out bool useProxy, out IWebProxy? proxy);
		handler.UseProxy = useProxy;
		handler.Proxy = proxy;
	}

	public static HttpClient CreateHttpClient(AppSettingsModel? model = null) {
		model ??= AppSettingsService.Instance.Model;

		HttpClientHandler handler = new() {
			AutomaticDecompression = DecompressionMethods.All,
			UseCookies = false,
		};
		Configure(handler, model);

		return new HttpClient(handler);
	}

	private static void Apply(AppSettingsModel model, out bool useProxy, out IWebProxy? proxy) {
		switch (model.ProxyMode) {
			case ProxyMode.None:
				useProxy = false;
				proxy = null;
				break;

			case ProxyMode.Custom:
				useProxy = true;
				WebProxy webProxy = new(model.ProxyHost.Trim(), model.ProxyPort);
				if (!string.IsNullOrWhiteSpace(model.ProxyUsername)) {
					webProxy.Credentials = new NetworkCredential(model.ProxyUsername, model.ProxyPassword);
				}
				proxy = webProxy;
				break;

			case ProxyMode.System:
			default:
				useProxy = true;
				proxy = HttpClient.DefaultProxy;
				break;
		}
	}
}
