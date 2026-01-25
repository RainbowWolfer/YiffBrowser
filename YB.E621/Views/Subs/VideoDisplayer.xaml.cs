using BaseFramework.Enums;
using FlyleafLib;
using FlyleafLib.MediaPlayer;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Helpers;
using YB.E621.Models.E621;

namespace YB.E621.Views.Subs;

public partial class VideoDisplayer : UserControl {


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




	private readonly DispatcherTimer dispatcherTimer;

	public Player Player { get; }
	public Config Config { get; }

	public VideoDisplayer() {
		InitializeComponent();
		Slider slider = new();

		//if (ViewHelper.IsInDesignerMode) {
		//	return;
		//}

		Config = new Config();

		Player = new Player(Config) {
			LoopPlayback = true,
		};

		dispatcherTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, Tick, Dispatcher) {
			IsEnabled = false,
		};

		Config.Player.AutoPlay = true;
		Config.Player.SeekAccurate = true;
		Config.Player.Stats = true;

		// Keep track of error messages
		Player.OpenCompleted += (o, e) => {
			Debug.WriteLine(e.Error);
		};
		Player.BufferingCompleted += (o, e) => {
			Debug.WriteLine(e.Error);
		};

		FlyleafHost.Player = Player;

		FlyleafHost.Surface.MouseDown += Surface_MouseDown;

		Loaded += VideoDisplayer_Loaded;
		Unloaded += VideoDisplayer_Unloaded;

		Player.Stop();
	}

	private void Surface_MouseDown(object sender, MouseButtonEventArgs e) {
		if (e.ChangedButton is MouseButton.Left) {
			ShowControls = !ShowControls;
		}
	}

	private void VideoDisplayer_Unloaded(object sender, RoutedEventArgs e) {
		dispatcherTimer.Stop();
	}

	private void VideoDisplayer_Loaded(object sender, RoutedEventArgs e) {
		dispatcherTimer.Start();
	}

	private void Tick(object? sender, EventArgs e) {

	}

	private void Update() {
		Update(Post);
	}

	private async void Update(E621Post? post) {
		if (post is null || post.GetFileType() is not FileType.WEBM) {
			return;
		}

		string? url = post.File?.URL;
		if (url != null) {
			MemoryStream memoryStream = new();
			await Download(url, memoryStream);

			Player.Open(memoryStream);
			Player.Play();

		}

	}

	private async Task Download(string url, MemoryStream memoryStream) {
		//if (!System.IO.File.Exists(tempFile)) {
		try {
			using HttpClient client = new();
			// 使用 HttpCompletionOption.ResponseHeadersRead 
			// 这样我们拿到 Header 就可以知道文件总大小，而不用等下载完
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
					double progress = (double)totalRead / totalBytes * 100;
					// 打印进度
					Debug.WriteLine($"下载进度: {progress:F2}% ({totalRead}/{totalBytes} bytes)");

					// 如果你 UI 上有 ProgressBar，可以在这里更新
					// Dispatcher.Invoke(() => downloadProgressBar.Value = progress);
				}
			}
			Debug.WriteLine("下载完成！");
		} catch (Exception ex) {
			Debug.WriteLine($"下载出错: {ex.Message}");
			return;
		}
		//}
		//// 调用 Flyleaf 播放本地文件
		//await Player.OpenAsync(tempFile);
	}

}
