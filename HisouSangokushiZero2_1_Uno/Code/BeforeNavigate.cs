using HisouSangokushiZero2_1_Uno.MyUtil;
using SkiaSharp;
using Svg.Skia;
using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class BeforeNavigate {
  internal static async Task ReadSvgs() {
    UIUtil.mapSvg=await ReadSvg("map.svg");
    UIUtil.armySvg=await ReadSvg("army.svg");
  }
  private static async Task<SKSvg> ReadSvg(string fileName) {
    using IRandomAccessStreamWithContentType stream = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/Svg/{fileName}"))).MyApplyF(f => f.OpenReadAsync());
    using Stream netStream = stream.AsStreamForRead();
    return new SKSvg().MyApplyA(svg => svg.Load(netStream));
  }
}