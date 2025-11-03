using BaseFramework.Enums;
using BaseFramework.Interfaces;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using System.Windows;
using System.Windows.Input;
using YB.E621.Services;

namespace YB.E621.Views.Subs;
public partial class UserLogin : UserControlBase {
	public UserLogin() {
		InitializeComponent();
	}
}

public class UserLoginViewModel(ModuleType siteType) : UserControlViewModel<UserLogin> {
	private string username = string.Empty;
	private string apiKey = string.Empty;

	public string Username {
		get => username;
		set => SetProperty(ref username, value);
	}

	public string ApiKey {
		get => apiKey;
		set => SetProperty(ref apiKey, value);
	}

	public E621UserService UserService { get; } = E621UserService.GetUserService(siteType);

	protected override void LoadedOnce(IViewBase viewBase) {
		base.LoadedOnce(viewBase);

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
