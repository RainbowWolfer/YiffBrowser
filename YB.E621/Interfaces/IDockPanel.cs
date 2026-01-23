using BaseFramework.Attributes;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using System.Reflection;
using System.Windows;

namespace YB.E621.Interfaces;

public interface IDockPanel {
	Guid InstanceID { get; set; }

	string Name { get; }
	object Content { get; }

	void Focus();

	void Closed();

	string? GetDockPanelID();
}

public abstract class DockPanelItemBase<T> : BindableBase, IDockPanel where T : FrameworkElement, new() {

	public Guid InstanceID { get; set; } = Guid.NewGuid();

	public string Name {
		get => GetProperty(() => Name);
		set => SetProperty(() => Name, value);
	}

	public T Content { get; }

	object IDockPanel.Content => Content;

	public DockPanelItemBase() {
		Content = new T();
		ViewModelExtensions.SetParameter(Content, this);
	}

	public virtual void Closed() {

	}

	public virtual void Focus() {
		Content.Focus();
	}

	public string? GetDockPanelID() {
		return GetType().GetCustomAttribute<DockPanelIDAttribute>()?.ID;
	}

}