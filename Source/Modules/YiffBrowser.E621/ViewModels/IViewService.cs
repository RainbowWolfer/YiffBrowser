using DevExpress.Mvvm;
using RW.Base.WPF.Interfaces;
using System.Windows.Controls;
using YiffBrowser.BaseFramework;
using YiffBrowser.E621.Services;

namespace YiffBrowser.E621.ViewModels;

internal interface IViewService {
	Dock TabDock { get; set; }
}

internal class ViewService(IE621ProfileService profileService) : BindableBase, IViewService, IAppInitialize {
	string IAppInitialize.Description => "";
	int IPriority.Priority => IntPriority.Normal;

	public Dock TabDock {
		get => GetProperty(() => TabDock);
		set => SetProperty(() => TabDock, value);
	}

	void IAppInitialize.AppInitialize(IStatusReport statusReport) {
		TabDock = Dock.Top;
	}






}
