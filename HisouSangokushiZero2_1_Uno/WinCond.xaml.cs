using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno;
public sealed partial class WinCond:UserControl {
  public WinCond(){
    InitializeComponent();
    MyInit(this);
  }
  private static void MyInit(WinCond page) {
    SetUIElements(page);
    page.Measure(new(double.PositiveInfinity,double.PositiveInfinity));
    page.Width = 1160;
    page.Height = page.DesiredSize.Height;
  }
  private static void SetUIElements(WinCond page) {
    page.WinCondScenarioName1.Text = GameInfo.scenarios.ElementAtOrDefault(0)?.Value;
    page.WinCondListPanel1.MySetChildren([.. CreateWinCondList(0,2)]).MyApplyA(v => { v.BorderThickness = new(UIUtil.dataListFrameWidth); v.BorderBrush = new SolidColorBrush(UIUtil.dataListFrameColor); });
    page.WinCondScenarioName2.Text = GameInfo.scenarios.ElementAtOrDefault(1)?.Value;
    page.WinCondListPanel2.MySetChildren([.. CreateWinCondList(1,2)]).MyApplyA(v => { v.BorderThickness = new(UIUtil.dataListFrameWidth); v.BorderBrush = new SolidColorBrush(UIUtil.dataListFrameColor); });
  }
  static StackPanel[] CreateWinCondList(int scenarioNo,int chunkBlockNum) {
    ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
    Dictionary<ECountry,CountryInfo>[] chunkedCountryInfoMaps = maybeScenarioInfo?.CountryMap.MyApplyF(elems => elems.OrderBy(v => v.Key).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
    return maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedCountryInfoMaps.Select(chunkedCountryInfoMap => CreateWinCondPanel(scenarioInfo,chunkedCountryInfoMap)).ToArray()) ?? [];
    static StackPanel CreateWinCondPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<ECountry,CountryInfo> includeCountryInfoMap) {
      return new StackPanel { Background = new SolidColorBrush(UIUtil.dataListFrameColor) }.MySetChildren([
        CreateCountryDataLine(
          Color.FromArgb(255,240,240,240),
          new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="勝利条件",HorizontalAlignment=HorizontalAlignment.Left }
        ), .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
          scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewColor??Colors.Transparent,
          new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=string.Join(' ',countryInfo.Value.WinConditionMessageFunc()),HorizontalAlignment=HorizontalAlignment.Left,TextWrapping=TextWrapping.Wrap}
        ))
      ]);
      static StackPanel CreateCountryDataLine(Color backColor,UIElement countryNameElem,UIElement winCondElem) {
        return new StackPanel { Orientation = Orientation.Horizontal,Background = new SolidColorBrush(backColor),}.MySetChildren([
          new Border{ Width=UIUtil.CalcElemWidth(3),BorderThickness=new(UIUtil.dataListFrameWidth),BorderBrush=new SolidColorBrush(UIUtil.dataListFrameColor) }.MySetChild(countryNameElem),
          new Border{ Width=UIUtil.CalcElemWidth(45),BorderThickness=new(UIUtil.dataListFrameWidth),BorderBrush=new SolidColorBrush(UIUtil.dataListFrameColor) }.MySetChild(winCondElem)
        ]);
      }
    }
  }
}