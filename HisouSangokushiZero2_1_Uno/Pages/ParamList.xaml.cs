using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class ParamList:UserControl {
  internal ParamList() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(ParamList page) {
      SetUIElements(page);
      static void SetUIElements(ParamList page) {
        page.PersonDataScenarioName1.Text = BaseData.scenarios.ElementAtOrDefault(0)?.Value;
        page.PersonDataListPanel1.MySetChildren([.. UIUtil.CreatePersonDataList(0,12)]);
        page.CountryDataListPanel1.MySetChildren([.. UIUtil.CreateCountryDataList(0,2)]);
        page.PersonDataScenarioName2.Text = BaseData.scenarios.ElementAtOrDefault(1)?.Value;
        page.PersonDataListPanel2.MySetChildren([.. UIUtil.CreatePersonDataList(1,12)]);
        page.CountryDataListPanel2.MySetChildren([.. UIUtil.CreateCountryDataList(1,1)]);
      }
    }
  }
  internal static void ResizeElem(ParamList page,double scaleFactor) {
    double pageWidth = page.RenderSize.Width;
    double contentWidth = pageWidth / scaleFactor - 5;
    page.Scroll.Width = pageWidth;
    page.ContentPanel.Width = contentWidth;
    page.ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
    ResizeElem(contentWidth,page.CountryDataListPanel1);
    ResizeElem(contentWidth,page.CountryDataListPanel2);
    page.PersonDataListPanel1.Width = contentWidth;
    page.PersonDataListPanel2.Width = contentWidth;
    page.PersonDataListPanel1.Measure(new(contentWidth,double.PositiveInfinity));
    page.PersonDataListPanel2.Measure(new(contentWidth,double.PositiveInfinity));
    page.ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),page.ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
    static void ResizeElem(double width,Panel countryDataListPanel) {
      countryDataListPanel.Children.OfType<Panel>().SelectMany(v => v.Children.OfType<Panel>()).ToList().ForEach(panel => (panel.Children.Last() as FrameworkElement)?.MyApplyA(v => v.MaxWidth = width - panel.Children.SkipLast(1).OfType<FrameworkElement>().Sum(v => v.Width) - 10));
    }
  }
}