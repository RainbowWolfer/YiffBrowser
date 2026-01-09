using BaseFramework.Enums;
using DevExpress.Mvvm;

namespace YB.E621.ViewModels;

internal abstract class E621ViewModelBase : ViewModelBase {
	public ModuleType ModuleType {
		get => GetProperty(() => ModuleType);
		private set => SetProperty(() => ModuleType, value);
	}

	protected bool IsInitialized { get; private set; }

	public void Initialize(ModuleType moduleType) {
		ModuleType = moduleType;
		OnInitialize();
		IsInitialized = true;
	}

	protected abstract void OnInitialize();
}
