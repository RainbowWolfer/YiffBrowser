using BaseFramework.Interfaces;

namespace BaseFramework.ViewModels;
public class WindowViewModel<T> : ViewModelBase<T> where T : IWindowBase, new() {

	public WindowViewModel() {

	}

}
