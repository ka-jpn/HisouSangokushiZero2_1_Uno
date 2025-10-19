using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record WinCondData(Brush Brush,ECountry Country,string? WinCondText1,string? WinCondText2);
internal sealed partial class WinCond:UserControl {
  internal ObservableCollection<WinCondData> winCondDataList1 = [];
  internal ObservableCollection<WinCondData> winCondDataList2 = [];
  internal WinCond(Grid contentGrid) {
    InitializeComponent();
    MyInit(this,contentGrid);
    void MyInit(WinCond page,Grid contentGrid) {
      page.SizeChanged += (_,_) => ResizeElem(page,UIUtil.GetScaleFactor(contentGrid.RenderSize,Game.scaleLevel));
      SetUIElements(page);
      void SetUIElements(WinCond page) {
        page.WinCondScenarioName1.Text = BaseData.scenarios.ElementAtOrDefault(0)?.Value;
        page.WinCondScenarioName2.Text = BaseData.scenarios.ElementAtOrDefault(1)?.Value;
        (Scenario.scenarios.Values.ElementAtOrDefault(0)?.MyApplyF(v => GetWinCondListData(v)) ?? []).ForEach(winCondDataList1.Add);
        (Scenario.scenarios.Values.ElementAtOrDefault(1)?.MyApplyF(v => GetWinCondListData(v)) ?? []).ForEach(winCondDataList2.Add);
        List<WinCondData> GetWinCondListData(ScenarioData scenario) {
          return [.. scenario.CountryMap.Select(ToCountryListItem)];
          WinCondData ToCountryListItem(KeyValuePair<ECountry,CountryData> countryInfo) {
            return new WinCondData(
              countryInfo.Value.ViewColor.ToBrush(),
              countryInfo.Key,
              scenario.WinConditionMap.GetValueOrDefault(countryInfo.Key)?.Messages.ElementAtOrDefault(0)?.MyApplyF(v => string.Join('＆',v)),
              scenario.WinConditionMap.GetValueOrDefault(countryInfo.Key)?.Messages.ElementAtOrDefault(1)?.MyApplyF(v => string.Join(' ',v))
            );
          }
        }
      }
      static void ResizeElem(WinCond page,double scaleFactor) {
        double pageWidth = page.RenderSize.Width;
        double contentWidth = pageWidth / scaleFactor - 5;
        page.Scroll.Width = pageWidth;
        page.ContentPanel.Width = contentWidth;
        page.ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
        page.ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),page.ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
      }
    }
  }
}