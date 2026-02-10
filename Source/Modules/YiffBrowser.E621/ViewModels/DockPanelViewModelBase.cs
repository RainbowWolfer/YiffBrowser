using DevExpress.Mvvm;
using YiffBrowser.E621.Interfaces;
using YiffBrowser.E621.Parameters;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.ViewModels;

internal class DockPanelViewModelBase<T> : ViewModelBase, IDisposable where T : IDockPanel {
	public T DockPanelItem {
		get => GetProperty(() => DockPanelItem);
		private set => SetProperty(() => DockPanelItem, value);
	}

	public ViewParameter ViewParameter {
		get => GetProperty(() => ViewParameter);
		private set => SetProperty(() => ViewParameter, value);
	}

	public E621MainViewModel MainViewModel {
		get => GetProperty(() => MainViewModel);
		private set => SetProperty(() => MainViewModel, value);
	}

	public DockPanelViewModelBase() {

	}

	protected override void OnParameterChanged(object parameter) {
		base.OnParameterChanged(parameter);

		if (parameter is DockPanelParameter _parameter) {
			DockPanelItem = (T)_parameter.DockPanel;
			MainViewModel = _parameter.E621MainView;
			ViewParameter = _parameter.ViewParameter;
		}

		OnInitialized();
	}

	protected virtual void OnInitialized() {

	}

	public virtual void Dispose() {

	}

}
