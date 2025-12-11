using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Text = HisouSangokushiZero2_1_Uno.Code.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record WinCondData(Brush Brush,ECountry Country,string WinCondText);
internal sealed partial class WinCond:UserControl {
  private const double listviewWidth = 750;
  private const double listviewHeight = 350;
  private static readonly List<ObservableCollection<WinCondData>> winCondDataList = [[],[]];
  private static UIElement? parent = null;
  internal WinCond() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      AttachEvent();
      SetUIElements();
      void AttachEvent() {
        SizeChanged += (_,_) => parent?.MyApplyA(ResizeElem);
        void ResizeElem(UIElement parent) {
          double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
          double contentWidth = RenderSize.Width / scaleFactor - 5;
          ContentPanel.Width = contentWidth;
          ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
          ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
        }
      }
      void SetUIElements() {
        List<TextBlock> WinCondScenarioNames = [WinCondScenarioName1,WinCondScenarioName2];
        WinCondScenarioNames.ForEachWithIndex((v,i) => v.Text = ScenarioBase.GetScenarioId(i)?.Value);
        winCondDataList.ForEachWithIndex((v,i) => ScenarioBase.GetScenarioId(i)?.MyApplyF(ScenarioBase.GetScenarioData)?.MyApplyF(scenario => GetWinCondListData(scenario)).ForEach(v.Add));
        List<WinCondData> GetWinCondListData(ScenarioData scenario) {
          return [.. scenario.CountryMap.Select(ToCountryListItem)];
          WinCondData ToCountryListItem(KeyValuePair<ECountry,CountryData> countryInfo) {
            WinCondMessage? maybeWinCondMessage = scenario.WinConditionMap.GetValueOrDefault(countryInfo.Key)?.Messages;
            string?[] maybeWinCondText = [maybeWinCondMessage?.Basic?.MyApplyF(v => string.Join('＆',v)) ?? Text.NoBasicWinCondText(Lang.ja),maybeWinCondMessage?.Extra?.MyApplyF(v => string.Join(' ',v))];
            string winCondMessage = maybeWinCondMessage.MyApplyF(v => string.Join('\n',maybeWinCondText.MyNonNull()));
            return new WinCondData(countryInfo.Value.ViewColor.ToBrush(),countryInfo.Key,winCondMessage);
          }
        }
      }
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
}