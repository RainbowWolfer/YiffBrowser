using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace BaseFramework.Helpers {
	public static class SerializationHelper {

		public static void SerializeObjectToJson(this object obj, string filePath) {
			try {
				using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
				fileStream.Position = 0;
				using StreamWriter streamWriter = new(fileStream);
				using JsonTextWriter jsonWriter = new(streamWriter);
				JsonSerializer serializer = new();
				serializer.Serialize(jsonWriter, obj);
				jsonWriter.Flush();
			} catch (Exception) {
				throw;
			}
		}

		public static T? DeserializeObjectFromJson<T>(this string filePath, T defaultWhenException = default) {
			try {
				using FileStream fileStream = new(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
				fileStream.Position = 0;

				using StreamReader streamReader = new(fileStream);
				using JsonTextReader jsonReader = new(streamReader);

				JsonSerializer serializer = new();

				T? result = serializer.Deserialize<T>(jsonReader);
				return result;
			} catch (Exception ex) {
				Debug.WriteLine(ex);
				return defaultWhenException;
			}

		}

	}
}
