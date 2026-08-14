using DevExpress.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YiffBrowser.BaseFramework.Services;
using YiffBrowser.E621.Enums;

namespace YiffBrowser.E621.ViewModels;

internal sealed class ClosedTabRecord {
	public Guid Id { get; init; } = Guid.NewGuid();
	public string[] Tags { get; init; } = [];
	public int Page { get; init; } = 1;
	public PostTabKind Kind { get; init; } = PostTabKind.Search;
	public int? PoolId { get; init; }
	public int? RelationsRootPostId { get; init; }

	public ClosedTabRecord() { }

	public ClosedTabRecord(PostTabItem tab) {
		Tags = tab.Tags ?? [];
		Page = Math.Max(1, tab.GetContentPage());
		Kind = tab.Kind;
		PoolId = tab.PoolId;
		RelationsRootPostId = tab.RelationsRootPostId;
	}

	public ClosedTabRecord(string[] tags, int page) {
		Tags = tags ?? [];
		Page = Math.Max(1, page);
	}

	public string DisplayText {
		get {
			string tagsText = Kind switch {
				PostTabKind.Pool when PoolId is int poolId => PostTabTitleHelper.ForPool(poolId),
				PostTabKind.Relations when RelationsRootPostId is int rootId => PostTabTitleHelper.ForRelations(rootId),
				_ => Tags.Length > 0 ? string.Join(" ", Tags) : "(empty)",
			};
			return Kind == PostTabKind.Relations ? tagsText : $"{tagsText} · p.{Page}";
		}
	}

	public string TagsDisplay => Kind switch {
		PostTabKind.Pool when PoolId is int poolId => PostTabTitleHelper.ForPool(poolId),
		PostTabKind.Relations when RelationsRootPostId is int rootId => PostTabTitleHelper.ForRelations(rootId),
		_ => Tags.Length > 0 ? string.Join(" ", Tags) : "(empty)",
	};

	public ClosedTabRecordState ToState() => new() {
		Id = Id,
		Tags = Tags,
		Page = Page,
		Kind = (int)Kind,
		PoolId = PoolId,
		RelationsRootPostId = RelationsRootPostId,
	};

	public static ClosedTabRecord FromState(ClosedTabRecordState state) => new() {
		Id = state.Id == Guid.Empty ? Guid.NewGuid() : state.Id,
		Tags = state.Tags ?? [],
		Page = Math.Max(1, state.Page),
		Kind = Enum.IsDefined(typeof(PostTabKind), state.Kind) ? (PostTabKind)state.Kind : PostTabKind.Search,
		PoolId = state.PoolId,
		RelationsRootPostId = state.RelationsRootPostId,
	};
}

internal sealed class ClosedTabBatch {
	public Guid Id { get; init; } = Guid.NewGuid();
	public List<ClosedTabRecord> Tabs { get; init; } = [];
	public DateTime ClosedAt { get; init; } = DateTime.Now;

	public string BatchHeader => Tabs.Count == 1
		? Tabs[0].DisplayText
		: $"{Tabs.Count} closed tabs";

	public ClosedTabBatchState ToState() => new() {
		Id = Id,
		ClosedAt = ClosedAt,
		Tabs = Tabs.Select(t => t.ToState()).ToList(),
	};

	public static ClosedTabBatch FromState(ClosedTabBatchState state) => new() {
		Id = state.Id == Guid.Empty ? Guid.NewGuid() : state.Id,
		ClosedAt = state.ClosedAt == default ? DateTime.Now : state.ClosedAt,
		Tabs = (state.Tabs ?? []).Select(ClosedTabRecord.FromState).ToList(),
	};
}

/// <summary>Flat row for the View All Closed Tabs DataGrid.</summary>
internal sealed class ClosedTabGridRow(ClosedTabBatch batch, ClosedTabRecord record) {
	public ClosedTabBatch Batch { get; } = batch;
	public ClosedTabRecord Record { get; } = record;

	public DateTime ClosedAt => Batch.ClosedAt;
	public int BatchSize => Batch.Tabs.Count;
	public bool IsMultiBatch => Batch.Tabs.Count > 1;
	public string BatchLabel => Batch.Tabs.Count == 1 ? "Single" : $"Batch · {Batch.Tabs.Count}";
	public string TagsDisplay => Record.TagsDisplay;
	public int Page => Record.Page;
}

/// <summary>Menu-bindable entry for hierarchical Recent Closed Tabs menus.</summary>
internal sealed class RecentClosedMenuEntry : BindableBase {
	public string Header {
		get => GetProperty(() => Header);
		set => SetProperty(() => Header, value);
	}

	public ICommand? Command {
		get => GetProperty(() => Command);
		set => SetProperty(() => Command, value);
	}

	public object? Parameter {
		get => GetProperty(() => Parameter);
		set => SetProperty(() => Parameter, value);
	}

	public bool IsSeparator {
		get => GetProperty(() => IsSeparator);
		set => SetProperty(() => IsSeparator, value);
	}

	public ObservableCollection<RecentClosedMenuEntry> Children { get; } = [];
}
