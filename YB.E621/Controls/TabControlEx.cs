using System.Windows;
using System.Windows.Controls;

namespace YB.E621.Controls;

internal class TabControlEx : TabControl {




	public DataTemplate TabListItemTemplate {
		get => (DataTemplate)GetValue(TabListItemTemplateProperty);
		set => SetValue(TabListItemTemplateProperty, value);
	}

	public static readonly DependencyProperty TabListItemTemplateProperty = DependencyProperty.Register(
		nameof(TabListItemTemplate),
		typeof(DataTemplate),
		typeof(TabControlEx),
		new PropertyMetadata(null)
	);

	private ScrollViewerEx? scrollViewer;

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();
		
		scrollViewer = GetTemplateChild("tabHeaderScrollViewer") as ScrollViewerEx;

		if (GetTemplateChild("tabItemsList") is ListBox listBox) {

		}
	}

	protected override void OnSelectionChanged(SelectionChangedEventArgs e) {
		base.OnSelectionChanged(e);

		Dispatcher.BeginInvoke(new Action(() => {
			if (ItemContainerGenerator.ContainerFromItem(SelectedItem) is FrameworkElement container) {
				scrollViewer?.ScrollToElement(container);
			}
		}), System.Windows.Threading.DispatcherPriority.Loaded);
	}
}
