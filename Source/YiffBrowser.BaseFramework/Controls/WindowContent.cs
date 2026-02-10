using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace YiffBrowser.BaseFramework.Controls;

public class WindowContent : ContentControl {

	public WindowContent() {

	}

	public override void OnApplyTemplate() {
		base.OnApplyTemplate();


		if (GetTemplateChild("ButtonIcon") is ButtonBase button) {
			button.Click += (s, e) => {
				//System.Windows.SystemCommands.ShowSystemMenuCommand.Execute(null, button);
			};

			button.MouseDoubleClick += (s, e) => {
				//Close();
			};
		}
	}
}
