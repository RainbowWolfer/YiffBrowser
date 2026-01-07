using BaseFramework.Enums;
using DevExpress.Mvvm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using YB.E621.Services;

namespace YB.E621.Views.Subs;

public partial class UserLogin : UserControl {
	public UserLogin() {
		InitializeComponent();
	}
}

public class UserLoginViewModel(ModuleType siteType) : ViewModelBase {
	public string Username {
		get => GetProperty(() => Username);
		set => SetProperty(() => Username, value);
	}

	public string ApiKey {
		get => GetProperty(() => ApiKey);
		set => SetProperty(() => ApiKey, value);
	}

	public E621UserService UserService { get; } = E621UserService.GetUserService(siteType);



	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	private void Loaded() {
		(string? username, string? apiKey) = UserService.GetUser();

		Username = username ?? string.Empty;
		ApiKey = apiKey ?? string.Empty;
	}

	public ICommand LoginCommand => new DelegateCommand(Login);

	public ModuleType SiteType { get; } = siteType;

	private async void Login() {
		Exception? exception = await UserService.TryLogin(Username, ApiKey);
		if (exception != null) {
			MessageBox.Show($"{exception.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}
	}
}
