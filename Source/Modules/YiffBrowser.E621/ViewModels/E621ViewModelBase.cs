using YiffBrowser.BaseFramework.Enums;
using DevExpress.Mvvm;
using System.Windows;
using YiffBrowser.E621.Parameters;

namespace YiffBrowser.E621.ViewModels;

internal abstract class E621ViewModelBase : ViewModelBase {
	public ViewParameter ViewParameter {
		get => GetProperty(() => ViewParameter);
		private set => SetProperty(() => ViewParameter, value);
	}

	protected bool IsInitialized { get; private set; }

	public void Initialize(ViewParameter viewParameter) {
		ViewParameter = viewParameter;
		OnInitialize();
		IsInitialized = true;
	}

	protected abstract void OnInitialize();
}
