using System.Windows;
using YB.E621.Interfaces;
using YB.E621.Models.E621;
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



	public static E621Post GetPost(DependencyObject obj) => (E621Post)obj.GetValue(PostProperty);

	public static void SetPost(DependencyObject obj, E621Post value) => obj.SetValue(PostProperty, value);

	public static readonly DependencyProperty PostProperty = DependencyProperty.RegisterAttached(
		"Post",
		typeof(E621Post),
		typeof(ViewInitializeExtension),
		new PropertyMetadata(null, OnPostChanged)
	);

	private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is FrameworkElement fe && fe.DataContext is IPostViewModel postViewModel) {
			postViewModel.Post = e.NewValue as E621Post;
		}
	}
}
