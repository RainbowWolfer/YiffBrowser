using System.ComponentModel;

namespace YiffBrowser.BaseFramework.Enums;

public enum TimeProgressFormat {
	[Description("Elapsed Time / Total Duration")] Time_Total,
	[Description("Elapsed Time / Remaining Time")] Time_Ramaining,
	[Description("Current Frame / Total Frames")] Frame_Total,
	[Description("Current Frame / Remaining Frames")] Frame_Ramaining,
}
