using YiffBrowser.BaseFramework.Services;
using LiteDB;
using RW.Base.WPF.DependencyInjections;
using YiffBrowser.E621.Models.Database;
using YiffBrowser.E621.Enums;

namespace YiffBrowser.E621.Services;

public interface ISearchRecordHistoryService {
	string GetTableName(ModuleType moduleType);

	int GetPageCount(int pageSize, ModuleType moduleType, string? searchCondition = null);

	void AddRecord(SearchTagsRecord record, ModuleType moduleType);
	int RemoveRecords(IEnumerable<long> idList, ModuleType moduleType);

	IEnumerable<SearchTagsRecord> GetPagedRecords(int page, int pageSize, ModuleType moduleType, string? searchCondition = null);
}


internal class SearchRecordHistoryService(IDatabaseService databaseService) : ISearchRecordHistoryService, ISingletonDependency {

	private readonly HashSet<string> _ensuredTables = [];

	public string GetTableName(ModuleType moduleType) {
		return $"SearchRecordHistory_{moduleType}";
	}

	private ILiteCollection<SearchTagsRecord> GetCollection(ModuleType moduleType) {
		string tableName = GetTableName(moduleType);
		ILiteCollection<SearchTagsRecord> collection = databaseService.Database.GetCollection<SearchTagsRecord>(tableName);

		if (!_ensuredTables.Contains(tableName)) {
			lock (_ensuredTables) {
				if (_ensuredTables.Add(tableName)) {
					collection.EnsureIndex(x => x.Tags);
				}
			}
		}

		return collection;
	}

	public int GetPageCount(int pageSize, ModuleType moduleType, string? searchCondition = null) {
		ILiteCollection<SearchTagsRecord> collection = GetCollection(moduleType);
		int totalCount = collection.Count();

		if (totalCount <= 0) {
			return 0;
		}

		int pageCount = (int)Math.Ceiling((double)totalCount / pageSize);
		return pageCount;
	}

	public IEnumerable<SearchTagsRecord> GetPagedRecords(int page, int pageSize, ModuleType moduleType, string? searchCondition = null) {
		ILiteCollection<SearchTagsRecord> collection = GetCollection(moduleType);

		int currentPage = Math.Max(1, page);

		return collection.Query()
			.OrderByDescending(x => x.ID)
			.Skip((currentPage - 1) * pageSize)
			.Limit(pageSize)
			.ToEnumerable();
	}

	public void AddRecord(SearchTagsRecord record, ModuleType moduleType) {
		ILiteCollection<SearchTagsRecord> collection = GetCollection(moduleType);
		_ = collection.Insert(record);
	}

	public int RemoveRecords(IEnumerable<long> idList, ModuleType moduleType) {
		if (idList == null || !idList.Any()) {
			return 0;
		}

		ILiteCollection<SearchTagsRecord> collection = GetCollection(moduleType);

		int deletedCount = collection.DeleteMany(x => idList.Contains(x.ID));

		return deletedCount;
	}
}
