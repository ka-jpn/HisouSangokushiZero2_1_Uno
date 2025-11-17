using HisouSangokushiZero2_1_Uno.MyUtil;
using Svg.Skia;
using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class BeforeNavigate {
  internal static async Task WaitForFonts() {
#if __WASM__
    try { await Uno.Foundation.WebAssemblyRuntime.InvokeAsync("document.fonts.ready"); } catch { }
#else
    await Task.Yield();
#endif
  }
  internal static async Task ReadSvgs() {
    UIUtil.mapSvg =await ReadSvg("map.svg");
    UIUtil.armySvg =await ReadSvg("army.svg");
  }
  internal static async Task<SKSvg> ReadSvg(string fileName) {
    using IRandomAccessStreamWithContentType stream = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/Svg/{fileName}")).AsTask().ContinueWith(f => f.Result.OpenReadAsync().AsTask()).Unwrap();
    using Stream netStream = stream.AsStreamForRead();
    return new SKSvg().MyApplyA(svg => svg.Load(netStream));
  }
}