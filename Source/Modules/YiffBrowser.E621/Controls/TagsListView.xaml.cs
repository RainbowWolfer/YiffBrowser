using DevExpress.Mvvm;
using HandyControl.Data;
using RW.Base.WPF.Events;
using RW.Base.WPF.Extensions;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using YiffBrowser.BaseFramework.Events;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.E621.Enums;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.Views;

namespace YiffBrowser.E621.Controls;

public partial class TagsListView : UserControl {


	public Tags? Tags {
		get => (Tags)GetValue(TagsProperty);
		set => SetValue(TagsProperty, value);
	}

	public static readonly DependencyProperty TagsProperty = DependencyProperty.Register(
		nameof(Tags),
		typeof(Tags),
		typeof(TagsListView),
		new PropertyMetadata(null, OnTagsChanged)
	);

	private static void OnTagsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		((TagsListView)d).Update();
	}

	public ObservableCollection<TagListItem> Items { get; } = [];
	public CollectionViewSource CollectionViewSource { get; } = new();

	private SubscriptionToken? subscriptionToken;

	public TagsListView() {
		InitializeComponent();

		CollectionViewSource.Source = Items;
		CollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TagListItem.Category)));
		TagsListBox.ItemsSource = CollectionViewSource.View;

		TagsSearchBar.SearchStarted += TagsSearchBar_SearchStarted;
		TagsSearchBar.TextChanged += TagsSearchBar_TextChanged;

		if (!ViewHelper.IsInDesignerMode) {
			Loaded += TagsListView_Loaded;
			Unloaded += TagsListView_Unloaded;
		}
	}

	private void TagsListView_Loaded(object sender, RoutedEventArgs e) {
		subscriptionToken ??= IoC.EventAggregator.GetEvent<ThemeChangedEvent>().Subscribe(OnThemeChanged);
	}

	private void TagsListView_Unloaded(object sender, RoutedEventArgs e) {
		if (subscriptionToken != null) {
			IoC.EventAggregator.GetEvent<ThemeChangedEvent>().Unsubscribe(subscriptionToken);
		}
	}

	private void OnThemeChanged(ThemeChangedEventArgs args) {
		foreach (TagListItem item in Items) {
			item.Raise();
		}
	}

	private void TagsSearchBar_SearchStarted(object? sender, FunctionEventArgs<string> e) {
		UpdateSearchCondition();
	}

	private void TagsSearchBar_TextChanged(object sender, TextChangedEventArgs e) {
		UpdateSearchCondition();
	}

	private void UpdateSearchCondition() {
		string searchText = TagsSearchBar.Text.SafeString().Trim().ToLower();

		if (string.IsNullOrWhiteSpace(searchText)) {
			CollectionViewSource.View.Filter = null;
		} else {
			CollectionViewSource.View.Filter = obj => {
				if (obj is TagListItem item) {
					return item.Text.Contains(searchText, StringComparison.CurrentCultureIgnoreCase);
				}
				return false;
			};
		}

	}

	public void ClearSearchTags() {
		TagsSearchBar.Text = string.Empty;
		UpdateSearchCondition();
	}

	private void Update() {
		Items.Clear();
		if (Tags == null) {
			return;
		}

		AddItems(E621TagCategory.Artists, Tags.Artist);
		AddItems(E621TagCategory.Director, Tags.Director);
		AddItems(E621TagCategory.Characters, Tags.Character);
		AddItems(E621TagCategory.Species, Tags.Species);
		AddItems(E621TagCategory.General, Tags.General);
		AddItems(E621TagCategory.Copyrights, Tags.Copyright);
		AddItems(E621TagCategory.Invalid, Tags.Invalid);
		AddItems(E621TagCategory.Lore, Tags.Lore);
		AddItems(E621TagCategory.Meta, Tags.Meta);
	}

	private void AddItems(E621TagCategory category, List<string>? list) {
		foreach (string item in list ?? []) {
			Items.Add(new TagListItem(category, item));
		}
	}

	private void CopyTag_Click(object sender, RoutedEventArgs e) {
		if (GetContextTag(sender) is TagListItem item) {
			item.Text.CopyToClipboard();
		}
	}

	private void CopyAllTags_Click(object sender, RoutedEventArgs e) {
		string tags = string.Join(' ', Items.Select(x => x.Text));
		if (tags.IsNotBlank()) {
			tags.CopyToClipboard();
		}
	}

	private void CopyCategoryTags_Click(object sender, RoutedEventArgs e) {
		if (GetContextGroup(sender) is CollectionViewGroup group) {
			string tags = string.Join(' ', group.Items.OfType<TagListItem>().Select(x => x.Text));
			if (tags.IsNotBlank()) {
				tags.CopyToClipboard();
			}
		}
	}

	private void SearchCategoryTags_Click(object sender, RoutedEventArgs e) {
		if (GetContextGroup(sender) is CollectionViewGroup group) {
			string[] tags = [.. group.Items.OfType<TagListItem>().Select(x => x.Text)];
			if (tags.Length > 0) {
				SearchTags(tags);
			}
		}
	}

	private void SearchWithTag_Click(object sender, RoutedEventArgs e) {
		if (GetButtonTag(sender) is TagListItem item) {
			SearchTags([item.Text]);
		}
	}

	private void SearchWithoutTag_Click(object sender, RoutedEventArgs e) {
		if (GetButtonTag(sender) is TagListItem item) {
			SearchTags([$"-{item.Text}"]);
		}
	}

	private void SearchWithTagIncludeCurrent_Click(object sender, RoutedEventArgs e) {
		if (GetContextTag(sender) is TagListItem item) {
			SearchTags(MergeWithCurrentTags(item.Text));
		}
	}

	private void SearchWithTagNew_Click(object sender, RoutedEventArgs e) {
		if (GetContextTag(sender) is TagListItem item) {
			SearchTags([item.Text]);
		}
	}

	private void SearchWithoutTagIncludeCurrent_Click(object sender, RoutedEventArgs e) {
		if (GetContextTag(sender) is TagListItem item) {
			SearchTags(MergeWithoutCurrentTags(item.Text));
		}
	}

	private void SearchWithoutTagNew_Click(object sender, RoutedEventArgs e) {
		if (GetContextTag(sender) is TagListItem item) {
			SearchTags([$"-{item.Text}"]);
		}
	}

	private void OpenTagInBrowser_Click(object sender, RoutedEventArgs e) {
		if (GetContextTag(sender) is not TagListItem item) {
			return;
		}

		ModuleType moduleType = GetModuleType();
		string url = $"https://{E621API.GetHost(moduleType)}/posts?tags={Uri.EscapeDataString(item.Text)}";
		url.OpenInBrowser();
	}

	private void SearchTags(string[] tags) {
		GetMainViewModel()?.SearchSubmit(tags);
	}

	private string[] GetCurrentTabTags() {
		return GetDetailViewModel()?.ParentViewModel?.TabItem.Tags ?? [];
	}

	private string[] MergeWithCurrentTags(string tag) {
		string[] current = GetCurrentTabTags();
		if (current.Length == 0) {
			return [tag];
		}

		List<string> tags = [.. current.Where(t => !string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)
			&& !string.Equals(t, $"-{tag}", StringComparison.OrdinalIgnoreCase))];
		tags.Add(tag);
		return [.. tags];
	}

	private string[] MergeWithoutCurrentTags(string tag) {
		string[] current = GetCurrentTabTags();
		string excluded = $"-{tag}";
		if (current.Length == 0) {
			return [excluded];
		}

		List<string> tags = [.. current.Where(t => !string.Equals(t, tag, StringComparison.OrdinalIgnoreCase)
			&& !string.Equals(t, excluded, StringComparison.OrdinalIgnoreCase))];
		tags.Add(excluded);
		return [.. tags];
	}

	private ModuleType GetModuleType() {
		return GetDetailViewModel()?.ParentViewModel?.ModuleType ?? ModuleType.E621;
	}

	private PostDetailViewModel? GetDetailViewModel() {
		if (DataContext is PostDetailViewModel detail) {
			return detail;
		}

		DependencyObject? current = this;
		while (current != null) {
			if (current is FrameworkElement { DataContext: PostDetailViewModel vm }) {
				return vm;
			}
			current = VisualTreeHelper.GetParent(current);
		}
		return null;
	}

	private E621MainViewModel? GetMainViewModel() {
		PostDetailViewModel? detail = GetDetailViewModel();
		return detail?.ParentViewModel?.TabItem.ParentViewModel;
	}

	private static TagListItem? GetContextTag(object sender) {
		if (sender is not MenuItem menuItem) {
			return null;
		}
		if (menuItem.DataContext is TagListItem item) {
			return item;
		}
		if (FindContextMenu(menuItem) is { PlacementTarget: FrameworkElement target }
			&& target.DataContext is TagListItem placementItem) {
			return placementItem;
		}
		return null;
	}

	private static CollectionViewGroup? GetContextGroup(object sender) {
		if (sender is not MenuItem menuItem) {
			return null;
		}
		if (menuItem.DataContext is CollectionViewGroup group) {
			return group;
		}
		if (FindContextMenu(menuItem) is { PlacementTarget: FrameworkElement target }) {
			return target.DataContext as CollectionViewGroup;
		}
		return null;
	}

	private static ContextMenu? FindContextMenu(DependencyObject? current) {
		while (current != null) {
			if (current is ContextMenu contextMenu) {
				return contextMenu;
			}
			current = LogicalTreeHelper.GetParent(current) ?? VisualTreeHelper.GetParent(current);
		}
		return null;
	}

	private static TagListItem? GetButtonTag(object sender) {
		if (sender is FrameworkElement { DataContext: TagListItem item }) {
			return item;
		}
		return null;
	}

}

public class TagListItem(E621TagCategory category, string text) : BindableBase {
	public E621TagCategory Category { get; } = category;
	public string Text { get; } = text;

	public void Raise() {
		RaisePropertyChanged(() => Category);
	}
}
