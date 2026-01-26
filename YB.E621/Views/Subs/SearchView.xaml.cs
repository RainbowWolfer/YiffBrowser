using BaseFramework.Controls;
using BaseFramework.Events;
using DevExpress.Mvvm;
using RW.Base.WPF.Events;
using RW.Base.WPF.ViewModelServices;
using RW.Common;
using RW.Common.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Controls;
using YB.E621.Models.E621;
using YB.E621.Services;
using YB.E621.ViewModels;

namespace YB.E621.Views.Subs;

public partial class SearchView : UserControl {
	public SearchView() {
		InitializeComponent();
	}
}

internal class SearchViewModel(IEventAggregator eventAggregator) : E621ViewModelBase {
	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();

	public IUIObjectService<ListBox> MainListBoxService => GetService<ITypedUIObjectService>(nameof(MainListBoxService)).As<ListBox>();
	public IUIObjectService<TextBoxExtend> SearchBoxService => GetService<ITypedUIObjectService>(nameof(SearchBoxService)).As<TextBoxExtend>();

	public TagsSearchService TagsSearchService { get; } = new();

	public E621API? Api { get; private set; }

	private E621MainViewModel? parentViewModel;

	protected override void OnInitialize() {
		Api = E621API.GetAPI(ViewParameter.ModuleType);
		TagsSearchService.Api = Api;
		eventAggregator.GetEvent<ThemeChangedEvent>().Subscribe(OnThemeChanged);
	}

	private void OnThemeChanged(ThemeChangedEventArgs args) {
		foreach (object? item in MainListBoxService.Object.Items) {
			if (MainListBoxService.Object.ItemContainerGenerator.ContainerFromItem(item) is ListBoxItem listBoxItem
				&& RW.Common.WPF.Helpers.ViewHelper.FindChild<SearchTagItemControl>(listBoxItem) is SearchTagItemControl control
			) {
				control.Refresh();
			}
		}
	}

	protected override void OnParentViewModelChanged(object parentViewModel) {
		base.OnParentViewModelChanged(parentViewModel);
		this.parentViewModel = (E621MainViewModel?)parentViewModel;
	}

	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	private void Loaded() {
		SearchBoxService.Focus();
	}

	public ICommand SubmitCommand => new DelegateCommand(Submit);
	private void Submit() {
		string[] tags = TagsSearchService.GetSearchTags();
		parentViewModel?.SearchSubmit(tags);
	}

	public ICommand OnSearchTextSelectionChangedCommand => new DelegateCommand<RoutedEventArgs>(e => {
		TagsSearchService.OnSearchTextSelectionChanged();
	});

	public ICommand OnSearchTextBoxPreviewKeyDownCommand => new DelegateCommand<KeyEventArgs>(OnSearchTextBoxPreviewKeyDown);
	private void OnSearchTextBoxPreviewKeyDown(KeyEventArgs args) {
		ListBox mainListBox = MainListBoxService.Object;
		if (args.Key == Key.Up && mainListBox.Items.IsNotEmpty()) {
			mainListBox.SelectedIndex = NumberHelper.Clamp(mainListBox.SelectedIndex - 1, 0, mainListBox.Items.Count - 1);
			args.Handled = true;
		} else if (args.Key == Key.Down && mainListBox.Items.IsNotEmpty()) {
			mainListBox.SelectedIndex = NumberHelper.Clamp(mainListBox.SelectedIndex + 1, 0, mainListBox.Items.Count - 1);
			args.Handled = true;
		} else if (args.Key == Key.Enter) {
			if (TagsSearchService.SelectedItem != null) {
				HandleItem(TagsSearchService.SelectedItem);
			} else {
				Submit();
			}
			args.Handled = true;
		} else if (args.Key == Key.Escape) {
			mainListBox.UnselectAll();
			args.Handled = true;
		}
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

	private DelegateCommand? openSearchPanelCommand;
	public IDelegateCommand OpenSearchPanelCommand => openSearchPanelCommand ??= new(OpenSearchPanel);
	private void OpenSearchPanel() {
		parentViewModel?.ShowSearchPanel();
	}


	private DelegateCommand? openSearchHistoryPanelCommand;
	public IDelegateCommand OpenSearchHistoryPanelCommand => openSearchHistoryPanelCommand ??= new(OpenSearchHistoryPanel);
	private void OpenSearchHistoryPanel() {
		parentViewModel?.ShowSearchHistoryPanel();
	}


}

public class SearchTagItem : BindableBase {
	public E621AutoComplete AutoComplete { get; }

	public bool IsSelected {
		get => GetProperty(() => IsSelected);
		set => SetProperty(() => IsSelected, value);
	}

	public SearchTagItem(E621AutoComplete autoComplete) {
		AutoComplete = autoComplete;
		IsSelected = false;
	}
}

public enum PostSearch {
	None, URL, PostID
}
