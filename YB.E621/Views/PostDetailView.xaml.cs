using BaseFramework.Models;
using BaseFramework.Services;
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
using YB.E621.Models;
using YB.E621.Models.E621;
using YB.E621.Services;

namespace YB.E621.Views {
	public partial class PostDetailView : UserControlBase {
		public PostDetailView() {
			InitializeComponent();
		}
	}

	public class PostDetailViewModel : UserControlViewModel<PostDetailView> {
		private E621Post? post = null;
		private bool showSidePanel = false;

		public bool HasPost => Post != null;

		public E621Post? Post {
			get => post;
			set {
				SetProperty(ref post, value);
				RaisePropertyChanged(nameof(HasPost));
			}
		}

		public bool ShowSidePanel {
			get => showSidePanel;
			set => SetProperty(ref showSidePanel, value);
		}

		public PostDetailViewModel() {
			Post = null;
		}

		public ICommand BackCommand => new DelegateCommand(Back);

		private void Back() {
			Post = null;
		}

		public ICommand NextCommand => new DelegateCommand(Next);
		public ICommand PreviousCommand => new DelegateCommand(Previous);

		private void Next() {

		}

		private void Previous() {

		}
	}
}
