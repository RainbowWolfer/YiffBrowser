using BaseFramework.Enums;
using BaseFramework.ViewModels;
using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.ViewModels;

public class E621CommentViewModel(E621Comment comment, ModuleType moduleType) : BindableBase, IDisposable {
	private readonly E621API api = new(moduleType);

	public E621Comment Comment { get; } = comment;

	public E621Post? Avatar {
		get => GetProperty(() => Avatar);
		set => SetProperty(() => Avatar, value);
	}

	public E621User? User {
		get => GetProperty(() => User);
		set => SetProperty(() => User, value);
	}

	//public GifImage? GifImage {
	//	get => GetProperty(() => GifImage);
	//	set => SetProperty(() => GifImage, value);
	//}

	//public BitmapImage? BitmapImage {
	//	get => GetProperty(() => BitmapImage);
	//	set => SetProperty(() => BitmapImage, value);
	//}

	public BitmapImage? BitmapImage {
		get => GetProperty(() => BitmapImage);
		set => SetProperty(() => BitmapImage, value);
	}

	public LoadingStatusViewModel LoadingStatus { get; } = new();
	//private BitmapCacheItem? bitmapCacheItem;

	private readonly CancellationTokenSource cts = new();

	public async void StartLoading() {
		try {
			LoadingStatus.InitialLoading();
			User = await api.GetUserAsync(Comment.CreatorId, cts.Token);
			Avatar = await api.GetPostAsync(User?.AvatarId, cts.Token);

			string? avatarUrl = Avatar?.Sample?.URL;
			if (avatarUrl.IsNotBlank()) {
				BitmapImage = new BitmapImage(new Uri(avatarUrl));
				BitmapImage.DownloadCompleted += BitmapImage_DownloadCompleted;
				BitmapImage.DownloadFailed += BitmapImage_DownloadFailed;
				BitmapImage.DownloadProgress += BitmapImage_DownloadProgress;
			} else {
				LoadingStatus.DoneLoading();
			}

			//bitmapCacheItem = BitmapCacheService.Get(Avatar?.Sample?.URL);
			//if (bitmapCacheItem.IsNull || bitmapCacheItem.HasCompleted) {
			//	GifImage = bitmapCacheItem.GifImage;
			//	BitmapImage = bitmapCacheItem.Image;
			//	LoadingStatus.DoneLoading();
			//} else {
			//	bitmapCacheItem.Initialize();
			//	bitmapCacheItem.Updated += CacheItem_Updated;
			//}

		} catch (Exception ex) {
			LoadingStatus.LoadingError(ex.Message);
		}
	}

	private void BitmapImage_DownloadFailed(object? sender, ExceptionEventArgs e) {
		LoadingStatus.LoadingError(e.ErrorException?.Message ?? "Loading Error");
	}

	private void BitmapImage_DownloadCompleted(object? sender, EventArgs e) {
		LoadingStatus.DoneLoading();
	}

	private void BitmapImage_DownloadProgress(object? sender, DownloadProgressEventArgs e) {

	}

	public void Dispose() {
		//cts.Cancel();
		//if (bitmapCacheItem != null) {
		//	bitmapCacheItem.Updated -= CacheItem_Updated;
		//}
	}

	//private void CacheItem_Updated(BitmapCacheItem sender, BitmapLoadingModel args) {
	//	if (cts.Token.IsCancellationRequested) {
	//		return;
	//	}

	//	if (args.HasCompleted) {
	//		GifImage = sender.GifImage;
	//		BitmapImage = sender.Image;
	//		LoadingStatus.DoneLoading();
	//	}

	//	if (args.HasError || args.Exception != null) {
	//		LoadingStatus.LoadingError(args.Exception?.Message ?? "Loading Error");
	//	}

	//}

}
