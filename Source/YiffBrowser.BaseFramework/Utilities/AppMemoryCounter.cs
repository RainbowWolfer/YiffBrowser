using DevExpress.Mvvm;
using RW.Common.WPF.Utilities.Timers;
using System.Diagnostics;
using System.Windows.Threading;

namespace YiffBrowser.BaseFramework.Utilities;

internal class AppMemoryCounter : BindableBase, IDisposable {

	public string InfoText {
		get => GetProperty(() => InfoText);
		private set => SetProperty(() => InfoText, value);
	}

	public string FullInfoText {
		get => GetProperty(() => FullInfoText);
		set => SetProperty(() => FullInfoText, value);
	}

	private readonly DispatcherTimerController dispatcherTimerController;
	public AppMemoryCounter() {
		dispatcherTimerController = new DispatcherTimerController(DispatcherPriority.Normal, TimeSpan.FromSeconds(1), Tick);
		dispatcherTimerController.Start();
	}

	public void Dispose() {
		dispatcherTimerController.Dispose();
	}

	private async Task Tick() {
		Stopwatch stopwatch = Stopwatch.StartNew();
		await Task.Run(() => {
			Process process = Process.GetCurrentProcess();

			long workingSet = process.WorkingSet64;
			InfoText = $"{FormatBytes(workingSet)}";

			FullInfoText = string.Join(Environment.NewLine, GetMemoryInfoLines(process));
		});
		stopwatch.Stop();
		//Debug.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");
	}

	private IEnumerable<string> GetMemoryInfoLines(Process process) {
		yield return $"Working Set (Physical Memory): {FormatBytes(process.WorkingSet64)}";
		yield return $"Private Memory Size: {FormatBytes(process.PrivateMemorySize64)}";
		yield return $"Paged Memory Size: {FormatBytes(process.PagedMemorySize64)}";
		yield return $"Virtual Memory Size: {FormatBytes(process.VirtualMemorySize64)}";
		yield return $"GC Heap Size: {FormatBytes(GC.GetTotalMemory(false))}";
		yield return $"Thread Count: {process.Threads.Count}";
		yield return $"Handle Count: {process.HandleCount}";
		yield return $"Start Time: {process.StartTime}";
	}

	private static string FormatBytes(long bytes) {
		return $"{bytes / 1024.0 / 1024.0:0.##} MB";
	}

}
