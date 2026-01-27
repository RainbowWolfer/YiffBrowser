using LiteDB;

namespace BaseFramework.Database.Models;

public class RecordBase {
	[BsonId]
	public long ID { get; set; }
}