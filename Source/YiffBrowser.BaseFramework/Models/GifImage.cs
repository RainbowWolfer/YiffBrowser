using System.IO;
using System.Windows.Media.Imaging;

namespace YiffBrowser.BaseFramework.Models;

/// <summary>
/// Holds the raw bytes of an animated gif plus its dimensions. Downloading is done by
/// <see cref="Services.MediaDownloadService"/>, this type only decodes and hands out the stream.
/// </summary>
public class GifImage(Uri uri) : IDisposable {
	private bool disposedValue;
	private byte[]? bytes;

	public Uri Uri { get; } = uri;

	/// <summary>
	/// A new stream positioned at the start, over the same bytes. Callers do not share a cursor,
	/// so a thumbnail and the detail view can decode the same gif at the same time.
	/// </summary>
	public MemoryStream? GetMemoryStream() {
		if (bytes == null) {
			return null;
		}
		return new MemoryStream(bytes, writable: false);
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

		bytes = data;
		IsInitialized = true;
	}

	protected virtual void Dispose(bool disposing) {
		if (!disposedValue) {
			if (disposing) {
				bytes = null;
			}

			disposedValue = true;
		}
	}

	void IDisposable.Dispose() {
		Dispose(disposing: true);
	}

}
