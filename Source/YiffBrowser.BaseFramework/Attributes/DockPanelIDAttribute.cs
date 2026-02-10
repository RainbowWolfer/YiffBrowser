namespace YiffBrowser.BaseFramework.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class DockPanelIDAttribute(string id, bool remainInstance) : Attribute {
	public string ID { get; } = id;
	public bool RemainInstance { get; } = remainInstance;
}
