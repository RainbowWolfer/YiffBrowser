using BaseFramework.Events;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BaseFramework.Models {
	public class GifImage(Uri uri) : IDisposable {
		private bool disposedValue;

		public event TypedEventHandler<GifImage, EventArgs>? DownloadCompleted;
		public event TypedEventHandler<GifImage, int>? DownloadProgress;
		public event TypedEventHandler<GifImage, Exception>? DownloadFailed;

		public Uri Uri { get; } = uri;

		private ThreadLocal<MemoryStream?> MemoryStream { get; } = new();

		public MemoryStream? GetMemoryStream() => MemoryStream.Value;

		public int Width { get; private set; }
		public int Height { get; private set; }

		public bool IsInitialized { get; private set; } = false;

		public async void Initialize() {
			if (IsInitialized) {
				return;
			}

			IsInitialized = true;

			Action? action = null;

			using MemoryStream memoryStream = new();
			bool success = false;

			await Task.Run(async () => {

				using HttpClient client = new();

				//NetCode.AddDefaultRequestHeaders(client, "", "");

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
						int read = await contentStream.ReadAsync(buffer);
						if (read == 0) {
							isMoreToRead = false;
						} else {
							await memoryStream.WriteAsync(buffer.AsMemory(0, read));
							totalRead += read;

							if (contentLength.HasValue) {
								double progress = (double)totalRead / contentLength.Value * 100;
								DownloadProgress?.Invoke(this, (int)Math.Round(progress));
							}
						}
					} while (isMoreToRead);

					memoryStream.Position = 0; // Reset stream position before usage

					// Create GifBitmapDecoder to get dimensions
					GifBitmapDecoder decoder = new(memoryStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
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
				MemoryStream.Value = new MemoryStream(memoryStream.ToArray(), false);
			}

			action?.Invoke();
		}

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					// TODO: dispose managed state (managed objects)
					MemoryStream?.Dispose();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override finalizer
				// TODO: set large fields to null
				disposedValue = true;
			}
		}

		// TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
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
}
