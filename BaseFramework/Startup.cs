using Autofac;
using FlyleafLib;
using RW.Base.WPF.Interfaces;

namespace BaseFramework;

internal class Startup : IStartup {
	public int Priority => IntPriority.Normal;

	public void Initialize(ContainerBuilder builder) {
		Engine.Start(new EngineConfig() {
#if DEBUG
			LogOutput = ":debug",
			LogLevel = LogLevel.Debug,
			FFmpegLogLevel = Flyleaf.FFmpeg.LogLevel.Warn,
#endif

			PluginsPath = ":Plugins",
			FFmpegPath = ":FFmpeg",

			// Use UIRefresh to update Stats/BufferDuration (and CurTime more frequently than a second)
			UIRefresh = true,
			UIRefreshInterval = 100
		});
	}
}
