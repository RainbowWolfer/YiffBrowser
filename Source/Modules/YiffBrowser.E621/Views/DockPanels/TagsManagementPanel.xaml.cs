using YiffBrowser.BaseFramework.Attributes;
using System.Windows.Controls;
using YiffBrowser.E621.Interfaces;
using YiffBrowser.E621.ViewModels;

namespace YiffBrowser.E621.Views.DockPanels;

public partial class TagsManagementPanel : UserControl {
	public TagsManagementPanel() {
		InitializeComponent();
	}
}

[DockPanelID(nameof(TagsManagementPanelItem), remainInstance: true)]
internal class TagsManagementPanelItem : DockPanelItemBase<TagsManagementPanel> {
	public TagsManagementPanelItem() {
		Name = "Tags Management";
		Icon = "\uE912";
	}
}

internal class TagsManagementPanelViewModel : DockPanelViewModelBase<TagsManagementPanelItem> {

}