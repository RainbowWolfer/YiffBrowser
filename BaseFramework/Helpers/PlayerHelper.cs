using FlyleafLib.MediaPlayer;

namespace BaseFramework.Helpers;

public static class PlayerHelper {

	public static void TogglePlayPauseEx(this Player player) {
		if (player.Status is Status.Playing) {
			player.Pause();
		} else if (player.Status is Status.Ended) {
			player.Seek(0);
			player.Play();
		} else {
			player.Play();
		}
	}

}
