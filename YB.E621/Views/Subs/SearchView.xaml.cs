using BaseFramework.Controls;
using BaseFramework.Helpers;
using DevExpress.Mvvm;
using RW.Base.WPF.ViewModelServices;
using RW.Common;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Models.E621;
using YB.E621.Services;
using YB.E621.ViewModels;

namespace YB.E621.Views.Subs;

public partial class SearchView : UserControl {
	public SearchView() {
		InitializeComponent();
	}
}

internal class SearchViewModel() : E621ViewModelBase {
	public IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();

	public IUIObjectService<ListBox> MainListBoxService => GetService<ITypedUIObjectService>(nameof(MainListBoxService)).As<ListBox>();
	public IUIObjectService<TextBoxExtend> SearchBoxService => GetService<ITypedUIObjectService>(nameof(SearchBoxService)).As<TextBoxExtend>();



	public event TypedEventHandler<SearchViewModel, string[]>? SearchSubmit;

	private CancellationTokenSource? cts = null;
	private string[]? currentTags;

	public ObservableCollection<SearchTagItem> AutoCompletes { get; } = [];


	public SearchTagItem? SelectedItem {
		get => GetProperty(() => SelectedItem);
		set => SetProperty(() => SelectedItem, value);
	}


	public string SearchText {
		get => GetProperty(() => SearchText);
		set {
			SetProperty(() => SearchText, value);
			OnSearchTextChanged();
		}
	}

	public int SearchTextSelectionStart {
		get => GetProperty(() => SearchTextSelectionStart);
		set => SetProperty(() => SearchTextSelectionStart, value);
	}


	public string AlternativeHintText {
		get => GetProperty(() => AlternativeHintText);
		set => SetProperty(() => AlternativeHintText, value);
	}


	public bool IsLoading {
		get => GetProperty(() => IsLoading);
		set => SetProperty(() => IsLoading, value);
	}

	private bool InternalChange { get; set; }

	public string? Word { get; set; }
	public string[] CurrentTags {
		get => currentTags ?? [];
		set => currentTags = value;
	}

	public E621API? Api { get; private set; }

	private E621MainViewModel? parentViewModel;

