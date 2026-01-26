using System.Windows;
using System.Windows.Controls;
using YB.E621.Models.E621;

namespace YB.E621.Controls;

internal class CommentControl : Control {


	public E621Comment Comment {
		get => (E621Comment)GetValue(CommentProperty);
		set => SetValue(CommentProperty, value);
	}

	public static readonly DependencyProperty CommentProperty = DependencyProperty.Register(
		nameof(Comment),
		typeof(E621Comment),
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
