using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Uno.Extensions.Specialized;
using Windows.UI;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using Person = HisouSangokushiZero2_1_Uno.Code.DefType.Person;
using Point = HisouSangokushiZero2_1_Uno.Code.DefType.Point;
using Post = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
namespace HisouSangokushiZero2_1_Uno;
public sealed partial class MainPage:Page {
  internal enum ViewMode { fit, fix };
  internal enum InfoPanelState { Explain, WinCond, PersonData, ChangeLog, Setting };
  private static readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();
  internal static readonly double fixModeMaxWidth = 1000;
  internal static readonly Size mapSize = new(2000,1750);
  private static readonly Point mapGridCount = new(9,10);
  internal static readonly GridLength infoFrameWidth = new(50);
  private static readonly Size areaSize = new(204,155);
  private static readonly CornerRadius areaCornerRadius = new(30);
  private static readonly Size personPutSize = new(99,70);
  internal static readonly double countryPersonPutPanelHeight = (personPutSize.Height + postFrameWidth * 4) * 4 + postFrameWidth * 4 * 2 + BasicStyle.textHeight;
  internal static readonly double personRankFontScale = 1.5;
  internal static readonly double personNameFontScale = 1.75;
  internal static readonly double infoTextWidth = BasicStyle.fontsize * 55;
  private static readonly Color landRoadColor = Color.FromArgb(150,120,120,50);
  private static readonly Color waterRoadColor = Color.FromArgb(150,50,50,150);
  internal static readonly double attackJudgePointSize = 10;
  private static readonly double postFrameWidth = 1;
  internal static readonly double dataListFrameWidth = 1;
  internal static readonly Color dataListFrameColor = Color.FromArgb(255,150,150,150);
  private static GameState game = GetGame.GetInitGameScenario(null);
  public MainPage() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(MainPage page) {
      UI.SetUIElements(page);
      UI.AttachEvent(page);
      UI.RefreshViewMode(page);
      UI.SwitchInfoButton(page,InfoPanelState.Explain);
      UI.UpdateCountryPostPanelZIndex(page,ERole.central);
    }
  }
  static StackPanel[] CreateWinCondList(int scenarioNo,int chunkBlockNum) {
    ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
    Dictionary<ECountry,CountryInfo>[] chunkedCountryInfoMaps = maybeScenarioInfo?.CountryMap.MyApplyF(elems => elems.OrderBy(v => v.Key).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
    return maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedCountryInfoMaps.Select(chunkedCountryInfoMap => CreateWinCondPanel(scenarioInfo,chunkedCountryInfoMap)).ToArray()) ?? [];
    static StackPanel CreateWinCondPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<ECountry,CountryInfo> includeCountryInfoMap) {
      return new StackPanel { Background = new SolidColorBrush(dataListFrameColor) }.MySetChildren([
        CreateCountryDataLine(
          Color.FromArgb(255,240,240,240),
          new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="勝利条件",HorizontalAlignment=HorizontalAlignment.Left }
        ), .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
          scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewColor??Colors.Transparent,
          new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=string.Join(' ',countryInfo.Value.WinConditionMessageFunc()),HorizontalAlignment=HorizontalAlignment.Left,TextWrapping=TextWrapping.Wrap}
        ))
      ]);
      static StackPanel CreateCountryDataLine(Color backColor,UIElement countryNameElem,UIElement winCondElem) {
        return new StackPanel { Orientation = Orientation.Horizontal,Background = new SolidColorBrush(backColor),}.MySetChildren([
          new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryNameElem),
          new Border{ Width=CalcElemWidth(45),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(winCondElem)
        ]);
      }
    }
  }
  static StackPanel[] CreatePersonDataList(int scenarioNo,int chunkBlockBlockSize,int chunkBlockNum) {
    ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
    Dictionary<Person,PersonParam>[] chunkedPersonInfoMaps = maybeScenarioInfo?.PersonMap.MyApplyF(elems => elems.OrderBy(v => v.Value.Country).ThenBy(v => v.Value.Role).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
    StackPanel[] personDataBlock = maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedPersonInfoMaps.Select(chunkedPersonInfoMap => CreatePersonDataPanel(scenarioInfo,chunkedPersonInfoMap)).ToArray()) ?? [];
    return [.. personDataBlock.Chunk(chunkBlockBlockSize).Select(v => new StackPanel() { Orientation = Orientation.Horizontal }.MySetChildren([.. v]))];
    static StackPanel CreatePersonDataPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<Person,PersonParam> includePersonInfoMap) {
      return new StackPanel { Background = new SolidColorBrush(dataListFrameColor) }.MySetChildren([
        CreatePersonDataLine(
          Color.FromArgb(255,240,240,240),
          new TextBlock { Text="陣営",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="人物名",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="ロール",Margin=new(0,0,-BasicStyle.fontsize*3*0.5,0),RenderTransform=new ScaleTransform() { ScaleX=0.5 } },
          new TextBlock { Text="ランク",Margin=new(0,0,-BasicStyle.fontsize*3*0.5,0),RenderTransform=new ScaleTransform() { ScaleX=0.5 } },
          new TextBlock { Text="生年",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="登場",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="没年",HorizontalAlignment=HorizontalAlignment.Center }
        ), .. includePersonInfoMap.Select(personInfo => CreatePersonDataLine(
          scenarioInfo.CountryMap.GetValueOrDefault(personInfo.Value.Country)?.ViewColor??Colors.Transparent,
          new TextBlock { Text=personInfo.Value.Country.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Key.Value,Margin=new(0,0,-BasicStyle.fontsize*(UI.CalcFullWidthLength(personInfo.Key.Value)-3),0),RenderTransform=new ScaleTransform() { ScaleX=Math.Min(1,3/UI.CalcFullWidthLength(personInfo.Key.Value)) } },
          new Image{ Source=new SvgImageSource(new($"ms-appx:///Assets/Svg/{personInfo.Value.Role}.svg")),Width=BasicStyle.fontsize,Height=BasicStyle.fontsize,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center },
          new TextBlock { Text=personInfo.Value.Rank.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Value.BirthYear.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=Code.Person.GetAppearYear(personInfo.Value).MyApplyF(appearYear=>appearYear>=scenarioInfo.StartYear?appearYear.ToString():"登場"),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=personInfo.Value.DeathYear.ToString(),HorizontalAlignment=HorizontalAlignment.Center }
        ))
      ]);
      static StackPanel CreatePersonDataLine(Color backColor,UIElement countryNameElem,UIElement personNameElem,UIElement personRoleElem,UIElement personRankElem,UIElement personBirthYearElem,UIElement personAppearYearElem,UIElement personDeathYearElem) {
        return new StackPanel { Orientation = Orientation.Horizontal,Background = new SolidColorBrush(backColor),}.MySetChildren([
          new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryNameElem),
          new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personNameElem),
          new Border{ Width=CalcElemWidth(1.5),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personRoleElem),
          new Border{ Width=CalcElemWidth(1.5),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personRankElem),
          new Border{ Width=CalcElemWidth(2),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personBirthYearElem),
          new Border{ Width=CalcElemWidth(2),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personAppearYearElem),
          new Border{ Width=CalcElemWidth(2),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(personDeathYearElem),
        ]);
      }
    }
  }
  static StackPanel[] CreateCountryDataList(int scenarioNo,int chunkBlockNum) {
    ScenarioData.ScenarioInfo? maybeScenarioInfo = ScenarioData.scenarios.MyNullable().ElementAtOrDefault(scenarioNo)?.Value;
    Dictionary<ECountry,CountryInfo>[] chunkedCountryInfoMaps = maybeScenarioInfo?.CountryMap.MyApplyF(elems => elems.OrderBy(v => v.Key).Chunk((int)Math.Ceiling((double)elems.Count / chunkBlockNum))).Select(v => v.ToDictionary()).ToArray() ?? [];
    return maybeScenarioInfo?.MyApplyF(scenarioInfo => chunkedCountryInfoMaps.Select(chunkedCountryInfoMap => CreateCountryDataPanel(scenarioInfo,chunkedCountryInfoMap)).ToArray()) ?? [];
    static StackPanel CreateCountryDataPanel(ScenarioData.ScenarioInfo scenarioInfo,Dictionary<ECountry,CountryInfo> includeCountryInfoMap) {
      return new StackPanel { Background = new SolidColorBrush(dataListFrameColor) }.MySetChildren([
        CreateCountryDataLine(
          Color.FromArgb(255,240,240,240),
          new TextBlock { Text="陣営名",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="資金",HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text="所属エリア",HorizontalAlignment=HorizontalAlignment.Center }
        ), .. includeCountryInfoMap.Select(countryInfo => CreateCountryDataLine(
          scenarioInfo.CountryMap.GetValueOrDefault(countryInfo.Key)?.ViewColor??Colors.Transparent,
          new TextBlock { Text=countryInfo.Key.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=countryInfo.Value.Fund.ToString(),HorizontalAlignment=HorizontalAlignment.Center },
          new TextBlock { Text=string.Join(",",scenarioInfo.AreaMap.Where(v=>v.Value.Country==countryInfo.Key).Select(v=>v.Key.ToString())),HorizontalAlignment=HorizontalAlignment.Left }
        ))
      ]);
      static StackPanel CreateCountryDataLine(Color backColor,UIElement countryNameElem,UIElement countryFundElem,UIElement countryAreasElem) {
        return new StackPanel { Orientation = Orientation.Horizontal,Background = new SolidColorBrush(backColor),}.MySetChildren([
          new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryNameElem),
          new Border{ Width=CalcElemWidth(3),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryFundElem),
          new Border{ Width=CalcElemWidth(42),BorderThickness=new(dataListFrameWidth),BorderBrush=new SolidColorBrush(dataListFrameColor) }.MySetChild(countryAreasElem),
        ]);
      }
    }
  }
  static double CalcElemWidth(double textlength) => BasicStyle.fontsize * textlength + dataListFrameWidth * 2;
  static void InitGame(MainPage page,Scenario? scenario) {
    game = GetGame.GetInitGameScenario(scenario);
    UpdateAreaUI(page,game);
    UpdateCountryPostPersons(page,game);
    UpdateCountryInfoPanel(page,game);
    UI.UpdateLogMessageUI(page,game);
    page.MapAnimationElementsCanvas.MySetChildren([]);
    page.ExPanel.MySetChildren([]);
    page.MovePersonCanvas.MySetChildren([]);
  }
  static void UpdateAreaUI(MainPage page,GameState game) {
    page.MapElementsCanvas.MySetChildren([.. game.NowScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.RoadConnections.Select(road => MaybeCreateRoadLine(game,road)).MyNonNull() ?? [],.. game.AreaMap.Select(info => CreateAreaPanel(page,game,info))]);
    static Line? MaybeCreateRoadLine(GameState game,ScenarioData.Road road) {
      return Area.GetAreaPoint(game,road.From,mapSize,areaSize,mapGridCount,infoFrameWidth.Value) is Point from && Area.GetAreaPoint(game,road.To,mapSize,areaSize,mapGridCount,infoFrameWidth.Value) is Point to ? CreateRoadLine(road,from,to) : null;
      static Line CreateRoadLine(ScenarioData.Road road,Point from,Point to) => new() { X1 = from.X,Y1 = from.Y,X2 = to.X,Y2 = to.Y,Stroke = new SolidColorBrush(road.Kind == RoadKind.land ? landRoadColor : waterRoadColor),StrokeThickness = 10 * Math.Pow(road.Easiness,1.7) / 2 + 20 };
    }
    static Grid CreateAreaPanel(MainPage page,GameState game,KeyValuePair<EArea,AreaInfo> info) {
      double capitalBorderWidth = 3.5;
      Grid areaPanel = new() { Width = areaSize.Width,Height = areaSize.Height,CornerRadius = areaCornerRadius };
      Border areaBorder = new() { Width = areaSize.Width,Height = areaSize.Height,BorderThickness = new(game.CountryMap.Values.Select(v => v.CapitalArea).Contains(info.Key)?capitalBorderWidth:0),CornerRadius = areaCornerRadius,BorderBrush = new SolidColorBrush(Colors.Red),Background = new SolidColorBrush(Country.GetCountryColor(game,info.Value.Country)) };
      Grid areaWrapPanel = new() { Width = areaSize.Width,Height = areaSize.Height,Background = Area.IsPlayerSelectable(game,info.Key) ? null :  new SolidColorBrush(Color.FromArgb(100,100,100,100))};
      StackPanel areaInnerPanel = new() { Width = areaSize.Width,VerticalAlignment = VerticalAlignment.Center };
      Area.GetAreaPoint(game,info.Key,mapSize,areaSize,mapGridCount,infoFrameWidth.Value)?.MyApplyA(v => Canvas.SetLeft(areaPanel,v.X - areaSize.Width / 2)).MyApplyA(v => Canvas.SetTop(areaPanel,v.Y - areaSize.Height / 2));
      areaPanel.PointerPressed += (_,_) => UI.PushArea.Push(info.Key);
      areaPanel.PointerExited += (_,_) => UI.PushArea.Exit();
      areaPanel.PointerReleased += (_,_) => MainPage.game = UI.PushArea.Release(page,MainPage.game,info);
      return areaPanel.MySetChildren([areaBorder,areaInnerPanel.MySetChildren(GetAreaElems(page,game,info)),areaWrapPanel]);
      static List<UIElement> GetAreaElems(MainPage page,GameState game,KeyValuePair<EArea,AreaInfo> areaInfo) => [
        new StackPanel { HorizontalAlignment=HorizontalAlignment.Center,Orientation=Orientation.Horizontal}.MySetChildren([
          new TextBlock { Text=areaInfo.Key.ToString() },
          new TextBlock { Text=$" {areaInfo.Value.Country?.ToString()??$"自治"}領" },
        ]),
        new StackPanel { HorizontalAlignment=HorizontalAlignment.Center,Orientation=Orientation.Horizontal,BorderBrush=new SolidColorBrush(UI.GetPostFrameColor(game,areaInfo.Key)),BorderThickness=new(postFrameWidth) }.MySetChildren([
          CreatePersonPutPanel(page,game,new(ERole.defense,new(areaInfo.Key)),game.PersonMap,"防"),CreatePersonPutPanel(page,game,new(ERole.affair,new(areaInfo.Key)),game.PersonMap,"政")
        ]),
        new StackPanel { HorizontalAlignment=HorizontalAlignment.Center,Orientation=Orientation.Horizontal}.MySetChildren([
          new TextBlock { Text=Math.Floor(areaInfo.Value.AffairParam.AffairNow).ToString() },
          new TextBlock { Text="/" },
          new TextBlock { Text=Math.Floor(areaInfo.Value.AffairParam.AffairsMax).ToString() },
        ]),
        new TextBlock { Text=areaInfo.Value.Country?.MyApplyF(country=>(Country.IsSleep(game,country)?$"休み {Country.GetSleepTurn(game,country)}":null)+(Country.IsFocusDefense(game,country)?"(防)":null)),TextAlignment=TextAlignment.Center }
      ];
    }
  }
  static void UpdateCountryPostPersons(MainPage page,GameState game) {
    UI.centralPanel.MySetChildren([CreatePersonHeadPostPanel(page,game,ERole.central),CreatePersonPostPanelElems(page,game,ERole.central)]);
    UI.affairPanel.MySetChildren([CreatePersonHeadPostPanel(page,game,ERole.affair),CreatePersonPostPanelElems(page,game,ERole.affair)]);
    UI.defensePanel.MySetChildren([CreatePersonHeadPostPanel(page,game,ERole.defense),CreatePersonPostPanelElems(page,game,ERole.defense)]);
    UI.attackPanel.MySetChildren([CreatePersonHeadPostPanel(page,game,ERole.attack),CreatePersonPostPanelElems(page,game,ERole.attack)]);
    static StackPanel CreatePersonHeadPostPanel(MainPage page,GameState game,ERole role) {
      Button autoPutPersonButton = new Button { Width = personPutSize.Width * 3,VerticalAlignment = VerticalAlignment.Stretch,Background = new SolidColorBrush(Color.FromArgb(100,100,100,100)) }.MyApplyA(v => v.Content = new TextBlock { Text = "オート配置" });
      autoPutPersonButton.Click += (_,_) => MainPage.game = AutoPutPersonButtonClick(MainPage.game);
      return new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
        new StackPanel { Orientation=Orientation.Horizontal,BorderBrush=new SolidColorBrush(UI.GetPostFrameColor(game,null)),BorderThickness=new(postFrameWidth) }.MySetChildren([
        CreatePersonPutPanel(page,game,new(role,new(PostHead.main)),game.PersonMap.Where(v => v.Value.Country==game.PlayCountry).ToDictionary(),"筆頭"),
        CreatePersonPutPanel(page,game,new(role,new(PostHead.sub)),game.PersonMap.Where(v => v.Value.Country==game.PlayCountry).ToDictionary(),"次席"),
      ]),
      autoPutPersonButton
      ]);
      GameState AutoPutPersonButtonClick(GameState game) => game.PlayCountry?.MyApplyF(country => Code.Post.GetAutoPutPost(game,country,role)).MyApplyF(postMap => UpdateGame.SetPersonPost(game,postMap)).MyApplyA(v => UpdateAreaUI(page,v)).MyApplyA(game => UpdateCountryPostPersons(page,game)) ?? game;
    }
    static StackPanel CreatePersonPostPanelElems(MainPage page,GameState game,ERole role) {
      return new StackPanel { BorderBrush = new SolidColorBrush(UI.GetPostFrameColor(game,null)),BorderThickness = new(postFrameWidth) }.MySetChildren([.. Enumerable.Range(0,BaseData.capitalPieceRowNum).Select(row => GetPersonPostLinePanel(page,game,role,row,game.PersonMap.Where(v => v.Value.Country == game.PlayCountry).ToDictionary()))]);
      static StackPanel GetPersonPostLinePanel(MainPage page,GameState game,ERole role,int rowNo,Dictionary<Person,PersonParam> personMap) => new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([.. Enumerable.Range(0,BaseData.capitalPieceColumnNum).Select(i => CreatePersonPutPanel(page,game,new(role,new(rowNo * BaseData.capitalPieceColumnNum + i)),personMap,(rowNo * BaseData.capitalPieceColumnNum + i + 1).ToString()))]);
    }
  }
  static void UpdateCountryInfoPanel(MainPage page,GameState game) {
    Button nextPhaseButton = new Button() { HorizontalAlignment = HorizontalAlignment.Stretch,Background = new SolidColorBrush(Color.FromArgb(100,100,100,100)),Height = BasicStyle.textHeight * 2.5 }.MySetChild(new TextBlock { Text = Code.Text.EndPhaseButtonText(game.Phase,Lang.ja) });
    nextPhaseButton.Click += (_,_) => {
      MainPage.game = MainPage.game.MyApplyF(game => game.Phase switch {
        Phase.Planning => EndPlanningPhase(page,game),
        Phase.Execution => EndExecutionPhase(page,game),
        _ => game
      }).MyApplyA(game => UpdateCountryInfoPanel(page,game));
    };
    page.CountryInfoContentsPanel.MySetChildren(game.Phase switch {
      Phase.Starting => ShowSelectScenario(page,game),
      Phase.Planning or Phase.Execution => ShowCountryInfo(game,nextPhaseButton),
      Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => ShowEndGameInfo(page,game)
    });
    static List<UIElement> ShowSelectScenario(MainPage page,GameState game) => [
      new StackPanel{ Height=BasicStyle.textHeight*2 },
      new TextBlock { Text="シナリオ",TextAlignment=TextAlignment.Center },
      new ComboBox {
        Height=BasicStyle.textHeight*2.5,
        HorizontalAlignment=HorizontalAlignment.Center,
        SelectedIndex=GameInfo.scenarios.MyGetIndex(v => v==game.NowScenario)??0,
        Foreground=new SolidColorBrush(Colors.Black),
        Background=new SolidColorBrush(Colors.White),
        Padding=new(20,0,10,0),
        ItemContainerStyle = new Style(typeof(ComboBoxItem)).MyApplyA(style=>style.Setters.Add(new Setter(FontSizeProperty,BasicStyle.fontsize*UI.GetScaleFactor(page)))),
      }.MyApplyA(elem => GameInfo.scenarios.Select(v =>v.Value).ToList().ForEach(elem.Items.Add)).MyApplyA(v=>v.SelectionChanged+=(_,_) => (v.SelectedItem as string)?.MyApplyA(text => InitGame(page,new(text)))),
      new TextBlock { Text=game.NowScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.MyApplyF(v=>$"{v.StartYear}年開始{v.EndYear}年終了"),TextAlignment=TextAlignment.Center },
      new StackPanel{ Height=BasicStyle.textHeight*2 },
      new TextBlock{ Text=$"プレイ勢力を選択後",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"情報が表示されます",TextAlignment=TextAlignment.Center },
      new StackPanel{ Height=BasicStyle.textHeight*2 }
    ];
    static List<UIElement> ShowCountryInfo(GameState game,Button nextPhaseButton) => [
      new TextBlock{ Text=Turn.GetCalendarText(game),TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"プレイ勢力:{game.PlayCountry}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"首都:{game.PlayCountry?.MyApplyF(country=>Area.GetCapitalArea(game,country))}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"資金:{game.PlayCountry?.MyApplyF(country=>Country.GetFund(game,country)?.ToString("0.####"))}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"内政力:{game.PlayCountry?.MyApplyF(country=>Country.GetAffairPower(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"内政難度:{game.PlayCountry?.MyApplyF(country=>Country.GetAffairDifficult(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"総内政値:{game.PlayCountry?.MyApplyF(country=>Country.GetTotalAffair(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"支出:{game.PlayCountry?.MyApplyF(country=>Country.GetOutFund(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"収入:{game.PlayCountry?.MyApplyF(country=>Country.GetInFund(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"侵攻:{Country.GetTargetArea(game,game.PlayCountry)?.ToString()??"なし"}",TextAlignment=TextAlignment.Center },
      nextPhaseButton,
    ];
    static List<UIElement> ShowEndGameInfo(MainPage page,GameState game) => [
      new TextBlock{ Text=Turn.GetCalendarText(game),TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"プレイ勢力:{game.PlayCountry}",TextAlignment=TextAlignment.Center },
      new StackPanel{ Height=BasicStyle.textHeight },
      new TextBlock{ Text=$"ゲーム終了",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"結果",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=game.Phase switch { Phase.PerishEnd=>"滅亡敗北",Phase.TurnLimitOverEnd=>"存続勝利",Phase.WinEnd=>"条件勝利",Phase.OtherWinEnd=>"他陣営条件勝利敗北",_ =>"" },TextAlignment=TextAlignment.Center },
      new StackPanel{ Height=BasicStyle.textHeight },
      new Button() { HorizontalAlignment = HorizontalAlignment.Stretch,Background = new SolidColorBrush(Color.FromArgb(100,100,100,100)),Height = BasicStyle.textHeight * 5.5 }.MySetChild(new TextBlock { Text = "ゲームログを表示" }).MyApplyA(button=>button.Click += (_,_) => ShowGameEndLogButtonClick(page,game))
    ];
    static void ShowGameEndLogButtonClick(MainPage page,GameState game) {
      string title = $"ゲームログ";
      TextBlock[] content = [.. game.GameLog.Select(log => new TextBlock { Text = log })];
      string okButtonText = "ゲームコメントを投稿する";
      UI.SetExPanel(page,title,content,okButtonText,(_,_)=>LogButtonClick(game),true);
    }
    static void LogButtonClick(GameState game) {
      string url = $"https://karintougames.com/siteContents/gameComment.php?caption={GameInfo.name.Value} ver.{GameInfo.version.Value}&comment={HttpUtility.UrlEncode(string.Join('\n',game.GameLog))}";
#if __WASM__
      Uno.Foundation.WebAssemblyRuntime.InvokeJS($"window.location.href='{url}';");
#else
      _ = Windows.System.Launcher.LaunchUriAsync(new Uri(url));
#endif
    }
    static GameState EndPlanningPhase(MainPage page,GameState game) {
      return game.MyApplyF(game => UpdateGame.AutoPutPostCPU(game,[ECountry.漢])).MyApplyF(CalcArmyTarget).MyApplyF(game => game with { Phase = Phase.Execution }).MyApplyA(game => UpdateAreaUI(page,game)).MyApplyF(game => Execution(page,game)).MyApplyA(game => UI.UpdateLogMessageUI(page,game));
      static GameState CalcArmyTarget(GameState game) {
        Dictionary<ECountry,EArea?> playerArmyTargetMap = game.PlayCountry.MyMaybeToList().Where(country => !Country.IsSleep(game,country)).ToDictionary(v => v,v => game.ArmyTargetMap.GetValueOrDefault(v));
        Dictionary<ECountry,EArea?> NPCArmyTargetMap = game.CountryMap.Keys.Except(game.PlayCountry.MyMaybeToList()).Where(country => !Country.IsSleep(game,country)).ToDictionary(country => country,country => country == ECountry.漢 ? null : RandomSelectNPCAttackTarget(game,country));
        return game with { ArmyTargetMap = new([.. NPCArmyTargetMap,.. playerArmyTargetMap]) };
        static EArea? RandomSelectNPCAttackTarget(GameState game,ECountry country) => Area.GetAdjacentAnotherCountryAllAreas(game,country).MyNullable().Append(null).MyPickAny().MyApplyF(area => area?.MyApplyF(game.AreaMap.GetValueOrDefault)?.Country == null && MyRandom.RandomJudge(0.9) ? null : area);
      }
      static GameState Execution(MainPage page,GameState game) {
        return ArmyAttack(game).MyApplyA(game => game.ArmyTargetMap.Where(v => v.Value != null).Select(attackInfo => {
          Point? srcPoint = Area.GetCapitalArea(game,attackInfo.Key)?.MyApplyF(capital => Area.GetAreaPoint(game,capital,mapSize,areaSize,mapGridCount,infoFrameWidth.Value));
          Point? dstPoint = attackInfo.Value?.MyApplyF(target => Area.GetAreaPoint(game,target,mapSize,areaSize,mapGridCount,infoFrameWidth.Value));
          string flagText = attackInfo.Key.ToString();
          TextBlock flagTextBlock = new() { Text = flagText,RenderTransform = new ScaleTransform { ScaleX = Math.Min(1,(double)2 / flagText.Length) * 2,ScaleY = 2 },Width = Math.Min(2,flagText.Length) * BasicStyle.fontsize * 2,Height = BasicStyle.fontsize * 2 };
          Grid flagPanel = new() { Width = BasicStyle.fontsize * 4,Height = BasicStyle.fontsize * 3,Background = new SolidColorBrush(Country.GetCountryColor(game,attackInfo.Key)),HorizontalAlignment = HorizontalAlignment.Left,VerticalAlignment = VerticalAlignment.Top };
          Image attackImage = new() { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/army.svg")),Width = BasicStyle.fontsize * 4,Height = BasicStyle.fontsize * 4,HorizontalAlignment = HorizontalAlignment.Right };
          Grid attackImagePanel = new Grid() { Width = BasicStyle.fontsize * 6,Height = BasicStyle.fontsize * 5 }.MySetChildren([attackImage,flagPanel.MySetChildren([flagTextBlock])]);
          page.MapAnimationElementsCanvas.Children.Add(attackImagePanel);
          List<Point> posList = [.. Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).Select(lerpWeight => srcPoint is Point src && dstPoint is Point dst ? new Point(double.Lerp(src.X,dst.X,lerpWeight) - attackImagePanel.Width / 2,double.Lerp(src.Y,dst.Y,lerpWeight) - attackImagePanel.Height / 2) : null)];
          return (attackImagePanel, posList);
        }).MyApplyA(async v => {
          await v.MyAsyncForEachConcurrent(async v => await v.posList.MyAsyncForEachSequential(async pos => {
            dispatcher.TryEnqueue(() => {
              Canvas.SetLeft(v.attackImagePanel,pos.X);
              Canvas.SetTop(v.attackImagePanel,pos.Y);
            });
            await Task.Delay(15);
          }));
          UpdateAreaUI(page,game);
        }));
      }
      static GameState ArmyAttack(GameState game) {
        return game.CountryMap.Keys.OrderBy(country => Country.GetTotalAffair(game,country)).Aggregate(game,(game,country) => {
          return game.ArmyTargetMap.GetValueOrDefault(country) is EArea target ? TryAttack(game,country,target) : game.ArmyTargetMap.ContainsKey(country) ? ExeDefense(game,country) : ExeRest(game,country);
          static GameState TryAttack(GameState game,ECountry country,EArea targetArea) {
            return Country.SuccessAttack(game,country) ? ExeAttack(game,country,targetArea) : FailAttack(game,country);
            static GameState ExeAttack(GameState game,ECountry country,EArea targetArea) => targetArea.MyApplyF(game.AreaMap.GetValueOrDefault)?.Country.MyApplyF(defeseSide => UpdateGame.Attack(game,country,targetArea,defeseSide,Country.IsFocusDefense(game,defeseSide))) ?? game;
            static GameState FailAttack(GameState game,ECountry country) => game.MyApplyF(game => UpdateGame.Defense(game,country,true)).MyApplyF(game => game with { ArmyTargetMap = game.ArmyTargetMap.MyRemove(country) });
          }
          static GameState ExeDefense(GameState game,ECountry country) => game.MyApplyF(game => UpdateGame.Defense(game,country,false));
          static GameState ExeRest(GameState game,ECountry country) => game.MyApplyF(game => UpdateGame.Rest(game,country));
        });
      }
    }
    static GameState EndExecutionPhase(MainPage page,GameState game) {
      page.MapAnimationElementsCanvas.MySetChildren([]);
      return game.MyApplyF(UpdateGame.GameEndJudge).MyApplyF(game => game.Phase is Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd ? game : game.MyApplyF(game => NextTurn(page,game)).MyApplyF(UpdateGame.GameEndJudge));
      static GameState NextTurn(MainPage page,GameState game) => game.MyApplyF(UpdateGame.NextTurn).MyApplyA(game => UpdateCountryPostPersons(page,game)).MyApplyF(v => v with { Phase = Phase.Planning,ArmyTargetMap = [] }).MyApplyA(game => UpdateAreaUI(page,game)).MyApplyA(game => UI.UpdateTurnLogUI(page,game)).MyApplyA(game => UI.UpdateTurnWinCondUI(page,game,true)).MyApplyF(UpdateGame.ClearTurnMyLog);
    }
  }
  static Grid CreatePersonPutPanel(MainPage page,GameState game,Post post,Dictionary<Person,PersonParam> putPersonMap,string backText) {
    Grid personPutPanel = new() { Width = personPutSize.Width,Height = personPutSize.Height,BorderBrush = new SolidColorBrush(UI.GetPostFrameColor(game,post.PostKind.MaybeArea)),BorderThickness = new(postFrameWidth),Background = new SolidColorBrush(Colors.Transparent) };
    StackPanel personPutInnerPanel = new StackPanel().MySetChildren([.. putPersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == post).MyMaybeToList().Select(param => CreatePersonPanel(page,param))]);
    TextBlock personPutBackText = new() { Text = backText,Foreground = new SolidColorBrush(Color.FromArgb(100,100,100,100)),HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Center,RenderTransform = new ScaleTransform() { ScaleX = 2,ScaleY = 2,CenterX = UI.CalcFullWidthLength(backText) * BasicStyle.fontsize / 2,CenterY = BasicStyle.textHeight / 2 } };
    personPutPanel.PointerEntered += (_,_) => EnterPersonPutPanel(MainPage.game,personPutInnerPanel,post);
    personPutPanel.PointerExited += (_,_) => ExitPersonPutPanel(personPutInnerPanel);
    return personPutPanel.MySetChildren([personPutBackText,personPutInnerPanel]);
    static void EnterPersonPutPanel(GameState game,StackPanel personPutInnerPanel,Post post) {
      if(game.Phase != Phase.Starting && (post.PostKind.MaybeArea?.MyApplyF(area => game.AreaMap.GetValueOrDefault(area)?.Country == game.PlayCountry) ?? true)) {
        personPutInnerPanel.Background = new SolidColorBrush(Color.FromArgb(150,255,255,255));
        UI.pointerover = (personPutInnerPanel, post);
      }
    }
    static void ExitPersonPutPanel(StackPanel personPutInnerPanel) {
      if(UI.pointerover != null) {
        personPutInnerPanel.Background = new SolidColorBrush(Colors.Transparent);
        UI.pointerover = null;
      }
    }
  }
  static Grid CreatePersonPanel(MainPage page,KeyValuePair<Person,PersonParam> person) {
    double minFullWidthLength = 2.25;
    double margin = page.FontSize * (UI.CalcFullWidthLength(person.Key.Value) - 2);
    Grid panel = new Grid { Width = personPutSize.Width,Height = personPutSize.Height,Background = new SolidColorBrush(Country.GetCountryColor(game,person.Value.Country)) }.MySetChildren([
      new StackPanel { HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,Background=new SolidColorBrush(Color.FromArgb((byte)(20*person.Value.Rank),0,0,0)) }.MySetChildren([
        GetRankPanel(page,person),
        new TextBlock { Text=person.Key.Value,TextAlignment=TextAlignment.Center,Margin=new(-margin/2,0),RenderTransform=new ScaleTransform{ ScaleX=minFullWidthLength/Math.Max(minFullWidthLength,UI.CalcFullWidthLength(person.Key.Value))*personNameFontScale,ScaleY=personNameFontScale,CenterX=personPutSize.Width/2+margin/minFullWidthLength  }  }
      ])
    ]);
    panel.PointerPressed += (_,e) => PickPersonPanel(page,e,panel,person.Key);
    return panel;
    static StackPanel GetRankPanel(MainPage page,KeyValuePair<Person,PersonParam> person) {
      bool matchRole = person.Value.Role == person.Value.Post?.PostRole;
      TextBlock baseTextBlock = new() { Margin = new(0,-3,0,3) };
      return new StackPanel { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center,RenderTransform = new ScaleTransform() { ScaleX = personRankFontScale,ScaleY = personRankFontScale,CenterX = page.FontSize / 2 } }.MySetChildren(matchRole ? GetMatchRankTextBlock() : GetUnMatchRankTextBlock());
      List<UIElement> GetMatchRankTextBlock() => [baseTextBlock.MyApplyA(v => v.Text = person.Value.Rank.ToString())];
      List<UIElement> GetUnMatchRankTextBlock() => [baseTextBlock.MyApplyA(v => { v.Text = (person.Value.Rank - 1).ToString(); v.Foreground = new SolidColorBrush(Colors.Red); }),new Image { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{person.Value.Role}.svg")),VerticalAlignment = VerticalAlignment.Top,Width = BasicStyle.textHeight * 0.75,Height = BasicStyle.textHeight * 0.75 }];
    }
    static void PickPersonPanel(MainPage page,PointerRoutedEventArgs e,Panel personPanel,Person person) {
      if(game.Phase != Phase.Starting && game.PersonMap.GetValueOrDefault(person)?.Country == game.PlayCountry && personPanel.Parent is Panel parentPanel) {
        personPanel.IsHitTestVisible = false;
        parentPanel.MySetChildren([]);
        page.MovePersonCanvas.MySetChildren([personPanel]);
        MovePerson(page,personPanel,e);
        UI.pick = (parentPanel, person);
      }
    }
  }
  static void MovePerson(MainPage page,UIElement personPanel,PointerRoutedEventArgs e) {
    Canvas.SetLeft(personPanel,e.GetCurrentPoint(page.MovePersonCanvas).Position.X - personPutSize.Width / 2);
    Canvas.SetTop(personPanel,e.GetCurrentPoint(page.MovePersonCanvas).Position.Y - personPutSize.Height / 2);
  }
  internal static class UI {
    internal static readonly Color buttonBackground = Color.FromArgb(175,255,255,255);
    internal static (Panel panel, Post post)? pointerover = null;
    internal static (Panel panel, Person person)? pick = null;
    private static InfoPanelState showInfoPanelState = InfoPanelState.Explain;
    private static ViewMode viewMode = ViewMode.fix;
    internal static readonly StackPanel centralPanel = [];
    internal static readonly StackPanel affairPanel = [];
    internal static readonly StackPanel defensePanel = [];
    internal static readonly StackPanel attackPanel = [];
    internal static readonly StackPanel countryCentralPostPanel = CreateCountryPostPanel(ERole.central,Color.FromArgb(255,240,240,210),centralPanel);
    internal static readonly StackPanel countryAffairPostPanel = CreateCountryPostPanel(ERole.affair,Color.FromArgb(255,240,240,240),affairPanel);
    internal static readonly StackPanel countryDefensePostPanel = CreateCountryPostPanel(ERole.defense,Color.FromArgb(255,210,210,240),defensePanel);
    internal static readonly StackPanel countryAttackPostPanel = CreateCountryPostPanel(ERole.attack,Color.FromArgb(255,240,210,210),attackPanel);
    private static readonly Dictionary<ERole,StackPanel> panelRoleMap = new() { { ERole.central,countryCentralPostPanel },{ ERole.affair,countryAffairPostPanel },{ ERole.defense,countryDefensePostPanel },{ ERole.attack,countryAttackPostPanel } };
    private static ERole nowActiveRoles = ERole.central;
    private static Action resizeExPanelAction = () => { };
    private static readonly WebView2 webView= new();
    internal static StackPanel CreateCountryPostPanel(ERole role,Color backColor,StackPanel innerPanel) {
      return new StackPanel() { Padding = new(1.5,0),Background = new SolidColorBrush(backColor) }.MySetChildren([
        new StackPanel() { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center }.MySetChildren([
          new TextBlock { Text = Code.Text.RoleToText(role,Lang.ja) },
          new Image { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{role}.svg")),Width = BasicStyle.textHeight,Height = BasicStyle.textHeight,VerticalAlignment = VerticalAlignment.Center }
        ]),
        innerPanel
      ]);
    }
    internal static double CalcFullWidthLength(string str) => str.Length - str.Where(v => v is '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' or '-' or '.').Count() * 0.4;
    internal static double GetScaleFactor(MainPage page) => Math.Max(Math.Max(fixModeMaxWidth,page.ContentGrid.ActualWidth) / mapSize.Width,Math.Max(fixModeMaxWidth,page.ContentGrid.ActualHeight) / mapSize.Height);
    internal static Color GetPostFrameColor(GameState game,EArea? area) => area != null && (game.NowScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.ChinaAreas ?? []).Contains(area.Value) ? Color.FromArgb(150,100,100,30) : Color.FromArgb(150,0,0,0);
    internal static void ResizeLogMessageUI(MainPage page,GameState game,double scaleFactor) {
      page.LogContentPanel.Margin = new(0,0,page.LogContentPanel.Width * (scaleFactor - 1),game.LogMessage.Length * BasicStyle.textHeight * (scaleFactor - 1));
      page.LogContentPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    }
    internal static void ResizeInfoMessageUI(MainPage page,double scaleFactor) {
      page.InfoContentPanel.Margin = new(0,0,page.InfoContentPanel.Width * (scaleFactor - 1),page.InfoContentPanel.ActualHeight * (scaleFactor - 1));
      page.InfoContentPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    }
    internal static void ResizeCountryPostUI(MainPage page,double scaleFactor) {
      double PostPanelLeftUnit = mapSize.Width / 4 + (page.ContentGrid.ActualWidth - page.MapPanel.Width) / scaleFactor / 3;
      panelRoleMap.Values.Select((elem,index) => (elem, index)).ToList().ForEach(v => Canvas.SetLeft(v.elem,PostPanelLeftUnit * v.index));
    }
    internal static void ResizeTurnWinCondPanelUI(MainPage page,double scaleFactor) {
      page.TurnWinCondPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.TurnWinCondPanel.ActualWidth / 2 };
    }
    internal static void UpdateLogMessageUI(MainPage page,GameState game) {
      page.LogContentPanel.MySetChildren([.. game.LogMessage.Select(logText => new TextBlock() { Text = logText })]);
      ResizeLogMessageUI(page,game,GetScaleFactor(page));
    }
    internal static void UpdateTurnLogUI(MainPage page,GameState game) {
      StackPanel panel = new StackPanel() {
        Background = new SolidColorBrush(Color.FromArgb(187,255,255,255)),
        Height = game.TrunLog.Length * BasicStyle.textHeight,
        IsHitTestVisible = false
      }.MySetChildren([.. game.TrunLog.Select(logText => new TextBlock() { Text = logText })]).MyApplyA(async elem => {
        elem.Opacity = 1;
        await Task.Delay(6000);
        await Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).MyAsyncForEachSequential(async v => {
          dispatcher.TryEnqueue(() => { elem.Opacity = 1 - v; });
          await Task.Delay(15);
        });
        page.TurnLogPanel.Children.Remove(elem);
        ResizeTurnLogUI(page);
      });
      page.TurnLogPanel.Children.Add(panel);
      ResizeTurnLogUI(page);
      static void ResizeTurnLogUI(MainPage page) => page.TurnLogPanel.Height = page.TurnLogPanel.Children.OfType<FrameworkElement>().Sum(v => v.Height) * GetScaleFactor(page);
    }
    internal static void UpdateTurnWinCondUI(MainPage page,GameState game,bool outputCheckString) {
      Dictionary<string,bool?> winCondMap = game.PlayCountry?.MyApplyF(game.CountryMap.GetValueOrDefault)?.WinConditionProgressExplainFunc(game) ?? [];
      StackPanel panel = new StackPanel() {
        Background = new SolidColorBrush(Color.FromArgb(187,255,255,255)),
        IsHitTestVisible = false
      }.MySetChildren([
        new TextBlock() { Text = $"勝利条件({Turn.GetCalendarText(game)})" },
        .. winCondMap.Select(winCond => new StackPanel(){ Orientation = Orientation.Horizontal }.MySetChildren([
          ..((outputCheckString&&winCond.Value.HasValue)?new Image() { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{(winCond.Value.Value? "checkOK.svg" :!winCond.Value.Value? "checkNG.svg":"")}")),Width = BasicStyle.fontsize ,Height = BasicStyle.fontsize }:null).MyMaybeToList(),
          new TextBlock() { Text = winCond.Key }
        ]))
      ]).MyApplyA(async elem => {
        elem.Opacity = 1;
        await Task.Delay(6000);
        await Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).MyAsyncForEachSequential(async v => {
          dispatcher.TryEnqueue(() => { elem.Opacity = 1 - v; });
          await Task.Delay(15);
        });
        page.TurnWinCondPanel.Children.Remove(elem);
      });
      page.TurnWinCondPanel.Children.Add(panel);
      page.TurnWinCondPanel.UpdateLayout();
      ResizeTurnWinCondPanelUI(page,GetScaleFactor(page));
    }
    internal static void RefreshViewMode(MainPage page) {
      page.SwitchViewModeButtonText.Text = viewMode == ViewMode.fix ? "▼" : "▲";
      page.ViewModeText.Text = viewMode == ViewMode.fix ? "固定幅" : "ウィンドウフィット";
      page.ContentGrid.MaxWidth = viewMode == ViewMode.fix ? fixModeMaxWidth : double.MaxValue;
      page.UpdateLayout();
      ScalingElements(page,game,GetScaleFactor(page));
    }
    internal static void SwitchInfoButton(MainPage page,InfoPanelState clickButtonInfoPanelState) {
      showInfoPanelState = clickButtonInfoPanelState;
      page.ExplainPanel.Visibility = showInfoPanelState == InfoPanelState.Explain ? Visibility.Visible : Visibility.Collapsed;
      page.WinCondPanel.Visibility = showInfoPanelState == InfoPanelState.WinCond ? Visibility.Visible : Visibility.Collapsed;
      page.PersonDataPanel.Visibility = showInfoPanelState == InfoPanelState.PersonData ? Visibility.Visible : Visibility.Collapsed;
      page.ChangeLogPanel.Visibility = showInfoPanelState == InfoPanelState.ChangeLog ? Visibility.Visible : Visibility.Collapsed;
      page.SettingPanel.Visibility = showInfoPanelState == InfoPanelState.Setting ? Visibility.Visible : Visibility.Collapsed;
      page.ExplainButton.Background = new SolidColorBrush(showInfoPanelState == InfoPanelState.Explain ? Colors.LightGray : Colors.WhiteSmoke);
      page.WinCondButton.Background = new SolidColorBrush(showInfoPanelState == InfoPanelState.WinCond ? Colors.LightGray : Colors.WhiteSmoke);
      page.PersonDataButton.Background = new SolidColorBrush(showInfoPanelState == InfoPanelState.PersonData ? Colors.LightGray : Colors.WhiteSmoke);
      page.ChangeLogButton.Background = new SolidColorBrush(showInfoPanelState == InfoPanelState.ChangeLog ? Colors.LightGray : Colors.WhiteSmoke);
      page.SettingButton.Background = new SolidColorBrush(showInfoPanelState == InfoPanelState.Setting ? Colors.LightGray : Colors.WhiteSmoke);
      page.UpdateLayout();
      ResizeInfoMessageUI(page,GetScaleFactor(page));
    }
    internal static void ScalingElements(MainPage page,GameState game,double scaleFactor) {
      page.CountryPostsPanel.Margin = new(0,0,page.CountryPostsPanel.ActualWidth * (scaleFactor - 1),page.CountryPostsPanel.ActualHeight * (scaleFactor - 1));
      page.CountryPostsPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.MapPanel.Width = mapSize.Width * scaleFactor;
      page.MapPanel.Height = mapSize.Height * scaleFactor;
      page.MapInnerPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.MovePersonCanvas.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.CountryInfoPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.CountryInfoPanel.Width,CenterY = page.CountryInfoPanel.Height };
      double buttonMargin = infoFrameWidth.Value * (scaleFactor - 1);
      page.InfoButtonsPanel.Margin = new(0,0,-page.InfoLayoutPanel.ActualWidth / scaleFactor + page.InfoLayoutPanel.ActualWidth - buttonMargin,buttonMargin);
      page.InfoButtonsPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.OpenLogButton.Margin = new(0,0,-page.InfoLayoutPanel.ActualWidth / scaleFactor + page.InfoLayoutPanel.ActualWidth - buttonMargin,buttonMargin);
      page.OpenLogButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.OpenInfoButton.Margin = new(0,0,buttonMargin,-page.InfoLayoutPanel.ActualHeight / scaleFactor + page.InfoLayoutPanel.ActualHeight - buttonMargin);
      page.OpenInfoButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.TopSwitchViewModeButton.Margin = new(0,0,buttonMargin,buttonMargin);
      page.TopSwitchViewModeButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.TurnLogPanel.Margin = new(infoFrameWidth.Value * scaleFactor,0,0,0);
      page.TurnLogPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.TurnWinCondPanel.Margin = new(0,infoFrameWidth.Value * scaleFactor,0,0);
      ResizeTurnWinCondPanelUI(page,scaleFactor);
      ResizeCountryPostUI(page,scaleFactor);
      ResizeLogMessageUI(page,game,scaleFactor);
      ResizeInfoMessageUI(page,scaleFactor);
      UpdateCountryInfoPanel(page,game);
    }
    internal static void SwitchViewMode(MainPage page) {
      viewMode = viewMode == ViewMode.fix ? ViewMode.fit : ViewMode.fix;
      RefreshViewMode(page);
#if __WASM__
      Uno.Foundation.WebAssemblyRuntime.InvokeJS($"window.parent.{viewMode}();");
#endif
    }
    internal static void MovePersonPanel(MainPage page,PointerRoutedEventArgs e) {
      if(pick != null) {
        page.MovePersonCanvas.Children.SingleOrDefault()?.MyApplyA(personPanel => MovePerson(page,personPanel,e));
      }
    }
    internal static void PutPersonPanel(MainPage page) {
      if(pick != null) {
        game = game.MyApplyF(game => swapPerson(page,game)).MyApplyF(game => putPerson(page,game));
        page.MovePersonCanvas.MySetChildren([]);
        pick = null;
        UpdateCountryInfoPanel(page,game);
      }
      static GameState swapPerson(MainPage page,GameState game) {
        KeyValuePair<Person,PersonParam>? maybeDestPersonInfo = game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Country == game.PlayCountry && v?.Value.Post == pointerover?.post);
        return UpdateGame.PutPersonFromUI(game,maybeDestPersonInfo?.Key,pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == maybeDestPersonInfo?.Key)?.MyApplyF(destPersonInfo => pick?.panel.MySetChildren([CreatePersonPanel(page,destPersonInfo)])));
      }
      static GameState putPerson(MainPage page,GameState game) {
        return UpdateGame.PutPersonFromUI(game,pick?.person,pointerover?.post ?? pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == pick?.person)?.MyApplyF(putPersonInfo => (pointerover?.panel ?? pick?.panel)?.MySetChildren([CreatePersonPanel(page,putPersonInfo)])));
      }
    }
    internal static void UpdateCountryPostPanelZIndex(MainPage page,ERole newActiveRoles) {
      List<StackPanel> resetPanels = (nowActiveRoles, newActiveRoles) switch {
        (ERole.central, ERole.affair) => [countryAffairPostPanel],
        (ERole.central, ERole.defense) => [countryAffairPostPanel,countryDefensePostPanel],
        (ERole.central, ERole.attack) => [countryAffairPostPanel,countryDefensePostPanel,countryAttackPostPanel],
        (ERole.affair, ERole.central) => [countryCentralPostPanel],
        (ERole.affair, ERole.defense) => [countryDefensePostPanel],
        (ERole.affair, ERole.attack) => [countryDefensePostPanel,countryAttackPostPanel],
        (ERole.defense, ERole.central) => [countryAffairPostPanel,countryCentralPostPanel],
        (ERole.defense, ERole.affair) => [countryAffairPostPanel],
        (ERole.defense, ERole.attack) => [countryAttackPostPanel],
        (ERole.attack, ERole.central) => [countryDefensePostPanel,countryAffairPostPanel,countryCentralPostPanel],
        (ERole.attack, ERole.affair) => [countryDefensePostPanel,countryAffairPostPanel],
        (ERole.attack, ERole.defense) => [countryDefensePostPanel],
        _ => []
      };
      if(resetPanels.Count != 0) {
        page.CountryPostsPanel.Children.ToList().ForEach(v=>v.IsHitTestVisible = false);
        resetPanels.ForEach(v => page.CountryPostsPanel.Children.Remove(v));
        resetPanels.ForEach(page.CountryPostsPanel.Children.Add);
        page.CountryPostsPanel.Children.Reverse().ToList().ForEach(v => v.IsHitTestVisible = true);
        nowActiveRoles = newActiveRoles;
      }
    }
    internal static void SetExPanel(MainPage page,string title,TextBlock[] contents,string okButtonText,RoutedEventHandler clickOkButtonEvent,bool enableOkButton) {
      double scaleFactor = GetScaleFactor(page);
      Button okButton = new Button { Height = 80,Background = buttonBackground,Margin = new(10),IsEnabled = enableOkButton }.MySetChild(new TextBlock { Text = okButtonText });
      Button closeButton = new Button { Height = 80,Background = buttonBackground,Margin = new(10) }.MySetChild(new TextBlock { Text = "閉じる" });
      TextBlock titleTextBlock = new() { Text = title,HorizontalAlignment = HorizontalAlignment.Center };
      StackPanel contentsPanel = new StackPanel { Height = contents.Length * BasicStyle.textHeight,VerticalAlignment=VerticalAlignment.Top }.MySetChildren([.. contents]);
      ScrollViewer contentsView = new ScrollViewer { Height = BasicStyle.textHeight * 15,Margin = new(0,5),Background = new SolidColorBrush(Colors.White) }.MySetChild(contentsPanel);
      StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center }.MySetChildren([okButton,new StackPanel { Margin = new(5) },closeButton]);
      okButton.Click += clickOkButtonEvent;
      closeButton.Click += (_,_) => page.ExPanel.MySetChildren([]);
      page.ExPanel.MySetChildren([new StackPanel { Padding = new(5),Background = new SolidColorBrush(Color.FromArgb(255,230,230,230)) }.MySetChildren([titleTextBlock,contentsView,buttonPanel])]);
      ResizeExPanelElem(page,titleTextBlock,contentsView,contentsPanel,buttonPanel,okButton,closeButton);
      resizeExPanelAction = () => ResizeExPanelElem(page,titleTextBlock,contentsView,contentsPanel,buttonPanel,okButton,closeButton);
      static void ResizeExPanelElem(MainPage page,TextBlock titleTextBlock,ScrollViewer contentsView,StackPanel contentsPanel,StackPanel buttonPanel,Button okButton,Button closeButton) {
        double scaleFactor = GetScaleFactor(page);
        double titleTextScaleFactor = 1.25;
        titleTextBlock.Margin = new(0,5 * scaleFactor,0,BasicStyle.textHeight * (scaleFactor - 1) + 5 * scaleFactor);
        titleTextBlock.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * titleTextScaleFactor,ScaleY = scaleFactor * titleTextScaleFactor,CenterX = CalcFullWidthLength(titleTextBlock.Text)*BasicStyle.fontsize* titleTextScaleFactor / 2 };
        contentsView.Width = Math.Min(page.ContentGrid.ActualWidth - 20 * scaleFactor,(600 * 2 + 10 / scaleFactor) * scaleFactor);
        contentsView.Height = page.ActualHeight*0.5;
        contentsPanel.Width = contentsView.Width / scaleFactor - 15;
        contentsPanel.Margin = new(contentsPanel.Width * (scaleFactor - 1) / 2,0,contentsPanel.Width * (scaleFactor - 1) / 2,contentsPanel.Height * (scaleFactor - 1));
        contentsPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = contentsPanel.Width / 2 };
        okButton.Width = Math.Min((page.ContentGrid.ActualWidth / 2 - 5 - 5d / 2) / scaleFactor,600);
        okButton.Margin = new(okButton.Width * (scaleFactor - 1) / 2,0,okButton.Width * (scaleFactor - 1) / 2,okButton.Height * (scaleFactor - 1));
        okButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = okButton.Width / 2 };
        closeButton.Width = Math.Min((page.ContentGrid.ActualWidth / 2 - 5 - 5d / 2) / scaleFactor,600);
        closeButton.Margin = new(closeButton.Width * (scaleFactor - 1) / 2,0,closeButton.Width * (scaleFactor - 1) / 2,closeButton.Height * (scaleFactor - 1));
        closeButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = closeButton.Width / 2 };
      }
    }
    internal class PushArea {
      internal static EArea? pushArea = null;
      internal static void Push(EArea area) => pushArea = area;
      internal static void Exit() => pushArea = null;
      internal static GameState Release(MainPage page,GameState game,KeyValuePair<EArea,AreaInfo> areaInfo) { 
        return pushArea != areaInfo.Key ? game : game.Phase == Phase.Starting ? areaInfo.Value.Country?.MyApplyF(country => ShowSelectPlayCountryPanel(page,game,country)) ?? game : Area.IsPlayerSelectable(game,areaInfo.Key) ? SelectTarget(page,game,areaInfo.Value.Country != game.PlayCountry ? areaInfo.Key : null) : game;
        static GameState ShowSelectPlayCountryPanel(MainPage page,GameState game,ECountry pushCountry) {
          string title = $"{pushCountry}陣営";
          TextBlock[] content = [
            new TextBlock{ Text=game.NowScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.MyApplyF(scenario=>$"シナリオ:{game.NowScenario.Value}({scenario.StartYear}年開始 {scenario.EndYear}年終了)"),HorizontalAlignment=HorizontalAlignment.Center },
            new TextBlock{ Text=$"[陣営初期情報]",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"首都:{Area.GetCapitalArea(game,pushCountry)}",HorizontalAlignment=HorizontalAlignment.Center },
            new TextBlock{ Text=$"資金:{Country.GetFund(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"内政力:{Country.GetAffairPower(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"内政難度:{Country.GetAffairDifficult(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"総内政値:{Country.GetTotalAffair(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"支出:{Country.GetOutFund(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"収入:{Country.GetInFund(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text="[勝利条件]",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text="以下の条件を全て満たす",HorizontalAlignment = HorizontalAlignment.Center },
            .. game.CountryMap.GetValueOrDefault(pushCountry)?.WinConditionMessageFunc().Select(v=>new TextBlock{ Text=v,HorizontalAlignment = HorizontalAlignment.Center })??[],
            new TextBlock{ Text=$"[初期人物]",HorizontalAlignment = HorizontalAlignment.Center  },
            .. Enum.GetValues<ERole>().SelectMany(role=>Code.Person.GetInitPersonMap(game,pushCountry,role).OrderBy(v=>v.Value.BirthYear).Select(v => new TextBlock { Text = $"{v.Key.Value}  {Code.Text.RoleToText(v.Value.Role,Lang.ja)} ランク{v.Value.Rank} 齢{Turn.GetYear(game)-v.Value.BirthYear}",HorizontalAlignment=HorizontalAlignment.Center  })),
          ];
          string okButtonText = pushCountry != ECountry.漢 ? "プレイする" : "(漢は選べません)";
          bool enableOkButton = pushCountry != ECountry.漢;
          SetExPanel(page,title,content,okButtonText,(_,_) => ClickOkButtonAction(page,game,pushCountry),enableOkButton);
          return game;
          static void ClickOkButtonAction(MainPage page,GameState game,ECountry playCountry) {
            MainPage.game = SelectPlayCountry(page,game,playCountry).MyApplyF(game => StartGame(page,game)).MyApplyA(game => UpdateCountryInfoPanel(page,game)); page.ExPanel.MySetChildren([]);
          }
          static GameState SelectPlayCountry(MainPage page,GameState game,ECountry playCountry) => UpdateGame.AttachGameStartData(game,playCountry).MyApplyA(game => UpdateCountryPostPersons(page,game));
          static GameState StartGame(MainPage page,GameState game) => (game with { Phase = Phase.Planning }).MyApplyA(v => UpdateAreaUI(page,v)).MyApplyF(UpdateGame.AppendGameStartLog).MyApplyA(game => UI.UpdateLogMessageUI(page,game)).MyApplyA(game => UI.UpdateTurnLogUI(page,game)).MyApplyA(game => UI.UpdateTurnWinCondUI(page,game,false));
        }
        static GameState SelectTarget(MainPage page,GameState game,EArea? area) => game.PlayCountry?.MyApplyF(playCountry => game.Phase == Phase.Planning && !Country.IsSleep(game,playCountry) ? (game with { ArmyTargetMap = game.ArmyTargetMap.MyAdd(playCountry,null).MyUpdate(playCountry,(_,_) => area) ?? game.ArmyTargetMap }).MyApplyA(game => { UpdateCountryInfoPanel(page,game); }) : null) ?? game;
      }
    }
    internal static void SetUIElements(MainPage page) {
#if __WASM__
      Uno.Foundation.WebAssemblyRuntime.InvokeJS(@"window.addEventListener('orientationchange',function(){let orientationType=screen.orientation.type;chrome.webview.postMessage(orientationType);});");
#endif
      page.CountryPostsPanel.MySetChildren([.. panelRoleMap.Values.Reverse()]);
      page.AttackCrushFillColor.Background = new SolidColorBrush(ThresholdFillColor(AttackJudge.crush));
      page.AttackCrushEdgeColor.Background = new SolidColorBrush(ThresholdEdgeColor(AttackJudge.crush));
      page.AttackWinFillColor.Background = new SolidColorBrush(ThresholdFillColor(AttackJudge.win));
      page.AttackWinEdgeColor.Background = new SolidColorBrush(ThresholdEdgeColor(AttackJudge.win));
      page.AttackLoseFillColor.Background = new SolidColorBrush(ThresholdFillColor(AttackJudge.lose));
      page.AttackLoseEdgeColor.Background = new SolidColorBrush(ThresholdEdgeColor(AttackJudge.lose));
      page.AttackRoutFillColor.Background = new SolidColorBrush(ThresholdFillColor(AttackJudge.rout));
      page.AttackRoutEdgeColor.Background = new SolidColorBrush(ThresholdEdgeColor(AttackJudge.rout));
      page.AttackCrushShape.MyApplyA(v => v.Fill = new SolidColorBrush(ThresholdFillColor(AttackJudge.crush))).MyApplyA(v => v.Points = [.. GetJudgeShapeCrds(AttackJudge.crush),.. GetJudgeShapeCrds(AttackJudge.win).Reverse()]);
      page.AttackWinShape.MyApplyA(v => v.Fill = new SolidColorBrush(ThresholdFillColor(AttackJudge.win))).MyApplyA(v => v.Points = [.. GetJudgeShapeCrds(AttackJudge.win),.. GetJudgeShapeCrds(AttackJudge.lose).Reverse()]);
      page.AttackLoseShape.MyApplyA(v => v.Fill = new SolidColorBrush(ThresholdFillColor(AttackJudge.lose))).MyApplyA(v => v.Points = [.. GetJudgeShapeCrds(AttackJudge.lose),.. GetJudgeShapeCrds(AttackJudge.rout).Reverse()]);
      page.AttackRoutShape.MyApplyA(v => v.Fill = new SolidColorBrush(ThresholdFillColor(AttackJudge.rout))).MyApplyA(v => v.Points = [.. GetJudgeShapeCrds(AttackJudge.rout),.. GetJudgeShapeCrds(null).Reverse()]);
      page.AttackJudgePointVisualPanel.MySetChildren([.. CreateRects(null),.. CreateRects(AttackJudge.crush),.. CreateRects(AttackJudge.win),.. CreateRects(AttackJudge.lose),.. CreateRects(AttackJudge.rout),.. CreateTexts(AttackJudge.win),.. CreateTexts(AttackJudge.lose),.. CreateTexts(AttackJudge.rout)]);
      page.AttackRankDiffTextPanel.MySetChildren([.. CreateRankDiffTexts()]);
      page.WinCondScenarioName1.Text = GameInfo.scenarios.ElementAtOrDefault(0)?.Value;
      page.WinCondListPanel1.MySetChildren([.. CreateWinCondList(0,2)]).MyApplyA(v => { v.BorderThickness = new(dataListFrameWidth); v.BorderBrush = new SolidColorBrush(dataListFrameColor); });
      page.WinCondScenarioName2.Text = GameInfo.scenarios.ElementAtOrDefault(1)?.Value;
      page.WinCondListPanel2.MySetChildren([.. CreateWinCondList(1,2)]).MyApplyA(v => { v.BorderThickness = new(dataListFrameWidth); v.BorderBrush = new SolidColorBrush(dataListFrameColor); });
      page.PersonDataScenarioName1.Text = GameInfo.scenarios.ElementAtOrDefault(0)?.Value;
      page.PersonDataListPanel1.MySetChildren([.. CreatePersonDataList(0,3,12)]).MyApplyA(v => { v.BorderThickness = new(dataListFrameWidth); v.BorderBrush = new SolidColorBrush(dataListFrameColor); });
      page.CountryDataListPanel1.MySetChildren([.. CreateCountryDataList(0,2)]).MyApplyA(v => { v.BorderThickness = new(dataListFrameWidth); v.BorderBrush = new SolidColorBrush(dataListFrameColor); });
      page.PersonDataScenarioName2.Text = GameInfo.scenarios.ElementAtOrDefault(1)?.Value;
      page.PersonDataListPanel2.MySetChildren([.. CreatePersonDataList(1,3,12)]).MyApplyA(v => { v.BorderThickness = new(dataListFrameWidth); v.BorderBrush = new SolidColorBrush(dataListFrameColor); });
      page.CountryDataListPanel2.MySetChildren([.. CreateCountryDataList(1,2)]).MyApplyA(v => { v.BorderThickness = new(dataListFrameWidth); v.BorderBrush = new SolidColorBrush(dataListFrameColor); });
      static Color ThresholdEdgeColor(AttackJudge? attackJudge) => attackJudge == AttackJudge.crush ? Color.FromArgb(255,240,135,135) : attackJudge == AttackJudge.win ? Color.FromArgb(255,230,230,65) : attackJudge == AttackJudge.lose ? Color.FromArgb(255,105,225,105) : attackJudge == AttackJudge.rout ? Color.FromArgb(255,135,135,240) : Color.FromArgb(255,165,165,165);
      static Color ThresholdFillColor(AttackJudge attackJudge) => attackJudge == AttackJudge.crush ? Color.FromArgb(255,240,190,190) : attackJudge == AttackJudge.win ? Color.FromArgb(255,235,235,160) : attackJudge == AttackJudge.lose ? Color.FromArgb(255,175,240,175) : Color.FromArgb(255,190,190,240);
      static Windows.Foundation.Point GetJudgePoint(AttackJudge? attackJudge,double rankDiff) => new(rankDiff * 9 + 50,Battle.GetThreshold(attackJudge,rankDiff));
      static Windows.Foundation.Point[] GetJudgeShapeCrds(AttackJudge? attackJudge) => [.. new double[] { -5.5,-5,-4.5,-4,-3.5,-3,-2.5,-2,-1,1,2,2.5,3,3.5,4,4.5,5,5.5 }.Select(i => CookPoint(GetJudgePoint(attackJudge,i)))];
      static Windows.Foundation.Point[] GetJudgePoints(AttackJudge? attackJudge) => [.. new double[] { -5,-4,-3,-2,-1,0,1,2,3,4,5 }.Select(i => GetJudgePoint(attackJudge,i))];
      static Windows.Foundation.Point CookPoint(Windows.Foundation.Point point) => point with { X = point.X * 11.5,Y = point.Y * 6 };
      static UIElement SetJudgePointCrds(UIElement elem,Windows.Foundation.Point crd,Size size) => elem.MyApplyA(elem => { Canvas.SetLeft(elem,crd.X - size.Width / 2); Canvas.SetTop(elem,crd.Y - size.Height / 2); });
      static TextBlock[] CreateTexts(AttackJudge? attackJudge) => [.. GetJudgePoints(attackJudge).Select(crd => new TextBlock { Text = crd.Y.ToString() }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd),new(UI.CalcFullWidthLength(crd.Y.ToString()) * BasicStyle.fontsize,BasicStyle.textHeight))))];
      static Rectangle[] CreateRects(AttackJudge? attackJudge) => [.. GetJudgePoints(attackJudge).Select(crd => new Rectangle { Width = attackJudgePointSize,Height = attackJudgePointSize,Fill = new SolidColorBrush(ThresholdEdgeColor(attackJudge)) }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd),new(attackJudgePointSize,attackJudgePointSize))))];
      static TextBlock[] CreateRankDiffTexts() => [.. new double[] { -5,-4,-3,-2,-1,0,1,2,3,4,5 }.Select(i => GetJudgePoint(null,i).MyApplyF(crd => new TextBlock { Text = i.ToString() }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd with { Y = 0 }),new(UI.CalcFullWidthLength(i.ToString()) * BasicStyle.fontsize,BasicStyle.textHeight)))))];
    }
    internal static void AttachEvent(MainPage page) {
      webView.WebMessageReceived += (_,_) => ScalingElements(page,game,GetScaleFactor(page));
      page.OpenLogButton.Click += (_,_) => ClickOpenLogButton(page);
      page.OpenInfoButton.Click += (_,_) => ClickOpenInfoButton(page);
      page.Page.SizeChanged += (_,_) => ScalingElements(page,game,GetScaleFactor(page));
      page.Page.Loaded += (_,_) => LoadedPage(page);
      page.Page.PointerMoved += (_,e) => MovePersonPanel(page,e);
      page.Page.PointerReleased += (_,e) => PutPersonPanel(page);
      page.ExplainButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.Explain);
      page.WinCondButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.WinCond);
      page.PersonDataButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.PersonData);
      page.ChangeLogButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.ChangeLog);
      page.SettingButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.Setting);
      page.InitGameButton.Click += (_,_) => InitGame(page,game.NowScenario);
      page.TopSwitchViewModeButton.Click += (_,_) => SwitchViewMode(page);
      page.InnerSwitchViewModeButton.Click += (_,_) => SwitchViewMode(page);
      page.ContentGrid.SizeChanged += (_,_) => ResizeExPanel();
      countryCentralPostPanel.PointerEntered += (_,_) => UpdateCountryPostPanelZIndex(page,ERole.central);
      countryAffairPostPanel.PointerEntered += (_,_) => UpdateCountryPostPanelZIndex(page,ERole.affair);
      countryDefensePostPanel.PointerEntered += (_,_) => UpdateCountryPostPanelZIndex(page,ERole.defense);
      countryAttackPostPanel.PointerEntered += (_,_) => UpdateCountryPostPanelZIndex(page,ERole.attack);
      static void ClickOpenLogButton(MainPage page) => (page.LogScrollPanel.Visibility = page.LogScrollPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible).MyApplyA(v => page.InfoPanel.Visibility = Visibility.Collapsed);
      static void ClickOpenInfoButton(MainPage page) => (page.InfoPanel.Visibility = page.InfoPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible).MyApplyA(v => page.LogScrollPanel.Visibility = Visibility.Collapsed);
      static void LoadedPage(MainPage page) {
        GameInfo.scenarios.FirstOrDefault()?.MyApplyA(scenario => InitGame(page,scenario));
        ScalingElements(page,game,GetScaleFactor(page));
      }
      static void ResizeExPanel() => resizeExPanelAction();
    }
  }
}