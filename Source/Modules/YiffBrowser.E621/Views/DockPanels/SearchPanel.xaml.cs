using DevExpress.Mvvm;
using RW.Base.WPF.Events;
using RW.Base.WPF.ViewModelServices;
using RW.Common.Helpers;
using RW.Common.WPF.Helpers;
using RW.Common.WPF.Models;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YiffBrowser.BaseFramework.Attributes;
using YiffBrowser.BaseFramework.Controls;
using YiffBrowser.BaseFramework.Events;
using YiffBrowser.BaseFramework.ViewModels;
using YiffBrowser.E621.Controls;
using YiffBrowser.E621.Interfaces;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Services;
using YiffBrowser.E621.ViewModels;
using YiffBrowser.E621.Views.Subs;

namespace YiffBrowser.E621.Views.DockPanels;

public partial class SearchPanel : UserControl {
	public SearchPanel() {
		InitializeComponent();
	}
}

[DockPanelID(nameof(SearchPanelItem), remainInstance: true)]
internal class SearchPanelItem : DockPanelItemBase<SearchPanel> {
	public SearchPanelItem() {
		Name = "Search";
		Icon = "\uE721";
	}
}

internal class SearchPanelViewModel(IEventAggregator eventAggregator) : DockPanelViewModelBase<SearchPanelItem> {

	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();

	public IUIObjectService<ListBox> MainListBoxService => GetService<ITypedUIObjectService>(nameof(MainListBoxService)).As<ListBox>();
	public IUIObjectService<TextBoxExtend> SearchBoxService => GetService<ITypedUIObjectService>(nameof(SearchBoxService)).As<TextBoxExtend>();
	public IUIObjectService<TextBox> RandomTagsTextBoxService => GetService<ITypedUIObjectService>(nameof(RandomTagsTextBoxService)).As<TextBox>();


	public string RandomTagText {
		get => GetProperty(() => RandomTagText);
		set => SetProperty(() => RandomTagText, value);
	}

	public BaseFramework.ViewModels.LoadingStatus RandomTagsLoadingStatus { get; } = new();

	public TagsSearchService TagsSearchService { get; } = new();

	public E621API? Api { get; private set; }


	protected override void OnInitialized() {
		base.OnInitialized();
		Api = E621API.GetAPI(ViewParameter.ModuleType);
		TagsSearchService.Api = Api;
		eventAggregator.GetEvent<ThemeChangedEvent>().Subscribe(OnThemeChanged);
	}

	private void OnThemeChanged(ThemeChangedEventArgs args) {
		foreach (object? item in MainListBoxService.Object.Items) {
			if (MainListBoxService.Object.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem listBoxItem
				&& ViewHelper.FindChild<SearchTagItemControl>(listBoxItem) is SearchTagItemControl control
			) {
				control.Refresh();
			}
		}
	}

	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	private void Loaded() {
		SearchBoxService.Focus();
	}



	public ICommand OnSearchTextSelectionChangedCommand => new DelegateCommand<RoutedEventArgs>(e => {
		TagsSearchService.OnSearchTextSelectionChanged();
	});

	public ICommand OnSearchTextBoxPreviewKeyDownCommand => new DelegateCommand<CommandEventArgs>(OnSearchTextBoxPreviewKeyDown);
	private void OnSearchTextBoxPreviewKeyDown(CommandEventArgs args) {
		if (args.TryGetArgs(out KeyEventArgs? e)) {
			ListBox mainListBox = MainListBoxService.Object;
			if (e.Key == Key.Up && mainListBox.Items.IsNotEmpty()) {
				mainListBox.SelectedIndex = NumberHelper.Clamp(mainListBox.SelectedIndex - 1, 0, mainListBox.Items.Count - 1);
				e.Handled = true;
			} else if (e.Key == Key.Down && mainListBox.Items.IsNotEmpty()) {
				mainListBox.SelectedIndex = NumberHelper.Clamp(mainListBox.SelectedIndex + 1, 0, mainListBox.Items.Count - 1);
				e.Handled = true;
			} else if (e.Key == Key.Enter) {
				if (TagsSearchService.SelectedItem != null) {
					HandleItem(TagsSearchService.SelectedItem);
				} else {
					Submit();
				}
				e.Handled = true;
			} else if (e.Key == Key.Escape) {
				mainListBox.UnselectAll();
				e.Handled = true;
			}
		}
	}

