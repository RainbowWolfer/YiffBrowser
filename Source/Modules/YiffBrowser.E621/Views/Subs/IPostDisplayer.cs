using DevExpress.Mvvm;

namespace YiffBrowser.E621.Views.Subs;

/// <summary>Shared surface used by <see cref="Controls.PostDisplayerContextMenu"/> for both image and video displayers.</summary>
public interface IPostDisplayer {
	IDelegateCommand ReloadCommand { get; }
	bool IsFileReady { get; }
}
