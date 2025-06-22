using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class PersonData:UserControl {
  internal PersonData() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(PersonData page) {
      SetUIElements(page);
      page.Measure(new(double.PositiveInfinity,double.PositiveInfinity));
      page.Width = 1124;
      page.Height = page.DesiredSize.Height;
      static void SetUIElements(PersonData page) {
        page.PersonDataScenarioName1.Text = BaseData.scenarios.ElementAtOrDefault(0)?.Value;
        page.PersonDataListPanel1.MySetChildren([.. UIUtil.CreatePersonDataList(0,3,12)]).MyApplyA(v => { v.BorderThickness = UIUtil.dataListFrameThickness; v.BorderBrush = UIUtil.dataListFrameBrush; });
        page.CountryDataListPanel1.MySetChildren([.. UIUtil.CreateCountryDataList(0,2)]).MyApplyA(v => { v.BorderThickness = UIUtil.dataListFrameThickness; v.BorderBrush = UIUtil.dataListFrameBrush; });
        page.PersonDataScenarioName2.Text = BaseData.scenarios.ElementAtOrDefault(1)?.Value;
        page.PersonDataListPanel2.MySetChildren([.. UIUtil.CreatePersonDataList(1,3,12)]).MyApplyA(v => { v.BorderThickness = UIUtil.dataListFrameThickness; v.BorderBrush = UIUtil.dataListFrameBrush; });
        page.CountryDataListPanel2.MySetChildren([.. UIUtil.CreateCountryDataList(1,2)]).MyApplyA(v => { v.BorderThickness = UIUtil.dataListFrameThickness; v.BorderBrush = UIUtil.dataListFrameBrush; });
      }
    }
  }
}