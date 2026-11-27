using DevExpress.Mvvm;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Input;
using YiffBrowser.BaseFramework.Helpers;
using YiffBrowser.E621.Enums;
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

	public SortBy SortBy {
		get => GetProperty(() => SortBy);
		set {
			SetProperty(() => SortBy, value);
			if (!updatingMeta) {
				ReplaceMeta("order:", value.GetMeta());
			}
		}
	}

	public E621FileType? FileType {
		get => GetProperty(() => FileType);
		set {
			SetProperty(() => FileType, value);
			if (!updatingMeta) {
				ReplaceMeta("type:", value.GetMeta());
			}
		}
	}

	public SearchRating Rating {
		get => GetProperty(() => Rating);
		set {
			SetProperty(() => Rating, value);
			if (!updatingMeta) {
				ReplaceMeta("rating:", value.GetMeta());
			}
		}
	}

	public bool InPool {
		get => GetProperty(() => InPool);
		set => SetFlag(() => InPool, value, "inpool:", "inpool:true");
	}

	public bool IsChildPost {
		get => GetProperty(() => IsChildPost);
		set => SetFlag(() => IsChildPost, value, "parent:", "parent:any");
	}

	public bool IsParentPost {
		get => GetProperty(() => IsParentPost);
		set => SetFlag(() => IsParentPost, value, "child:", "child:any");
	}

	public MetaCompare ScoreCompare {
		get => GetProperty(() => ScoreCompare);
		set {
			SetProperty(() => ScoreCompare, value);
			WriteCount("score:", value, ScoreFrom, ScoreTo);
		}
	}

	public int ScoreFrom {
		get => GetProperty(() => ScoreFrom);
		set {
			SetProperty(() => ScoreFrom, value);
			WriteCount("score:", ScoreCompare, value, ScoreTo);
		}
	}

	public int ScoreTo {
		get => GetProperty(() => ScoreTo);
		set {
			SetProperty(() => ScoreTo, value);
			WriteCount("score:", ScoreCompare, ScoreFrom, value);
		}
	}

	public MetaCompare FavCompare {
		get => GetProperty(() => FavCompare);
		set {
			SetProperty(() => FavCompare, value);
			WriteCount("favcount:", value, FavFrom, FavTo);
		}
	}

	public int FavFrom {
		get => GetProperty(() => FavFrom);
		set {
			SetProperty(() => FavFrom, value);
			WriteCount("favcount:", FavCompare, value, FavTo);
		}
	}

	public int FavTo {
		get => GetProperty(() => FavTo);
		set {
			SetProperty(() => FavTo, value);
			WriteCount("favcount:", FavCompare, FavFrom, value);
		}
	}

	private bool updatingMeta = false;

	public TagsSearchService() {
		SortBy = SortBy.Default;
		FileType = null;
		Rating = SearchRating.All;
		ScoreCompare = MetaCompare.None;
		FavCompare = MetaCompare.None;
		ScoreFrom = 20;
		ScoreTo = 100;
		FavFrom = 20;
		FavTo = 100;
	}

	private void SetFlag<T>(System.Linq.Expressions.Expression<Func<T>> expression, T value, string root, string token) {
		SetProperty(expression, value);
		if (updatingMeta) {
			return;
		}
		if (value is true) {
			ReplaceMeta(root, token);
		} else {
			RemoveMeta(root);
		}
	}

	private void WriteCount(string root, MetaCompare compare, int from, int to) {
		if (updatingMeta) {
			return;
		}
		ReplaceMeta(root, CountTag.Format(root, compare, from, to));
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
			Suggest(Word);

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
			Suggest(current);
		}

		CalculateCurrentTags();
	}

	public void CancelLoading() {
		cts?.Cancel();
		cts = null;
		IsLoading = false;
	}

	private void Suggest(string word) {
		if (QueriesTags(word)) {
			LoadAutoSuggestionAsync(word);
			return;
		}

		CancelLoading();
		ShowCompletes(E621MetaCompletions.Match(word));
	}

	private static bool QueriesTags(string word) {
		string body = word.Trim().TrimStart('-');
		return body.Length > 2 && !body.Contains(':');
	}

	private void ShowCompletes(IEnumerable<E621AutoComplete> items) {
		AutoCompletes.Clear();
		foreach (E621AutoComplete item in items) {
			AutoCompletes.Add(new SearchTagItem(item));
		}
	}

	private async void LoadAutoSuggestionAsync(string tag) {
		if (Api is null) {
			ShowCompletes(E621MetaCompletions.Match(tag));
			return;
		}

		cts?.Cancel();
		CancellationTokenSource _cts = new();
		cts = _cts;

		E621AutoComplete[] meta = E621MetaCompletions.Match(tag);
		ShowCompletes(meta);

		try {
			await Task.Delay(500, _cts.Token);
		} catch (TaskCanceledException) {
			IsLoading = false;
			return;
		}

		IsLoading = true;

		E621AutoComplete[] completes = await Api.GetE621AutoCompleteAsync(tag, _cts.Token);

		if (_cts.IsCancellationRequested) {
			IsLoading = false;
			return;
		}

		ShowCompletes(meta.Concat(completes));

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
		if (updatingMeta) {
			return;
		}
		InternalChange = true;
		string? found = FindMeta(root);
		if (target.IsBlank()) {
			if (found.IsNotBlank()) {
				SearchText = RemoveToken(SearchText, found);
			}
		} else if (found.IsNotBlank()) {
			SearchText = SearchText.Replace(found, target).Trim();
		} else {
			SearchText = (SearchText.Trim() + " " + target).Trim();
		}
		PutSelectionAtTheEnd();
	}

	private void RemoveMeta(string root) {
		ReplaceMeta(root, "");
	}

	private static string RemoveToken(string text, string token) {
		string[] parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		return string.Join(' ', parts.Where(part => !part.Equals(token, StringComparison.OrdinalIgnoreCase)));
	}

	private void UpdateMetaProperties() {
		updatingMeta = true;
		try {
			SortBy = CurrentTags.GetSortBy();
			FileType = CurrentTags.GetFileType();
			Rating = CurrentTags.GetSearchRating();
			InPool = CurrentTags.Contains("inpool:true");
			IsChildPost = CurrentTags.Contains("parent:any");
			IsParentPost = CurrentTags.Contains("child:any");
			ApplyCount("score:", CurrentTags, compare => ScoreCompare = compare, value => ScoreFrom = value, value => ScoreTo = value);
			ApplyCount("favcount:", CurrentTags, compare => FavCompare = compare, value => FavFrom = value, value => FavTo = value);
		} finally {
			updatingMeta = false;
		}
	}

	private static void ApplyCount(string root, IEnumerable<string> tags, Action<MetaCompare> setCompare, Action<int> setFrom, Action<int> setTo) {
		foreach (string tag in tags) {
			if (!CountTag.TryParse(tag, root, out MetaCompare compare, out int from, out int to)) {
				continue;
			}
			setCompare(compare);
			setFrom(from);
			if (compare == MetaCompare.Range) {
				setTo(to);
			}
			return;
		}
		setCompare(MetaCompare.None);
	}

}
