using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record WinCondData(Brush Brush,ECountry Country,string WinCondText);
internal sealed partial class WinCond:UserControl {
  private const double listviewWidth = 750;
  private const double listviewHeight = 350;
  private readonly List<ObservableCollection<WinCondData>> winCondDataList = [[],[]];
  internal WinCond() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      List<TextBlock> WinCondScenarioNames = [WinCondScenarioName1,WinCondScenarioName2];
      SizeChanged += (_,_) => ResizeElem();
      SetUIElements();
      void SetUIElements() {
        WinCondScenarioNames.ForEachWithIndex((v,i) => v.Text = ScenarioBase.GetScenarioId(i)?.Value);
        winCondDataList.ForEachWithIndex((v,i) => ScenarioBase.GetScenarioId(i)?.MyApplyF(ScenarioBase.GetScenarioData)?.MyApplyF(scenario => GetWinCondListData(scenario)).ForEach(v.Add));
        List<WinCondData> GetWinCondListData(ScenarioData scenario) {
          return [.. scenario.CountryMap.Select(ToCountryListItem)];
          WinCondData ToCountryListItem(KeyValuePair<ECountry,CountryData> countryInfo) {
            WinCondMessage? message = scenario.WinConditionMap.GetValueOrDefault(countryInfo.Key)?.Messages;
            string winCondMessage = message.MyApplyF(v => string.Join('\n',new[] { v?.Basic?.MyApplyF(v => string.Join('＆',v)),v?.Extra?.MyApplyF(v => string.Join(' ',v)) }.MyNonNull()));
            return new WinCondData(countryInfo.Value.ViewColor.ToBrush(),countryInfo.Key,winCondMessage);
          }
        }
      }
      void ResizeElem() {
        double scaleFactor = UIUtil.GetScaleFactor(RenderSize with { Height = 0 });
        double contentWidth = RenderSize.Width / scaleFactor - 5;
        ContentPanel.Width = contentWidth;
        ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
        ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
        Scroll.Width = RenderSize.Width;
      }
    }
  }
}