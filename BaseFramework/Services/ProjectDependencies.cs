using BaseFramework.Models;

namespace BaseFramework.Services;

public static class ProjectDependencies {

	// dotnet list package --include-transitive

	public static readonly DependencyDescriptor[] AllDependencies = [.. new DependencyDescriptor[]{
		new("ControlzEx", "7.0.2", "https://github.com/ControlzEx/ControlzEx"),
		new("gong-wpf-dragdrop", "4.0.0", "https://github.com/punker76/gong-wpf-dragdrop"),
		new("HandyControls", "3.6.0", "https://github.com/ghost1372/HandyControls"),
		new("PolySharp", "1.15.0", "https://github.com/Sergio0694/PolySharp"),
		new("System.Collections.Immutable", "10.0.1", "https://github.com/dotnet/runtime"),
		new("XamlAnimatedGif", "2.3.1", "https://github.com/XamlAnimatedGif/XamlAnimatedGif"),
		new("Autofac", "9.0.0", "https://github.com/autofac/Autofac"),
		new("AutoMapper", "10.1.1", "https://github.com/LuckyPennySoftware/AutoMapper"),
		new("DevExpressMvvm", "24.1.6", "https://github.com/DevExpress/DevExpress.Mvvm.Free"),
		new("Microsoft.Bcl.AsyncInterfaces", "10.0.0", "https://github.com/dotnet/runtime"),
		new("Microsoft.Xaml.Behaviors.Wpf", "1.1.135", "https://github.com/Microsoft/XamlBehaviorsWpf"),
		new("Newtonsoft.Json", "13.0.4", "https://www.newtonsoft.com/json"),
		new("System.Buffers", "4.6.1", "https://github.com/dotnet/runtime"),
		new("System.Diagnostics.DiagnosticSource", "10.0.0", "https://github.com/dotnet/runtime"),
		new("System.Memory", "4.6.3", "https://github.com/dotnet/runtime"),
		new("System.Numerics.Vectors", "4.6.1", "https://github.com/dotnet/runtime"),
		new("System.Runtime.CompilerServices.Unsafe", "6.1.2", "https://github.com/dotnet/runtime"),
		new("System.Threading.Tasks.Extensions", "4.6.3", "https://github.com/dotnet/runtime"),
		new("Microsoft.CSharp", "4.7.0", "https://github.com/dotnet/runtime"),
		new("System.Reflection.Emit", "4.7.0", "https://github.com/dotnet/runtime"),
		new("System.Text.Json", "8.0.5", "https://github.com/dotnet/runtime"),
		new("System.IO.Compression", "4.3.0", "https://github.com/dotnet/runtime"),
	}.OrderBy(x => x.Name)];
}