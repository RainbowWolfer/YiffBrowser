using DevExpress.Mvvm;
using RW.Base.WPF.ViewModelServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace YiffBrowser.BaseFramework.ViewModels;

public interface IDialogViewModel {
	string DialogTitle { get; }
	bool CustomDialogIcon { get; }
	ImageSource? DialogIcon { get; }

	DialogWindowParameter DialogWindowParameter { get; }

	IReadOnlyList<DialogCommand> DialogCommands { get; }

	IDelegateCommand LoadedCommand { get; }

	void InitializeDialogCommands();

	void OnWindowInitialized(Window window);

	DialogCommandResult? HandleDialogCommand(DialogCommand command);

	void OnWindowClosed();

}

public class DialogCommand : BindableBase {

	public object? Tag {
		get => GetProperty(() => Tag);
		set => SetProperty(() => Tag, value);
	}

	public string Content {
		get => GetProperty(() => Content);
		set => SetProperty(() => Content, value);
	}

	public object? Icon {
		get => GetProperty(() => Icon);
		set => SetProperty(() => Icon, value);
	}

	public ICommand? Command {
		get => GetProperty(() => Command);
		set => SetProperty(() => Command, value);
	}

	public bool IsEnabled {
		get => GetProperty(() => IsEnabled);
		set => SetProperty(() => IsEnabled, value);
	}

	public bool IsPrimary {
		get => GetProperty(() => IsPrimary);
		set => SetProperty(() => IsPrimary, value);
	}

	public DialogCommand() {
		Tag = null;
		Content = string.Empty;
		Icon = null;
		Command = null;
		IsEnabled = true;
	}

	public DialogCommand(object tag, string content, ICommand command) {
		Tag = tag;
		Content = content;
		Icon = null;
		Command = command;
		IsEnabled = true;
	}

	public DialogCommand(object tag, string content, object? icon, ICommand command, bool isEnabled) {
		Tag = tag;
		Content = content;
		Icon = icon;
		Command = command;
		IsEnabled = isEnabled;
	}

}

public class DialogCommandResult {
	public bool CloseWindow { get; set; } = true;
	public bool? DialogResultFlag { get; set; } = null;
}

public interface IDialogOwnerSetter {
	Window? Owner { get; }
}

public record class DialogWindowParameter(
	WindowStartupLocation WindowStartupLocation = WindowStartupLocation.CenterOwner,
	ResizeMode ResizeMode = ResizeMode.NoResize,
	SizeToContent SizeToContent = SizeToContent.WidthAndHeight,
	WindowStyle WindowStyle = WindowStyle.SingleBorderWindow,
	bool ShowInTaskbar = true,
	bool AllowsTransparency = false,
	bool AllowDrop = true,
	bool TopMost = false,
	bool EscapeToClose = true
) {
	public static DialogWindowParameter Default { get; } = new();

	//todo 
	public static DialogWindowParameter Resizable { get; } = Default with {

	};
}

public abstract class DialogViewModel<T> : ViewModelBase, IDialogViewModel {
	protected IMessageBoxServiceEx MessageBoxService => GetService<IMessageBoxServiceEx>();
	protected IDispatcherServiceEx DispatcherService => GetService<IDispatcherServiceEx>();
	protected IUIObjectService<UserControl> DialogUserControlService => GetService<ITypedUIObjectService>().As<UserControl>();





	private bool isParameterLoaded = false;
	private bool isParentViewModelLoaded = false;

	public string DialogTitle {
		get => GetProperty(() => DialogTitle);
		set => SetProperty(() => DialogTitle, value);
	}

	public bool CustomDialogIcon {
		get => GetProperty(() => CustomDialogIcon);
		set => SetProperty(() => CustomDialogIcon, value);
	}

	public ImageSource? DialogIcon {
		get => GetProperty(() => DialogIcon);
		set => SetProperty(() => DialogIcon, value);
	}

	public virtual DialogWindowParameter DialogWindowParameter { get; } = DialogWindowParameter.Default;


	public IReadOnlyList<DialogCommand> DialogCommands {
		get => GetProperty(() => DialogCommands);
		set => SetProperty(() => DialogCommands, value);
	}


	public new T Parameter {
		get => GetProperty(() => Parameter);
		protected set => SetProperty(() => Parameter, value);
	}

