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
  internal WinCond() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(WinCond page) {
      SetUIElements(page);
      page.Measure(new(double.PositiveInfinity,double.PositiveInfinity));
      page.Width = 1160;
      page.Height = page.DesiredSize.Height;
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
              CreateCountryDataLine(Color.FromArgb(255,240,240,240),new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },[new TextBlock { Text="勝利条件",HorizontalAlignment=HorizontalAlignment.Left }]),
              .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
                scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewBrush??UIUtil.transparentBrush,
                new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
                countryInfo.Value.WinConditionMessages.MyApplyF(messages=>new List<UIElement?>{
                  messages.ElementAtOrDefault(0)?.MyApplyF(v=>string.Join('＆',v)).MyApplyF(v=>new TextBlock { Text=v,HorizontalAlignment=HorizontalAlignment.Left,Margin=new(0,0,winCondElemsWidth-BasicStyle.fontsize * UIUtil.CalcFullWidthTextLength(v),0),RenderTransform=new ScaleTransform{ ScaleX= winCondElemsWidth / Math.Max(winCondElemsWidth,BasicStyle.fontsize * UIUtil.CalcFullWidthTextLength(v)) } }),
                  messages.ElementAtOrDefault(1)?.MyApplyF(v=>string.Join(' ',v)).MyApplyF(v=>new TextBlock { Text=v,HorizontalAlignment=HorizontalAlignment.Left,Margin=new(0,0,winCondElemsWidth-BasicStyle.fontsize * UIUtil.CalcFullWidthTextLength(v),0),RenderTransform=new ScaleTransform{ ScaleX= winCondElemsWidth / Math.Max(winCondElemsWidth,BasicStyle.fontsize * UIUtil.CalcFullWidthTextLength(v)) } })
                }).MyNonNull()
              ))
            ]);
            static StackPanel CreateCountryDataLine(Brush backColor,UIElement countryNameElem,List<UIElement> winCondElems) {
              return new StackPanel { Orientation = Orientation.Horizontal,Background = backColor,}.MySetChildren([
                new Border{ Width=countryNameElemWidth,BorderThickness=UIUtil.dataListFrameThickness,BorderBrush=UIUtil.dataListFrameBrush }.MySetChild(countryNameElem),
                new Border{ Width=winCondElemsWidth,BorderThickness=UIUtil.dataListFrameThickness,BorderBrush=UIUtil.dataListFrameBrush }.MySetChild(new StackPanel().MySetChildren(winCondElems))
              ]);
            }
          }
        }
      }
    }
  }
}