using System.ComponentModel;

namespace YiffBrowser.BaseFramework.Enums;

public enum ProxyMode {
	[Description("No proxy")] None = 0,
	[Description("Use system proxy")] System = 1,
	[Description("Custom proxy")] Custom = 2,
}
