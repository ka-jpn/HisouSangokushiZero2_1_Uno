using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno;
public sealed partial class PersonData:UserControl {
  public PersonData(){
    InitializeComponent();
    MyInit(this);
  }
  private static void MyInit(PersonData page) {
    SetUIElements(page);
    page.Measure(new(double.PositiveInfinity,double.PositiveInfinity));
    page.Width = 1124;
    page.Height = page.DesiredSize.Height;
  }
  private static void SetUIElements(PersonData page) {
    page.PersonDataScenarioName1.Text = GameInfo.scenarios.ElementAtOrDefault(0)?.Value;
    page.PersonDataListPanel1.MySetChildren([.. UIUtil.CreatePersonDataList(0,3,12)]).MyApplyA(v => { v.BorderThickness = new(UIUtil.dataListFrameWidth); v.BorderBrush = new SolidColorBrush(UIUtil.dataListFrameColor); });
    page.CountryDataListPanel1.MySetChildren([.. UIUtil.CreateCountryDataList(0,2)]).MyApplyA(v => { v.BorderThickness = new(UIUtil.dataListFrameWidth); v.BorderBrush = new SolidColorBrush(UIUtil.dataListFrameColor); });
    page.PersonDataScenarioName2.Text = GameInfo.scenarios.ElementAtOrDefault(1)?.Value;
    page.PersonDataListPanel2.MySetChildren([.. UIUtil.CreatePersonDataList(1,3,12)]).MyApplyA(v => { v.BorderThickness = new(UIUtil.dataListFrameWidth); v.BorderBrush = new SolidColorBrush(UIUtil.dataListFrameColor); });
    page.CountryDataListPanel2.MySetChildren([.. UIUtil.CreateCountryDataList(1,2)]).MyApplyA(v => { v.BorderThickness = new(UIUtil.dataListFrameWidth); v.BorderBrush = new SolidColorBrush(UIUtil.dataListFrameColor); });
  }
}