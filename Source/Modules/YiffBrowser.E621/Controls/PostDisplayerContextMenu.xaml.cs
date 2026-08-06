using System.Windows;
using System.Windows.Controls;
using YiffBrowser.BaseFramework.Services;

namespace YiffBrowser.E621.Controls;

public partial class PostDisplayerContextMenu : ContextMenu {

	/// <summary>The <see cref="Views.Subs.IPostDisplayer"/> owning this menu. Its DataContext carries the post commands.</summary>
	public object? Displayer {
		get => GetValue(DisplayerProperty);
		set => SetValue(DisplayerProperty, value);
	}

	public static readonly DependencyProperty DisplayerProperty = DependencyProperty.Register(
		nameof(Displayer),
		typeof(object),
		typeof(PostDisplayerContextMenu),
		new PropertyMetadata(null)
	);

	public bool IsVideo {
		get => (bool)GetValue(IsVideoProperty);
		set => SetValue(IsVideoProperty, value);
	}

	public static readonly DependencyProperty IsVideoProperty = DependencyProperty.Register(
		nameof(IsVideo),
		typeof(bool),
		typeof(PostDisplayerContextMenu),
		new PropertyMetadata(false)
	);

	/// <summary>Set by the image displayer only, so the image specific items do not bind against a video displayer.</summary>
	public object? ImageDisplayer {
		get => GetValue(ImageDisplayerProperty);
		set => SetValue(ImageDisplayerProperty, value);
	}

	public static readonly DependencyProperty ImageDisplayerProperty = DependencyProperty.Register(
		nameof(ImageDisplayer),
		typeof(object),
		typeof(PostDisplayerContextMenu),
		new PropertyMetadata(null)
	);

	public VideoControlsParameters? VideoControlsParameters {
		get => (VideoControlsParameters?)GetValue(VideoControlsParametersProperty);
		set => SetValue(VideoControlsParametersProperty, value);
	}

	public static readonly DependencyProperty VideoControlsParametersProperty = DependencyProperty.Register(
		nameof(VideoControlsParameters),
		typeof(VideoControlsParameters),
		typeof(PostDisplayerContextMenu),
		new PropertyMetadata(null)
	);

	public PostDisplayerContextMenu() {
		InitializeComponent();
	}
}
