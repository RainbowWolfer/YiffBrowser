using Newtonsoft.Json;
using System;

namespace YB.E621.Models.E621{
	public class E621User {
		[JsonProperty("wiki_page_version_count")]
		public int WikiPageVersionCount { get; set; }

		[JsonProperty("artist_version_count")]
		public int ArtistVersionCount { get; set; }

		[JsonProperty("pool_version_count")]
		public int PoolVersionCount { get; set; }

		[JsonProperty("forum_post_count")]
		public int ForumPostCount { get; set; }

		[JsonProperty("comment_count")]
		public int CommentCount { get; set; }

		[JsonProperty("appeal_count")]
		public int AppealCount { get; set; }

		[JsonProperty("flag_count")]
		public int FlagCount { get; set; }

		[JsonProperty("positive_feedback_count")]
		public int PositiveFeedbackCount { get; set; }

		[JsonProperty("neutral_feedback_count")]
		public int NeutralFeedbackCount { get; set; }

		[JsonProperty("negative_feedback_count")]
		public int NegativeFeedbackCount { get; set; }

		[JsonProperty("upload_limit")]
		public int UploadLimit { get; set; }

		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("created_at")]
		public DateTime CreatedAt { get; set; }

		[JsonProperty("name")]
		public string? Name { get; set; }

		[JsonProperty("level")]
		public int Level { get; set; }

		[JsonProperty("base_upload_limit")]
		public int BaseUploadLimit { get; set; }

		[JsonProperty("post_upload_count")]
		public int PostUploadCount { get; set; }

		[JsonProperty("post_update_count")]
		public int PostUpdateCount { get; set; }

		[JsonProperty("note_update_count")]
		public int NoteUpdateCount { get; set; }

		[JsonProperty("is_banned")]
		public bool IsBanned { get; set; }

		[JsonProperty("can_approve_posts")]
		public bool CanApprovePosts { get; set; }

		[JsonProperty("can_upload_free")]
		public bool CanUploadFree { get; set; }

		[JsonProperty("level_string")]
		public string? LevelString { get; set; }

		[JsonProperty("avatar_id")]
		public int? AvatarId { get; set; }

		[JsonProperty("show_avatars")]
		public bool ShowAvatars { get; set; }

		[JsonProperty("blacklist_avatars")]
		public bool BlacklistAvatars { get; set; }

		[JsonProperty("blacklist_users")]
		public bool BlacklistUsers { get; set; }

		[JsonProperty("description_collapsed_initially")]
		public bool DescriptionCollapsedInitially { get; set; }

		[JsonProperty("hide_comments")]
		public bool HideComments { get; set; }

		[JsonProperty("show_hidden_comments")]
		public bool ShowHiddenComments { get; set; }

		[JsonProperty("show_post_statistics")]
		public bool ShowPostStatistics { get; set; }

		[JsonProperty("has_mail")]
		public bool HasMail { get; set; }

		[JsonProperty("receive_email_notifications")]
		public bool ReceiveEmailNotifications { get; set; }

		[JsonProperty("enable_keyboard_navigation")]
		public bool EnableKeyboardNavigation { get; set; }

		[JsonProperty("enable_privacy_mode")]
		public bool EnablePrivacyMode { get; set; }

		[JsonProperty("style_usernames")]
		public bool StyleUsernames { get; set; }

		[JsonProperty("enable_auto_complete")]
		public bool EnableAutoComplete { get; set; }

		[JsonProperty("has_saved_searches")]
		public bool HasSavedSearches { get; set; }

		[JsonProperty("disable_cropped_thumbnails")]
		public bool DisableCroppedThumbnails { get; set; }

		[JsonProperty("disable_mobile_gestures")]
		public bool DisableMobileGestures { get; set; }

		[JsonProperty("enable_safe_mode")]
		public bool EnableSafeMode { get; set; }

		[JsonProperty("disable_responsive_mode")]
		public bool DisableResponsiveMode { get; set; }

		[JsonProperty("disable_post_tooltips")]
		public bool DisablePostTooltips { get; set; }

		[JsonProperty("no_flagging")]
		public bool NoFlagging { get; set; }

		[JsonProperty("no_feedback")]
		public bool NoFeedback { get; set; }

		[JsonProperty("disable_user_dmails")]
		public bool DisableUserDmails { get; set; }

		[JsonProperty("enable_compact_uploader")]
		public bool EnableCompactUploader { get; set; }

		[JsonProperty("updated_at")]
		public DateTime UpdatedAt { get; set; }

		[JsonProperty("email")]
		public string? Email { get; set; }

		[JsonProperty("last_logged_in_at")]
		public DateTime LastLoggedInAt { get; set; }

		[JsonProperty("last_forum_read_at")]
		public DateTime LastForumReadAt { get; set; }

		[JsonProperty("recent_tags")]
		public string? RecentTags { get; set; }

		[JsonProperty("comment_threshold")]
		public int CommentThreshold { get; set; }

		[JsonProperty("default_image_size")]
		public string? DefaultImageSize { get; set; }

		[JsonProperty("favorite_tags")]
		public string? FavoriteTags { get; set; }

		[JsonProperty("blacklisted_tags")]
		public string? BlacklistedTags { get; set; }

		[JsonProperty("time_zone")]
		public string? TimeZone { get; set; }

		[JsonProperty("per_page")]
		public int PerPage { get; set; }

		[JsonProperty("custom_style")]
		public string? CustomStyle { get; set; }

		[JsonProperty("favorite_count")]
		public int FavoriteCount { get; set; }

		[JsonProperty("api_regen_multiplier")]
		public int ApiRegenMultiplier { get; set; }

		[JsonProperty("api_burst_limit")]
		public int ApiBurstLimit { get; set; }

		[JsonProperty("remaining_api_limit")]
		public int RemainingApiLimit { get; set; }

		[JsonProperty("statement_timeout")]
		public int StatementTimeout { get; set; }

		[JsonProperty("favorite_limit")]
		public int FavoriteLimit { get; set; }

		[JsonProperty("tag_query_limit")]
		public int TagQueryLimit { get; set; }
	}

}
