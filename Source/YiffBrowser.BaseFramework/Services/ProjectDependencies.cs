using YiffBrowser.BaseFramework.Models;

namespace YiffBrowser.BaseFramework.Services;

public static class ProjectDependencies {

	// dotnet list package --include-transitive

	public static readonly DependencyDescriptor[] AllDependencies = [.. new DependencyDescriptor[]{
		new("ControlzEx", "7.0.2", "https://github.com/ControlzEx/ControlzEx"),
		new("Flyleaf.FFmpeg.ABI", "9.1.0", "https://github.com/SuRGeoNix/Flyleaf"),
		new("FlyleafLib", "3.12.3", "https://github.com/SuRGeoNix/Flyleaf"),
		new("gong-wpf-dragdrop", "4.0.0", "https://github.com/punker76/gong-wpf-dragdrop"),
		new("HandyControls", "3.6.0", "https://github.com/ghost1372/HandyControls"),
		new("PolySharp", "1.15.0", "https://github.com/Sergio0694/PolySharp"),
		new("RW.Base.WPF", "1.0.38", "https://github.com/RainbowWolfer/RW.Common"),
		new("RW.Common", "1.0.38", "https://github.com/RainbowWolfer/RW.Common"),
		new("RW.Common.WPF", "1.0.38", "https://github.com/RainbowWolfer/RW.Common"),
		new("XamlAnimatedGif", "2.3.1", "https://github.com/XamlAnimatedGif/XamlAnimatedGif"),
		new("Autofac", "9.0.0", "https://github.com/autofac/Autofac"),
		new("AutoMapper", "10.1.1", "https://github.com/AutoMapper/AutoMapper"),
		new("DevExpressMvvm", "24.1.6", "https://github.com/DevExpress/DevExpress.Mvvm.Free"),
		new("Microsoft.Xaml.Behaviors.Wpf", "1.1.135", "https://github.com/Microsoft/XamlBehaviorsWpf"),
		new("Newtonsoft.Json", "13.0.4", "https://www.newtonsoft.com/json"),
		new("SharpGen.Runtime", "2.4.2-beta", "https://github.com/SharpGenTools/SharpGenTools"),
		new("SharpGen.Runtime.COM", "2.4.2-beta", "https://github.com/SharpGenTools/SharpGenTools"),
		new("Vortice.D3DCompiler", "3.8.3", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.Direct2D1", "3.8.3", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.Direct3D11", "3.8.3", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.DirectComposition", "3.8.3", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.DirectX", "3.8.3", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.DXGI", "3.8.3", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.Mathematics", "2.1.1", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.MediaFoundation", "3.8.3", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.XAudio2", "3.8.3", "https://github.com/amerkoleci/Vortice.Windows"),
	}.OrderBy(x => x.Name)];
}