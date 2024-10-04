using Newtonsoft.Json;

namespace YB.E621.Models.E621 {
	public class E621Vote {
		[JsonProperty("id")]
		public int Score { get; set; }
		[JsonProperty("up")]
		public int Up { get; set; }
		[JsonProperty("down")]
		public int Down { get; set; }
		[JsonProperty("our_score")]
		public int OurScore { get; set; }
	}
}
