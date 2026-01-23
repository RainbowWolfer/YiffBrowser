using BaseFramework.Attributes;
using System.Windows.Controls;
using YB.E621.Interfaces;
using YB.E621.ViewModels;

namespace YB.E621.Views.DockPanels;

public partial class TagsManagementPanel : UserControl {
	public TagsManagementPanel() {
		InitializeComponent();
	}
}

[DockPanelID(nameof(TagsManagementPanelItem), remainInstance: true)]
public class TagsManagementPanelItem : DockPanelItemBase<TagsManagementPanel> {
	public TagsManagementPanelItem() {
		Name = "Tags Management";
	}
}

internal class TagsManagementPanelViewModel : DockPanelViewModelBase<TagsManagementPanelItem> {

}