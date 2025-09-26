using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class ParamList:UserControl {
  private static readonly Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>> Country_Role_BirthYear_SortFunc = v => v.OrderBy(v => v.Value.Country).ThenBy(v => v.Value.Role).ThenBy(v => v.Value.BirthYear).ToDictionary();
  private static readonly Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>> Rank_BirthYear_SortFunc = v => v.OrderByDescending(v => v.Value.Rank).ThenBy(v => v.Value.BirthYear).ToDictionary();
  private static readonly Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>> BirthYear_SortFunc = v => v.OrderBy(v => v.Value.BirthYear).ToDictionary();
  private static readonly Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>> DeathYear_SortFunc = v => v.OrderBy(v => v.Value.DeathYear).ToDictionary();
  private static readonly Dictionary<string,Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>>> buttonInfoList = new([new("国役割別",Country_Role_BirthYear_SortFunc),new("ランク順",Rank_BirthYear_SortFunc),new("生年順",BirthYear_SortFunc),new("没年順",DeathYear_SortFunc)]);
  private static readonly Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>> InitSortFunc = Country_Role_BirthYear_SortFunc;
  internal ParamList() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(ParamList page) {
      SetUIElements(page);
      static void SetUIElements(ParamList page) {
        page.PersonDataScenarioName1.Text = BaseData.scenarios.ElementAtOrDefault(0)?.Value;
        page.PersonDataListPanel1.MySetChildren([.. UIUtil.CreatePersonDataList(0,12,InitSortFunc)]);
        page.CountryDataListPanel1.MySetChildren([.. UIUtil.CreateCountryDataList(0,2)]);
        page.PersonDataScenarioName2.Text = BaseData.scenarios.ElementAtOrDefault(1)?.Value;
        page.PersonDataListPanel2.MySetChildren([.. UIUtil.CreatePersonDataList(1,12,InitSortFunc)]);
        page.CountryDataListPanel2.MySetChildren([.. UIUtil.CreateCountryDataList(1,1)]);
        page.SortButtonPanel1.MySetChildren([.. CreateSortButtons(page,0)]);
        page.SortButtonPanel2.MySetChildren([.. CreateSortButtons(page,1)]);
        RefreshSortButtonPanelColor(page.SortButtonPanel1,InitSortFunc);
        RefreshSortButtonPanelColor(page.SortButtonPanel2,InitSortFunc);
        static List<Button> CreateSortButtons(ParamList page,int scenarioNo) {
          return [.. buttonInfoList.Select(v => CreateSortButton(page,scenarioNo,v.Key,v.Value))];
          static Button CreateSortButton(ParamList page,int scenarioNo,string text,Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>> sortFunc) {
            return new Button { MaxWidth = 300,Height = 50,Margin=new Thickness(5,0) }.MySetChild(new TextBlock { Text = text }).MyApplyA(v => v.Click += (_,_) => {
              (scenarioNo switch { 0 => page.SortButtonPanel1, _ => page.SortButtonPanel2 }).MyApplyA(panel => RefreshSortButtonPanelColor(panel,sortFunc));
              (scenarioNo switch { 0 => page.PersonDataListPanel1, _ => page.PersonDataListPanel2 }).MySetChildren([.. UIUtil.CreatePersonDataList(scenarioNo,12,sortFunc)]);
            });
          }
        }
        static void RefreshSortButtonPanelColor(StackPanel buttonPanel,Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>> sortFunc) {
          buttonInfoList.MyGetIndex(v => v.Value == sortFunc)?.MyApplyA(index => RefreshColor([.. buttonPanel.Children.OfType<Button>()],index));
          static void RefreshColor(List<Button> elems,int index) => elems.Select((button,index) => (button, index)).ToList().ForEach(v => { v.button.Background = v.index == index ? Colors.LightGray : Colors.WhiteSmoke; });
        }
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
    page.SortButtonPanel1.Children.OfType<Button>().ToList().ForEach(v => v.Width = contentWidth / 4 - 5 * 2);
    page.SortButtonPanel2.Children.OfType<Button>().ToList().ForEach(v => v.Width = contentWidth / 4 - 5 * 2);
    static void ResizeElem(double width,Panel countryDataListPanel) {
      countryDataListPanel.Children.OfType<Panel>().SelectMany(v => v.Children.OfType<Panel>()).ToList().ForEach(panel => (panel.Children.Last() as FrameworkElement)?.MyApplyA(v => v.MaxWidth = width - panel.Children.SkipLast(1).OfType<FrameworkElement>().Sum(v => v.Width) - 10));
    }
  }
}