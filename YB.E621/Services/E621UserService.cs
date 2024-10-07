using BaseFramework.Enums;
using BaseFramework.Events;
using BaseFramework.Helpers;
using BaseFramework.Models;
using BaseFramework.Models.Apps;
using BaseFramework.Services;
using YB.E621.Models.E621;

namespace YB.E621.Services {
	public class E621UserService(ModuleType moduleType) {

		private static E621UserService User_E621 { get; } = new(ModuleType.E621);
		private static E621UserService User_E6AI { get; } = new(ModuleType.E6AI);
		private static E621UserService User_E926 { get; } = new(ModuleType.E926);

		public static E621UserService GetUserService(ModuleType moduleType) {
			return moduleType switch {
				ModuleType.E621 => User_E621,
				ModuleType.E6AI => User_E6AI,
				ModuleType.E926 => User_E926,
				_ => throw new NotImplementedException(),
			};
		}

		public event TypedEventHandler<E621User?, E621Post?>? LoginChanged;

		public E621API Api { get; } = E621API.GetAPI(moduleType);

		public ModuleType ModuleType { get; } = moduleType;

		public BindObject<bool> IsUserLoading { get; } = false;

		private UserModel currentUser;

		private UserModel CurrentUser {
			get => currentUser;
			set {
				currentUser = value;
				LoginChanged?.Invoke(CurrentUser.User, CurrentUser.AvatarPost);
			}
		}

		public async Task Initialize() {
			(string? username, string? apiKey) = GetUser();
			if (username.IsNotBlank() && apiKey.IsNotBlank()) {
				await TryLogin(username, apiKey);
			}
		}

		private readonly record struct UserModel(E621User User, E621Post? AvatarPost);

		public async ValueTask<Exception?> TryLogin(string username, string apiKey) {
			IsUserLoading.Value = true;

			UserModel userModel = default;
			try {
				if (username.IsBlank() || apiKey.IsBlank()) {
					return new Exception("Username or ApiKey is empty");
				}

				HttpResult<string> result = await NetCode.ReadURLAsync($"https://{Api.GetHost()}/favorites.json", username, apiKey, null);

				if (result.Result != HttpResultType.Success) {
					return new Exception("Login failed");
				}

				E621User? user = await Api.GetUserAsync(username, apiKey);
				if (user is null) {
					return new Exception($"Unable to find user ({username})");
				}

				E621Post? avatarPost = await Api.GetPostAsync(user.AvatarId);

				userModel = new UserModel(user, avatarPost);

				return null;
			} finally {
				if (userModel.User is null) {
					SetUser(string.Empty, string.Empty);
				} else {
					SetUser(username, apiKey);
				}
				AppProfile.Save();
				CurrentUser = userModel;

				IsUserLoading.Value = false;
			}
		}

		public void Logout() {
			SetUser(string.Empty, string.Empty);
			AppProfile.Save();

			CurrentUser = default;

		}

		public (string? username, string? apiKey) GetUser() {
			return ModuleType switch {
				ModuleType.E621 => (AppProfile.Instance.E621_Username, AppProfile.Instance.E621_ApiKey),
				ModuleType.E6AI => (AppProfile.Instance.E6AI_Username, AppProfile.Instance.E6AI_ApiKey),
				ModuleType.E926 => (AppProfile.Instance.E926_Username, AppProfile.Instance.E926_ApiKey),
				_ => throw new NotImplementedException(),
			};
		}

		private void SetUser(string username, string apiKey) {
			switch (ModuleType) {
				case ModuleType.E621:
					AppProfile.Instance.E621_Username = username;
					AppProfile.Instance.E621_ApiKey = apiKey;
					break;
				case ModuleType.E6AI:
					AppProfile.Instance.E6AI_Username = username;
					AppProfile.Instance.E6AI_ApiKey = apiKey;
					break;
				case ModuleType.E926:
					AppProfile.Instance.E926_Username = username;
					AppProfile.Instance.E926_ApiKey = apiKey;
					break;
				default:
					throw new NotImplementedException();
			}
		}
	}

}
