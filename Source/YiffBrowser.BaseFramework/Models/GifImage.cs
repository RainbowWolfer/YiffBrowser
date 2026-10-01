using System.IO;
using System.Windows.Media.Imaging;

namespace YiffBrowser.BaseFramework.Models;

/// <summary>
/// Holds the raw bytes of an animated gif plus its dimensions. Downloading is done by
/// <see cref="Services.MediaDownloadService"/>, this type only decodes and hands out the stream.
/// </summary>
public class GifImage(Uri uri) : IDisposable {
	private bool disposedValue;
	private MemoryStream? memoryStream;

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

	/// <summary>Decodes downloaded bytes. Safe to call from a background thread.</summary>
	public void Load(byte[] data) {
		// The decoder reads lazily, so it gets a stream of its own and consumers get another one.
		using MemoryStream decodeStream = new(data, writable: false);

		GifBitmapDecoder decoder = new(decodeStream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
		if (decoder.Frames.Count > 0) {
			Width = decoder.Frames[0].PixelWidth;
			Height = decoder.Frames[0].PixelHeight;
		} else {
			Width = 0;
			Height = 0;
		}

		memoryStream = new MemoryStream(data, writable: false);
		IsInitialized = true;
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
