using DevExpress.Mvvm;
using RW.Base.WPF.Interfaces;
using System.ComponentModel;
using YiffBrowser.BaseFramework.Enums;

namespace YiffBrowser.BaseFramework.Services;

public interface IVideoControlsService {
	VideoControlsParameters Parameters { get; }
}

internal class VideoControlsService(IAppProfileService appProfileService) : IVideoControlsService, IAppInitialize {
	public string Description => "";
	public int Priority => IntPriority.Normal;

	public VideoControlsParameters Parameters { get; } = new();


	public void AppInitialize(IStatusReport statusReport) {
		Parameters.Apply(appProfileService.Model.VideoControls);
		Parameters.PropertyChanged += Parameters_PropertyChanged;
	}

	private void Parameters_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
		Parameters.WriteTo(appProfileService.Model.VideoControls);
		appProfileService.ScheduleSave();
	}

}

public class VideoControlsParameters : BindableBase {

	public VideoControlsParameters() {
		Apply(new VideoControlsState());
	}

	public void Apply(VideoControlsState state) {
		AlwaysShowControls = state.AlwaysShowControls;
		ControlLocation = state.ControlLocation;
		ShowScrubberBar = state.ShowScrubberBar;
		ScrubberBarLocation = state.ScrubberBarLocation;
		TimeProgressFormat = state.TimeProgressFormat;
		ShowTimeDisplay = state.ShowTimeDisplay;
		NextPreviousFrameButton = state.NextPreviousFrameButton;
		LoopButton = state.LoopButton;
		ReversePlaybackButton = state.ReversePlaybackButton;
		VolumeButton = state.VolumeButton;
	}

	public void WriteTo(VideoControlsState state) {
		state.AlwaysShowControls = AlwaysShowControls;
		state.ControlLocation = ControlLocation;
		state.ShowScrubberBar = ShowScrubberBar;
		state.ScrubberBarLocation = ScrubberBarLocation;
		state.TimeProgressFormat = TimeProgressFormat;
		state.ShowTimeDisplay = ShowTimeDisplay;
		state.NextPreviousFrameButton = NextPreviousFrameButton;
		state.LoopButton = LoopButton;
		state.ReversePlaybackButton = ReversePlaybackButton;
		state.VolumeButton = VolumeButton;
	}

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