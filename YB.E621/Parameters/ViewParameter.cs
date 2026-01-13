using BaseFramework.Enums;
using System.Windows;
using YB.E621.Views;

namespace YB.E621.Parameters;

public class ViewParameter : DependencyObject {



	public ModuleType ModuleType {
		get => (ModuleType)GetValue(ModuleTypeProperty);
		set => SetValue(ModuleTypeProperty, value);
	}

	public static readonly DependencyProperty ModuleTypeProperty = DependencyProperty.Register(
		nameof(ModuleType),
		typeof(ModuleType),
		typeof(ViewParameter),
		new PropertyMetadata(null)
	);




	public ModuleNavigationActions ModuleNavigationActions {
		get => (ModuleNavigationActions)GetValue(ModuleNavigationActionsProperty);
		set => SetValue(ModuleNavigationActionsProperty, value);
	}

	public static readonly DependencyProperty ModuleNavigationActionsProperty = DependencyProperty.Register(
		nameof(ModuleNavigationActions),
		typeof(ModuleNavigationActions),
		typeof(ViewParameter),
		new PropertyMetadata(null)
	);




}
