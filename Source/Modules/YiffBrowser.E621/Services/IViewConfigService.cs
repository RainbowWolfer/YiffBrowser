using DevExpress.Mvvm;
using HandyControl.Themes;
using RW.Base.WPF.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using YiffBrowser.BaseFramework;

namespace YiffBrowser.E621.Services;

internal interface IViewConfigService {
	event TypedEventHandler<IViewConfigService, EventArgs> PostItemSizeChanged;

	Dock TabDock { get; set; }
	double PostItemHeight { get; set; }
	double PostItemWidth { get; set; }

	IReadOnlyList<PostItemWidthOption> PostItemWidthOptions { get; }
	PostItemWidthOption SelectedPostItemWidthOption { get; set; }

}

internal class ViewConfigService(IE621ProfileService profileService) : BindableBase, IViewConfigService, IAppInitialize {

	string IAppInitialize.Description => "";
	int IPriority.Priority => IntPriority.Normal;


	public event TypedEventHandler<IViewConfigService, EventArgs>? PostItemSizeChanged;

	public Dock TabDock {
		get => GetProperty(() => TabDock);
		set => SetProperty(() => TabDock, value);
	}

	public double PostItemHeight {
		get => GetProperty(() => PostItemHeight);
		set => SetProperty(() => PostItemHeight, value);
	}

	public double PostItemWidth {
		get => GetProperty(() => PostItemWidth);
		set => SetProperty(() => PostItemWidth, value);
	}

	public ObservableCollection<PostItemWidthOption> PostItemWidthOptions { get; } = [];
	IReadOnlyList<PostItemWidthOption> IViewConfigService.PostItemWidthOptions => PostItemWidthOptions;

	public PostItemWidthOption SelectedPostItemWidthOption {
		get => GetProperty(() => SelectedPostItemWidthOption);
		set {
			SetProperty(() => SelectedPostItemWidthOption, value);
			if (value != null) {
				PostItemWidth = value.Value;
			}
			PostItemSizeChanged?.Invoke(this, EventArgs.Empty);
		}
	}

	void IAppInitialize.AppInitialize(IStatusReport statusReport) {
		TabDock = Dock.Top;

		PostItemHeight = 50;
		PostItemWidth = 396;

		PostItemWidthOptions.Add(new PostItemWidthOption("Extra Small", 150));
		PostItemWidthOptions.Add(new PostItemWidthOption("Small", 250));
		PostItemWidthOptions.Add(new PostItemWidthOption("Default", 300));
		PostItemWidthOptions.Add(new PostItemWidthOption("Large", 396));
		PostItemWidthOptions.Add(new PostItemWidthOption("Extra Large", 460));

		SelectedPostItemWidthOption = PostItemWidthOptions[3];
	}






}

public record class PostItemWidthOption(string Name, double Value);