	protected override void OnInitialize() {
		Api = E621API.GetAPI(ViewParameter.ModuleType);
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

	public ICommand ClearCommand => new DelegateCommand(Clear);

	private void Clear() {
		SearchText = string.Empty;
	}

	public ICommand SubmitCommand => new DelegateCommand(Submit);

	private void Submit() {
		string[] tags = GetSearchTags();
		SearchSubmit?.Invoke(this, tags);
		parentViewModel?.SearchSubmit(tags);
	}

	public ICommand OnSearchTextBoxLoadedCommand => new DelegateCommand(OnSearchTextBoxLoaded);
	public ICommand OnSearchTextSelectionChangedCommand => new DelegateCommand<RoutedEventArgs>(OnSearchTextSelectionChanged);

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
			if (SelectedItem != null) {
				HandleItem(SelectedItem);
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

		int lastSpace = SearchText.LastIndexOf(' ');
		if (lastSpace == -1) {
			SearchText = tag;
		} else {
			string cut = SearchText[..lastSpace].Trim();
			SearchText = $"{cut} {tag}";
		}

		CalculateCurrentTags();

		InternalChange = true;

		FocusSearchBox();
		PutSelectionAtTheEnd();

		//strange issue: if dont wait a little, double clicking will cause popup to lose focus and auto close
		await Task.Delay(100);
		AutoCompletes.Clear();
	}

	private void PutSelectionAtTheEnd() {
		SearchTextSelectionStart = SearchText.Length;
	}

	private void OnSearchTextBoxLoaded() => FocusSearchBox();

	public void FocusSearchBox() {
		DispatcherService.Dispatcher.Invoke(SearchBoxService.Focus, DispatcherPriority.Loaded);
	}

	private void OnSearchTextSelectionChanged(RoutedEventArgs o) {
		if (Word == null || SearchText.Length == 0) {
			return;
		}

		if (InternalChange) {
			InternalChange = false;
			return;
		}

		//only if word changes
		string current = GetCurrentWord();
		if (current != Word) {
			Word = current;

			if (current.Length <= 2 || current.Contains(':')) {
				AutoCompletes.Clear();
				CancelLoading();
			} else {
				LoadAutoSuggestionAsync(Word);
			}
		}

		CalculateCurrentTags();
	}

	private void OnSearchTextChanged() {
		if (InternalChange) {
			InternalChange = false;
			return;
		}

		if (SearchText.IsBlank()) {
			Word = null;
		}

		PostSearch postSearchResult = GetPostSearchResult(SearchText, out string? resultPostID);

		if (postSearchResult != PostSearch.None && resultPostID.IsNotBlank()) {
			if (postSearchResult == PostSearch.PostID) {
				AlternativeHintText = "Post ID Detected";
			} else if (postSearchResult == PostSearch.URL) {
				AlternativeHintText = "URL Detected";
			}

		} else {
			AlternativeHintText = string.Empty;
			Word = GetCurrentWord();

			if (Word.Length <= 2 || Word.Contains(':')) {
				AutoCompletes.Clear();
				CancelLoading();
			} else {
				LoadAutoSuggestionAsync(Word);
			}

			CalculateCurrentTags();
		}

		//UpdateMetaViews();
	}

	private void CancelLoading() {
		cts?.Cancel();
		cts = null;
		IsLoading = false;
	}

	private async void LoadAutoSuggestionAsync(string tag) {
		if (Api is null) {
			return;
		}

		cts?.Cancel();
		CancellationTokenSource _cts = new();
		cts = _cts;

		try {
			await Task.Delay(500, _cts.Token);
		} catch (TaskCanceledException) {
			IsLoading = false;
			return;
		}

		IsLoading = true;

		AutoCompletes.Clear();

		E621AutoComplete[] completes = await Api.GetE621AutoCompleteAsync(tag, _cts.Token);

		if (_cts.IsCancellationRequested) {
			IsLoading = false;
			return;
		}

		foreach (E621AutoComplete item in completes) {
			AutoCompletes.Add(new SearchTagItem(item));
		}

		IsLoading = false;
	}

	private void CalculateCurrentTags() {
		List<string> tags = [];
		foreach (string item in SearchText.Trim().Split(' ').Where(s => s.IsNotBlank()).ToList()) {
			tags.Add(item.ToLower());
		}
		CurrentTags = [.. tags];
	}

	private string GetCurrentWord() {
		return GetCurrentWord(out int _);
	}

	private string GetCurrentWord(out int index) {
		index = 0;
		int start = SearchTextSelectionStart;
		string text = SearchText;
		int spaceInStart = 0;
		foreach (char item in text) {
			if (char.IsWhiteSpace(item)) {
				spaceInStart++;
			} else {
				break;
			}
		}

		if (start > text.Length || ((start >= text.Length || text[start] == ' ') && start > 0 && text[start - 1] == ' ')) {
			return "";
		} else {
			string[] tags = text.Trim().Split(' ').Where(t => t.IsNotBlank()).ToArray();
			string word = "";

			int length = spaceInStart;
			for (int i = 0; i < tags.Length; i++) {
				length += tags[i].Length + 1;
				if (length > start) {
					word = tags[i];
					index = length;
					break;
				}
			}

			return word;
		}

	}

	private PostSearch GetPostSearchResult(string text, out string? resultPostID) {
		resultPostID = null;

		if (Api is null) {
			return PostSearch.None;
		}

		string[] split = text.Split([' '], StringSplitOptions.RemoveEmptyEntries);
		foreach (string item in split) {
			if (item.Length >= 2 && item.OnlyContainDigits()) {
				resultPostID = item;
				return PostSearch.PostID;
			} else if (item.StartsWith($"https://{Api.GetHost()}/posts/") ||
					item.StartsWith($"{Api.GetHost()}/posts/")
				) {
				string endPostID = "";
				int startIndex = item.LastIndexOf('/');
				int endIndex = item.LastIndexOf('?');
				if (endIndex == -1) {
					endIndex = item.Length;
				}
				for (int i = startIndex + 1; i < endIndex; i++) {
					endPostID += item[i];
				}
				if (endPostID.Length >= 2 && endPostID.OnlyContainDigits()) {
					resultPostID = endPostID;
					return PostSearch.URL;
				}
			}
		}
		return PostSearch.None;
	}

	public string GetSearchText() => SearchText.Trim().ToLower();

	public string[] GetSearchTags() {
		string text = GetSearchText();
		string[] array = text.Split(' ').Where(s => s.IsNotBlank()).ToArray();
		if (array.IsEmpty()) {
			array = [""];
		}
		return array;
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
