using DevExpress.Mvvm;
using RW.Base.WPF.Interfaces;
using YiffBrowser.BaseFramework.Enums;

namespace YiffBrowser.BaseFramework.Services;

public interface IVideoControlsService {
	VideoControlsParameters Parameters { get; }
}

internal class VideoControlsService : IVideoControlsService, IAppInitialize {
	public string Description => "";
	public int Priority => IntPriority.Normal;

	public VideoControlsParameters Parameters { get; } = new();


	public void AppInitialize(IStatusReport statusReport) {

	}


}

public class VideoControlsParameters : BindableBase {

	public bool AlwaysShowControls {
		get => GetProperty(() => AlwaysShowControls);
		set => SetProperty(() => AlwaysShowControls, value);
	}

	public VerticalPlacement ControlLocation {
		get => GetProperty(() => ControlLocation);
		set => SetProperty(() => ControlLocation, value);
	}

	public bool ShowScrubberBar {
		get => GetProperty(() => ShowScrubberBar);
		set => SetProperty(() => ShowScrubberBar, value);
	}

	public VerticalPlacement ScrubberBarLocation {
		get => GetProperty(() => ScrubberBarLocation);
		set => SetProperty(() => ScrubberBarLocation, value);
	}

	public TimeProgressFormat TimeProgressFormat {
		get => GetProperty(() => TimeProgressFormat);
		set => SetProperty(() => TimeProgressFormat, value);
	}

	public bool ShowTimeDisplay {
		get => GetProperty(() => ShowTimeDisplay);
		set => SetProperty(() => ShowTimeDisplay, value);
	}

	public bool NextPreviousFrameButton {
		get => GetProperty(() => NextPreviousFrameButton);
		set => SetProperty(() => NextPreviousFrameButton, value);
	}

	public bool LoopButton {
		get => GetProperty(() => LoopButton);
		set => SetProperty(() => LoopButton, value);
	}

	public bool ReversePlaybackButton {
		get => GetProperty(() => ReversePlaybackButton);
		set => SetProperty(() => ReversePlaybackButton, value);
	}

	public bool VolumeButton {
		get => GetProperty(() => VolumeButton);
		set => SetProperty(() => VolumeButton, value);
	}

}