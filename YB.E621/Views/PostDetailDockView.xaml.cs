using BaseFramework.Helpers;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using YB.E621.Models.E621;

namespace YB.E621.Views {
	public partial class PostDetailDockView : UserControlBase {

		public PostDetailDockView() {
			InitializeComponent();
		}
	}

	public class PostDetailDockViewModel : UserControlViewModel<PostDetailDockView> {
		private E621Post? post;
		private string description = string.Empty;
		private string[] sourceURLs = [];

		public E621Post? Post {
			get => post;
			set {
				SetProperty(ref post, value);
				Update();
			}
		}

		public string Description {
			get => description;
			set => SetProperty(ref description, value);
		}

		public string[] SourceURLs {
			get => sourceURLs;
			set {
				SetProperty(ref sourceURLs, value);
				RaisePropertyChanged(nameof(SourceTitle));
			}
		}

		public string SourceTitle {
			get {
				if (sourceURLs.IsEmpty()) {
					return "No Source";
				} else if (sourceURLs.Length == 1) {
					return "Source";
				} else {
					return "Sources";
				}
			}
		}

		public PostDetailDockViewModel() {

		}

		private void Update() {
			Description = Post?.Description.NotBlankCheck() ?? "No Description";
			SourceURLs = Post?.Sources?.ToArray() ?? [];
		}

	}
}