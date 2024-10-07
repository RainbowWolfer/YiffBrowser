using Newtonsoft.Json;

namespace BaseFramework.Models {
	[Serializable]
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	[method: JsonConstructor]
	public readonly struct VersionStruct(int major, int minor, int product) {
		[JsonProperty] public int Major { get; } = major;
		[JsonProperty] public int Minor { get; } = minor;
		[JsonProperty] public int Product { get; } = product;

		public override string ToString() => $"{Major}.{Minor}.{Product}";
	}
}
