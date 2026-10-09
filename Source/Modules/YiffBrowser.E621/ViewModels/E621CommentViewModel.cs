using YiffBrowser.BaseFramework.ViewModels;
using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YiffBrowser.BaseFramework.Models;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Helpers;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.Enums;

namespace YiffBrowser.E621.ViewModels;

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

	public LoadingStatus LoadingStatus { get; } = new();
	private BitmapCacheItem? bitmapCacheItem;

	private readonly CancellationTokenSource cts = new();

	public async void StartLoading() {
		try {
			LoadingStatus.Initialize();
			User = await api.GetUserAsync(Comment.CreatorId, cts.Token);
			Avatar = await api.GetPostAsync(User?.AvatarId, cts.Token);
			cts.Token.ThrowIfCancellationRequested();

			string? avatarUrl = Avatar?.Sample?.URL;
			if (avatarUrl.IsBlank()) {
				LoadingStatus.Done();
				return;
			}

			UnbindAvatar();
			bitmapCacheItem = BitmapCacheService.Get(avatarUrl, E621MediaCacheKeys.For(Avatar, E621MediaCacheKeys.Sample, avatarUrl));
			bitmapCacheItem.Updated += AvatarUpdated;
			bitmapCacheItem.Initialize();
			if (bitmapCacheItem.HasCompleted) {
				ApplyAvatar(bitmapCacheItem);
			}
		} catch (OperationCanceledException) {
		} catch (Exception ex) {
			LoadingStatus.Error(ex.Message);
		}
	}

	private void AvatarUpdated(BitmapCacheItem sender, CacheLoadingModel args) {
		if (!args.HasCompleted && !args.HasError) {
			return;
		}

		sender.Updated -= AvatarUpdated;
		if (cts.IsCancellationRequested) {
			return;
		}

		if (args.HasCompleted) {
			ApplyAvatar(sender);
		} else if (args.HasError) {
			LoadingStatus.Error(args.Exception?.Message ?? "Loading Error");
		}
	}

	private void ApplyAvatar(BitmapCacheItem item) {
		item.Updated -= AvatarUpdated;
		BitmapImage = item.Image;
		LoadingStatus.Done();
	}

	private void UnbindAvatar() {
		if (bitmapCacheItem != null) {
			bitmapCacheItem.Updated -= AvatarUpdated;
			bitmapCacheItem = null;
		}
	}

	public void Dispose() {
		cts.Cancel();
		UnbindAvatar();
	}

}
