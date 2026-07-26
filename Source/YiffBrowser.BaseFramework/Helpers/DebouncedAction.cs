using System.Windows;
using System.Windows.Threading;

namespace YiffBrowser.BaseFramework.Helpers;

/// <summary>Coalesces repeated calls into a single delayed action on the UI dispatcher.</summary>
public sealed class DebouncedAction(TimeSpan delay, Action action) {
	private readonly TimeSpan delay = delay;
	private readonly Action action = action ?? throw new ArgumentNullException(nameof(action));
	private DispatcherTimer? timer;

	private static Dispatcher UIDispatcher =>
		Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;

	public void Schedule() {
		Dispatcher dispatcher = UIDispatcher;
		if (!dispatcher.CheckAccess()) {
			dispatcher.BeginInvoke(Schedule);
			return;
		}

		if (timer == null) {
			timer = new DispatcherTimer(delay, DispatcherPriority.Background, OnTick, dispatcher);
		} else {
			timer.Stop();
			timer.Interval = delay;
		}

		timer.Start();
	}

	public void Flush() {
		Dispatcher dispatcher = UIDispatcher;
		if (!dispatcher.CheckAccess()) {
			dispatcher.Invoke(Flush);
			return;
		}

		timer?.Stop();
		action();
	}

	public void Cancel() {
		Dispatcher dispatcher = UIDispatcher;
		if (!dispatcher.CheckAccess()) {
			dispatcher.BeginInvoke(Cancel);
			return;
		}

		timer?.Stop();
	}

	private void OnTick(object? sender, EventArgs e) {
		timer?.Stop();
		action();
	}
}
