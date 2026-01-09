using DevExpress.Mvvm;
using System.Windows.Controls;
using System.Windows.Input;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.Views.Subs;

public partial class UserView : UserControl {
	public UserView() {
		InitializeComponent();
	}
}

public class UserViewModel : ViewModelBase {

	public E621User? User {
		get => GetProperty(() => User);
		set => SetProperty(() => User, value);
	}

	public E621Post? AvatarPost {
		get => GetProperty(() => AvatarPost);
		set => SetProperty(() => AvatarPost, value);
	}

	public E621UserService UserService {
		get => GetProperty(() => UserService);
		private set => SetProperty(() => UserService, value);
	}

	protected override void OnParentViewModelChanged(object parentViewModel) {
		base.OnParentViewModelChanged(parentViewModel);
		E621MainWindowViewModel mainViewModel = (E621MainWindowViewModel)parentViewModel;

		UserService = E621UserService.GetUserService(mainViewModel.ModuleType);
		UserService.LoginChanged += UserService_LoginChanged;
	}

	private void UserService_LoginChanged(E621User? sender, E621Post? args) {
		User = sender;
		AvatarPost = args;
	}

	public ICommand LogoutCommand => new DelegateCommand(Logout);
	private void Logout() {
		UserService.Logout();
	}
}
