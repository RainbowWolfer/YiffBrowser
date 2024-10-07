using BaseFramework.Enums;
using BaseFramework.Interfaces;
using BaseFramework.Models;
using BaseFramework.ViewModels;
using BaseFramework.Views;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using YB.E621.Models.E621;

namespace YB.E621.Views {
	public partial class PostDetailView : UserControlBase {
		public PostDetailView() {
			InitializeComponent();
		}
	}

	public class PostDetailViewModel : UserControlViewModel<PostDetailView> {
		private E621Post? post = null;

		public bool HasPost => Post != null;

		public E621Post? Post {
			get => post;
			set {
				SetProperty(ref post, value);
				RaisePropertyChanged(nameof(HasPost));
				PostDetailDockViewModel.Post = value;
			}
		}

		public ModuleType ModuleType { get; }

		public GridDefinitionModel LeftSideGrid { get; } = new(true, 150, new GridLength(220, GridUnitType.Pixel));
		public GridDefinitionModel RightSideGrid { get; } = new(false, 150, new GridLength(300, GridUnitType.Pixel));

		public PostDetailDockViewModel PostDetailDockViewModel { get; } = new();

		public PostDetailViewModel(ModuleType moduleType) {
			ModuleType = moduleType;
			Post = null;
			View.KeyDown += View_KeyDown;
		}

		public void Focus() {
			View.Dispatcher.Invoke(() => {
				View.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
			}, DispatcherPriority.Loaded);
		}

		protected override void Loaded(IViewBase viewBase) {
			base.Loaded(viewBase);
		}

		private void View_KeyDown(object sender, KeyEventArgs e) {
			if (e.Key == Key.Escape) {
				Back();
				e.Handled = true;
			}
		}

		public ICommand BackCommand => new DelegateCommand(Back);

		public void Back() {
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
