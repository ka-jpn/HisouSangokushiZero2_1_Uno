using HisouSangokushiZero2_1_Uno;
using Uno.UI.Hosting;
var host = UnoPlatformHostBuilder.Create()
    .App(() => new App())
    .UseWebAssembly()
    .Build();
//await Uno.UI.Xaml.Media.FontFamilyHelper.PreloadAsync("ms-appx:///Assets/Fonts/SourceHanSansJP-Medium.otf",new(500),Windows.UI.Text.FontStretch.Normal,Windows.UI.Text.FontStyle.Normal);
await host.RunAsync();
