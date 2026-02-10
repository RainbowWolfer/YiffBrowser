using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.BaseFramework.ViewModels;
using DevExpress.Mvvm;
using FlyleafLib;
using FlyleafLib.MediaPlayer;
using RW.Common.Helpers;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using YiffBrowser.E621.Helpers;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Enums;

namespace YiffBrowser.E621.Views.Subs;

public partial class VideoDisplayer : UserControl, INotifyPropertyChanged {
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

	public LoadingStatusViewModel LoadingStatus {
		get => (LoadingStatusViewModel)GetValue(LoadingStatusProperty);
		private set => SetValue(LoadingStatusPropertyKey, value);
	}

	private static readonly DependencyPropertyKey LoadingStatusPropertyKey = DependencyProperty.RegisterReadOnly(
		nameof(LoadingStatus),
		typeof(LoadingStatusViewModel),
		typeof(VideoDisplayer),
		new PropertyMetadata(new LoadingStatusViewModel())
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


	private readonly DispatcherTimer dispatcherTimer;

	private long fileSize = 0;

	public Player? Player { get; private set; }
	public Config? Config { get; private set; }

	public VideoDisplayer() {
		InitializeComponent();

		dispatcherTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, Tick, Dispatcher) {
			IsEnabled = false,
		};

		Loaded += VideoDisplayer_Loaded;
		Unloaded += VideoDisplayer_Unloaded;

	}

	private void VideoDisplayer_Loaded(object sender, RoutedEventArgs e) {
		dispatcherTimer.Start();

		//if (ViewHelper.IsInDesignerMode) {
		//	return;
		//}

		Config = new Config();

		Player = new Player(Config) {
			LoopPlayback = true,
		};

		Config.Player.AutoPlay = true;
		Config.Player.SeekAccurate = true;
		Config.Player.Stats = true;

		// Keep track of error messages
		Player.OpenCompleted += (o, e) => {
			if (e.Error.IsNotBlank()) {
				Debug.WriteLine("Player.OpenCompleted" + e.Error);
			}
		};
		Player.BufferingCompleted += (o, e) => {
			if (e.Error.IsNotBlank()) {
				Debug.WriteLine("Player.BufferingCompleted" + e.Error);
			}
		};

		FlyleafHost.Player = Player;

		FlyleafHost.Surface.Title = $"{Guid.NewGuid()}";
		FlyleafHost.Surface.MouseDown += Surface_MouseDown;
		FlyleafHost.Surface.MouseDoubleClick += Surface_MouseDoubleClick;
		FlyleafHost.Surface.ContextMenu = CreateContextMenu();

		FlyleafHost.Surface.SetBinding(ContextMenuService.IsEnabledProperty, new Binding(nameof(IsFileReady)) {
			Source = this,
		});

		Player.Stop();

		Raise(nameof(Config));
		Raise(nameof(Player));
	}

	private void VideoDisplayer_Unloaded(object sender, RoutedEventArgs e) {
		dispatcherTimer.Stop();
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

	}

	private void Update() {
		Update(Post);
	}

	private async void Update(E621Post? post) {
		if (Player is null) {
			return;
		}

		if (post is null || !post.GetFileType().IsVideo()) {
			return;
		}

		IsFileReady = false;
		fileSize = post.File?.Size ?? 0;

		string? url = post.File?.URL;
		if (url != null) {
			LoadingStatus.InitialLoading();

			MemoryStream memoryStream = new();
			await Download(url, memoryStream);

			Player.Open(memoryStream);
			Player.Play();
		}

	}

	private async Task Download(string url, MemoryStream memoryStream) {
		try {
			using HttpClient client = new();
			using HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode();

			long totalBytes = response.Content.Headers.ContentLength ?? -1L;
			bool canReportProgress = totalBytes != -1;

			using Stream contentStream = await response.Content.ReadAsStreamAsync();

			byte[] buffer = new byte[8192];
			long totalRead = 0;
			int read;

			while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
				await memoryStream.WriteAsync(buffer, 0, read);
				totalRead += read;
				if (canReportProgress) {
					double progress = (double)totalRead / totalBytes;
					long downloaded = (long)(fileSize * progress);
					string downloadInfo = $"{downloaded.FileSizeToKB()} / {fileSize.FileSizeToKB()}";

					LoadingStatus.SetProgress(progress, downloadInfo);
				}
			}

			LoadingStatus.DoneLoading();
			IsFileReady = true;
		} catch (Exception ex) {
			LoadingStatus.LoadingError($"Loading Error : {ex.Message}");
			return;
		}
	}


	private DelegateCommand? reloadCommand;
	public IDelegateCommand ReloadCommand => reloadCommand ??= new(Reload);
	private void Reload() {

	}


	private ContextMenu CreateContextMenu() {
		ContextMenu contextMenu = new();

		contextMenu.Items.Add(new MenuItem() {
			Header = "1",
		});

		contextMenu.Items.Add(new MenuItem() {
			Header = "2",
		});

		return contextMenu;
	}

}
