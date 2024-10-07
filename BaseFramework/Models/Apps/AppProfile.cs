using BaseFramework.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseFramework.Models.Apps {
	[JsonObject]
	public class AppProfile {
		private static AppProfile? instance;

		public static AppProfile Instance {
			get => instance ??= new AppProfile();
			private set => instance = value;
		}

		public static void Save() {
			Instance.SerializeObjectToJson(FolderConfig.AppProfileFilePath);
		}

		public static void Load() {
			Instance = FolderConfig.AppProfileFilePath.DeserializeObjectFromJson<AppProfile>() ?? new();
		}

		public string? E621_Username { get; set; }
		public string? E621_ApiKey { get; set; }

		public string? E6AI_Username { get; set; }
		public string? E6AI_ApiKey { get; set; }

		public string? E926_Username { get; set; }
		public string? E926_ApiKey { get; set; }



	}
}
