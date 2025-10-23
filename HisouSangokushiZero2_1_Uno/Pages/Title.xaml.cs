using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Svg.Skia;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Streams;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Title:Page {
  public Title() {
    InitializeComponent();
    MyInit(this);
    async void MyInit(Title page) {
      page.MapCanvas.PaintSurface += (_,e) => UIUtil.MapCanvas_PaintSurface(e);
      page.StartButton.Click += (_,_) => { GameData.game = GetInitGameData(); NavigateToGamePage(); };
      page.LoadButton.Click += async (_,_) => { GameData.game = (await Storage.ReadStorageData(1)).Item2 ?? GetInitGameData(); NavigateToGamePage(); };
      page.TopSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
      page.Content.SizeChanged += (_,_) => ScalingElements(page,GameData.game,UIUtil.GetScaleFactor(page.Content.RenderSize,1));
      UIUtil.SwitchViewModeActions.Add(() => { RefreshViewMode(page); ScalingElements(page,GameData.game,UIUtil.GetScaleFactor(page.Content.RenderSize,1)); });
      RefreshViewMode(page);
      await ReadMapSvg();
      page.MapCanvas.Invalidate();
      async Task ReadMapSvg() {
        using IRandomAccessStreamWithContentType stream = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Svg/map.svg")).AsTask().ContinueWith(f => f.Result.OpenReadAsync().AsTask()).Unwrap();
        using Stream netStream = stream.AsStreamForRead();        
        UIUtil.mapSvg = new SKSvg().MyApplyA(svg=> svg.Load(netStream));
      }
      GameState GetInitGameData() => GetGame.GetInitGameScenario(BaseData.scenarios.FirstOrDefault());
      void NavigateToGamePage() => (Window.Current?.Content as Frame)?.Navigate(typeof(Game));
      void RefreshViewMode(Title page) {
        page.SwitchViewModeButtonText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "▼" : "▲";
        page.Content.MaxWidth = UIUtil.viewMode == UIUtil.ViewMode.fix ? UIUtil.fixModeMaxWidth : double.MaxValue;
      }
      void ScalingElements(Title page,GameState game,double scaleFactor) {
        double infoFramebuttonMargin = UIUtil.infoFrameWidth.Value * (scaleFactor - 1);
        page.TopSwitchViewModeButton.Margin = new(0,0,infoFramebuttonMargin,infoFramebuttonMargin);
        page.TopSwitchViewModeButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      }
    }
  }
}