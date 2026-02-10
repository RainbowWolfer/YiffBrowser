using RW.Base.WPF.Interfaces;
using RW.Common.Interfaces;
using RW.Common.Utilities;
using RW.Common.WPF.Converters;
using RW.Common.WPF.Utilities;
using YiffBrowser.Strings;

namespace YiffBrowser.BaseFramework.Services;

public class EnumDisplayService : IAppInitializeAsync {
	string IAppInitializeAsync.Description => "Loading Enum Display Service";

	int IPriority.Priority => IntPriority.High;

	public static EnumDisplayManagerBase EnumDisplayManager { get; } = new _EnumDisplayManager();

	async Task IAppInitializeAsync.AppInitializeAsync(IStatusReport splashScreenViewModel) {
		EnumDisplayConverter.EnumDisplayManager = EnumDisplayManager;
		await Task.CompletedTask;
	}

	private class _EnumDisplayManager() : EnumDisplayManagerWPF(typeof(AppStrings)) {
		protected override Dictionary<Type, IEnumDisplayHandler> GetDefaultHandlers() {
			Dictionary<Type, IEnumDisplayHandler> dict = base.GetDefaultHandlers();


			return dict;
		}
	}

}

public static class EnumDisplayExtensions {
	public static string GetEnumDisplay(this Enum @enum) {
		return EnumDisplayService.EnumDisplayManager.GetDisplayText(@enum);
	}
}