using FlyleafLib.MediaPlayer;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Enums;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.BaseFramework.Services;

namespace YiffBrowser.BaseFramework.Controls;

public class PlaybackControl : ContentControl, INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;
	private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

	public Player? Player {
		get => (Player?)GetValue(PlayerProperty);
		set => SetValue(PlayerProperty, value);
	}

	public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register(
		nameof(Player),
		typeof(Player),
		typeof(PlaybackControl),
		new PropertyMetadata(null)
	);


	public VideoControlsParameters VideoControlsParameters {
		get => (VideoControlsParameters)GetValue(VideoControlsParametersProperty);
		set => SetValue(VideoControlsParametersProperty, value);
	}

	public static readonly DependencyProperty VideoControlsParametersProperty = DependencyProperty.Register(
		nameof(VideoControlsParameters),
		typeof(VideoControlsParameters),
		typeof(PlaybackControl),
		new PropertyMetadata(null)
	);


	public bool IsPlaying => Player != null && Player.IsPlaying;

	public long CurTime {
		get => Player?.CurTime ?? 0;
		set => Player?.CurTime = value;
	}

	private bool wasPlayingBeforeDrag = false;
	private bool isHandingDragStarted = false;

	private TextBlock? playTimeText;
	private readonly DispatcherTimer dispatcherTimer;

	public PlaybackControl() {
		dispatcherTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, Tick, Dispatcher);
		dispatcherTimer.Start();
	}

	private void Tick(object? sender, EventArgs e) {
		if (IsVisible) {
			Raise(nameof(IsPlaying));
			Raise(nameof(CurTime));
			Raise("Player.CurTime");
			UpdatePlayTimeText();
		}
	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (GetTemplateChild("PositionSlider") is SliderEx slider) {
			slider.PreviewMouseLeftButtonDown2 += Slider_DragStarted;
			slider.PreviewMouseLeftButtonDown += Slider_DragStarted;
			slider.PreviewMouseLeftButtonUp += Slider_DragCompleted;
			slider.LostMouseCapture += Slider_LostMouseCapture;
		}

		if (GetTemplateChild("PlayTimeBorder") is Border playTimeBorder) {
			playTimeBorder.PreviewMouseDown += PlayTimeBorder_PreviewMouseDown;
		}

		playTimeText = GetTemplateChild("PlayTimeText") as TextBlock;
		UpdatePlayTimeText();

		if (GetTemplateChild("PlayButton") is ButtonBase playButton) {
			playButton.Click += PlayButton_Click;
		}

	}

	private void PlayButton_Click(object sender, RoutedEventArgs e) {
		Player?.TogglePlayPauseEx();
	}

	private void PlayTimeBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
		if (e.ChangedButton is MouseButton.Left && VideoControlsParameters != null) {
			TimeProgressFormat[] values = Enum.GetValues<TimeProgressFormat>();
			int index = Array.IndexOf(values, VideoControlsParameters.TimeProgressFormat);
			VideoControlsParameters.TimeProgressFormat = values[(index + 1) % values.Length];
			UpdatePlayTimeText();
			e.Handled = true;
		}
	}

	private void UpdatePlayTimeText() {
		if (playTimeText is null) {
			return;
		}

		long cur = Player?.CurTime ?? 0;
		long duration = Player?.Duration ?? 0;
		TimeProgressFormat format = VideoControlsParameters?.TimeProgressFormat ?? TimeProgressFormat.Time_Total;

		playTimeText.Text = format switch {
			TimeProgressFormat.Time_Ramaining => $"{FormatTime(cur)} / -{FormatTime(Math.Max(0, duration - cur))}",
			TimeProgressFormat.Frame_Total => $"{FormatFrame(cur)} / {FormatFrame(duration)}",
			TimeProgressFormat.Frame_Ramaining => $"{FormatFrame(cur)} / -{FormatFrame(Math.Max(0, duration - cur))}",
			_ => $"{FormatTime(cur)} / {FormatTime(duration)}",
		};
	}

	private static string FormatTime(long ticks) {
		TimeSpan timeSpan = TimeSpan.FromTicks(Math.Max(0, ticks));
		return $"{timeSpan:mm\\:ss}";
	}

	// Approximate frame index from time using 30fps when decoder fps is unavailable.
	private static string FormatFrame(long ticks) {
		double seconds = TimeSpan.FromTicks(Math.Max(0, ticks)).TotalSeconds;
		return $"{(int)Math.Round(seconds * 30)}";
	}

	private void Slider_DragStarted(object sender, MouseButtonEventArgs e) {
		if (Player is null || isHandingDragStarted) {
			return;
		}
		isHandingDragStarted = true;
		wasPlayingBeforeDrag = Player.IsPlaying;
		Dispatcher.BeginInvoke(() => {
			Player.Pause();
			isHandingDragStarted = false;
		}, DispatcherPriority.Normal);
	}

	private void Slider_DragCompleted(object sender, MouseButtonEventArgs e) {
		if (Player is null) {
			return;
		}
		if (wasPlayingBeforeDrag) {
			Player.Play();
		}
	}

	private void Slider_LostMouseCapture(object sender, MouseEventArgs e) {
		if (Player is null) {
			return;
		}
		if (wasPlayingBeforeDrag) {
			Player.Play();
		}
	}

}
