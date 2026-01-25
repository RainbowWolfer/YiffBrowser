using FlyleafLib.MediaPlayer;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace BaseFramework.Controls;

public class PlaybackControl : ContentControl {


	public Player Player {
		get => (Player)GetValue(PlayerProperty);
		set => SetValue(PlayerProperty, value);
	}

	public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register(
		nameof(Player),
		typeof(Player),
		typeof(PlaybackControl),
		new PropertyMetadata(null)
	);


	public PlaybackControl() {

	}


	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

		if (GetTemplateChild("PositionSlider") is SliderEx slider) {
			slider.PreviewMouseLeftButtonDown2 += Slider_DragStarted;
			slider.PreviewMouseLeftButtonDown += Slider_DragStarted;
			slider.PreviewMouseLeftButtonUp += Slider_DragCompleted;
			slider.LostMouseCapture += Slider_LostMouseCapture;
		}

	}

	private bool wasPlayingBeforeDrag;

	private void Slider_DragStarted(object sender, MouseButtonEventArgs e) {
		wasPlayingBeforeDrag = Player.IsPlaying;
		Dispatcher.BeginInvoke(() => {
			Player.Pause();
		}, DispatcherPriority.Background);
	}

	private void Slider_DragCompleted(object sender, MouseButtonEventArgs e) {
		if (wasPlayingBeforeDrag) {
			Player.Play();
		}
	}

	private void Slider_LostMouseCapture(object sender, MouseEventArgs e) {
		if (wasPlayingBeforeDrag) {
			Player.Play();
		}
	}

}
