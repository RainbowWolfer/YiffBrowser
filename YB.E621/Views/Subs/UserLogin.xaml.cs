using BaseFramework.Interfaces;
using BaseFramework.Models.Apps;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YB.E621.Services;

namespace YB.E621.Views.Subs {
	public partial class UserLogin : UserControlBase {
		public UserLogin() {
			InitializeComponent();
		}
	}

	public class UserLoginViewModel : UserControlViewModel<UserLogin> {
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

		public UserLoginViewModel() {
			
		}

		protected override void LoadedOnce(IViewBase viewBase) {
			base.LoadedOnce(viewBase);

			string username = AppProfile.Instance.E621_Username ?? string.Empty;
			string apiKey = AppProfile.Instance.E621_ApiKey ?? string.Empty;

			Username = username;
			ApiKey = apiKey;
		}

		public ICommand LoginCommand => new DelegateCommand(Login);

		private async void Login() {
			Exception? exception = await E621UserService.TryLogin(Username, ApiKey);
			if (exception != null) {
				MessageBox.Show($"{exception.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
