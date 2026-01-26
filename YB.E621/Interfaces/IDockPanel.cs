using BaseFramework.Attributes;
using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using System.Reflection;
using System.Windows;
using YB.E621.Parameters;
using YB.E621.Views;

namespace YB.E621.Interfaces;

public interface IDockPanel {
	Guid InstanceID { get; set; }

	string Name { get; }
	object Content { get; }

	void Focus();

	void Closed();

	string? GetDockPanelID();
}

internal interface IDockPanelEx : IDockPanel {
	void Initialize(DockPanelParameter parameter);
}

internal record class DockPanelParameter(
	IDockPanel DockPanel,
	E621MainViewModel E621MainView,
	ViewParameter ViewParameter
);

internal abstract class DockPanelItemBase<T> : BindableBase, IDockPanelEx where T : FrameworkElement, new() {

	public Guid InstanceID { get; set; } = Guid.NewGuid();

	public string Name {
		get => GetProperty(() => Name);
		set => SetProperty(() => Name, value);
	}

	public T Content { get; }

	object IDockPanel.Content => Content;

	public DockPanelItemBase() {
		Content = new T();
	}

	public void Initialize(DockPanelParameter parameter) {
		ViewModelExtensions.SetParameter(Content, parameter);
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