using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code; 
internal static class UIUtil {
  internal enum ViewMode { fit, fix };
  internal enum PersonViewSortMode { Country_Role_BirthYear, Rank_BirthYear, BirthYear, DeathYear };
  internal static readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();
  internal static ViewMode viewMode = ViewMode.fix;
  internal static List<Action> SwitchViewModeActions = [];
  internal static List<Action> SaveGameActions = [];
  internal static List<Action> LoadGameActions = [];
  internal static List<Action> InitGameActions = [];
  internal static readonly Size mapSize = new(2000,1750);
  internal static readonly Point mapGridCount = new(9,10);
  internal static readonly GridLength infoFrameWidth = new(80);
  internal static readonly GridLength infoButtonHeight = new(80);
  internal static readonly double fixModeMaxWidth = 1000;
  internal static readonly double scaleSliderHeight = 20;
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
  internal static readonly Color grayoutColor = new(100,100,100,100);
  internal static readonly Color transparentColor = new(0,0,0,0);
  internal static readonly Color dataBackColor = new(255,150,150,150);
  internal static readonly Thickness dataMargin = new(1);
  internal static readonly int capitalPieceRowNum = 3;
  internal static readonly int capitalPieceColumnNum = 5;
  internal static readonly int capitalPieceCellNum = capitalPieceRowNum * capitalPieceColumnNum;
  internal static readonly string[] yearItems = ["春","夏","秋","冬"];
  internal static double CalcFullWidthTextLength(string str) => str.Length - str.Count("0123456789-.()".Contains) * 0.4 - str.Count(" ".Contains) * 0.8;
  internal static double CalcDataListElemWidth(double textlength) => BasicStyle.fontsize * textlength + dataMargin.Left + dataMargin.Right;
  internal static void SwitchViewMode() {
    viewMode = viewMode == ViewMode.fix ? ViewMode.fit : ViewMode.fix;
    SwitchViewModeActions.ForEach(v=>v());
#if __WASM__
    Uno.Foundation.WebAssemblyRuntime.InvokeJS($"window.parent.{viewMode}();");
#endif
  }
  internal static double GetScaleFactor(Windows.Foundation.Size size,double scaleLevel) => Math.Max(size.Width / mapSize.Width,(size.Height - scaleSliderHeight) / mapSize.Height) * Math.Pow(1.08,scaleLevel);
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
