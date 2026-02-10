using DevExpress.Mvvm;
using System.Windows.Controls;
using System.Windows.Input;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views.Subs;

public partial class UserView : UserControl {
	public UserView() {
		InitializeComponent();
	}
}

internal class UserViewModel : E621ViewModelBase {

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

	protected override void OnInitialize() {
		UserService = E621UserService.GetUserService(ViewParameter.ModuleType);
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
