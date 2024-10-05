using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using YB.E621.Models.E621;

namespace YB.E621.Controls {
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

		public TagsListView() {
			InitializeComponent();
			CollectionViewSource.Source = Items;
			CollectionViewSource.GroupDescriptions.Add(new PropertyGroupDescription(nameof(TagListItem.Category)));
			TagsistBox.ItemsSource = CollectionViewSource.View;
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
			list ??= [];
			Items.AddRange(list.Select(x => new TagListItem(category, x)));
		}

	}

	public class TagListItem(E621TagCategory category, string text) {
		public E621TagCategory Category { get; } = category;
		public string Text { get; } = text;
	}
}
