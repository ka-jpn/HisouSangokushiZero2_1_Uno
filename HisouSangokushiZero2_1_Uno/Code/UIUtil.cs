using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class UIUtil {
    internal enum ViewMode { fit, fix };
    internal static ViewMode viewMode = ViewMode.fix;
    internal static Action InitGameAction = () => { };
    internal static List<Action> SwitchViewModeAction = [];
    internal static readonly Size mapSize = new(2000,1750);
    internal static readonly Point mapGridCount = new(9,10);
    internal static readonly GridLength infoFrameWidth = new(50);
    internal static readonly double dataListFrameWidth = 1;
    internal static readonly Color dataListFrameColor = Color.FromArgb(255,150,150,150);
    internal static readonly double fixModeMaxWidth = 1000;
    internal static readonly Size areaSize = new(204,155);
    internal static readonly CornerRadius areaCornerRadius = new(30);
    internal static readonly Size personPutSize = new(99,70);
    internal static readonly double countryPersonPutPanelHeight = (personPutSize.Height + postFrameWidth * 4) * 4 + postFrameWidth * 4 * 2 + BasicStyle.textHeight;
    internal static readonly double personRankFontScale = 1.5;
    internal static readonly double personNameFontScale = 1.75;
    internal static readonly Color landRoadColor = Color.FromArgb(150,120,120,50);
    internal static readonly Color waterRoadColor = Color.FromArgb(150,50,50,150);
    internal static readonly double postFrameWidth = 1;
     internal static double CalcFullWidthLength(string str) => str.Length - str.Count("0123456789-.()".Contains) * 0.4;
    internal static double CalcElemWidth(double textlength) => BasicStyle.fontsize * textlength + dataListFrameWidth * 2;
    internal static StackPanel[] CreatePersonDataList(int scenarioNo,int chunkBlockBlockSize,int chunkBlockNum) {
      ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
      Dictionary<DefType.Person,PersonParam>[] chunkedPersonInfoMaps = maybeScenarioInfo?.PersonMap.MyApplyF(elems => elems.OrderBy(v => v.Value.Country).ThenBy(v => v.Value.Role).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
      StackPanel[] personDataBlock = maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedPersonInfoMaps.Select(chunkedPersonInfoMap => CreatePersonDataPanel(scenarioInfo,chunkedPersonInfoMap)).ToArray()) ?? [];
      return [.. personDataBlock.Chunk(chunkBlockBlockSize).Select(v => new StackPanel() { Orientation = Orientation.Horizontal }.MySetChildren([.. v]))];
      static StackPanel CreatePersonDataPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<DefType.Person,PersonParam> includePersonInfoMap) {
        return new StackPanel { Background = new SolidColorBrush(dataListFrameColor) }.MySetChildren([
          CreatePersonDataLine(
          Color.FromArgb(255,240,240,240),
          new TextBlock { Text="陣営",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="人物名",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="ロール",Margin=new(0,0,-BasicStyle.fontsize*3*0.5,0),RenderTransform=new ScaleTransform() { ScaleX=0.5 } },
          new TextBlock { Text="ランク",Margin=new(0,0,-BasicStyle.fontsize*3*0.5,0),RenderTransform=new ScaleTransform() { ScaleX=0.5 } },
          new TextBlock { Text="生年",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="登場",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="没年",HorizontalAlignment=HorizontalAlignment.Center }
        ), .. includePersonInfoMap.Select(personInfo => CreatePersonDataLine(
          scenarioInfo.CountryMap.GetValueOrDefault(personInfo.Value.Country)?.ViewColor??Colors.Transparent,
          new TextBlock { Text=personInfo.Value.Country.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Key.Value,Margin=new(0,0,-BasicStyle.fontsize*(UIUtil.CalcFullWidthLength(personInfo.Key.Value)-3),0),RenderTransform=new ScaleTransform() { ScaleX=Math.Min(1,3/UIUtil.CalcFullWidthLength(personInfo.Key.Value)) } },
          new Image{ Source=new SvgImageSource(new($"ms-appx:///Assets/Svg/{personInfo.Value.Role}.svg")),Width=BasicStyle.fontsize,Height=BasicStyle.fontsize,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center },
          new TextBlock { Text=personInfo.Value.Rank.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Value.BirthYear.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=Person.GetAppearYear(personInfo.Value).MyApplyF(appearYear=>appearYear>=scenarioInfo.StartYear?appearYear.ToString():"登場"),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Value.DeathYear.ToString(),HorizontalAlignment=HorizontalAlignment.Center }
        ))
        ]);
        static StackPanel CreatePersonDataLine(Color backColor,UIElement countryNameElem,UIElement personNameElem,UIElement personRoleElem,UIElement personRankElem,UIElement personBirthYearElem,UIElement personAppearYearElem,UIElement personDeathYearElem) {
          return new StackPanel { Orientation = Orientation.Horizontal,Background = new SolidColorBrush(backColor),}.MySetChildren([
            new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryNameElem),
          new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personNameElem),
          new Border{ Width=CalcElemWidth(1.5),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personRoleElem),
          new Border{ Width=CalcElemWidth(1.5),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personRankElem),
          new Border{ Width=CalcElemWidth(2),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personBirthYearElem),
          new Border{ Width=CalcElemWidth(2),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personAppearYearElem),
          new Border{ Width=CalcElemWidth(2),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personDeathYearElem),
        ]);
        }
      }
    }
    internal static StackPanel[] CreateCountryDataList(int scenarioNo,int chunkBlockNum) {
      ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
      Dictionary<ECountry,CountryInfo>[] chunkedCountryInfoMaps = maybeScenarioInfo?.CountryMap.MyApplyF(elems => elems.OrderBy(v => v.Key).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
      return maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedCountryInfoMaps.Select(chunkedCountryInfoMap => CreateCountryDataPanel(scenarioInfo,chunkedCountryInfoMap)).ToArray()) ?? [];
      static StackPanel CreateCountryDataPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<ECountry,CountryInfo> includeCountryInfoMap) {
        return new StackPanel { Background = new SolidColorBrush(dataListFrameColor) }.MySetChildren([
          CreateCountryDataLine(
          Color.FromArgb(255,240,240,240),
          new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="資金",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="所属エリア",HorizontalAlignment=HorizontalAlignment.Center }
        ), .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
          scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewColor??Colors.Transparent,
          new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=countryInfo.Value.Fund.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=string.Join(",",scenarioInfo.AreaMap.Where(v=>v.Value.Country==countryInfo.Key).Select(v=>v.Key.ToString())),HorizontalAlignment=HorizontalAlignment.Left }
        ))
        ]);
        static StackPanel CreateCountryDataLine(Color backColor,UIElement countryNameElem,UIElement countryFundElem,UIElement countryAreasElem) {
          return new StackPanel { Orientation = Orientation.Horizontal,Background = new SolidColorBrush(backColor),}.MySetChildren([
            new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryNameElem),
            new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryFundElem),
            new Border{ Width=CalcElemWidth(40),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryAreasElem),
          ]);
        }
      }
    }
    internal static void SwitchViewMode() {
      viewMode = viewMode == ViewMode.fix ? ViewMode.fit : ViewMode.fix;
      SwitchViewModeAction.ForEach(v=>v());
#if __WASM__
      Uno.Foundation.WebAssemblyRuntime.InvokeJS($"window.parent.{viewMode}();");
#endif
    }
    internal static void InitGame() {
      InitGameAction();
    }
  }
	internal static class 汎用拡張メソッド {
		internal static T MySetChildren<T>(this T panel,List<UIElement> elements) where T : Panel => panel.MyApplyA(v => v.Children.Clear()).MyApplyA(v => elements.ForEach(v.Children.Add));
		internal static T MySetChild<T>(this T control,UIElement element) where T : ContentControl => control.MyApplyA(v => v.Content=element);
		internal static Border MySetChild(this Border border,UIElement element) => border.MyApplyA(v => v.Child=element);
  }
}
