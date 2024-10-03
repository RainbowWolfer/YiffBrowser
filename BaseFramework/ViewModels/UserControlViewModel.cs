using BaseFramework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseFramework.ViewModels {
	public class UserControlViewModel<T> : ViewModelBase<T> where T : IUserControlBase, new() {

		public UserControlViewModel() {

		}

	}
}
