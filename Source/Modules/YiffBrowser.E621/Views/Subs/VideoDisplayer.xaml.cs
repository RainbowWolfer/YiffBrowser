using DevExpress.Mvvm;
using FlyleafLib;
using FlyleafLib.MediaPlayer;
using RW.Common.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.Views;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.E621.Controls;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Helpers;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Views.Subs;

public partial class VideoDisplayer : UserControl, INotifyPropertyChanged, IPostDisplayer {
	public event PropertyChangedEventHandler? PropertyChanged;
	private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


	public E621Post? Post {
		get => (E621Post)GetValue(PostProperty);
		set => SetValue(PostProperty, value);
	}

	public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
		nameof(Post),
		typeof(E621Post),
		typeof(VideoDisplayer),
		new PropertyMetadata(null, OnPostChanged)
	);

	private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		((VideoDisplayer)d).Update();
	}



	public VideoControlsParameters VideoControlsParameters {
		get => (VideoControlsParameters)GetValue(VideoControlsParametersProperty);
		set => SetValue(VideoControlsParametersProperty, value);
	}

	public static readonly DependencyProperty VideoControlsParametersProperty = DependencyProperty.Register(
		nameof(VideoControlsParameters),
		typeof(VideoControlsParameters),
		typeof(VideoDisplayer),
		new PropertyMetadata(null)
	);




	public bool ShowControls {
		get => (bool)GetValue(ShowControlsProperty);
		set => SetValue(ShowControlsProperty, value);
	}

	public static readonly DependencyProperty ShowControlsProperty = DependencyProperty.Register(
		nameof(ShowControls),
		typeof(bool),
		typeof(VideoDisplayer),
		new PropertyMetadata(false)
	);

	public BaseFramework.ViewModels.LoadingStatus LoadingStatus {
		get => (BaseFramework.ViewModels.LoadingStatus)GetValue(LoadingStatusProperty);
		private set => SetValue(LoadingStatusPropertyKey, value);
	}

	private static readonly DependencyPropertyKey LoadingStatusPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(LoadingStatus),
		typeof(BaseFramework.ViewModels.LoadingStatus),
		typeof(VideoDisplayer),
		new PropertyMetadata(new BaseFramework.ViewModels.LoadingStatus())
	);

	public static readonly DependencyProperty LoadingStatusProperty = LoadingStatusPropertyKey.DependencyProperty;

	public bool IsFileReady {
		get => (bool)GetValue(IsFileReadyProperty);
		private set => SetValue(IsFileReadyPropertyKey, value);
	}

	public static readonly DependencyPropertyKey IsFileReadyPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(IsFileReady),
		typeof(bool),
		typeof(VideoDisplayer),
		new PropertyMetadata(false)
	);

	public static readonly DependencyProperty IsFileReadyProperty = IsFileReadyPropertyKey.DependencyProperty;


	private static readonly List<WeakReference<VideoDisplayer>> activePlayers = [];

	private readonly DispatcherTimer dispatcherTimer;

	private long fileSize = 0;
	private int openedPostId;
	private bool videoOpenRetried;
	private int playerToken;
	private bool surfaceHooksReady;
	private bool playbackShown;
	private bool resumeWhenVisible;
	private bool registered;
	private bool sawPlaying;
	private int mediaTimelineTick;

	private VideoCacheItem? videoCacheItem;
	private bool? publishedPlaying;

	public Player? Player { get; private set; }
	public Config? Config { get; private set; }

	public VideoDisplayer() {
		InitializeComponent();

		dispatcherTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(100), DispatcherPriority.Normal, Tick, Dispatcher) {
			IsEnabled = false,
		};

		Loaded += VideoDisplayer_Loaded;
		Unloaded += VideoDisplayer_Unloaded;
		IsVisibleChanged += VideoDisplayer_IsVisibleChanged;
	}

	private void VideoDisplayer_Loaded(object sender, RoutedEventArgs e) {
		if (IsVisible) {
			ShowPlayback();
		}
	}

	private void VideoDisplayer_Unloaded(object sender, RoutedEventArgs e) {
		HidePlayback();
	}

	private void VideoDisplayer_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
		if (IsVisible) {
			ShowPlayback();
		} else {
			HidePlayback();
		}
	}

	private void ShowPlayback() {
		if (playbackShown && Player != null) {
			return;
		}

		playbackShown = true;
		bool created = Player == null;
		EnsurePlayer();
		if (!dispatcherTimer.IsEnabled) {
			dispatcherTimer.Start();
		}
		if (created) {
			if (!LoadingStatus.ShowLoading) {
				Update(Post);
			}
			return;
		}

		// The player and its position stay. Resume only when this page was playing before it was hidden.
		if (resumeWhenVisible) {
			PlayExclusive();
		}
		WindowsMediaSession.Update(this, claim: true);
	}

	private void HidePlayback() {
		if (!playbackShown) {
			return;
		}

		playbackShown = false;
		if (Player != null) {
			bool playing = IsPlayerPlaying(Player);
			if (playing || Player.Status is Status.Paused or Status.Ended or Status.Stopped) {
				resumeWhenVisible = playing;
			}
			if (playing) {
				Player.Pause();
			}
		}

		dispatcherTimer.Stop();
		PublishPlaying(false);
		sawPlaying = false;
		HideSurface();
		WindowsMediaSession.Update(this, claim: false);
	}

	private void PlayExclusive() {
		if (Player == null || !IsVisible) {
			resumeWhenVisible = false;
			return;
		}

		PauseOthers();
		Player.Play();
		resumeWhenVisible = true;
		PublishPlaying(true);
		WindowsMediaSession.Update(this, claim: true);
	}

	private void PauseOthers() {
		for (int i = activePlayers.Count - 1; i >= 0; i--) {
			if (!activePlayers[i].TryGetTarget(out VideoDisplayer? other)) {
				activePlayers.RemoveAt(i);
				continue;
			}
			if (ReferenceEquals(other, this) || other.Player == null || !IsPlayerPlaying(other.Player)) {
				continue;
			}
			other.Player.Pause();
			other.PublishPlaying(false);
		}
	}

	private void RegisterPlayer() {
		if (registered) {
			return;
		}
		registered = true;
		activePlayers.Add(new WeakReference<VideoDisplayer>(this));
	}

	private static bool IsPlayerPlaying(Player player) => player.IsPlaying || player.Status == Status.Playing;

	private void EnsurePlayer() {
		if (Player != null) {
			AttachSurfaceHooks();
			return;
		}

		int token = ++playerToken;
		Config = new Config();
		Player player = new(Config) {
			LoopPlayback = true,
			Stats = false,
		};
		Config.Playback.AutoPlay = false;
		Config.Playback.SeekAccurate = false;

		player.OpenCompleted += (o, e) => {
			if (token != playerToken || e.Error.IsBlank()) {
				return;
			}
			Debug.WriteLine("Player.OpenCompleted" + e.Error);
			Dispatcher.BeginInvoke(() => {
				if (token == playerToken) {
					HandleVideoOpenError(e.Error);
				}
			});
		};
		player.BufferingCompleted += (o, e) => {
			if (token == playerToken && e.Error.IsNotBlank()) {
				Debug.WriteLine("Player.BufferingCompleted" + e.Error);
			}
		};

		Player = player;
		FlyleafHost.Player = player;
		RegisterPlayer();
		AttachSurfaceHooks();
		player.Stop();

		Raise(nameof(Config));
		Raise(nameof(Player));
	}

	private void DisposePlayer() {
		Player? player = Player;
		if (player == null) {
			return;
		}

		playerToken++;
		Player = null;
		Config = null;
		FlyleafHost.Player = null;
		try {
			player.Stop();
		} catch (Exception ex) {
			Debug.WriteLine(ex.Message);
		}
		try {
			player.Dispose();
		} catch (Exception ex) {
			Debug.WriteLine(ex.Message);
		}
		Raise(nameof(Config));
		Raise(nameof(Player));
	}

	internal void ReleasePlayer() {
		playbackShown = false;
		resumeWhenVisible = false;
		sawPlaying = false;
		dispatcherTimer.Stop();
		videoCacheItem?.Updated -= Item_Updated;
		PublishPlaying(false);
		DisposePlayer();
		HideSurface();
		WindowsMediaSession.Close(this);
	}

	internal static void ReleaseIn(DependencyObject root) {
		int count = VisualTreeHelper.GetChildrenCount(root);
		for (int i = 0; i < count; i++) {
			DependencyObject child = VisualTreeHelper.GetChild(root, i);
			if (child is VideoDisplayer video) {
				video.ReleasePlayer();
			}
			ReleaseIn(child);
		}
	}

	private void AttachSurfaceHooks() {
		if (surfaceHooksReady || FlyleafHost.Surface == null) {
			return;
		}
		surfaceHooksReady = true;
		FlyleafHost.Surface.MouseDown += Surface_MouseDown;
		FlyleafHost.Surface.MouseDoubleClick += Surface_MouseDoubleClick;
		FlyleafHost.Surface.ContextMenu = CreateContextMenu();
	}

	private void HideSurface() {
		try {
			FlyleafHost.Overlay?.Hide();
			FlyleafHost.Surface?.Hide();
		} catch (Exception ex) {
			Debug.WriteLine(ex.Message);
		}
	}

	private void Surface_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
		if (Player is null) {
			return;
		}

		Player.TogglePlayPauseEx();

		Dispatcher.BeginInvoke(() => {
			if (Player.Status is Status.Playing) {
				ShowControls = false;
			} else {
				ShowControls = true;
			}
		}, DispatcherPriority.Normal);
	}

	private void Surface_MouseDown(object sender, MouseButtonEventArgs e) {
		if (e.ChangedButton is MouseButton.Left) {
			ShowControls = !ShowControls;
		}
	}

	private void Tick(object? sender, EventArgs e) {
		bool playing = Player != null && IsPlayerPlaying(Player);
		if (playing && !sawPlaying) {
			PauseOthers();
		}
		bool stateChanged = playing != sawPlaying;
		sawPlaying = playing;
		PublishPlaying(playing);
		if (IsVisible && Player != null) {
			Raise("Player.CurTime");
			Raise("CurTime");
			QuickProgressBar.Value = Player.CurTime;
			if (stateChanged || mediaTimelineTick >= 10) {
				mediaTimelineTick = 0;
				WindowsMediaSession.Update(this, claim: false);
			} else {
				mediaTimelineTick++;
			}
		}
	}

	internal bool IsMediaPlaying => Player != null && IsPlayerPlaying(Player);

	internal long MediaPosition => Player?.CurTime ?? 0;

	internal long MediaDuration => Player?.Duration ?? 0;

	internal E621Post? MediaPost => Post;

	internal void PlayFromSystem() {
		if (Player == null) {
			return;
		}
		if (Player.Status == Status.Ended) {
			Player.CurTime = 0;
		}
		PauseOthers();
		Player.Play();
		resumeWhenVisible = true;
		sawPlaying = true;
		PublishPlaying(true);
		WindowsMediaSession.Update(this, claim: true);
	}

	internal void PauseFromSystem() {
		if (Player == null) {
			return;
		}
		Player.Pause();
		resumeWhenVisible = false;
		sawPlaying = false;
		PublishPlaying(false);
		WindowsMediaSession.Update(this, claim: true);
	}

	internal void SeekFromSystem(TimeSpan position) {
		if (Player == null || Player.Duration <= 0) {
			return;
		}
		long ticks = Math.Clamp(position.Ticks, 0, Player.Duration);
		Player.CurTime = ticks;
		mediaTimelineTick = 0;
		WindowsMediaSession.Update(this, claim: true);
	}

	private void PublishPlaying(bool playing) {
		if (publishedPlaying == playing) {
			return;
		}
		publishedPlaying = playing;
		if (VisualHelper.GetParent<PostDetailView>(this)?.DataContext is PostDetailViewModel detail) {
			detail.ParentViewModel?.SetDetailVideoPlaying(playing);
		}
	}

	private void Update() {
		Update(Post);
	}

	private async void Update(E621Post? post) {
		if (Player is null) {
			return;
		}

		videoCacheItem?.Updated -= Item_Updated;

		ClearVideo();

		if (post is null || !post.GetFileType().IsVideo()) {
			return;
		}

		IsFileReady = false;
		fileSize = post.File?.Size ?? 0;
		if (openedPostId != post.ID) {
			openedPostId = post.ID;
			videoOpenRetried = false;
		}

		string? url = post.File?.URL;
		if (url != null && post.File != null) {
			LoadingStatus.Initialize();

			videoCacheItem = VideoCacheService.Get(url, post.File.Size, E621MediaCacheKeys.For(post, E621MediaCacheKeys.File, url));
			if (videoCacheItem.IsNull) {
				LoadingStatus.ErrorClose("Invalid media URL");
				return;
			}

			videoCacheItem.Initialize();

			if (videoCacheItem.HasCompleted) {
				OpenCached(videoCacheItem);
				LoadingStatus.Done();
				IsFileReady = true;
			} else {
				videoCacheItem.Updated += Item_Updated;
			}

		}

	}

	private void Item_Updated(VideoCacheItem sender, CacheLoadingModel args) {
		if (!CheckAccess()) {
			Dispatcher.Invoke(Item_Updated, DispatcherPriority.Loaded, sender, args);
			return;
		}

		if (Player == null) {
			return;
		}

		if (args.HasCompleted) {
			LoadingStatus.Done();
			OpenCached(sender);
			IsFileReady = true;
		} else if (args.HasError) {
			LoadingStatus.Error($"Loading Error : {args.Exception?.Message}");
		} else {
			double progress = args.Progress / 100d;
			long downloaded = (long)(fileSize * (args.Progress / 100d));
			string downloadInfo = $"{downloaded.FileSizeToKB()} / {fileSize.FileSizeToKB()}";

			LoadingStatus.SetProgress(progress, downloadInfo);
		}
	}

	private void HandleVideoOpenError(string error) {
		VideoCacheItem? item = videoCacheItem;
		if (item?.LocalPath.IsNotBlank() != true || videoOpenRetried) {
			LoadingStatus.Error(error);
			IsFileReady = false;
			return;
		}

		videoOpenRetried = true;
		item.DiscardDisk();
		Update();
	}

	private void OpenCached(VideoCacheItem item) {
		if (Player == null) {
			return;
		}
		if (item.LocalPath.IsNotBlank()) {
			_ = Player.Open(item.LocalPath);
			PlayExclusive();
			return;
		}

		SetVideo(item.MemoryStream);
	}

	private void SetVideo(MemoryStream? memoryStream) {
		if (memoryStream is null || Player == null) {
			return;
		}
		_ = Player.Open(memoryStream);
		PlayExclusive();
	}

	private void ClearVideo() {
		Player?.Stop();
		PublishPlaying(false);
		//Player?.Open(null);
	}


	private DelegateCommand? reloadCommand;
	public IDelegateCommand ReloadCommand => reloadCommand ??= new(Reload);
	private void Reload() {
		ClearVideo();
		videoOpenRetried = false;
		videoCacheItem?.DiscardDisk();
		Update();
	}


	private ContextMenu CreateContextMenu() {
		PostDisplayerContextMenu contextMenu = new() {
			IsVideo = true,
		};

		contextMenu.SetBinding(PostDisplayerContextMenu.DisplayerProperty, new Binding() {
			Source = this,
		});
		contextMenu.SetBinding(PostDisplayerContextMenu.VideoControlsParametersProperty, new Binding(nameof(VideoControlsParameters)) {
			Source = this,
		});

		return contextMenu;
	}

}