	public object ParentViewModel {
		get => GetProperty(() => ParentViewModel);
		set => SetProperty(() => ParentViewModel, value);
	}

	public DialogViewModel() {
		DialogTitle = string.Empty;
	}

	public void InitializeDialogCommands() {
		DialogCommands = [.. GetDialogCommands()];
	}

	protected override void OnParameterChanged(object parameter) {
		base.OnParameterChanged(parameter);
		isParameterLoaded = true;
		if (parameter is T _parameter) {
			Parameter = _parameter;
		}
		TryInitialize();
	}

	protected override void OnParentViewModelChanged(object parentViewModel) {
		base.OnParentViewModelChanged(parentViewModel);
		isParentViewModelLoaded = true;
		ParentViewModel = parentViewModel;
		TryInitialize();
	}

	private void TryInitialize() {
		if (isParameterLoaded && isParentViewModelLoaded) {
			OnInitialized();
		}
	}

	protected virtual void OnInitialized() {

	}

	protected virtual IEnumerable<DialogCommand> GetDialogCommands() {
		yield break;
	}

	public virtual void OnWindowInitialized(Window window) {

	}

	public virtual void OnWindowClosed() {

	}

	public virtual DialogCommandResult? HandleDialogCommand(DialogCommand command) {
		ICommand? _command = command.Command;
		if (_command is null) {
			return null;
		}

		DialogCommandResult result = new();

		if (command.IsEnabled && _command.CanExecute(result)) {
			_command.Execute(result);
		}

		return result;
	}


	private DelegateCommand? loadedCommand;
	public IDelegateCommand LoadedCommand => loadedCommand ??= new(Loaded);
	protected virtual void Loaded() {

	}


}

public abstract class DialogViewModelOk<T> : DialogViewModel<T> {
	protected DialogCommand ConfirmDialogCommand { get; }

	public DialogViewModelOk() {
		ConfirmDialogCommand = new DialogCommand(MessageBoxResult.OK, "OK", ConfirmCommand);
	}

	protected override IEnumerable<DialogCommand> GetDialogCommands() {
		yield return ConfirmDialogCommand;
	}

	private DelegateCommand<DialogCommandResult>? confirmCommand;
	public IDelegateCommand ConfirmCommand => confirmCommand ??= new(Confirm);
	protected virtual void Confirm(DialogCommandResult result) {
		result.CloseWindow = true;
		result.DialogResultFlag = true;
		OnConfirmed();
	}

	protected virtual void OnConfirmed() {

	}
}

public abstract class DialogViewModelOkCancel<T> : DialogViewModel<T> {
	protected DialogCommand ConfirmDialogCommand { get; }
	protected DialogCommand CancelDialogCommand { get; }

	public DialogViewModelOkCancel() {
		ConfirmDialogCommand = new DialogCommand(MessageBoxResult.OK, "Confirm", ConfirmCommand) {
			IsPrimary = true,
		};
		CancelDialogCommand = new DialogCommand(MessageBoxResult.Cancel, "Cancel", CancelCommand);
	}

	protected override IEnumerable<DialogCommand> GetDialogCommands() {
		yield return ConfirmDialogCommand;
		yield return CancelDialogCommand;
	}


	private DelegateCommand<DialogCommandResult>? confirmCommand;
	public IDelegateCommand ConfirmCommand => confirmCommand ??= new(Confirm);
	protected virtual void Confirm(DialogCommandResult result) {
		if (Validate(out string message)) {
			if (OnConfirmed()) {
				result.CloseWindow = true;
				result.DialogResultFlag = true;
			} else {
				result.CloseWindow = false;
				result.DialogResultFlag = null;
			}
		} else {
			result.CloseWindow = false;
			result.DialogResultFlag = null;
			ShowErrorDialog(message);
		}
	}


	private DelegateCommand<DialogCommandResult>? cancelCommand;
	public IDelegateCommand CancelCommand => cancelCommand ??= new(Cancel);
	protected virtual void Cancel(DialogCommandResult result) {
		result.CloseWindow = true;
		result.DialogResultFlag = false;
	}

	protected virtual bool Validate(out string message) {
		message = string.Empty;
		return true;
	}

	protected virtual void ShowErrorDialog(string message) {
		MessageBox.Show(message, "Error");
	}


	protected virtual bool OnConfirmed() {
		return true;
	}
}