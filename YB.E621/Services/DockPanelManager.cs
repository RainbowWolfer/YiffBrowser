using BaseFramework.Attributes;
using DevExpress.Mvvm;
using HandyControl.Themes;
using RW.Base.WPF.Interfaces;
using RW.Common.Helpers;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using YB.E621.Interfaces;
using YB.E621.Parameters;
using YB.E621.Views;

namespace YB.E621.Services;

internal class DockPanelManager : BindableBase {
	public event TypedEventHandler<DockPanelManager, IDockPanel>? DockPanelClosed;

	public ObservableCollection<IDockPanel> DockPanels { get; } = [];
	public bool HasDockPanels => DockPanels.Count > 0;

	private readonly Dictionary<string, Type> dockPanelIdTypePool = [];
	private readonly Dictionary<Type, DockPanelIDAttribute> attributesPool = [];
	private readonly Dictionary<Type, IDockPanel> dockPanelInstances = [];

	private ViewParameter? viewParameter;
	private E621MainViewModel? mainViewModel;

	public DockPanelManager() {
		DockPanels.CollectionChanged += DockPanels_CollectionChanged;
	}

	private void DockPanels_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
		RaisePropertyChanged(() => DockPanels);
		RaisePropertyChanged(() => HasDockPanels);
	}

	public async void Initialize(IApplication application, E621MainViewModel mainViewModel) {
		this.mainViewModel = mainViewModel;
		this.viewParameter = mainViewModel.ViewParameter;

		Type[] types = ReflectionHelper.FindAllDerivedTypes<IDockPanel>(application.DllLoader.GetTypes());
		foreach (Type type in types) {
			if (type.GetCustomAttribute<DockPanelIDAttribute>() is { } attribute) {
				dockPanelIdTypePool[attribute.ID] = type;
				attributesPool[type] = attribute;
			}
		}
	}

	#region Show Single

	public IDockPanel? ShowSingle(string dockPanelID) {
		if (dockPanelIdTypePool.TryGetValue(dockPanelID, out Type? type)) {
			return ShowSingle(type);
		} else {
			return null;
		}
	}

	public T ShowSingle<T>() where T : IDockPanel, new() {
		return (T)ShowSingle(typeof(T));
	}

	public IDockPanel ShowSingle(Type type) {
		string? id = type.GetCustomAttribute<DockPanelIDAttribute>()?.ID;
		if (id != null && DockPanels.FirstOrDefault(x => x.GetDockPanelID() == id) is { } found) {
			return found;
		} else {
			if (attributesPool.TryGetValue(type, out DockPanelIDAttribute? attribute) && attribute.RemainInstance) {
				if (!dockPanelInstances.TryGetValue(type, out IDockPanel? dockPanel)) {
					dockPanel = CreateDockPanel(type);
				}
				dockPanelInstances[type] = dockPanel;
				DockPanels.Add(dockPanel);
				return dockPanel;
			} else {
				IDockPanel instance = CreateDockPanel(type);
				DockPanels.Add(instance);
				return instance;
			}
		}
	}

	#endregion

	private IDockPanel CreateDockPanel(Type type) {
		IDockPanel instance = (IDockPanel)Activator.CreateInstance(type)!;

		if(instance is IDockPanelEx dockPanelEx) {
			dockPanelEx.Initialize(new DockPanelParameter(
				instance,
				mainViewModel,
				viewParameter
			));
		}


		return instance;
	}

	public void Close(IDockPanel dockPanel) {
		DockPanels.Remove(dockPanel);
		DisposeItem(dockPanel);
		dockPanel.Closed();
		DockPanelClosed?.Invoke(this, dockPanel);
	}

	public void Close<T>() where T : IDockPanel {
		T[] temp = DockPanels.OfType<T>().ToArray();
		foreach (IDockPanel item in temp) {
			Close(item);
		}
	}

	public void CloseAll() {
		IDockPanel[] temp = DockPanels.ToArray();
		foreach (IDockPanel item in temp) {
			DisposeItem(item);
			item.Closed();
			DockPanelClosed?.Invoke(this, item);
		}
		DockPanels.Clear();
	}

	private void DisposeItem(IDockPanel item) {
		if (item is null) {
			return;
		}

		if (item.Content is FrameworkElement frameworkElement && frameworkElement.DataContext is IDisposable disposable) {
			disposable.Dispose();
		}

	}

	public bool Contains(Guid id) {
		return DockPanels.Any(x => x.InstanceID == id);
	}

	public bool Contains<T>() {
		return DockPanels.Any(x => x is T);
	}

	public bool Contains(IDockPanel dockPanel) {
		return DockPanels.Any(x => x == dockPanel);
	}
}
