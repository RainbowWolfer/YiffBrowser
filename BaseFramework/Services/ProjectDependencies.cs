using BaseFramework.Models;

namespace BaseFramework.Services;

public static class ProjectDependencies {

	// dotnet list package --include-transitive

	public static readonly DependencyDescriptor[] AllDependencies = [.. new DependencyDescriptor[]{
		new("ControlzEx", "7.0.2", "https://github.com/ControlzEx/ControlzEx"),
		new("Flyleaf.FFmpeg.Bindings", "8.0.1", "https://github.com/SuRGeoNix/Flyleaf"),
		new("FlyleafLib.Controls.WPF", "1.6.2", "https://github.com/SuRGeoNix/Flyleaf"),
		new("FlyleafLib", "3.10.2", "https://github.com/SuRGeoNix/Flyleaf"),
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
		new("Dragablz", "0.0.3.234", "https://github.com/ButchersBoy/Dragablz"),
		new("MaterialDesignColors", "5.3.1-ci1146", "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit"),
		new("MaterialDesignThemes", "5.3.1-ci1146", "https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit"),
		new("Microsoft.Xaml.Behaviors.Wpf", "1.1.135", "https://github.com/Microsoft/XamlBehaviorsWpf"),
		new("Newtonsoft.Json", "13.0.4", "https://www.newtonsoft.com/json"),
		new("SharpGen.Runtime", "2.4.2-beta", "https://github.com/SharpGenTools/SharpGenTools"),
		new("SharpGen.Runtime.COM", "2.4.2-beta", "https://github.com/SharpGenTools/SharpGenTools"),
		new("Vortice.D3DCompiler", "3.7.6-beta", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.Direct2D1", "3.7.6-beta", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.Direct3D11", "3.7.6-beta", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.DirectComposition", "3.7.6-beta", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.DirectX", "3.7.6-beta", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.DXGI", "3.7.6-beta", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.Mathematics", "1.9.3", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.MediaFoundation", "3.7.6-beta", "https://github.com/amerkoleci/Vortice.Windows"),
		new("Vortice.XAudio2", "3.7.6-beta", "https://github.com/amerkoleci/Vortice.Windows"),
		new("WpfColorFontDialog", "1.0.8", "https://github.com/sskodje/WpfColorFont/"),
	}.OrderBy(x => x.Name)];
}