using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using System.Windows;
using System.Windows.Controls;
using YiffBrowser.BaseFramework.Converters;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models.E621;

namespace YiffBrowser.E621.Controls;

public partial class PostHeaderSimpleInfoView : UserControl {


	public E621Post? Post {
		get => (E621Post)GetValue(PostProperty);
		set => SetValue(PostProperty, value);
	}

	public static readonly DependencyProperty PostProperty = DependencyProperty.Register(
		nameof(Post),
		typeof(E621Post),
		typeof(PostHeaderSimpleInfoView),
		new PropertyMetadata(null, OnPostChanged)
	);



	public ModuleType ModuleType {
		get => (ModuleType)GetValue(ModuleTypeProperty);
		set => SetValue(ModuleTypeProperty, value);
	}

	public static readonly DependencyProperty ModuleTypeProperty = DependencyProperty.Register(
		nameof(ModuleType),
		typeof(ModuleType),
		typeof(PostHeaderSimpleInfoView),
		new PropertyMetadata(ModuleType.E621)
	);

	private static void OnPostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		((PostHeaderSimpleInfoView)d).Update();
	}

	public PostHeaderSimpleInfoView() {
		InitializeComponent();
		Update();
	}

	private void Update() {
		if (Post is null) {

		} else {
			E621FileType type = Post.GetFileType();
			TypeIcon.Text = type.GetIconText();
			TypeBorder.ToolTip = $"Type: {type}";

			DurationText.Text = TimeDurationConverter.Convert(Post.Duration);
			DurationText.Visibility = Post.Duration.IsNotBlank().ToVisibility();

			PostIDButton.ToolTip = $"Rating: {Post.Rating}";

			PostIDText.Text = $"{Post.ID} ({Post.Rating.ToString()[..1]})";

			SoundWarningType soundWarningType = Post.GetSoundWarningType();

			switch (soundWarningType) {
				case SoundWarningType.None:
				default:
					Icon_NoSound.Visibility = Visibility.Collapsed;
					Icon_Sound.Visibility = Visibility.Collapsed;
					Icon_SoundWarning.Visibility = Visibility.Collapsed;
					break;
				case SoundWarningType.NoSound:
					Icon_NoSound.Visibility = Visibility.Visible;
					Icon_Sound.Visibility = Visibility.Collapsed;
					Icon_SoundWarning.Visibility = Visibility.Collapsed;
					break;
				case SoundWarningType.Sound:
					Icon_NoSound.Visibility = Visibility.Collapsed;
					Icon_Sound.Visibility = Visibility.Visible;
					Icon_SoundWarning.Visibility = Visibility.Collapsed;
					break;
				case SoundWarningType.SoundWarning:
					Icon_NoSound.Visibility = Visibility.Collapsed;
					Icon_Sound.Visibility = Visibility.Collapsed;
					Icon_SoundWarning.Visibility = Visibility.Visible;
					break;
			}

			CreatedDateText.Text = $"{Post.CreatedAt}";
			UpdatedAtText.Text = $"{Post.UpdatedAt}";
			SizeInfoText.Text = $"{Post.File?.SizeInfo}";
			ApproverIdText.Text = $"{Post.ApproverId}";
			UploaderId.Text = $"{Post.UploaderId}";

		}
	}

	private void CopyUrlButton_Click(object sender, RoutedEventArgs e) {
		Post?.GetPostLink(ModuleType).CopyToClipboard();
	}

	private void OpenBrowserButton_Click(object sender, RoutedEventArgs e) {
		Post?.GetPostLink(ModuleType).OpenInBrowser();
	}
}
