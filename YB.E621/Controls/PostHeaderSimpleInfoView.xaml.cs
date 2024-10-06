using BaseFramework.Converters;
using BaseFramework.Enums;
using BaseFramework.Helpers;
using System.Windows;
using System.Windows.Controls;
using YB.E621.Helpers;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.Controls {
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
				FileType type = Post.GetFileType();
				TypeIcon.Text = type switch {
					FileType.PNG or FileType.JPG => "\uEB9F",
					FileType.GIF => "\uF4A9",
					FileType.WEBM => "\uE714",
					_ => "\uE9CE",
				};
				TypeBorder.ToolTip = $"Type: {type}";

				DurationText.Text = TimeDurationConverter.Convert(Post.Duration);
				DurationText.Visibility = Post.Duration.IsNotBlank().ToVisibility();

				PostIDButton.ToolTip = $"Rating: {Post.Rating}";

				PostIDText.Text = $"{Post.ID} ({Post.Rating.ToString()[..1]})";

				if (Post.Tags is null) {
					SoundBorder.Visibility = Visibility.Collapsed;
					SoundWarningBorder.Visibility = Visibility.Collapsed;
				} else {
					List<string> tags = Post.Tags.GetAllTags();
					if (tags.Contains("sound_warning")) {
						SoundBorder.Visibility = Visibility.Collapsed;
						SoundWarningBorder.Visibility = Visibility.Visible;
					} else if (tags.Contains("sound")) {
						SoundBorder.Visibility = Visibility.Visible;
						SoundWarningBorder.Visibility = Visibility.Collapsed;
					} else {
						SoundBorder.Visibility = Visibility.Collapsed;
						SoundWarningBorder.Visibility = Visibility.Collapsed;
					}
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
}
