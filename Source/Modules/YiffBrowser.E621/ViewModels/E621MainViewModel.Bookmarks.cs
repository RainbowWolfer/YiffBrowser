using DevExpress.Mvvm;
using GongSolutions.Wpf.DragDrop;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Models.Database;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.ViewModels;

internal partial class E621MainViewModel {
	private DelegateCommand<BookmarkNode?>? newBookmarkFolderCommand;
	public IDelegateCommand NewBookmarkFolderCommand => newBookmarkFolderCommand ??= new(NewBookmarkFolder);
	private void NewBookmarkFolder(BookmarkNode? relative) {
		BookmarkNode? parent;
		if (relative != null) {
			parent = relative.IsFolder ? relative : relative.Parent;
		} else {
			parent = SelectedBookmark?.IsFolder == true ? SelectedBookmark : SelectedBookmark?.Parent;
		}

		ObservableCollection<BookmarkNode> siblings = parent?.Children ?? BookmarkRoots;
		string name = NextUniqueName(siblings, "New Folder");
		BookmarkNode folder = BookmarkNode.CreateFolder(name, parent);
		siblings.Add(folder);
		SelectedBookmark = folder;
		ScheduleBookmarksSave();
	}

	private DelegateCommand<BookmarkNode?>? deleteBookmarkCommand;
	public IDelegateCommand DeleteBookmarkCommand => deleteBookmarkCommand ??= new(DeleteBookmark, CanDeleteBookmark);
	private void DeleteBookmark(BookmarkNode? node) {
		BookmarkNode? target = node ?? SelectedBookmark;
		if (!CanDeleteBookmark(target) || target is null) {
			return;
		}

		ObservableCollection<BookmarkNode> siblings = target.Parent?.Children ?? BookmarkRoots;
		siblings.Remove(target);
		if (ReferenceEquals(SelectedBookmark, target)) {
			SelectedBookmark = null;
		}

		ScheduleBookmarksSave();
	}
	private bool CanDeleteBookmark(BookmarkNode? node) => (node ?? SelectedBookmark) != null;

	private DelegateCommand<BookmarkNode?>? openBookmarkCommand;
	public IDelegateCommand OpenBookmarkCommand => openBookmarkCommand ??= new(OpenBookmark, CanOpenBookmark);
	private void OpenBookmark(BookmarkNode? node) {
		BookmarkNode? target = node ?? SelectedBookmark;
		if (!CanOpenBookmark(target) || target?.Tags is null) {
			return;
		}

		PostTabItem item = CreateTabItem(target.Tags, target.Page);
		Tabs.Add(item);
		TabSelectedIndex = Tabs.Count - 1;
		IsTabsManageSelecting = false;
		ShowTabsManage = false;
	}
	private bool CanOpenBookmark(BookmarkNode? node) => (node ?? SelectedBookmark)?.IsBookmark == true;

	private DelegateCommand<BookmarkNode>? beginRenameBookmarkCommand;
	public IDelegateCommand BeginRenameBookmarkCommand => beginRenameBookmarkCommand ??= new(BeginRenameBookmark, CanBeginRenameBookmark);
	private void BeginRenameBookmark(BookmarkNode node) {
		if (!CanBeginRenameBookmark(node)) {
			return;
		}

		SelectedBookmark = node;
		CloseAllBookmarkPopups();
		node.DraftName = node.Name;
		DispatcherService.Dispatcher.BeginInvoke(() => {
			node.IsRenamePopupOpen = true;
		}, DispatcherPriority.Input);
	}
	private bool CanBeginRenameBookmark(BookmarkNode node) => node?.IsFolder == true;

	private DelegateCommand<BookmarkNode>? confirmRenameBookmarkCommand;
	public IDelegateCommand ConfirmRenameBookmarkCommand => confirmRenameBookmarkCommand ??= new(ConfirmRenameBookmark, CanConfirmRenameBookmark);
	private void ConfirmRenameBookmark(BookmarkNode node) {
		if (!CanConfirmRenameBookmark(node)) {
			return;
		}

		node.Name = node.DraftName.Trim();
		node.IsRenamePopupOpen = false;
		ScheduleBookmarksSave();
	}
	private bool CanConfirmRenameBookmark(BookmarkNode node) => node?.IsFolder == true && node.DraftName.IsNotBlank();

	private DelegateCommand<BookmarkNode>? cancelRenameBookmarkCommand;
	public IDelegateCommand CancelRenameBookmarkCommand => cancelRenameBookmarkCommand ??= new(CancelRenameBookmark);
	private void CancelRenameBookmark(BookmarkNode node) {
		if (node != null) {
			node.IsRenamePopupOpen = false;
		}
	}

	private DelegateCommand<BookmarkNode>? beginEditBookmarkCommand;
	public IDelegateCommand BeginEditBookmarkCommand => beginEditBookmarkCommand ??= new(BeginEditBookmark, CanBeginEditBookmark);
	private void BeginEditBookmark(BookmarkNode node) {
		if (!CanBeginEditBookmark(node)) {
			return;
		}

		SelectedBookmark = node;
		CloseAllBookmarkPopups();
		node.DraftTagsText = node.Tags is { Length: > 0 } ? string.Join(" ", node.Tags) : string.Empty;
		node.DraftPage = node.Page;
		DispatcherService.Dispatcher.BeginInvoke(() => {
			node.IsEditPopupOpen = true;
		}, DispatcherPriority.Input);
	}
	private bool CanBeginEditBookmark(BookmarkNode node) => node?.IsBookmark == true;

