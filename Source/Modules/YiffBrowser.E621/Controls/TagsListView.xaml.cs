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
using YiffBrowser.BaseFramework.Events;
using YiffBrowser.E621.Models.E621;

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

}

public class TagListItem(E621TagCategory category, string text) : BindableBase {
	public E621TagCategory Category { get; } = category;
	public string Text { get; } = text;

	public void Raise() {
		RaisePropertyChanged(() => Category);
	}
}
