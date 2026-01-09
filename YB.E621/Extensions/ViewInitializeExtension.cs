using BaseFramework.Enums;
using System.Windows;
using YB.E621.ViewModels;

namespace YB.E621.Extensions;

public static class ViewInitializeExtension {


	public static ModuleType? GetModuleType(DependencyObject obj) => (ModuleType?)obj.GetValue(ModuleTypeProperty);

	public static void SetModuleType(DependencyObject obj, ModuleType? value) => obj.SetValue(ModuleTypeProperty, value);

	public static readonly DependencyProperty ModuleTypeProperty = DependencyProperty.RegisterAttached(
		"ModuleType",
		typeof(ModuleType?),
		typeof(ViewInitializeExtension),
		new PropertyMetadata(null, OnModuleTypeChanged)
	);

	private static void OnModuleTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (e.NewValue is ModuleType moduleType) {
			if (d is FrameworkElement fe && fe.DataContext is E621ViewModelBase viewModelBase) {
				viewModelBase.Initialize(moduleType);
			}
		}
	}
}
