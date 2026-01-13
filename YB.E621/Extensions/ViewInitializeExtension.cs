using System.Windows;
using YB.E621.Parameters;
using YB.E621.ViewModels;

namespace YB.E621.Extensions;

public static class ViewInitializeExtension {


	public static ViewParameter GetViewParameter(DependencyObject obj) => (ViewParameter)obj.GetValue(ViewParameterProperty);

	public static void SetViewParameter(DependencyObject obj, ViewParameter value) => obj.SetValue(ViewParameterProperty, value);

	public static readonly DependencyProperty ViewParameterProperty = DependencyProperty.RegisterAttached(
		"ViewParameter",
		typeof(ViewParameter),
		typeof(ViewInitializeExtension),
		new PropertyMetadata(null, OnViewParameterChanged)
	);

	private static void OnViewParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (e.NewValue is ViewParameter viewParameter) {
			if (d is FrameworkElement fe && fe.DataContext is E621ViewModelBase viewModelBase) {
				viewModelBase.Initialize(viewParameter);
			}
		}
	}
}
