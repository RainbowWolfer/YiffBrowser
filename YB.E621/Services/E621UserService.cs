using BaseFramework.Events;
using BaseFramework.Helpers;
using BaseFramework.Models;
using BaseFramework.Models.Apps;
using BaseFramework.Services;
using YB.E621.Models.E621;

namespace YB.E621.Services {
	public static class E621UserService {
		public static event TypedEventHandler<E621User?, E621Post?>? LoginChanged;

		public static BindObject<bool> IsUserLoading { get; } = false;

		private static UserModel currentUser;

		private static UserModel CurrentUser {
			get => currentUser;
			set {
				currentUser = value;
				LoginChanged?.Invoke(CurrentUser.User, CurrentUser.AvatarPost);
			}
		}


		public static async Task Initialize() {
			string? username = AppProfile.Instance.E621_Username;
			string? apiKey = AppProfile.Instance.E621_ApiKey;
			if (username.IsNotBlank() && apiKey.IsNotBlank()) {
				await TryLogin(username, apiKey);
			}
		}

		private readonly record struct UserModel(E621User User, E621Post? AvatarPost);

		public static async ValueTask<Exception?> TryLogin(string username, string apiKey) {
			IsUserLoading.Value = true;

			UserModel userModel = default;
			try {
				if (username.IsBlank() || apiKey.IsBlank()) {
					return new Exception("Username or ApiKey is empty");
				}

				HttpResult<string> result = await NetCode.ReadURLAsync($"https://{E621API.GetHost()}/favorites.json", username, apiKey, null);

				if (result.Result != HttpResultType.Success) {
					//Local.Settings.ClearLocalUser();
					return new Exception("Login failed");
				} else {
					//success = true;
					//Local.Settings.SetLocalUser(UserName, API);
				}

				E621User? user = await E621API.GetUserAsync(username, apiKey);
				if (user is null) {
					return new Exception($"Unable to find user ({username})");
				}

				E621Post? avatarPost = await E621API.GetPostAsync(user.AvatarId);

				userModel = new UserModel(user, avatarPost);

				return null;
			} finally {
				if (userModel.User is null) {
					AppProfile.Instance.E621_Username = string.Empty;
					AppProfile.Instance.E621_ApiKey = string.Empty;
				} else {
					AppProfile.Instance.E621_Username = username;
					AppProfile.Instance.E621_ApiKey = apiKey;
				}
				AppProfile.Save();
				CurrentUser = userModel;

				IsUserLoading.Value = false;
			}
		}

		public static void Logout() {
			AppProfile.Instance.E621_Username = string.Empty;
			AppProfile.Instance.E621_ApiKey = string.Empty;
			AppProfile.Save();

			CurrentUser = default;

		}
	}

}
