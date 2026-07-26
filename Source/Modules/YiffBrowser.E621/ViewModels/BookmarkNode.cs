using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using YiffBrowser.BaseFramework.Services;

namespace YiffBrowser.E621.ViewModels;

internal class BookmarkNode : BindableBase {
	public BookmarkNode? Parent { get; set; }

	public ObservableCollection<BookmarkNode> Children { get; } = [];

	public string Name {
		get => GetProperty(() => Name);
		set {
			SetProperty(() => Name, value);
			RaisePropertyChanged(() => DisplayText);
		}
	}

	/// <summary>Null means folder; non-null means a saved tab (tags + page).</summary>
	public string[]? Tags {
		get => GetProperty(() => Tags);
		set {
			SetProperty(() => Tags, value);
			RaisePropertyChanged(() => IsFolder);
			RaisePropertyChanged(() => IsBookmark);
			RaisePropertyChanged(() => DisplayText);
		}
	}

	public int Page {
		get => GetProperty(() => Page);
		set {
			SetProperty(() => Page, Math.Max(1, value));
			RaisePropertyChanged(() => DisplayText);
		}
	}

	public bool IsFolder => Tags == null;
	public bool IsBookmark => Tags != null;

	public string DisplayText {
		get {
			if (IsFolder) {
				return Name;
			}

			string tags = Tags is { Length: > 0 } ? string.Join(" ", Tags) : "(empty)";
			return $"{tags} · p.{Page}";
		}
	}

	public bool IsRenamePopupOpen {
		get => GetProperty(() => IsRenamePopupOpen);
		set => SetProperty(() => IsRenamePopupOpen, value);
	}

	public bool IsEditPopupOpen {
		get => GetProperty(() => IsEditPopupOpen);
		set => SetProperty(() => IsEditPopupOpen, value);
	}

	public string DraftName {
		get => GetProperty(() => DraftName);
		set => SetProperty(() => DraftName, value);
	}

	public string DraftTagsText {
		get => GetProperty(() => DraftTagsText);
		set => SetProperty(() => DraftTagsText, value);
	}

	public int DraftPage {
		get => GetProperty(() => DraftPage);
		set => SetProperty(() => DraftPage, Math.Max(1, value));
	}

	public static BookmarkNode CreateFolder(string name, BookmarkNode? parent = null) => new() {
		Name = name,
		Tags = null,
		Page = 1,
		Parent = parent,
	};

	public static BookmarkNode CreateBookmark(string[] tags, int page, string? name = null, BookmarkNode? parent = null) {
		string[] safeTags = tags ?? [];
		return new BookmarkNode {
			Name = name ?? (safeTags.Length > 0 ? string.Join(" ", safeTags) : "Bookmark"),
			Tags = safeTags,
			Page = Math.Max(1, page),
			Parent = parent,
		};
	}

	public BookmarkNodeState ToState() => new() {
		Name = Name,
		Tags = Tags,
		Page = Page,
		Children = Children.Select(c => c.ToState()).ToList(),
	};

	public static BookmarkNode FromState(BookmarkNodeState state, BookmarkNode? parent = null) {
		string fallbackName = state.Tags is { Length: > 0 } ? string.Join(" ", state.Tags) : "Folder";
		BookmarkNode node = new() {
			Name = state.Name.IsNotBlank() ? state.Name : fallbackName,
			Tags = state.Tags,
			Page = Math.Max(1, state.Page),
			Parent = parent,
		};
		foreach (BookmarkNodeState child in state.Children ?? []) {
			node.Children.Add(FromState(child, node));
		}

		return node;
	}
}
