using HisouSangokushiZero2_1_Uno;
using Uno.UI.Hosting;
App.InitializeLogging();
var host = UnoPlatformHostBuilder.Create()
    .App(() => new App())
    .UseWebAssembly()
    .Build();
await host.RunAsync();
