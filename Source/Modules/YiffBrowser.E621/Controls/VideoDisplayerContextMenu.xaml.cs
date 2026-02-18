using System.Windows;
using System.Windows.Controls;
using YiffBrowser.E621.Views.Subs;

namespace YiffBrowser.E621.Controls;

public partial class VideoDisplayerContextMenu : ContextMenu {
	public VideoDisplayer VideoDisplayer {
		get => (VideoDisplayer)GetValue(VideoDisplayerProperty); 
		set => SetValue(VideoDisplayerProperty, value);
	}

	public static readonly DependencyProperty VideoDisplayerProperty =DependencyProperty.Register(
		nameof(VideoDisplayer),
		typeof(VideoDisplayer),
		typeof(VideoDisplayerContextMenu),
		new PropertyMetadata(null)
	);



	public VideoDisplayerContextMenu() {
		InitializeComponent();
	}

}
