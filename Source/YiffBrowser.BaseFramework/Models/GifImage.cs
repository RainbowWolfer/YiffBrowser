using RW.Common;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using YiffBrowser.BaseFramework.Helpers;

namespace YiffBrowser.BaseFramework.Models;

public class GifImage(Uri uri) : IDisposable {
	private bool disposedValue;
	private MemoryStream? memoryStream;

	public event TypedEventHandler<GifImage, EventArgs>? DownloadCompleted;
	public event TypedEventHandler<GifImage, int>? DownloadProgress;
	public event TypedEventHandler<GifImage, Exception>? DownloadFailed;

	public Uri Uri { get; } = uri;

	public MemoryStream? GetMemoryStream() {
		if (memoryStream != null) {
			memoryStream.Position = 0;
		}
		return memoryStream;
	}

	public int Width { get; private set; }
	public int Height { get; private set; }

	public bool IsInitialized { get; private set; } = false;

	public async void Initialize() {
		if (IsInitialized) {
			return;
		}

		IsInitialized = true;

		Action? action = null;

		using MemoryStream downloadStream = new();
		bool success = false;

		await Task.Run(async () => {

			using HttpClient client = ProxySettingsHelper.CreateHttpClient();

			try {
				using HttpRequestMessage request = new(HttpMethod.Get, Uri);
				using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

				response.EnsureSuccessStatusCode();
				long? contentLength = response.Content.Headers.ContentLength;

				using Stream contentStream = await response.Content.ReadAsStreamAsync();

				long totalRead = 0L;
				byte[] buffer = new byte[8192 * 10];
				bool isMoreToRead = true;

				do {
					int read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
					if (read == 0) {
						isMoreToRead = false;
					} else {
						await downloadStream.WriteAsync(buffer, 0, read);
						totalRead += read;

						if (contentLength.HasValue) {
							double progress = (double)totalRead / contentLength.Value * 100;
							DownloadProgress?.Invoke(this, (int)Math.Round(progress));
						}
					}
				} while (isMoreToRead);

				downloadStream.Position = 0;

				// Create GifBitmapDecoder to get dimensions
				GifBitmapDecoder decoder = new(downloadStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
				if (decoder.Frames.Count > 0) {
					Width = decoder.Frames[0].PixelWidth;
					Height = decoder.Frames[0].PixelHeight;
				} else {
					Width = 0;
					Height = 0;
				}

				success = true;

				action = () => {
					DownloadCompleted?.Invoke(this, EventArgs.Empty);
				};
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				action = () => {
					DownloadFailed?.Invoke(this, ex);
				};
				IsInitialized = false;
			}
		});

		if (success) {
			memoryStream = new MemoryStream(downloadStream.ToArray(), false);
		}

		action?.Invoke();
	}

	protected virtual void Dispose(bool disposing) {
		if (!disposedValue) {
			if (disposing) {
				memoryStream?.Dispose();
				memoryStream = null;
			}

			disposedValue = true;
		}
	}

	// override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
	~GifImage() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: false);
	}

	void IDisposable.Dispose() {
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

}
