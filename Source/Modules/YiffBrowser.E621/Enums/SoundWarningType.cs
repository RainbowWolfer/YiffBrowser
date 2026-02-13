using RW.Common.Helpers;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Enums;

internal enum SoundWarningType {
	None,
	NoSound,
	Sound,
	SoundWarning,
}

internal static class SoundWarningTypeExtension {
	public static SoundWarningType GetSoundWarningType(this E621Post post) {
		if (post is null) {
			return SoundWarningType.None;
		}

		HashSet<string>? tags = post.Tags?.GetAllTags();
		if (tags.IsEmpty()) {
			return SoundWarningType.None;
		} else if (tags.Contains("sound_warning")) {
			return SoundWarningType.SoundWarning;
		} else if (tags.Contains("sound")) {
			return SoundWarningType.Sound;
		} else if (tags.Contains("no_sound")) {
			return SoundWarningType.NoSound;
		} else {
			return SoundWarningType.None;
		}
	}
}