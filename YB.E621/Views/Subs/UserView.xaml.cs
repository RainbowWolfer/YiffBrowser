﻿using BaseFramework.Enums;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using System.Windows.Input;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.Views.Subs {
	public partial class UserView : UserControlBase {
		public UserView() {
			InitializeComponent();
		}
	}

	public class UserViewModel : UserControlViewModel<UserView> {
		private E621User? user = null;
		private E621Post? avatarPost = null;

		public E621User? User {
			get => user;
			set => SetProperty(ref user, value);
		}

		public E621Post? AvatarPost {
			get => avatarPost;
			set => SetProperty(ref avatarPost, value);
		}

		public E621UserService UserService { get; }

		public UserViewModel(ModuleType moduleType) {
			ModuleType = moduleType;

			UserService = E621UserService.GetUserService(moduleType);
			UserService.LoginChanged += UserService_LoginChanged;
		}

		private void UserService_LoginChanged(E621User? sender, E621Post? args) {
			User = sender;
			AvatarPost = args;
		}

		public ICommand LogoutCommand => new DelegateCommand(Logout);

		public ModuleType ModuleType { get; }

		private void Logout() {
			UserService.Logout();
		}
	}
}
