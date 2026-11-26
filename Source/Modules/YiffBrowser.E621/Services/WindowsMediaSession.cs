using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using Windows.Media;
using Windows.Storage.Streams;
using YiffBrowser.BaseFramework;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Views.Subs;

namespace YiffBrowser.E621.Services;

/// <summary>
/// Publishes the single active video to the Windows system media controls
/// (volume flyout, keyboard media keys, headset buttons).
/// </summary>
internal static class WindowsMediaSession {
	private static SystemMediaTransportControls? controls;
	private static IntPtr windowHandle;
	private static VideoDisplayer? current;
	private static int publishedPostId = -1;

	public static void Update(VideoDisplayer source, bool claim) {
		try {
			if (claim) {
				current = source;
			}
			if (!ReferenceEquals(current, source)) {
				return;
			}

			SystemMediaTransportControls? session = EnsureControls(source);
			if (session == null || source.Player == null) {
				return;
			}

			session.IsEnabled = true;
			session.PlaybackStatus = source.IsMediaPlaying
				? MediaPlaybackStatus.Playing
				: MediaPlaybackStatus.Paused;
			PublishMetadata(session, source.MediaPost);
			PublishTimeline(session, source);
		} catch (Exception ex) {
			Debug.WriteLine($"Windows media session: {ex.Message}");
		}
	}

	public static void Close(VideoDisplayer source) {
		if (!ReferenceEquals(current, source)) {
			return;
		}

		current = null;
		publishedPostId = -1;
		if (controls == null) {
			return;
		}

		try {
			controls.PlaybackStatus = MediaPlaybackStatus.Closed;
			controls.DisplayUpdater.ClearAll();
			controls.IsEnabled = false;
		} catch (Exception ex) {
			Debug.WriteLine($"Windows media session: {ex.Message}");
		}
	}

	private static SystemMediaTransportControls? EnsureControls(VideoDisplayer source) {
		Window? window = Window.GetWindow(source);
		if (window == null) {
			return controls;
		}

		IntPtr handle = new WindowInteropHelper(window).EnsureHandle();
		if (handle == IntPtr.Zero) {
			return controls;
		}
		if (controls != null && windowHandle == handle) {
			return controls;
		}

		if (controls != null) {
			controls.ButtonPressed -= OnButtonPressed;
			controls.PlaybackPositionChangeRequested -= OnPositionRequested;
			controls.IsEnabled = false;
		}

		controls = SystemMediaTransportControlsInterop.GetForWindow(handle);
		windowHandle = handle;
		controls.IsPlayEnabled = true;
		controls.IsPauseEnabled = true;
		controls.IsStopEnabled = false;
		controls.IsNextEnabled = false;
		controls.IsPreviousEnabled = false;
		controls.ButtonPressed += OnButtonPressed;
		controls.PlaybackPositionChangeRequested += OnPositionRequested;
		publishedPostId = -1;
		return controls;
	}

	private static void PublishMetadata(SystemMediaTransportControls session, E621Post? post) {
		int postId = post?.ID ?? 0;
		if (postId == publishedPostId) {
			return;
		}
		publishedPostId = postId;

		SystemMediaTransportControlsDisplayUpdater updater = session.DisplayUpdater;
		updater.ClearAll();
		updater.Type = MediaPlaybackType.Video;
		string artists = FormatArtists(post?.Tags?.Artist);
		if (artists.Length > 0) {
			updater.VideoProperties.Title = artists;
			updater.VideoProperties.Subtitle = postId > 0 ? $"#{postId}" : AppConfig.DisplayAppName;
		} else {
			updater.VideoProperties.Title = postId > 0 ? $"#{postId}" : AppConfig.DisplayAppName;
			updater.VideoProperties.Subtitle = AppConfig.DisplayAppName;
		}

		if (Uri.TryCreate(post?.Preview?.URL, UriKind.Absolute, out Uri? preview)) {
			updater.Thumbnail = RandomAccessStreamReference.CreateFromUri(preview);
		}
		updater.Update();
	}

	private static void PublishTimeline(SystemMediaTransportControls session, VideoDisplayer source) {
		long durationTicks = Math.Max(0, source.MediaDuration);
		long positionTicks = durationTicks == 0
			? 0
			: Math.Clamp(source.MediaPosition, 0, durationTicks);
		TimeSpan duration = TimeSpan.FromTicks(durationTicks);
		TimeSpan position = TimeSpan.FromTicks(positionTicks);
		session.UpdateTimelineProperties(new SystemMediaTransportControlsTimelineProperties {
			StartTime = TimeSpan.Zero,
			MinSeekTime = TimeSpan.Zero,
			Position = position,
			MaxSeekTime = duration,
			EndTime = duration,
		});
	}

	private static void OnButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args) {
		VideoDisplayer? displayer = current;
		if (displayer == null) {
			return;
		}

		displayer.Dispatcher.InvokeAsync(() => {
			if (!ReferenceEquals(current, displayer)) {
				return;
			}
			if (args.Button == SystemMediaTransportControlsButton.Play) {
				displayer.PlayFromSystem();
			} else if (args.Button == SystemMediaTransportControlsButton.Pause) {
				displayer.PauseFromSystem();
			}
		});
	}

	private static void OnPositionRequested(SystemMediaTransportControls sender, PlaybackPositionChangeRequestedEventArgs args) {
		VideoDisplayer? displayer = current;
		if (displayer == null) {
			return;
		}

		TimeSpan position = args.RequestedPlaybackPosition;
		displayer.Dispatcher.InvokeAsync(() => {
			if (!ReferenceEquals(current, displayer)) {
				return;
			}
			displayer.SeekFromSystem(position);
		});
	}

	private static string FormatArtists(List<string>? artists) {
		if (artists == null || artists.Count == 0) {
			return "";
		}
		return string.Join(", ", artists.Take(3).Select(name => name.Replace('_', ' ')));
	}
}
