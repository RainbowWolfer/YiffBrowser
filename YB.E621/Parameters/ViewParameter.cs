using BaseFramework.Enums;
using System.Windows;

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



}
