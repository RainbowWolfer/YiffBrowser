using System.Windows;
using System.Windows.Input;

namespace BaseFramework.Extensions {
	public class CommandProxy : Freezable, ICommand {
		public CommandProxy() {

		}

		public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
			nameof(Command),
			typeof(ICommand),
			typeof(CommandProxy),
			new PropertyMetadata(new PropertyChangedCallback(OnCommandChanged))
		);

		public ICommand Command {
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		#region ICommand Members

		public bool CanExecute(object? parameter) {
			return Command != null && Command.CanExecute(parameter);
		}

		public void Execute(object? parameter) {
			Command.Execute(parameter);
		}

		public event EventHandler? CanExecuteChanged;

		private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			CommandProxy commandReference = (CommandProxy)d;

			if (e.OldValue is ICommand oldCommand) {
				oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
			}
			if (e.NewValue is ICommand newCommand) {
				newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
			}
		}

		#endregion

		#region Freezable

		protected override Freezable CreateInstanceCore() {
			return this;
		}

		#endregion
	}
}
