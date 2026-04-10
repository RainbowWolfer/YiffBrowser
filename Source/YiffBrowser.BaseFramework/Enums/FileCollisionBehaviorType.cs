using System.ComponentModel;

namespace YiffBrowser.BaseFramework.Enums;

public enum FileCollisionBehaviorType {
	[Description("Skip")] Skip = 0,
	[Description("Skip if same size")] SkipIfSameSize = 1,
	[Description("Overwrite")] Overwrite = 2,
	[Description("Auto-rename")] AutoRename = 3,
}