	public ICommand SubmitCommand => new DelegateCommand(Submit);
	private void Submit() {
		string[] tags = TagsSearchService.GetSearchTags();
		MainViewModel?.SearchSubmit(tags);
	}


	public ICommand HandleItemCommand => new DelegateCommand<SearchTagItem?>(HandleItem);
	private async void HandleItem(SearchTagItem? item) {
		if (item is null) {
			return;
		}
		E621AutoComplete autoComplete = item.AutoComplete;
		string tag = autoComplete.Name ?? string.Empty;

		int lastSpace = TagsSearchService.SearchText.LastIndexOf(' ');
		if (lastSpace == -1) {
			TagsSearchService.SearchText = tag;
		} else {
			string cut = TagsSearchService.SearchText[..lastSpace].Trim();
			TagsSearchService.SearchText = $"{cut} {tag}";
		}

		TagsSearchService.CalculateCurrentTags();

		TagsSearchService.InternalChange = true;

		FocusSearchBox();
		PutSelectionAtTheEnd();

		//strange issue: if dont wait a little, double clicking will cause popup to lose focus and auto close
		await Task.Delay(100);
		TagsSearchService.AutoCompletes.Clear();
	}

	private void PutSelectionAtTheEnd() {
		TagsSearchService.SearchTextSelectionStart = TagsSearchService.SearchText.Length;
	}

	public ICommand OnSearchTextBoxLoadedCommand => new DelegateCommand(OnSearchTextBoxLoaded);
	private void OnSearchTextBoxLoaded() => FocusSearchBox();

	public void FocusSearchBox() {
		DispatcherService.Dispatcher.Invoke(SearchBoxService.Focus, DispatcherPriority.Loaded);
	}



	private DelegateCommand? openSearchHistoryPanelCommand;
	public IDelegateCommand OpenSearchHistoryPanelCommand => openSearchHistoryPanelCommand ??= new(OpenSearchHistoryPanel);
	private void OpenSearchHistoryPanel() {
		MainViewModel?.ShowSearchHistoryPanel();
	}




	private DelegateCommand? copyRandomTagsCommand;
	public IDelegateCommand CopyRandomTagsCommand => copyRandomTagsCommand ??= new(CopyRandomTags, CanCopyRandomTags);
	private void CopyRandomTags() {
		if (CanCopyRandomTags()) {
			RandomTagText.ReplaceLineEndings(" ").CopyToClipboard();
		}
	}
	private bool CanCopyRandomTags() => RandomTagText.IsNotBlank();


	private AsyncCommand? getRandomTagsCommand;
	public IDelegateCommand GetRandomTagsCommand => getRandomTagsCommand ??= new(GetRandomTags, CanGetRandomTags);
	private async Task GetRandomTags() {
		if (CanGetRandomTags()) {
			try {
				RandomTagsLoadingStatus.Initialize();

				E621Post[] posts = await Api.GetPostsByTagsAsync(new E621PostParameters() {
					Page = 1,
					PageLimit = 1,
					Tags = ["order:random"],
				});

				if (posts.IsNotEmpty()) {
					IEnumerable<string> tags = posts.Where(x => x.Tags != null).SelectMany(x => x.Tags!.GetAllTags());

					if (tags.IsNotEmpty()) {
						int countToTake = RandomHelper.Shared.Next(4, 8);
						string[] randomTags = [.. tags.Distinct().OrderBy(x => RandomHelper.Shared.Next()).Take(countToTake)];

						RandomTagText = string.Join(Environment.NewLine, randomTags);
					}

				}

				RandomTagsTextBoxService.Focus();

				RandomTagsLoadingStatus.Done();
			} catch (Exception ex) {
				RandomTagsLoadingStatus.Error(ex.Message);
				RandomTagText = ex.Message;
			}
		}
	}
	[MemberNotNullWhen(true, nameof(Api))]
	private bool CanGetRandomTags() => Api != null && !RandomTagsLoadingStatus.ShowLoading;



}