using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record PersonListData(Brush Brush,ECountry? Country,string Name,string Role,int Rank,int BirthYear,string AppearYear,int DeathYear,string Biography);
internal record CountryListData(Brush Brush,ECountry Country,decimal Fund,string AreasText);
internal sealed partial class ParamList:UserControl {
  private enum SortButtonKind { 国役割別, ランク順, 生年順, 没年順 };
  internal ObservableCollection<PersonListData> personDataList1 = [];
  internal ObservableCollection<PersonListData> personDataList2 = [];
  internal ObservableCollection<CountryListData> countryDataList1 = [];
  internal ObservableCollection<CountryListData> countryDataList2 = [];
  internal ParamList(Grid contentGrid) {
    InitializeComponent();
    MyInit(this,contentGrid);
    void MyInit(ParamList page,Grid contentGrid) {
      SortButtonKind initSortKind = SortButtonKind.国役割別;
      Dictionary<SortButtonKind,Func<Dictionary<PersonId,PersonData>,Dictionary<PersonId,PersonData>>> buttonActionMap = new([
        new(SortButtonKind.国役割別,v =>v.OrderBy(v => v.Value.Country).ThenBy(v => v.Value.Role).ThenBy(v => v.Value.BirthYear).ToDictionary()),
        new(SortButtonKind.ランク順,v =>v.OrderByDescending(v => v.Value.Rank).ThenBy(v => v.Value.BirthYear).ToDictionary()),
        new(SortButtonKind.生年順,v =>v.OrderBy(v => v.Value.BirthYear).ToDictionary()),
        new(SortButtonKind.没年順,v =>v.OrderBy(v => v.Value.DeathYear).ToDictionary())
      ]);
      page.SizeChanged += (_,_) => ResizeElem(page,UIUtil.GetScaleFactor(contentGrid.RenderSize,Game.scaleLevel));
      SetUIElements(page);
      void SetUIElements(ParamList page) {
        page.PersonDataScenarioName1.Text = BaseData.scenarios.ElementAtOrDefault(0)?.Value;
        page.PersonDataScenarioName2.Text = BaseData.scenarios.ElementAtOrDefault(1)?.Value;
        page.SortButtonPanel1.MySetChildren([.. CreateSortButtons(page,0)]);
        page.SortButtonPanel2.MySetChildren([.. CreateSortButtons(page,1)]);
        RefreshSortButtonPanelColor(page.SortButtonPanel1,initSortKind);
        RefreshSortButtonPanelColor(page.SortButtonPanel2,initSortKind);
        (Scenario.scenarios.Values.ElementAtOrDefault(0)?.MyApplyF(v=>GetPersonListData(v,initSortKind)) ?? []).ForEach(personDataList1.Add);
        (Scenario.scenarios.Values.ElementAtOrDefault(1)?.MyApplyF(v=>GetPersonListData(v,initSortKind)) ?? []).ForEach(personDataList2.Add);
        (Scenario.scenarios.Values.ElementAtOrDefault(0)?.MyApplyF(v=>GetCountryListData(v)) ?? []).ForEach(countryDataList1.Add);
        (Scenario.scenarios.Values.ElementAtOrDefault(1)?.MyApplyF(v=>GetCountryListData(v)) ?? []).ForEach(countryDataList2.Add);
        List<Button> CreateSortButtons(ParamList page,int scenarioNo) {
          return [.. buttonActionMap.Keys.Select(v => CreateSortButton(page,scenarioNo,v))];
          Button CreateSortButton(ParamList page,int scenarioNo,SortButtonKind buttonKind) {
            return new Button { MaxWidth = 300,Height = 50,Margin = new Thickness(5,0) }.MySetChild(new TextBlock { Text = buttonKind.ToString() }).MyApplyA(v => v.Click += (_,_) => {
              (scenarioNo switch { 0 => page.SortButtonPanel1, _ => page.SortButtonPanel2 }).MyApplyA(panel => RefreshSortButtonPanelColor(panel,buttonKind));
              (scenarioNo switch { 0 => personDataList1, _ => personDataList2 }).MyApplyA(v=>v.Clear()).MyApplyA(v=>(Scenario.scenarios.Values.ElementAtOrDefault(scenarioNo)?.MyApplyF(v => GetPersonListData(v,buttonKind)) ?? []).ForEach(v.Add));
            });
          }
        }
        void RefreshSortButtonPanelColor(StackPanel buttonPanel,SortButtonKind buttonKind) {
          buttonActionMap.Keys.MyGetIndex(v => v == buttonKind)?.MyApplyA(index => RefreshColor([.. buttonPanel.Children.OfType<Button>()],index));
          static void RefreshColor(List<Button> elems,int index) => elems.Select((button,index) => (button, index)).ToList().ForEach(v => { v.button.Background = v.index == index ? Colors.LightGray : Colors.WhiteSmoke; });
        }
         List<PersonListData> GetPersonListData(ScenarioData scenario,SortButtonKind buttonKind) {
          return [.. buttonActionMap.GetValueOrDefault(buttonKind)?.Invoke(scenario.PersonMap.ToDictionary()).Select(ToPersonListItem) ?? []];
          PersonListData ToPersonListItem(KeyValuePair<PersonId,PersonData> personInfo) {
            return new PersonListData((scenario.CountryMap.GetValueOrDefault(personInfo.Value.Country)?.ViewColor ?? UIUtil.transparentColor).ToBrush(),personInfo.Value.Country,personInfo.Key.Value,Code.Text.RoleToText(personInfo.Value.Role,Lang.ja),/*new SvgImageSource(new($"ms-appx:///Assets/Svg/{personInfo.Value.Role}.svg")),*/personInfo.Value.Rank,personInfo.Value.BirthYear,Person.GetAppearYear(personInfo.Value).MyApplyF(appearYear => appearYear >= scenario.StartYear ? appearYear.ToString() : "登場"),personInfo.Value.DeathYear,BaseData.BiographyMap.GetValueOrDefault(personInfo.Key)??string.Empty);
          }
        }
        List<CountryListData> GetCountryListData(ScenarioData scenario) {
          return [.. scenario.CountryMap.Select(ToCountryListItem)];
          CountryListData ToCountryListItem(KeyValuePair<ECountry,CountryData> countryInfo) {
            return new CountryListData((countryInfo.Value.ViewColor ?? UIUtil.transparentColor).ToBrush(),countryInfo.Key,countryInfo.Value.Fund,string.Join(",",scenario.AreaMap.Where(v => v.Value.Country == countryInfo.Key).Select(v => v.Key.ToString())));
          }
        }
      }
      static void ResizeElem(ParamList page,double scaleFactor) {
        double pageWidth = page.RenderSize.Width;
        double contentWidth = pageWidth / scaleFactor - 5;
        page.Scroll.Width = pageWidth;
        page.ContentPanel.Width = contentWidth;
        page.ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
        page.PersonListView1.Width = contentWidth;
        page.PersonListView2.Width = contentWidth;
        page.CountryListView1.Width = contentWidth;
        page.CountryListView2.Width = contentWidth;
        page.SortButtonPanel1.Children.OfType<Button>().ToList().ForEach(v => v.Width = contentWidth / 4 - 5 * 2);
        page.SortButtonPanel2.Children.OfType<Button>().ToList().ForEach(v => v.Width = contentWidth / 4 - 5 * 2);
        page.ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),page.ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
      }
    }
  }
}