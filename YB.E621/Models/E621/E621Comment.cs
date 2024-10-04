using Newtonsoft.Json;
using System;

namespace YB.E621.Models.E621{
	public class E621Comment {
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }

		[JsonProperty("post_id")]
		public int PostId { get; set; }

		[JsonProperty("creator_id")]
		public int CreatorId { get; set; }

		[JsonProperty("body")]
		public string? Body { get; set; }

		[JsonProperty("score")]
		public int Score { get; set; }

		[JsonProperty("updated_at")]
		public DateTime UpdatedAt { get; set; }

		[JsonProperty("updater_id")]
		public int UpdaterId { get; set; }

		[JsonProperty("do_not_bump_post")]
		public bool DoNotBumpPost { get; set; }

		[JsonProperty("is_hidden")]
		public bool IsHidden { get; set; }

		[JsonProperty("is_sticky")]
		public bool IsSticky { get; set; }

		[JsonProperty("warning_type")]
		public object? WarningType { get; set; }

		[JsonProperty("warning_user_id")]
		public object? WarningUserId { get; set; }

		[JsonProperty("creator_name")]
		public string? CreatorName { get; set; }

		[JsonProperty("updater_name")]
		public string? UpdaterName { get; set; }
	}

}
