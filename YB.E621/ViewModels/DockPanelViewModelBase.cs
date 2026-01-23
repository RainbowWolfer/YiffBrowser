using DevExpress.Mvvm;
using YB.E621.Interfaces;

namespace YB.E621.ViewModels;

public class DockPanelViewModelBase<T> : ViewModelBase, IDisposable where T : IDockPanel {
	public T DockPanelItem {
		get => GetProperty(() => DockPanelItem);
		private set => SetProperty(() => DockPanelItem, value);
	}

	public DockPanelViewModelBase() {

	}

	protected override void OnParameterChanged(object parameter) {
		base.OnParameterChanged(parameter);
		DockPanelItem = (T)parameter;

		OnInitialized();
	}

	protected virtual void OnInitialized() {

	}

	public virtual void Dispose() {

	}

}
