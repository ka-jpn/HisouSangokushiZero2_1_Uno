using HisouSangokushiZero2_1_Uno;
using Uno.UI.Hosting;
App.InitializeLogging();
await Uno.UI.Xaml.Media.FontFamilyHelper.PreloadAsync(new Microsoft.UI.Xaml.Media.FontFamily("ms-appx:///Assets/Fonts/SourceHanSansJP-Medium.otf#Source Han Sans JP Medium"),new Windows.UI.Text.FontWeight(500),Windows.UI.Text.FontStretch.Normal,Windows.UI.Text.FontStyle.Normal);
var host = UnoPlatformHostBuilder.Create()
    .App(() => new App())
    .UseWebAssembly()
    .Build();
await host.RunAsync();
