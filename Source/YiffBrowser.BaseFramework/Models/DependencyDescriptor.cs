namespace YiffBrowser.BaseFramework.Models;

public record class DependencyDescriptor(
	string Name,
	string Version,
	string SourceURL
);
