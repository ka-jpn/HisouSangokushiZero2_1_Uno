using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
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
        page.WinCondListPanel1.MySetChildren([.. CreateWinCondList(0,2)]).MyApplyA(v => { v.BorderThickness = UIUtil.dataListFrameThickness; v.BorderBrush = UIUtil.dataListFrameColor.ToBrush(); });
        page.WinCondScenarioName2.Text = BaseData.scenarios.ElementAtOrDefault(1)?.Value;
        page.WinCondListPanel2.MySetChildren([.. CreateWinCondList(1,2)]).MyApplyA(v => { v.BorderThickness = UIUtil.dataListFrameThickness; v.BorderBrush = UIUtil.dataListFrameColor.ToBrush(); });
        static StackPanel[] CreateWinCondList(int scenarioNo,int chunkBlockNum) {
          Scenario.ScenarioData? maybeScenarioData = Scenario.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
          Dictionary<ECountry,CountryData>[] chunkedCountryDataMaps = maybeScenarioData?.CountryMap.MyApplyF(elems => elems.OrderBy(v => v.Key).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
          return maybeScenarioData?.MyApplyF(scenarioInfo => chunkedCountryDataMaps.Select(chunkedCountryInfoMap => CreateWinCondPanel(scenarioInfo,chunkedCountryInfoMap)).ToArray()) ?? [];
          static StackPanel CreateWinCondPanel(Scenario.ScenarioData scenarioInfo,Dictionary<ECountry,CountryData> includeCountryInfoMap) {
            return new StackPanel { Background = UIUtil.dataListFrameColor.ToBrush() }.MySetChildren([
              CreateCountryDataLine(new Color(255,240,240,240),new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },[new TextBlock { Text="勝利条件",HorizontalAlignment=HorizontalAlignment.Center }]),
              .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
                scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewColor??new(0,0,0,0),
                new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center },
                scenarioInfo.WinConditionMap.GetValueOrDefault(countryInfo.Key)?.Messages.MyApplyF(messages=>new List<UIElement?>{
                  messages.ElementAtOrDefault(0)?.MyApplyF(v=>string.Join('＆',v)).MyApplyF(v=>new TextBlock { Text=v,HorizontalAlignment=HorizontalAlignment.Left }),
                  messages.ElementAtOrDefault(1)?.MyApplyF(v=>string.Join(' ',v)).MyApplyF(v=>new TextBlock { Text=v,HorizontalAlignment=HorizontalAlignment.Left })
                }).MyNonNull() ?? []
              ))
            ]);
            static StackPanel CreateCountryDataLine(Color backColor,UIElement countryNameElem,List<UIElement> winCondElems) {
              Border winCond = new Border { Width = winCondElemsWidth,BorderThickness = UIUtil.dataListFrameThickness,BorderBrush = UIUtil.dataListFrameColor.ToBrush() }.MySetChild(new StackPanel().MySetChildren(winCondElems));
              winCondBorders.Add(winCond); 
              return new StackPanel { Orientation = Orientation.Horizontal,Background = backColor.ToBrush() }.MySetChildren([
                new Border{ Width=countryNameElemWidth,BorderThickness=UIUtil.dataListFrameThickness,BorderBrush=UIUtil.dataListFrameColor.ToBrush(),VerticalAlignment=VerticalAlignment.Stretch }.MySetChild(countryNameElem),
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