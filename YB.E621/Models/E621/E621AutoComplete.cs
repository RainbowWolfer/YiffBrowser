using Newtonsoft.Json;

namespace YB.E621.Models.E621 {
	public class E621AutoComplete {
		[JsonProperty("id")]
		public string? Id { get; set; }

		[JsonProperty("name")]
		public string? Name { get; set; }

		[JsonProperty("post_count")]
		public int PostCount { get; set; }

		[JsonProperty("category")]
		public int Category { get; set; }

		[JsonProperty("antecedent_name")]
		public string? AntecedentName { get; set; }
	}

}
