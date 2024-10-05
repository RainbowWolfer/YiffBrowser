using BaseFramework.ViewModels;
using BaseFramework.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YB.E621.Views {
	public partial class PostDetailView : UserControlBase {
		public PostDetailView() {
			InitializeComponent();
		}
	}

	public class PostDetailViewModel : UserControlViewModel<PostDetailView> {
		
	}
}
