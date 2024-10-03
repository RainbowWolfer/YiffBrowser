using BaseFramework.Helpers;
using BaseFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BaseFramework.Views {
	public class UserControlBase : UserControl, IUserControlBase {
		UserControl IUserControlBase.UserControl => this;


		public UserControlBase() {
			
		}

	}
}
