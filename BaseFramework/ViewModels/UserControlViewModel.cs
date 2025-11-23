using BaseFramework.Interfaces;

namespace BaseFramework.ViewModels;

public class UserControlViewModel<T> : ViewModelBase<T> where T : IUserControlBase, new() {

    public UserControlViewModel() {

    }

}
