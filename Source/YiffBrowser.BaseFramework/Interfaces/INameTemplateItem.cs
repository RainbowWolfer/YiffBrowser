namespace YiffBrowser.BaseFramework.Interfaces;

public interface INameTemplateItem {
	string Site { get; }
	string Id { get; }
	string Md5 { get; }
	IEnumerable<string> Authors { get; }
	string Extension { get; }
}
