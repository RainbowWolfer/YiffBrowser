using BaseFramework.Enums;
using BaseFramework.ViewModels;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.Views;

public partial class PostDetailDockView : UserControl, INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;
	private void Raise(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));



	public ModuleType? ModuleType {
		get => (ModuleType?)GetValue(ModuleTypeProperty);
		set => SetValue(ModuleTypeProperty, value);
	}

	public static readonly DependencyProperty ModuleTypeProperty = DependencyProperty.Register(
		nameof(ModuleType),
		typeof(ModuleType?),
		typeof(PostDetailDockView),
		new PropertyMetadata(null, OnModuleTypeChanged)
	);

	private static void OnModuleTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is PostDetailDockView view) {
			view.LoadComments();
		}
	}

	public E621Post Post {
		get => (E621Post)GetValue(PostProperty);
		set => SetValue(PostProperty, value);
	}

	public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
		nameof(Post),
		typeof(E621Post),
		typeof(PostDetailDockView),
		new PropertyMetadata(null, OnPostChanged)
	);

	private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is PostDetailDockView view) {
			view.Raise(nameof(Description));
			view.Raise(nameof(SourceTitle));
			view.Raise(nameof(SourceURLs));
			view.LoadComments();
		}
	}

	public string Description => Post?.Description.NotBlankCheck() ?? "No Description";
	public string[] SourceURLs => Post?.Sources?.ToArray() ?? [];

	public string SourceTitle {
		get {
			if (SourceURLs.IsEmpty()) {
				return "No Source";
			} else if (SourceURLs.Length == 1) {
				return "Source";
			} else {
				return "Sources";
			}
		}
	}

	public ObservableCollection<E621Comment> Comments { get; } = [];

	public LoadingStatusViewModel LoadingStatus { get; } = new();

	private CancellationTokenSource? comment_cts;

	private async void LoadComments() {
		if (ModuleType is null || Post is null) {
			return;
		}

		comment_cts?.Cancel();

		CancellationTokenSource newCts = new();
		comment_cts = newCts;
		CancellationToken token = newCts.Token;

		try {
			LoadingStatus.InitialLoading();

			token.ThrowIfCancellationRequested();
			Comments.Clear();

			E621Comment[] comments = await E621API.GetAPI(ModuleType.Value).GetCommentsAsync(Post.ID, token);
			
			token.ThrowIfCancellationRequested();

			foreach (E621Comment item in comments) {
				Comments.Add(item);
			}

		} catch (OperationCanceledException) {

		} catch (Exception ex) {
			if (!token.IsCancellationRequested) {
				LoadingStatus.LoadingError(ex.Message);
				Debug.WriteLine(ex);
			}
		} finally {
			if (!token.IsCancellationRequested) {
				LoadingStatus.DoneLoading();
			}
		}
	}


	public PostDetailDockView() {
		InitializeComponent();
	}
}
