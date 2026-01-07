using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Windows.Controls;
using YB.E621.Models.E621;

namespace YB.E621.Views;

public partial class PostDetailDockView : UserControl {

	public PostDetailDockView() {
		InitializeComponent();
	}
}

public class PostDetailDockViewModel : ViewModelBase {
	private string description = string.Empty;
	private string[] sourceURLs = [];


	public E621Post? Post {
		get => GetProperty(() => Post);
		set {
			SetProperty(() => Post, value);
			Update();
		}
	}


	public string Description {
		get => GetProperty(() => Description);
		set => SetProperty(() => Description, value);
	}


	public string[] SourceURLs {
		get => GetProperty(() => SourceURLs);
		set {
			SetProperty(() => SourceURLs, value);
			RaisePropertyChanged(() => SourceTitle);
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