using HisouSangokushiZero2_1_Uno;
using System;
using Uno.UI.Hosting;
internal class Program {
  [STAThread]public static void Main(string[] _) {
    var host = UnoPlatformHostBuilder.Create()
      .App(() => new App())
      .UseX11()
      .UseLinuxFrameBuffer()
      .UseMacOS()
      .UseWin32()
      .Build();
    host.Run();
  }
}