using BaseFramework.Helpers;
using FlyleafLib.MediaPlayer;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace BaseFramework.Controls;

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

	private readonly DispatcherTimer dispatcherTimer;

	public bool IsPlaying => Player != null && Player.IsPlaying;

	public long CurTime {
		get => Player?.CurTime ?? 0;
		set {
			if (Player != null) {
				Player.CurTime = value;
			}
		}
	}

	private bool wasPlayingBeforeDrag;

	public PlaybackControl() {
		dispatcherTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, Tick, Dispatcher);
		dispatcherTimer.Start();
	}

	private void Tick(object? sender, EventArgs e) {
		Raise(nameof(IsPlaying));
		Raise(nameof(CurTime));
		Raise("Player.CurTime");
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

		if (GetTemplateChild("PlayButton") is ButtonBase playButton) {
			playButton.Click += PlayButton_Click;
		}

	}

	private void PlayButton_Click(object sender, RoutedEventArgs e) {
		Player?.TogglePlayPauseEx();
	}

	private void PlayTimeBorder_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
		if (e.ChangedButton is MouseButton.Left) {
			//todo : switch play time mode
			// 1. 00:04 / 00:20
			// 2. 00:04 / -00:16
			// 2. 128 / 1227 (frame count)
			// 2. 128 / -1099 (frame count)
			e.Handled = true;
		}
	}

	private void Slider_DragStarted(object sender, MouseButtonEventArgs e) {
		if (Player is null) {
			return;
		}
		wasPlayingBeforeDrag = Player.IsPlaying;
		Dispatcher.BeginInvoke(() => {
			Player.Pause();
		}, DispatcherPriority.Background);
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
