using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;
using YB.E621.Services;
using YB.E621.ViewModels;

namespace YB.E621.Views.Subs;

public partial class UserLogin : UserControl {
	public UserLogin() {
		InitializeComponent();
	}
}

internal class UserLoginViewModel() : E621ViewModelBase {
	public string Username {
		get => GetProperty(() => Username);
		set {
			SetProperty(() => Username, value);
			LoginCommand.RaiseCanExecuteChanged();
		}
	}

	public string ApiKey {
		get => GetProperty(() => ApiKey);
		set {
			SetProperty(() => ApiKey, value);
			LoginCommand.RaiseCanExecuteChanged();
		}
	}

	public E621UserService UserService {
		get => GetProperty(() => UserService);
		private set => SetProperty(() => UserService, value);
	}

	protected override void OnInitialize() {
		UserService = E621UserService.GetUserService(ViewParameter.ModuleType);

		(string? username, string? apiKey) = UserService.GetUser();

		Username = username ?? string.Empty;
		ApiKey = apiKey ?? string.Empty;
	}

	private AsyncCommand? loginCommand;
	public IDelegateCommand LoginCommand => loginCommand ??= new(Login, CanLogin);
	private async Task Login() {
		Exception? exception = await UserService.TryLogin(Username, ApiKey);
		if (exception != null) {
			MessageBox.Show($"{exception.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
	private bool CanLogin() {
		return Username.IsNotBlank() && ApiKey.IsNotBlank();
	}

}
