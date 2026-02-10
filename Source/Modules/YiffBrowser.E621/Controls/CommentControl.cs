using System.Windows;
using System.Windows.Controls;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Controls;

internal class CommentControl : Control {

	public E621CommentViewModel Comment {
		get => (E621CommentViewModel)GetValue(CommentProperty);
		set => SetValue(CommentProperty, value);
	}

	public static readonly DependencyProperty CommentProperty = DependencyProperty.Register(
		nameof(Comment),
		typeof(E621CommentViewModel),
		typeof(CommentControl),
		new PropertyMetadata(null, OnCommentChanged)
	);

	private static void OnCommentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		
	}

	public CommentControl() {

	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();

	}



}
