using YiffBrowser.BaseFramework.Helpers;
using DevExpress.Mvvm;
using RW.Common.Helpers;
using RW.Common.WPF.Utilities;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YiffBrowser.E621.Models.E621;
using YiffBrowser.E621.Views.Subs;

namespace YiffBrowser.E621.Services;

internal class TagsSearchService : BindableBase {

	public E621API? Api { get; set; }


	private CancellationTokenSource? cts = null;
	private string[]? currentTags;

	public bool InternalChange { get; set; }

	public string? Word { get; set; }
	public string[] CurrentTags {
		get => currentTags ?? [];
		set => currentTags = value;
	}

	public string SearchText {
		get => GetProperty(() => SearchText) ?? string.Empty;
		set {
			SetProperty(() => SearchText, value);
			OnSearchTextChanged();
		}
	}

	public ObservableCollection<SearchTagItem> AutoCompletes { get; } = [];

	public SearchTagItem? SelectedItem {
		get => GetProperty(() => SelectedItem);
		set => SetProperty(() => SelectedItem, value);
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

		UpdateMetaProperties();
	}


	public ICommand ClearCommand => new DelegateCommand(Clear);
	public void Clear() {
		SearchText = string.Empty;
	}

	public void OnSearchTextSelectionChanged() {
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

	public void CancelLoading() {
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

	public void CalculateCurrentTags() {
		List<string> tags = [];
		foreach (string item in SearchText.Trim().Split(' ').Where(s => s.IsNotBlank()).ToList()) {
			tags.Add(item.ToLower());
		}
		CurrentTags = [.. tags];
	}

	public string GetCurrentWord() {
		return GetCurrentWord(out int _);
	}

	public string GetCurrentWord(out int index) {
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


	public PostSearch GetPostSearchResult(string text, out string? resultPostID) {
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

	public string GetSearchText() {
		string text = SearchText.Trim().ToLower();
		text = text.ReplaceLineEndings(" ");
		return text;
	}

	public string[] GetSearchTags() {
		string text = GetSearchText();
		string[] array = [.. text.Split(' ').Where(s => s.IsNotBlank())];
		if (array.IsEmpty()) {
			array = [""];
		}
		return array;
	}

	private void PutSelectionAtTheEnd() {
		SearchTextSelectionStart = SearchText.Length;
	}

	private string? FindMeta(string root) {//order:score
		string text = " " + SearchText + " ";
		string textLower = text.ToLower();
		int startIndex = textLower.IndexOf(root);
		if (startIndex == -1) {
			return null;
		}
		int endIndex = -1;
		for (int i = startIndex; i < textLower.Length; i++) {
			if (textLower[i] == ' ') {
				endIndex = i;
				break;
			}
		}
		if (endIndex == -1) {
			return null;
		}
		return text[startIndex..endIndex];
	}

	private void ReplaceMeta(string root, string target) {
		InternalChange = true;
		string? found = FindMeta(root);
		if (found.IsNotBlank()) {
			SearchText = SearchText.Replace(found, target).Trim();
		} else {
			if (target.IsNotBlank()) {
				SearchText = (SearchText.Trim() + " " + target).Trim();
			}
		}
		//FocusSearchBox();
		PutSelectionAtTheEnd();
	}

	private void UpdateMetaProperties() {
		if (CurrentTags == null) {
			return;
		}
		
		//if (CurrentTags.Contains("order:new")) {
		//	OrderDropDownText = "New";
		//} else if (CurrentTags.Contains("order:rank")) {
		//	OrderDropDownText = "Rank";
		//} else if (CurrentTags.Contains("order:random")) {
		//	OrderDropDownText = "Random";
		//} else if (CurrentTags.Contains("order:favorite")) {
		//	OrderDropDownText = "Favorite";
		//} else if (CurrentTags.Contains("order:score")) {
		//	OrderDropDownText = "Score";
		//} else {
		//	OrderDropDownText = "Order";
		//}

		//if (CurrentTags.Contains("type:jpg")) {
		//	TypeDropDownText = "JPG";
		//} else if (CurrentTags.Contains("type:png")) {
		//	TypeDropDownText = "PNG";
		//} else if (CurrentTags.Contains("type:gif")) {
		//	TypeDropDownText = "GIF";
		//} else if (CurrentTags.Contains("type:webm")) {
		//	TypeDropDownText = "WEBM";
		//} else if (CurrentTags.Contains("type:anim")) {
		//	TypeDropDownText = "ANIM";
		//} else {
		//	TypeDropDownText = "Type";
		//}

		//if (CurrentTags.Contains("rating:safe")) {
		//	RatingDropDownText = "Safe";
		//} else if (CurrentTags.Contains("rating:questionable")) {
		//	RatingDropDownText = "Questionable";
		//} else if (CurrentTags.Contains("rating:explicit")) {
		//	RatingDropDownText = "Explicit";
		//} else {
		//	RatingDropDownText = "Rating";
		//}
	}

}
