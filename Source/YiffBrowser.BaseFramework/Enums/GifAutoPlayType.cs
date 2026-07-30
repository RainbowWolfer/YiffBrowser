using System.ComponentModel;

namespace YiffBrowser.BaseFramework.Enums;

public enum GifAutoPlayType {
	[Description("Never play")] Never = 0,
	[Description("Play on hover")] WhenMouseOver = 1,
	[Description("Play when visible")] Always = 2,
}
