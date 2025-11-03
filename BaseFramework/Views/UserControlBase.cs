using BaseFramework.Interfaces;
using System.Windows.Controls;

namespace BaseFramework.Views;
public class UserControlBase : UserControl, IUserControlBase {
	UserControl IUserControlBase.UserControl => this;


	public UserControlBase() {

	}

}
