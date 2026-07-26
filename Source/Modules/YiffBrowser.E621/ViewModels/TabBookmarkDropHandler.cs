using GongSolutions.Wpf.DragDrop;
using System.Windows;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.ViewModels;

/// <summary>
/// Handles drops onto the tabs-manage bookmark TreeView:
/// - PostTabItem from the card grid → add bookmark under folder/root
/// - BookmarkNode moves within the tree
/// </summary>
internal sealed class TabBookmarkDropHandler(E621MainViewModel owner) : IDropTarget {
	public void DragOver(IDropInfo dropInfo) {
		object? data = dropInfo.Data;
		if (data is not PostTabItem and not BookmarkNode) {
			dropInfo.Effects = DragDropEffects.None;
			return;
		}

		if (data is BookmarkNode moving && dropInfo.TargetItem is BookmarkNode targetNode) {
			if (ReferenceEquals(moving, targetNode) || IsDescendant(moving, targetNode)) {
				dropInfo.Effects = DragDropEffects.None;
				return;
			}
		}

		if (dropInfo.TargetItem is BookmarkNode { IsBookmark: true } && data is PostTabItem) {
			dropInfo.AcceptChildItem = false;
		} else if (dropInfo.TargetItem is BookmarkNode { IsFolder: true } || dropInfo.TargetItem is null) {
			dropInfo.AcceptChildItem = true;
		}

		dropInfo.Effects = data is PostTabItem ? DragDropEffects.Copy : DragDropEffects.Move;
		dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
	}

	public void Drop(IDropInfo dropInfo) {
		switch (dropInfo.Data) {
			case PostTabItem tab:
				owner.AddBookmarkFromTab(tab, dropInfo.TargetItem as BookmarkNode);
				break;
			case BookmarkNode node:
				owner.MoveBookmarkNode(node, dropInfo.TargetItem as BookmarkNode, dropInfo.InsertPosition);
				break;
		}
	}

	private static bool IsDescendant(BookmarkNode ancestor, BookmarkNode possibleDescendant) {
		BookmarkNode? current = possibleDescendant;
		while (current != null) {
			if (ReferenceEquals(current, ancestor)) {
				return true;
			}

			current = current.Parent;
		}

		return false;
	}
}
