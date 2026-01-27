using BaseFramework.Database.Models;

namespace YB.E621.Models.Database;

public class SearchTagsRecord : RecordBase {
	public required DateTime DateTime { get; set; }
	public required string[] Tags { get; set; }


	public static SearchTagsRecord Create(IEnumerable<string> tags) {
		return new SearchTagsRecord() {
			DateTime = DateTime.Now,
			Tags = [.. tags],
		};
	}
}