	private DelegateCommand<BookmarkNode>? confirmEditBookmarkCommand;
	public IDelegateCommand ConfirmEditBookmarkCommand => confirmEditBookmarkCommand ??= new(ConfirmEditBookmark, CanConfirmEditBookmark);
	private void ConfirmEditBookmark(BookmarkNode node) {
		if (!CanConfirmEditBookmark(node)) {
			return;
		}

		string[] tags = (node.DraftTagsText ?? string.Empty)
			.Split([' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		node.Tags = tags;
		node.Page = Math.Max(1, node.DraftPage);
		node.Name = tags.Length > 0 ? string.Join(" ", tags) : "Bookmark";
		node.IsEditPopupOpen = false;
		ScheduleBookmarksSave();
	}
	private bool CanConfirmEditBookmark(BookmarkNode node) => node?.IsBookmark == true;

	private DelegateCommand<BookmarkNode>? cancelEditBookmarkCommand;
	public IDelegateCommand CancelEditBookmarkCommand => cancelEditBookmarkCommand ??= new(CancelEditBookmark);
	private void CancelEditBookmark(BookmarkNode node) {
		if (node != null) {
			node.IsEditPopupOpen = false;
		}
	}

	private void CloseAllBookmarkPopups() {
		foreach (BookmarkNode node in EnumerateBookmarkNodes(BookmarkRoots)) {
			node.IsRenamePopupOpen = false;
			node.IsEditPopupOpen = false;
		}
	}

	private static IEnumerable<BookmarkNode> EnumerateBookmarkNodes(IEnumerable<BookmarkNode> roots) {
		foreach (BookmarkNode node in roots) {
			yield return node;
			foreach (BookmarkNode child in EnumerateBookmarkNodes(node.Children)) {
				yield return child;
			}
		}
	}

	public void AddBookmarkFromTab(PostTabItem tab, BookmarkNode? target) {
		BookmarkNode? folder = ResolveFolderTarget(target);
		ObservableCollection<BookmarkNode> siblings = folder?.Children ?? BookmarkRoots;
		BookmarkNode bookmark = BookmarkNode.CreateBookmark(tab.Tags, tab.CurrentPage, parent: folder);
		siblings.Add(bookmark);
		SelectedBookmark = bookmark;
		ScheduleBookmarksSave();
	}

	public void MoveBookmarkNode(BookmarkNode node, BookmarkNode? target, RelativeInsertPosition insertPosition) {
		if (target != null && (ReferenceEquals(node, target) || IsDescendantOf(node, target))) {
			return;
		}

		ObservableCollection<BookmarkNode> oldSiblings = node.Parent?.Children ?? BookmarkRoots;
		oldSiblings.Remove(node);

		if (target == null) {
			node.Parent = null;
			BookmarkRoots.Add(node);
		} else if (target.IsFolder && insertPosition.HasFlag(RelativeInsertPosition.TargetItemCenter)) {
			node.Parent = target;
			target.Children.Add(node);
		} else {
			BookmarkNode? parent = target.Parent;
			ObservableCollection<BookmarkNode> siblings = parent?.Children ?? BookmarkRoots;
			node.Parent = parent;
			int index = siblings.IndexOf(target);
			if (index < 0) {
				siblings.Add(node);
			} else if (insertPosition.HasFlag(RelativeInsertPosition.BeforeTargetItem)) {
				siblings.Insert(index, node);
			} else {
				siblings.Insert(Math.Min(index + 1, siblings.Count), node);
			}
		}

		ScheduleBookmarksSave();
	}

	private static BookmarkNode? ResolveFolderTarget(BookmarkNode? target) {
		if (target == null) {
			return null;
		}

		return target.IsFolder ? target : target.Parent;
	}

	private static bool IsDescendantOf(BookmarkNode ancestor, BookmarkNode node) {
		BookmarkNode? current = node;
		while (current != null) {
			if (ReferenceEquals(current, ancestor)) {
				return true;
			}

			current = current.Parent;
		}

		return false;
	}

	private static string NextUniqueName(IEnumerable<BookmarkNode> siblings, string baseName) {
		HashSet<string> names = new(siblings.Select(s => s.Name), StringComparer.OrdinalIgnoreCase);
		if (!names.Contains(baseName)) {
			return baseName;
		}

		for (int i = 2; i < 10_000; i++) {
			string candidate = $"{baseName} ({i})";
			if (!names.Contains(candidate)) {
				return candidate;
			}
		}

		return $"{baseName} ({Guid.NewGuid():N})";
	}

	private void LoadBookmarks() {
		if (ViewParameter == null) {
			return;
		}

		string module = ViewParameter.ModuleType.ToString();
		BookmarkRoots.Clear();

		ModuleBookmarksState? saved = appProfileService.Model.ModuleBookmarks
			.FirstOrDefault(m => string.Equals(m.Module, module, StringComparison.OrdinalIgnoreCase));
		if (saved?.Roots is { Count: > 0 }) {
			foreach (BookmarkNodeState root in saved.Roots) {
				BookmarkRoots.Add(BookmarkNode.FromState(root));
			}
		}

		bookmarksLoaded = true;
	}

	private void ScheduleBookmarksSave() {
		if (!bookmarksLoaded || ViewParameter == null) {
			return;
		}

		string module = ViewParameter.ModuleType.ToString();
		ModuleBookmarksState state = new() {
			Module = module,
			Roots = BookmarkRoots.Select(r => r.ToState()).ToList(),
		};

		List<ModuleBookmarksState> all = appProfileService.Model.ModuleBookmarks
			.Where(m => !string.Equals(m.Module, module, StringComparison.OrdinalIgnoreCase))
			.ToList();
		all.Add(state);
		appProfileService.Model.ModuleBookmarks = all;
		appProfileService.ScheduleSave();
	}
}
