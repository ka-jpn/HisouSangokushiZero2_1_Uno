using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class WinCond:UserControl {
  private static readonly double countryNameElemWidth = UIUtil.CalcDataListElemWidth(3);
  private static readonly double winCondElemsWidth = UIUtil.CalcDataListElemWidth(45);
  private static readonly List<Border> winCondBorders = [];
  internal WinCond() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(WinCond page) {
      SetUIElements(page);
      static void SetUIElements(WinCond page) {
        page.WinCondScenarioName1.Text = BaseData.scenarios.ElementAtOrDefault(0)?.Value;
        page.WinCondListPanel1.MySetChildren([.. CreateWinCondList(0,2)]).MyApplyA(v => { v.BorderThickness = UIUtil.dataListFrameThickness; v.BorderBrush = UIUtil.dataListFrameBrush; });
        page.WinCondScenarioName2.Text = BaseData.scenarios.ElementAtOrDefault(1)?.Value;
        page.WinCondListPanel2.MySetChildren([.. CreateWinCondList(1,2)]).MyApplyA(v => { v.BorderThickness = UIUtil.dataListFrameThickness; v.BorderBrush = UIUtil.dataListFrameBrush; });
        static StackPanel[] CreateWinCondList(int scenarioNo,int chunkBlockNum) {
          ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
          Dictionary<ECountry,CountryInfo>[] chunkedCountryInfoMaps = maybeScenarioInfo?.CountryMap.MyApplyF(elems => elems.OrderBy(v => v.Key).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
          return maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedCountryInfoMaps.Select(chunkedCountryInfoMap => CreateWinCondPanel(scenarioInfo,chunkedCountryInfoMap)).ToArray()) ?? [];
          static StackPanel CreateWinCondPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<ECountry,CountryInfo> includeCountryInfoMap) {
            return new StackPanel { Background = UIUtil.dataListFrameBrush }.MySetChildren([
              CreateCountryDataLine(Color.FromArgb(255,240,240,240),new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },[new TextBlock { Text="勝利条件",HorizontalAlignment=HorizontalAlignment.Center }]),
              .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
                scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewBrush??UIUtil.transparentBrush,
                new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center },
                countryInfo.Value.WinConditionMessages.MyApplyF(messages=>new List<UIElement?>{
                  messages.ElementAtOrDefault(0)?.MyApplyF(v=>string.Join('＆',v)).MyApplyF(v=>new TextBlock { Text=v,HorizontalAlignment=HorizontalAlignment.Left }),
                  messages.ElementAtOrDefault(1)?.MyApplyF(v=>string.Join(' ',v)).MyApplyF(v=>new TextBlock { Text=v,HorizontalAlignment=HorizontalAlignment.Left })
                }).MyNonNull()
              ))
            ]);
            static StackPanel CreateCountryDataLine(Brush backColor,UIElement countryNameElem,List<UIElement> winCondElems) {
              Border winCond = new Border { Width = winCondElemsWidth,BorderThickness = UIUtil.dataListFrameThickness,BorderBrush = UIUtil.dataListFrameBrush }.MySetChild(new StackPanel().MySetChildren(winCondElems));
              winCondBorders.Add(winCond);
              return new StackPanel { Orientation = Orientation.Horizontal,Background = backColor }.MySetChildren([
                new Border{ Width=countryNameElemWidth,BorderThickness=UIUtil.dataListFrameThickness,BorderBrush=UIUtil.dataListFrameBrush,VerticalAlignment=VerticalAlignment.Stretch }.MySetChild(countryNameElem),
                winCond
              ]);
            }
          }
        }
      }
    }
  }
  internal static void ResizeElem(WinCond page,double scaleFactor) {
    double pageWidth = page.RenderSize.Width;
    double contentWidth = pageWidth / scaleFactor - 5;
    page.Scroll.Width = pageWidth;
    page.ContentPanel.Width = contentWidth;
    page.ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
    winCondBorders.ForEach(v => v.MaxWidth = contentWidth - countryNameElemWidth);
    page.ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),page.ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
  }
}