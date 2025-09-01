using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code; 
	internal static class UIUtil {
  internal enum ViewMode { fit, fix };
  internal static ViewMode viewMode = ViewMode.fix;
  internal static List<Action> SwitchViewModeActions = [];
  internal static List<Action> SaveGameActions = [];
  internal static List<Action> LoadGameActions = [];
  internal static List<Action> InitGameActions = [];
  internal static readonly Size mapSize = new(2000,1750);
  internal static readonly Point mapGridCount = new(9,10);
  internal static readonly GridLength infoFrameWidth = new(50);
  internal static readonly GridLength infoButtonHeight = new(60);
  internal static readonly Thickness dataListFrameThickness = new(1);
  internal static readonly double fixModeMaxWidth = 1000;
  internal static readonly Size areaSize = new(204,155);
  internal static readonly CornerRadius areaCornerRadius = new(30);
  internal static readonly double postFrameWidth = 1;
  internal static readonly Size personPutSize = new(99,70);
  internal static readonly double countryPersonPutPanelHeight = personPutSize.Height * 4 + postFrameWidth * 2 * 2 + BasicStyle.textHeight;
  internal static readonly double personRankFontScale = 1.5;
  internal static readonly double personNameFontScale = 1.75;
  internal static readonly double personPutFontScale = 2;
  internal static readonly Color landRoadColor = new(150,120,120,50);
  internal static readonly Color waterRoadColor = new(150,50,50,150);
  internal static readonly Color dataBackColor = new(255,240,240,240);
  internal static readonly Color dataListFrameColor = new(255,150,150,150);
  internal static readonly Color grayoutColor = new(100,100,100,100);
  internal static readonly Color transparentColor = new(0,0,0,0);
  internal static readonly int capitalPieceRowNum = 3;
  internal static readonly int capitalPieceColumnNum = 5;
  internal static readonly int capitalPieceCellNum = capitalPieceRowNum * capitalPieceColumnNum;
  internal static readonly string[] yearItems = ["春","夏","秋","冬"];
  internal static double CalcFullWidthTextLength(string str) => str.Length - str.Count("0123456789-.()".Contains) * 0.4 - str.Count(" ".Contains) * 0.8;
  internal static double CalcDataListElemWidth(double textlength) => BasicStyle.fontsize * textlength + dataListFrameThickness.Left + dataListFrameThickness.Right;
  internal static StackPanel[] CreatePersonDataList(int scenarioNo,int chunkBlockNum) {
    KeyValuePair<ScenarioId,ScenarioData>? maybeScenarioInfo = Scenario.scenarios.MyNullable().ElementAtOrDefault(scenarioNo);
    Dictionary<PersonId,PersonData>[] chunkedPersonInfoMaps = maybeScenarioInfo?.Value.PersonMap.MyApplyF(elems => elems.OrderBy(v => v.Value.Country).ThenBy(v => v.Value.Role).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
    StackPanel[] personDataBlock = maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedPersonInfoMaps.Select(chunkedPersonInfoMap => CreatePersonDataPanel(scenarioInfo,chunkedPersonInfoMap)).ToArray()) ?? [];
    return personDataBlock;
    static StackPanel CreatePersonDataPanel(KeyValuePair<ScenarioId,ScenarioData> scenarioInfo,Dictionary<PersonId,PersonData> includePersonInfoMap) {
      return new StackPanel { Background = dataListFrameColor.ToBrush() }.MySetChildren([
        CreatePersonDataLine(
          dataBackColor,
          new TextBlock { Text="陣営",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="人物名",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="ロール",Margin=new(0,0,-BasicStyle.fontsize*3*0.5,0),RenderTransform=new ScaleTransform() { ScaleX=0.5 } },
          new TextBlock { Text="ランク",Margin=new(0,0,-BasicStyle.fontsize*3*0.5,0),RenderTransform=new ScaleTransform() { ScaleX=0.5 } },
          new TextBlock { Text="生年",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="登場",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="没年",HorizontalAlignment=HorizontalAlignment.Center }
        ), .. includePersonInfoMap.Select(personInfo => CreatePersonDataLine(
          scenarioInfo.Value.CountryMap.GetValueOrDefault(personInfo.Value.Country)?.ViewColor??transparentColor,
          new TextBlock { Text=personInfo.Value.Country.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Key.Value,Margin=new(0,0,-BasicStyle.fontsize*(CalcFullWidthTextLength(personInfo.Key.Value)-3),0),RenderTransform=new ScaleTransform() { ScaleX=Math.Min(1,3/CalcFullWidthTextLength(personInfo.Key.Value)) } },
          new Image{ Source=new SvgImageSource(new($"ms-appx:///Assets/Svg/{personInfo.Value.Role}.svg")),Width=BasicStyle.fontsize,Height=BasicStyle.fontsize,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center },
          new TextBlock { Text=personInfo.Value.Rank.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Value.BirthYear.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=Person.GetAppearYear(scenarioInfo.Key,personInfo.Key).MyApplyF(appearYear=>appearYear>=scenarioInfo.Value.StartYear?appearYear.ToString():"登場"),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Value.DeathYear.ToString(),HorizontalAlignment=HorizontalAlignment.Center }
        ))
      ]);
      static StackPanel CreatePersonDataLine(Color backColor,UIElement countryNameElem,UIElement personNameElem,UIElement personRoleElem,UIElement personRankElem,UIElement personBirthYearElem,UIElement personAppearYearElem,UIElement personDeathYearElem) {
        return new StackPanel { Orientation = Orientation.Horizontal,Background = backColor.ToBrush()}.MySetChildren([
          new Border{ Width=CalcDataListElemWidth(3),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(countryNameElem),
          new Border{ Width=CalcDataListElemWidth(3),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(personNameElem),
          new Border{ Width=CalcDataListElemWidth(1.5),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(personRoleElem),
          new Border{ Width=CalcDataListElemWidth(1.5),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(personRankElem),
          new Border{ Width=CalcDataListElemWidth(2),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(personBirthYearElem),
          new Border{ Width=CalcDataListElemWidth(2),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(personAppearYearElem),
          new Border{ Width=CalcDataListElemWidth(2),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(personDeathYearElem),
        ]);
      }
    }
  }
  internal static StackPanel[] CreateCountryDataList(int scenarioNo,int chunkBlockNum) {
    ScenarioData? maybeScenarioInfo = Scenario.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
    Dictionary<ECountry,CountryData>[] chunkedCountryInfoMaps = maybeScenarioInfo?.CountryMap.MyApplyF(elems => elems.OrderBy(v => v.Key).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
    return maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedCountryInfoMaps.Select(chunkedCountryInfoMap => CreateCountryDataPanel(scenarioInfo,chunkedCountryInfoMap)).ToArray()) ?? [];
    static StackPanel CreateCountryDataPanel(ScenarioData scenarioInfo,Dictionary<ECountry,CountryData> includeCountryInfoMap) {
      return new StackPanel { Background = dataListFrameColor.ToBrush() }.MySetChildren([
        CreateCountryDataLine(
          dataBackColor,
          new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="資金",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="所属エリア",HorizontalAlignment=HorizontalAlignment.Center }
        ), .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
          scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewColor??transparentColor,
          new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=countryInfo.Value.Fund.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=string.Join(",",scenarioInfo.AreaMap.Where(v=>v.Value.Country==countryInfo.Key).Select(v=>v.Key.ToString())),HorizontalAlignment=HorizontalAlignment.Left }
        ))
      ]);
      static StackPanel CreateCountryDataLine(Color backColor,UIElement countryNameElem,UIElement countryFundElem,UIElement countryAreasElem) {
        return new StackPanel { Orientation = Orientation.Horizontal,Background = backColor.ToBrush() }.MySetChildren([
          new Border{ Width=CalcDataListElemWidth(3),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(countryNameElem),
          new Border{ Width=CalcDataListElemWidth(3),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(countryFundElem),
          new Border{ Width=CalcDataListElemWidth(40),BorderThickness=dataListFrameThickness,BorderBrush=dataListFrameColor.ToBrush() }.MySetChild(countryAreasElem),
        ]);
      }
    }
  }
  internal static void SwitchViewMode() {
    viewMode = viewMode == ViewMode.fix ? ViewMode.fit : ViewMode.fix;
    SwitchViewModeActions.ForEach(v=>v());
#if __WASM__
    Uno.Foundation.WebAssemblyRuntime.InvokeJS($"window.parent.{viewMode}();");
#endif
  }
  internal static double GetScaleFactor(Windows.Foundation.Size size) => Math.Max(Math.Max(fixModeMaxWidth,size.Width) / mapSize.Width,Math.Max(fixModeMaxWidth,size.Height) / mapSize.Height);
  internal static void SaveGame() => SaveGameActions.ForEach(v => v());
  internal static void LoadGame() => LoadGameActions.ForEach(v => v());
  internal static void InitGame() => InitGameActions.ForEach(v => v());
}
internal static class 汎用拡張メソッド {
		internal static T MySetChildren<T>(this T panel,List<UIElement> elements) where T : Panel => panel.MyApplyA(v => v.Children.Clear()).MyApplyA(v => elements.ForEach(v.Children.Add));
		internal static T MySetChild<T>(this T control,UIElement element) where T : ContentControl => control.MyApplyA(v => v.Content=element);
		internal static Border MySetChild(this Border border,UIElement element) => border.MyApplyA(v => v.Child=element);
  internal static SolidColorBrush ToBrush(this Color color) => new(Windows.UI.Color.FromArgb(color.A,color.R,color.G,color.B));
}
