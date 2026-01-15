using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using HisouSangokushiZero2_1_Uno.Pages.Common;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using SkiaSharp.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Windows.UI.Core;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Commander = HisouSangokushiZero2_1_Uno.Code.Commander;
using Post = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
using Text = HisouSangokushiZero2_1_Uno.Data.Language.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
public sealed partial class Game:Page {
  private record AreaElems(Border Back,TextBlock AreaNameText,StackPanel DefensePersonPanel,StackPanel AffairPersonPanel,Post DefensePost,Post AffairPost,TextBlock AffairText,TextBlock ExText,Grid WrapPanel);
  private static readonly double countryPostPanelWidth = 255;
  private static readonly Dictionary<Post,StackPanel> playerCountryPostPersonPanel = Enum.GetValues<ERole>().SelectMany(role => new List<(Post, StackPanel)>([
    .. Enum.GetValues<PostHead>().Select(headPost=>(new Post(role,new(headPost)),new StackPanel())),
    .. Enumerable.Range(0,UIUtil.capitalPieceCellNum).Select(cellNo=>(new Post(role,new(cellNo)),new StackPanel()))
  ])).ToDictionary();
  private static readonly Dictionary<EArea,AreaElems> areaElemsMap = [];
  private static readonly Dictionary<ERole, Grid> countryPostPanelMap = [];
  private readonly List<TaskToken> taskTokens = [];
  private ERole activePanelRole = ERole.Central;
  private (Panel panel, Post post)? pointerover = null;
  private (Panel panel, PersonId person)? pick = null;
  private double lastScaleFactor = double.NaN;
  private double mapScale = 1;
  internal static double zoomLevel = 0;
  internal static readonly double initContentGridMaxWidth = UIUtil.GetContentMaxWidth();
  public Game() {
    InitializeComponent();
    MyInit(this);
    void MyInit(Game page) {
      AttachEvent(page);
      SetCountryPostsPanel();
      UIUtil.SwitchViewModeActions.Add(RefreshViewMode);
      UIUtil.ChangeScaleActions.Add(ResizeMap);
      Ask.Init(MainGrid);
      CharacterRemark.Init(MainGrid);
      UIUtil.SaveGameActions.Add(async () => {
        await Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await SaveAndLoad.Show(SaveDataPanel, true, async _ => {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => ShowMessage([Text.ProgressSaveText()]));
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => UIUtil.SetVisibility(SaveDataPanel, false));
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => ShowMessage([Text.CompleteSaveText()]));
          }, () => UIUtil.SetVisibility(SaveDataPanel, false), MainGrid.RenderSize));
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => UIUtil.SetVisibility(SaveDataPanel, true));
        });
      });
      UIUtil.LoadGameActions.Add(async () => {
        await Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low, async () => await SaveAndLoad.Show(SaveDataPanel,false,async maybeRead => maybeRead?.MyApplyF(async read => {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage([Text.ProgressLoadText()]));
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UIUtil.SetVisibility(SaveDataPanel,false));
            await Task.Yield();
            await (read.MaybeGame?.MyApplyF(InitGame) ?? Task.CompletedTask);
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage([Text.CompleteLoadText(read)]));
          }),() => UIUtil.SetVisibility(SaveDataPanel,false),MainGrid.RenderSize));
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UIUtil.SetVisibility(SaveDataPanel,true));
        });
      });
      UIUtil.InitGameActions.Add(async () => {
        await Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage([Text.ProgressInitText()]));
          await InitGame(GetGame.GetInitGameScenario(GameData.game.NowScenario));
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage([Text.CompleteInitText()]));
        });
      });
      _ = LoadPage(GameData.game);
      async Task LoadPage(GameState startGame) {
        await Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,RefreshViewMode);
          await Task.Yield();
          await InitGame(startGame);
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,ResizeMap);
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UIUtil.SetVisibility(LoadingImagePanel,false));
        });
      }
      void AttachEvent(Game page) {
        MainGrid.SizeChanged += (_, _) => ResizeMap();
        InfoFramePanel.SizeChanged += (_, _) => ResizeInfo();
        OpenLogButton.Click += (_,_) => { UIUtil.ReverseVisibility(GameLogPanel); UIUtil.SetVisibility(ExInfoPanel, false); };
        OpenInfoButton.Click += (_,_) => { UIUtil.ReverseVisibility(ExInfoPanel); UIUtil.SetVisibility(GameLogPanel, false); };
        page.PointerMoved += (_,e) => (pick is { } && MovePersonCanvas.Children.SingleOrDefault() is UIElement personPanel ? () => MovePerson(personPanel,e) : MyUtil.MyUtil.nothing).Invoke();
        page.PointerReleased += (_,e) => (pick is { } ? () => GameData.game = PutPersonPanel(GameData.game) : MyUtil.MyUtil.nothing).Invoke();
        TopSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
        MapCanvas.PaintSurface += (_,e) => UIUtil.MapCanvas_PaintSurface(e);
      }
      void ResizeMap() {
        double mapScaleFactor = UIUtil.SolveMapScale(ContentGrid.RenderSize.Height, MainGrid.RenderSize) * GetZoomFactor();
        StateInfo.ResizeElem(StateInfoPanel, UIUtil.GetScaleFactor(ContentGrid.RenderSize with { Height = 0 }), mapScaleFactor);
        RelayoutCountryPostUI(mapScaleFactor);
        if (mapScaleFactor != lastScaleFactor) {
          RescaleMap(mapScaleFactor);
          lastScaleFactor = mapScaleFactor;
        }
        mapScale = mapScaleFactor;
        void RescaleMap(double scaleFactor) {
          ScaleTransform mapScaleTransform = new() { ScaleX = mapScaleFactor, ScaleY = mapScaleFactor };
          CountryPostsPanel.Margin = new(0, 0, MainGrid.RenderSize.Width * (scaleFactor - 1), CountryPostsPanel.Height * (scaleFactor - 1));
          CountryPostsPanel.RenderTransform = mapScaleTransform;
          MapCanvas.Width = UIUtil.mapSize.Width * scaleFactor;
          MapCanvas.Height = UIUtil.mapSize.Height * scaleFactor;
          MapElementsCanvas.Margin = new(0, 0, MapPanel.RenderSize.Width / scaleFactor * (scaleFactor - 1), MapPanel.RenderSize.Height / scaleFactor * (scaleFactor - 1));
          MapElementsCanvas.RenderTransform = mapScaleTransform;
          MapAnimationElementsCanvas.Margin = new(0, 0, MapPanel.RenderSize.Width / scaleFactor * (scaleFactor - 1), MapPanel.RenderSize.Height / scaleFactor * (scaleFactor - 1));
          MapAnimationElementsCanvas.RenderTransform = mapScaleTransform;
          TurnLogPanel.Margin = new(UIUtil.infoFrameWidth * scaleFactor, 0, 0, 0);
          TurnLogPanel.RenderTransform = mapScaleTransform;
          TurnWinCondPanel.Margin = new(UIUtil.infoFrameWidth * scaleFactor, UIUtil.infoFrameWidth * scaleFactor, 0, 0);
          MovePersonCanvas.RenderTransform = mapScaleTransform;
          RescaleTurnWinCondPanelUI(scaleFactor);
        }
        void RelayoutCountryPostUI(double scaleFactor) {
          double PostPanelLeftUnit = (MainGrid.RenderSize.Width / scaleFactor - countryPostPanelWidth) / (countryPostPanelMap.Count - 1);
          countryPostPanelMap.Values.Select((elem, index) => (elem, index)).ToList().ForEach(v => Canvas.SetLeft(v.elem, PostPanelLeftUnit * v.index));
        }
      }
      void ResizeInfo() {
        InfoLayoutPanel.Margin = new(0, 0, InfoFramePanel.RenderSize.Width / mapScale * (mapScale - 1), InfoFramePanel.RenderSize.Height / mapScale * (mapScale - 1));
        InfoLayoutPanel.RenderTransform = new ScaleTransform() { ScaleX = mapScale, ScaleY = mapScale };
        InfoLayoutPanel.Width = InfoFramePanel.RenderSize.Width / mapScale;
      }
      void SetCountryPostsPanel() {
        Dictionary<ERole, Color> countryRolePanelColorMap = new([
          new(ERole.Central,new Color(255,240,240,210)),new(ERole.Affair,new Color(255,240,240,240)),
            new(ERole.Defense,new Color(255,210,210,240)),new(ERole.Attack,new Color(255,240,210,210))
        ]);
        Dictionary<ERole, Grid> rolePanelMap = countryRolePanelColorMap.ToDictionary(v => v.Key, v => CreateCountryPostPanel(v.Key, v.Value));
        rolePanelMap.ToList().ForEach(v => v.Value.PointerEntered += (_, _) => UpdateCountryPostPanelZIndex(v.Key));
        countryPostPanelMap.Clear();
        rolePanelMap.ToList().ForEach(v => countryPostPanelMap.Add(v.Key, v.Value));
        CountryPostsPanel.Children.Clear();
        rolePanelMap.Reverse().ToList().ForEach(v => CountryPostsPanel.Children.Add(v.Value));
      }
      void RefreshViewMode() {
        SwitchViewModeButtonText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "▼" : "▲";
        ContentGrid.MaxWidth = UIUtil.GetContentMaxWidth();
      }
    }
  }
  private async Task InitGame(GameState newGameState) {
    await SetInitUI(newGameState);
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateAreaPanels(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateCountryPosts(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateCountryInfoPanel(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => CleanUI(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => GameLog.UpdateLogMessageUI(newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowCharacterRemark(newGameState));
    GameData.game = newGameState;
    GameData.startGameDateTime = DateTime.Now;
    void CleanUI(GameState game) {
      UIUtil.SetVisibility(AskPanel,false);
      UIUtil.SetVisibility(CharacterRemarkPanel,false);
      MovePersonCanvas.MySetChildren([]);
      MapAnimationElementsCanvas.MySetChildren([]);
    }
    async Task SetInitUI(GameState game) {
      areaElemsMap.Clear();
      game.AreaMap.ToDictionary(v => v.Key, v => CreateAreaElems(game, v.Key)).ToList().ForEach(v => areaElemsMap.Add(v.Key, v.Value));
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,MapElementsCanvas.Children.Clear);
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => game.NowScenario?.MyApplyF(ScenarioBase.GetScenarioData)?.RoadConnections.ToList().ForEach(road => MapElementsCanvas.Children.Add(MaybeCreateRoadLine(game,road))));
      await Task.Yield();
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => areaElemsMap.ToList().ForEach(v => MapElementsCanvas.Children.Add(CreateAreaPanelFromAreaElems(game,v))));
      static Line? MaybeCreateRoadLine(GameState game,Road road) {
        return Area.GetAreaPoint(game,road.From,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth) is Point from && Area.GetAreaPoint(game,road.To,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth) is Point to ? CreateRoadLine(road,from,to) : null;
        static Line CreateRoadLine(Road road,Point from,Point to) => new() { X1 = from.X,Y1 = from.Y,X2 = to.X,Y2 = to.Y,Stroke = (road.Kind switch { RoadKind.Land => UIUtil.landRoadColor, RoadKind.Water => UIUtil.waterRoadColor }).ToBrush(),StrokeThickness = 5 * Math.Pow(road.Easiness,1.7) / 2 + 10 };
      }
      static AreaElems CreateAreaElems(GameState game,EArea area) {
        Border areaBackBorder = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height,CornerRadius = UIUtil.areaCornerRadius,BorderBrush = Colors.Red };
        TextBlock areaNameText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,1,0,-1) };
        StackPanel areaDefensePersonPanel = [];
        StackPanel areaAffairPersonPanel = [];
        Post areaDefensePost = new(ERole.Defense,new(area));
        Post areaAffairPost = new(ERole.Affair,new(area));
        TextBlock affairText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,-1) };
        TextBlock exText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,-1) };
        Grid areaWrapPanel = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height };
        return new(areaBackBorder,areaNameText,areaDefensePersonPanel,areaAffairPersonPanel,areaDefensePost,areaAffairPost,affairText,exText,areaWrapPanel);
      }
      Grid CreateAreaPanelFromAreaElems(GameState game,KeyValuePair<EArea,AreaElems> areaElemInfo) {
        Grid areaPanel = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height,CornerRadius = UIUtil.areaCornerRadius };
        StackPanel personPutAreaPanel = new() {
          HorizontalAlignment = HorizontalAlignment.Center,Orientation = Orientation.Horizontal,
          BorderBrush = GetPostFrameColor(game,areaElemInfo.Key).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth),Margin = new(0,-1)
        };
        areaPanel.PointerPressed += (_,_) => PushArea.Push(areaElemInfo.Key);
        areaPanel.PointerExited += (_,_) => PushArea.Exit();
        areaPanel.PointerReleased += (_,_) => GameData.game = PushArea.Release(this,GameData.game,areaElemInfo.Key);
        Area.GetAreaPoint(game,areaElemInfo.Key,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth)?.MyApplyA(v => Canvas.SetLeft(areaPanel,v.X - UIUtil.areaSize.Width / 2)).MyApplyA(v => Canvas.SetTop(areaPanel,v.Y - UIUtil.areaSize.Height / 2));
        return areaPanel.MySetChildren([
          areaElemInfo.Value.Back,
          new StackPanel{ Width = UIUtil.areaSize.Width,VerticalAlignment = VerticalAlignment.Center }.MySetChildren([
            areaElemInfo.Value.AreaNameText,
            personPutAreaPanel.MySetChildren([
              CreatePersonPutPanel(game,areaElemInfo.Value.DefensePost,Text.AreaPostDefenseText(),areaElemInfo.Value.DefensePersonPanel),
              CreatePersonPutPanel(game,areaElemInfo.Value.AffairPost,Text.AreaPostAffairText(),areaElemInfo.Value.AffairPersonPanel)
            ]),
            areaElemInfo.Value.AffairText,
            areaElemInfo.Value.ExText
          ]),
          areaElemInfo.Value.WrapPanel
        ]);
      }
    }
  }
  private void UpdateAreaPanels(GameState game) {
    double capitalBorderWidth = 1.75;
    areaElemsMap.ToList().ForEach(areaElems => {
      AreaData? areaData = game.AreaMap.GetValueOrDefault(areaElems.Key);
      areaElems.Value.Back.BorderThickness = new(game.CountryMap.Values.Select(v => v.CapitalArea).Contains(areaElems.Key) ? capitalBorderWidth : 0);
      areaElems.Value.Back.Background = Country.GetCountryColor(game,areaData?.Country).ToBrush();
      areaElems.Value.AreaNameText.Text = Text.AreaText(areaElems.Key, areaData?.Country);
      areaElems.Value.AreaNameText.RenderTransform = new ScaleTransform { ScaleX = Math.Min(1,5 / UIUtil.CalcFullWidthTextLength(areaElems.Value.AreaNameText.Text)),CenterX = UIUtil.CalcFullWidthTextLength(areaElems.Value.AreaNameText.Text) * BasicStyle.fontsize / 2 };
      areaElems.Value.DefensePersonPanel.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == areaElems.Value.DefensePost)?.MyApplyF(param => CreatePersonPanel(game,param)).MyMaybeToList() ?? []]);
      areaElems.Value.AffairPersonPanel.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == areaElems.Value.AffairPost)?.MyApplyF(param => CreatePersonPanel(game,param)).MyMaybeToList() ?? []]);
      areaElems.Value.AffairText.Text = $"{Math.Floor(areaData?.AffairParam.AffairNow ?? 0)}/{Math.Floor(areaData?.AffairParam.AffairsMax ?? 0)}";
      areaElems.Value.ExText.Text = areaData?.Country?.MyApplyF(country => (Country.IsSleep(game,country) ? Text.CountrySleepText(game, country) : null) + (Country.IsFocusDefense(game,country) ? Text.CountryFocusDefenseText() : null));
      areaElems.Value.WrapPanel.Background = Area.IsPlayerSelectable(game,areaElems.Key) ? null : UIUtil.grayoutColor.ToBrush();
    });
  }
  private void UpdateCountryPosts(GameState game) {
    playerCountryPostPersonPanel.ToList().ForEach(countryPostInfo => countryPostInfo.Value.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Country == game.PlayCountry && v?.Value.Post == countryPostInfo.Key)?.MyApplyF(param => CreatePersonPanel(game,param)).MyMaybeToList() ?? []]));
    UIUtil.SetVisibility(CountryPostsPanel,IsShowCountryPostPanel(game.Phase));
    static bool IsShowCountryPostPanel(Phase phase) => phase is Phase.Planning or Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd;
  }
  private void UpdateCountryInfoPanel(GameState game) {
    List<UIElement> contents = game.Phase switch {
      Phase.Starting => ShowSelectScenario(game),
      Phase.Planning or Phase.Execution => ShowCountryInfo(game),
      Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => ShowEndGameInfo(game)
    };
    string? buttonText = Text.EndPhaseButtonText(game.Phase);
    StateInfo.Show(StateInfoPanel,contents,buttonText,() => GameData.game = ButtonAction(GameData.game));
    GameState ButtonAction(GameState game) {
      taskTokens.Clear();
      return game.Phase switch {
        Phase.Starting => throw new Exception(),
        Phase.Planning => game.MyApplyF(EndPlanningPhase).MyApplyA(UpdateCountryInfoPanel),
        Phase.Execution => game.MyApplyF(EndExecutionPhase).MyApplyA(UpdateCountryInfoPanel),
        Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => game.MyApplyA(ShowGameEndLogButtonClick)
      };
    }
    List<UIElement> ShowSelectScenario(GameState game) => [
      new Grid().MySetChildren([
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,0)),
        new TextBlock { Text=Text.ScenarioCaptionText(),VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,2)),
        new ComboBox {
          Width=100, VerticalAlignment=VerticalAlignment.Stretch,
          SelectedIndex=ScenarioBase.GetScenarioIds().MyGetIndex(v=>v==game.NowScenario)??0,
          Foreground=Colors.Black, Background=Colors.White, Padding=new(10,0,0,0), Margin=new(0,1),
          ItemContainerStyle = new Style(typeof(ComboBoxItem)).MyApplyA(style=>style.Setters.Add(new Setter(FontSizeProperty,BasicStyle.fontsize*UIUtil.GetScaleFactor(MainGrid.RenderSize)))),
        }.MyApplyA(elem => ScenarioBase.GetScenarioIds().Select(v=>v.Value).ToList().ForEach(elem.Items.Add)).MyApplyA(v=>
          v.SelectionChanged+=(_,_)=>(v.SelectedItem as string)?.MyApplyA(async text => await InitGame(GetGame.GetInitGameScenario(new(text))))
        ).MyApplyA(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,4)),
        new TextBlock{ Text=game.NowScenario?.MyApplyF(ScenarioBase.GetScenarioData)?.MyApplyF(Text.StartYearText),VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,5)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,6)),
        new TextBlock{ Text=game.NowScenario?.MyApplyF(ScenarioBase.GetScenarioData)?.MyApplyF(Text.EndYearText),VerticalAlignment=VerticalAlignment.Center}.MyApplyA(v=>Grid.SetColumn(v,7)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,8)),
        new TextBlock{ Text=Text.ClickMapAreaText(),VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,9)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,10)),
     ]).MyApplyA(v=>new List<ColumnDefinition>([
        new() { Width = new GridLength(5, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(5, GridUnitType.Star) },
      ]).ForEach(v.ColumnDefinitions.Add))];
    static List<UIElement> ShowCountryInfo(GameState game) => [
      new Grid().MySetChildren([
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,0)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=Text.GetCalendarText(game) }, new TextBlock{ Text=Text.PlayCountryParamText(game) },
        ]).MyApplyA(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,2)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=Text.PlayCountryCapitalAreaParamText(game) }, new TextBlock{ Text=Text.PlayCountryFundParamText(game) },
        ]).MyApplyA(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,4)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=Text.PlayCountryAreaNumParamText(game) }, new TextBlock{ Text=Text.PlayCountryAffairDifficultParamText(game) },
        ]).MyApplyA(v=>Grid.SetColumn(v,5)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,6)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=Text.PlayCountryAffairPowerParamText(game) }, new TextBlock{ Text=Text.PlayCountryTotalAffairParamText(game) },
        ]).MyApplyA(v=>Grid.SetColumn(v,7)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,8)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=Text.PlayCountryInFundParamText(game) }, new TextBlock{ Text=Text.PlayCountryOutFundParamText(game) },
        ]).MyApplyA(v=>Grid.SetColumn(v,9)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,10)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=Text.PlayCountryArmyTargetAreaParamText(game) },
        ]).MyApplyA(v=>Grid.SetColumn(v,11)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,12)),
      ]).MyApplyA(v=>new List<ColumnDefinition>([
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(10, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
      ]).ForEach(v.ColumnDefinitions.Add))
    ];
    static List<UIElement> ShowEndGameInfo(GameState game) => [
      new Grid().MySetChildren([
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,0)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=Text.GetCalendarText(game) }, new TextBlock{ Text=Text.PlayCountryParamText(game) },
        ]).MyApplyA(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,2)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=Text.GameEndText() }, new TextBlock{ Text=Text.GameResultText(game) },
        ]).MyApplyA(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,4)),
      ]).MyApplyA(v=>new List<ColumnDefinition>([
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(10, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(10, GridUnitType.Star) },
      ]).ForEach(v.ColumnDefinitions.Add))
    ];
    void ShowGameEndLogButtonClick(GameState game) {
      string title = Text.GameEndLogCaptionText();
      List<TextBlock> contents = [.. game.GameLog.Select(log => new TextBlock { Text = log })];
      string okButtonText = Text.PostGameEndLogText();
      Ask.SetElems(AskPanel,title,contents,okButtonText,() => LogButtonClick(game),false);
      static void LogButtonClick(GameState game) {
        string url = $"https://karintougames.com/siteContents/gameComment.php?caption={BaseData.name.Value} ver.{BaseData.version.Value}&comment={HttpUtility.UrlEncode(string.Join('\n',game.GameLog))}";
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS($"top.location.href='{url}';");
#else
        _ = Windows.System.Launcher.LaunchUriAsync(new Uri(url));
#endif
      }
    }
    GameState EndPlanningPhase(GameState game) {
      ResetTurnUI();
      return game.MyApplyF(game => UpdateGame.AutoPutPostCPU(game,[ECountry.漢])).MyApplyF(CalcArmyTarget).MyApplyF(game => game with { Phase = Phase.Execution })
        .MyApplyA(UpdateAreaPanels).MyApplyA(ExecutionMoveFlag).MyApplyF(ArmyAttack).MyApplyA(GameLog.UpdateLogMessageUI).MyApplyA(ShowCharacterRemark);
      void ResetTurnUI() => new List<UIElement>([CharacterRemarkPanel,CountryPostsPanel]).ForEach(v => UIUtil.SetVisibility(v,false));
      static GameState CalcArmyTarget(GameState game) {
        Dictionary<ECountry,EArea?> playerArmyTargetMap = game.PlayCountry.MyMaybeToList().Where(country => !Country.IsSleep(game,country)).ToDictionary(v => v,v => game.ArmyTargetMap.GetValueOrDefault(v));
        Dictionary<ECountry,EArea?> NPCArmyTargetMap = game.CountryMap.Keys.Except(game.PlayCountry.MyMaybeToList()).Where(country => !Country.IsSleep(game,country)).ToDictionary(country => country,country => country == ECountry.漢 ? null : RandomSelectNPCAttackTarget(game,country));
        return game with { ArmyTargetMap = new([.. NPCArmyTargetMap,.. playerArmyTargetMap]) };
        static EArea? RandomSelectNPCAttackTarget(GameState game,ECountry country) {
          List<EArea> targetAreas = Area.GetCellEachAdjacentAnotherCountryAreas(game,country);
          Dictionary<EArea,int> targetAreaCountMap = targetAreas.CountBy(v => v).ToDictionary();
          List<EArea?> selectWeightTargetAreas = [.. targetAreaCountMap.SelectMany(v => Enumerable.Repeat(v.Key,v.Value * v.Value)).MyNullable().Append(null)];
          return selectWeightTargetAreas.MyPickAny().MyApplyF(area => area?.MyApplyF(game.AreaMap.GetValueOrDefault)?.Country == null && MyRandom.RandomJudge(0.9) ? null : area);
        }
      }
      void ExecutionMoveFlag(GameState game) {
        game.ArmyTargetMap.Where(v => v.Value != null && Country.SuccessAttack(game,v.Key)).ToDictionary(attackInfo => GetFlag(game,attackInfo.Key),attackInfo => CalcFlagMovePos(game,attackInfo)).MyApplyA(flagMap => MapAnimationElementsCanvas.MySetChildren([.. flagMap.Keys])).MyApplyA(flagMap => MoveFlags(flagMap));
        static Grid GetFlag(GameState game,ECountry attackCountry) {
          return CreateFlag(game,attackCountry).MyApplyF(v => AttachFlag(game,v,attackCountry));
          static Grid CreateFlag(GameState game,ECountry attackCountry) {
            string flagText = attackCountry.ToString();
            double flagTextMaxLength = 2.2, flagTextScale = 1.6, flagTextLength = UIUtil.CalcFullWidthTextLength(flagText);
            TextBlock flagTextBlock = new() {
              Text = flagText,Width = Math.Min(flagTextMaxLength,flagTextLength) * BasicStyle.fontsize * flagTextScale,
              HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Center,
              RenderTransform = new ScaleTransform { ScaleX = Math.Min(1,flagTextMaxLength / flagTextLength) * flagTextScale,ScaleY = flagTextScale,CenterY = BasicStyle.fontsize / 2 }
            };
            Grid flagPanel = new() { Width = BasicStyle.fontsize * 4,Height = BasicStyle.fontsize * 3,Background = Country.GetCountryColor(game,attackCountry).ToBrush(),HorizontalAlignment = HorizontalAlignment.Left,VerticalAlignment = VerticalAlignment.Top };
            SKXamlCanvas attackImage = new() { Width = BasicStyle.fontsize * 4,Height = BasicStyle.fontsize * 4,HorizontalAlignment = HorizontalAlignment.Right };
            Grid armyCanvas = new Grid() { Width = BasicStyle.fontsize * 6,Height = BasicStyle.fontsize * 5 }.MySetChildren([attackImage,flagPanel.MySetChildren([flagTextBlock])]);
            attackImage.PaintSurface += (_,e) => UIUtil.ArmyCanvas_PaintSurface(e);
            return armyCanvas;
          }
          static Grid AttachFlag(GameState game,Grid rawFlag,ECountry attackCountry) {
            double fontsize = 21, lineheight = 22, shadowWidth = 1.5;
            decimal attackRank = Commander.CommanderRank(game,Commander.GetAttackCommander(game,attackCountry),ERole.Attack);
            Grid attackRankPanel = new Grid() { HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Bottom,Margin = new(0,-shadowWidth,0,shadowWidth) }.MySetChildren([
              .. UIUtil.CreateMargin(shadowWidth).Select(margin => CreateRankText().MyApplyA(v => v.Margin = margin)),CreateRankText().MyApplyA(v => v.Foreground = Colors.White)
            ]);
            return new Grid() { Width = BasicStyle.fontsize * 6,Height = BasicStyle.fontsize * 5,Visibility = Visibility.Collapsed }.MySetChildren([rawFlag,attackRankPanel]);
            TextBlock CreateRankText() => new() { Text = $"Rank{attackRank}",FontSize = fontsize,LineHeight = lineheight };
          }
        }
        static List<Point> CalcFlagMovePos(GameState game,KeyValuePair<ECountry,EArea?> attackInfo) {
          EArea[] route = attackInfo.Value?.MyApplyF(target => Route.SolveAtackArmyRoute(game,attackInfo.Key,target)) ?? [];
          List<(Point from, Point to)> routeAreaPoints = [.. route.Select(v => AreaToPoint(game,v)).MyNonNull().MyAdjacentCombinations()];
          List<Point> posList = [.. routeAreaPoints.SelectMany(v => Enumerable.Range(0,60).Select(index => new Point(double.Lerp(v.from.X,v.to.X,index / 60d),double.Lerp(v.from.Y,v.to.Y,index / 60d)))).MyApplyF(v => v.Chunk(v.Count() / 60).Select(v => v.FirstOrDefault()).Append(routeAreaPoints.LastOrDefault().to).MyNonNull())];
          return posList;
          static Point? AreaToPoint(GameState game,EArea area) => Area.GetAreaPoint(game,area,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth);
        }
        void MoveFlags(Dictionary<Grid,List<Point>> flags) {
          TaskToken token = new TaskToken().MyApplyA(taskTokens.Add);
          DateTime startTime = DateTime.Now;
          Task.Run(async () => {
            await flags.MyAsyncForEachConcurrent(async v => {
              await v.Value.Select((value,index)=>(value, index)).MyAsyncForEachSequential(async pos => {
                double nextWaitSeconds = UIUtil.nextStepDelaySeconds * pos.index - (DateTime.Now - startTime).TotalSeconds;
                if(nextWaitSeconds <= 0) return;
                await Task.Delay(TimeSpan.FromSeconds(nextWaitSeconds));
                if(!taskTokens.Contains(token)) return;
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => {
                  Canvas.SetLeft(v.Key,pos.value.X - v.Key.Width / 2);
                  Canvas.SetTop(v.Key,pos.value.Y - v.Key.Height / 2);
                  v.Key.Visibility = Visibility.Visible;
                });
              });
            });
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateAreaPanels(GameData.game));
          });
        }
      }
      static GameState ArmyAttack(GameState game) {
        return game.CountryMap.Keys.OrderBy(country => Country.GetTotalAffair(game,country)).Aggregate(game,(game,country) => {
          return game.ArmyTargetMap.GetValueOrDefault(country) is EArea target ? TryAttack(game,country,target) : game.ArmyTargetMap.ContainsKey(country) ? ExeDefense(game,country) : ExeRest(game,country);
          static GameState TryAttack(GameState game,ECountry country,EArea targetArea) {
            return Country.SuccessAttack(game,country) ? ExeAttack(game,country,targetArea) : FailAttack(game,country,targetArea);
            static GameState ExeAttack(GameState game,ECountry country,EArea targetArea) => targetArea.MyApplyF(game.AreaMap.GetValueOrDefault)?.Country.MyApplyF(defeseSide => UpdateGame.Attack(game,country,targetArea,defeseSide,Country.IsFocusDefense(game,defeseSide))) ?? game;
            static GameState FailAttack(GameState game,ECountry country,EArea targetArea) => game.MyApplyF(game => UpdateGame.Defense(game,country,true)).MyApplyF(game => country == game.PlayCountry ? UpdateGame.AppendStartExecutionRemark(game,[Text.StartExecutionFailAttackCharacterRemarkText(targetArea)]) : game);
          }
          static GameState ExeDefense(GameState game,ECountry country) => game.MyApplyF(game => UpdateGame.Defense(game,country,false));
          static GameState ExeRest(GameState game,ECountry country) => game.MyApplyF(game => UpdateGame.Rest(game,country));
        });
      }
    }
    GameState EndExecutionPhase(GameState game) {
      MapAnimationElementsCanvas.MySetChildren([]);
      UIUtil.SetVisibility(CharacterRemarkPanel,false);
      UIUtil.SetVisibility(CountryPostsPanel,true);
      return game.MyApplyF(UpdateGame.GameEndJudge).MyApplyF(game => game.Phase is Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd ? game : game.MyApplyF(NextTurn));
      GameState NextTurn(GameState game) {
        return game.MyApplyF(UpdateGame.NextTurn).MyApplyF(v => v with { Phase = Phase.Planning }).MyApplyF(game => UpdateGame.AppendStartPlanningRemark(game,[.. Text.StartPlanningCharacterRemarkTexts(game)]))
          .MyApplyA(UpdateCountryPosts).MyApplyA(UpdateAreaPanels).MyApplyA(UpdateTurnLogUI).MyApplyA(UpdateTurnWinCondUI).MyApplyA(GameLog.UpdateLogMessageUI).MyApplyF(UpdateGame.GameEndJudge).MyApplyA(ShowCharacterRemark).MyApplyF(ResetParam);
      }
      static GameState ResetParam(GameState game) => game with { ArmyTargetMap = [],StartPlanningCharacterRemark = [],StartExecutionCharacterRemark = [] };
    }
  }
  private Grid CreatePersonPutPanel(GameState game,Post post,string backText,StackPanel personPutInnerPanel) {
    Grid personPutPanel = new() { Width = UIUtil.personPutSize.Width,Height = UIUtil.personPutSize.Height,BorderBrush = GetPostFrameColor(game,post.PostKind.MaybeArea).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth),Background = Colors.Transparent };
    TextBlock personPutBackText = new() {
      Text = backText,Foreground = Windows.UI.Color.FromArgb(100,100,100,100),HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Center,
      RenderTransform = new ScaleTransform() { ScaleX = UIUtil.personPutFontScale,ScaleY = UIUtil.personPutFontScale,CenterX = UIUtil.CalcFullWidthTextLength(backText) * BasicStyle.fontsize / 2,CenterY = BasicStyle.fontsize / 2 }
    };
    personPutPanel.PointerEntered += (_,_) => EnterPersonPutPanel(GameData.game,personPutInnerPanel,post);
    personPutPanel.PointerExited += (_,_) => ExitPersonPutPanel(personPutInnerPanel);
    return personPutPanel.MySetChildren([personPutBackText,personPutInnerPanel]);
    void EnterPersonPutPanel(GameState game,StackPanel personPutInnerPanel,Post post) {
      if(game.Phase != Phase.Starting && (post.PostKind.MaybeArea?.MyApplyF(area => game.AreaMap.GetValueOrDefault(area)?.Country == game.PlayCountry) ?? true)) {
        pointerover?.MyApplyA(v => v.panel.Background = Colors.Transparent);
        personPutInnerPanel.Background = Windows.UI.Color.FromArgb(150,255,255,255);
        pointerover = (personPutInnerPanel, post);
      }
    }
    void ExitPersonPutPanel(StackPanel personPutInnerPanel) {
      if(pointerover != null) {
        personPutInnerPanel.Background = Colors.Transparent;
        pointerover = null;
      }
    }
  }
  private Grid CreatePersonPanel(GameState game,KeyValuePair<PersonId,PersonData> person) {
    double minFullWidthLength = 2.25;
    double margin = FontSize * (UIUtil.CalcFullWidthTextLength(person.Key.Value) - 2);
    Grid panel = new Grid { Width = UIUtil.personPutSize.Width,Height = UIUtil.personPutSize.Height,Background = Country.GetCountryColor(game,Person.GetPersonCountry(game,person.Key)).ToBrush() }.MySetChildren([
      new StackPanel { HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,Background=Windows.UI.Color.FromArgb((byte)(20*Person.GetPersonRank(game,person.Key)),0,0,0) }.MySetChildren([
        GetRankPanel(game,person),
        new TextBlock { Text=person.Key.Value,TextAlignment=TextAlignment.Center,Margin=new(-margin/2,0),RenderTransform=new ScaleTransform{ ScaleX=minFullWidthLength/Math.Max(minFullWidthLength,UIUtil.CalcFullWidthTextLength(person.Key.Value))*UIUtil.personNameFontScale,ScaleY=UIUtil.personNameFontScale,CenterX=UIUtil.personPutSize.Width/2+margin/minFullWidthLength  }  }
      ])
    ]);
    panel.PointerPressed += (_,e) => PickPersonPanel(GameData.game,e,panel,person.Key);
    return panel;
    StackPanel GetRankPanel(GameState game,KeyValuePair<PersonId,PersonData> person) {
      int postRank = Person.CalcRoleRank(game,person.Key,person.Value.Post?.PostRole);
      int personRank = Person.GetPersonRank(game,person.Key);
      return new StackPanel { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center,RenderTransform = new ScaleTransform() { ScaleX = UIUtil.personRankFontScale,ScaleY = UIUtil.personRankFontScale,CenterX = FontSize / 2 } }.MySetChildren(GetRankTextBlock(game,person.Key,postRank,personRank == postRank));
      static List<UIElement> GetRankTextBlock(GameState game,PersonId person,int rank,bool isMatchRole) => [
        new TextBlock() { Margin = new(0,-1.25,0,0),Text = rank.ToString(),Foreground = isMatchRole ? Colors.Black : Colors.Red },
        .. (isMatchRole?null:new Image { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{Person.GetPersonRole(game,person)}.svg")),VerticalAlignment = VerticalAlignment.Top,Width = BasicStyle.textHeight * 0.75,Height = BasicStyle.textHeight * 0.75 }).MyMaybeToList()
      ];
    }
    void PickPersonPanel(GameState game,PointerRoutedEventArgs e,Panel personPanel,PersonId person) {
      if(game.Phase != Phase.Starting && Person.GetPersonCountry(game,person) == game.PlayCountry && personPanel.Parent is Panel parentPanel) {
        personPanel.IsHitTestVisible = false;
        parentPanel.MySetChildren([]);
        MovePersonCanvas.MySetChildren([personPanel]);
        MovePerson(personPanel,e);
        pick = (parentPanel, person);
      }
    }
  }
  private void MovePerson(UIElement personPanel,PointerRoutedEventArgs e) {
    Canvas.SetLeft(personPanel,e.GetCurrentPoint(MovePersonCanvas).Position.X - UIUtil.personPutSize.Width / 2);
    Canvas.SetTop(personPanel,e.GetCurrentPoint(MovePersonCanvas).Position.Y - UIUtil.personPutSize.Height / 2);
  }
  private Grid CreateCountryPostPanel(ERole role,Color backColor) {
    return new Grid() { Width = countryPostPanelWidth, Background = backColor.ToBrush() }.MySetChildren([
      new StackPanel() {HorizontalAlignment = HorizontalAlignment.Center }.MySetChildren([
        new StackPanel() { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center }.MySetChildren([
          new TextBlock { Text = Text.RoleToText(role) },
          new Image { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{role}.svg")),Width = BasicStyle.textHeight,Height = BasicStyle.textHeight,VerticalAlignment = VerticalAlignment.Center }
        ]),
        CreateCountryPosts(GameData.game,role)
      ])
    ]);
    StackPanel CreateCountryPosts(GameState game,ERole role) {
      return new StackPanel().MySetChildren([
        new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
            CreatePersonHeadPostPanel(game,role),
            CreateAutoPutPersonButton(game,role)
          ]),
          CreatePersonPostPanelElems(game,role)
      ]);
      Button CreateAutoPutPersonButton(GameState game,ERole role) {
        Button autoPutPersonButton = new Button { Width = UIUtil.personPutSize.Width * 3,VerticalAlignment = VerticalAlignment.Stretch,Background = Windows.UI.Color.FromArgb(100,100,100,100) }.MyApplyA(v => v.Content = new TextBlock { Text = Text.AutoPutPersonButtonText() });
        autoPutPersonButton.Click += (_,_) => GameData.game = AutoPutPersonButtonClick(GameData.game);
        return autoPutPersonButton;
        GameState AutoPutPersonButtonClick(GameState game) => game.PlayCountry?.MyApplyF(country => Code.Post.GetAutoPutPost(game,country,role)).MyApplyF(postMap => UpdateGame.SetPersonPost(game,postMap)).MyApplyA(v => UpdateAreaPanels(v)).MyApplyA(game => UpdateCountryPosts(game)) ?? game;
      }
      StackPanel CreatePersonHeadPostPanel(GameState game,ERole role) {
        return new StackPanel { Orientation = Orientation.Horizontal,BorderBrush = GetPostFrameColor(game,null).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) }.MySetChildren([
          .. Enum.GetValues<PostHead>().Select(v=> new Post(role,new(v)).MyApplyF(post=> CreatePersonPutPanel(game,post,Text.PlayerCountryPostText(post.PostKind),playerCountryPostPersonPanel.GetValueOrDefault(post)??[]))),
          ]);
      }
      StackPanel CreatePersonPostPanelElems(GameState game,ERole role) {
        return new StackPanel { BorderBrush = GetPostFrameColor(game,null).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) }.MySetChildren([.. Enumerable.Range(0,UIUtil.capitalPieceRowNum).Select(row => GetPersonPostLinePanel(game,role,row,game.PersonMap.Where(v => Person.GetPersonCountry(game,v.Key) == game.PlayCountry).ToDictionary()))]);
        StackPanel GetPersonPostLinePanel(GameState game,ERole role,int rowNo,Dictionary<PersonId,PersonData> personMap) => new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
          .. Enumerable.Range(0,UIUtil.capitalPieceColumnNum).Select(i => new Post(role,new(rowNo * UIUtil.capitalPieceColumnNum + i)).MyApplyF(post=> CreatePersonPutPanel(game,post,Text.PlayerCountryPostText(post.PostKind),playerCountryPostPersonPanel.GetValueOrDefault(post) ?? [])))
        ]);
      }
    }
  }
  private static Color GetPostFrameColor(GameState game,EArea? area) => area != null && (game.NowScenario?.MyApplyF(ScenarioBase.GetScenarioData)?.ChinaAreas ?? []).Contains(area.Value) ? new Color(150,100,100,30) : new Color(120,0,0,0);
  private void RescaleTurnWinCondPanelUI(double scaleFactor) {
    TurnWinCondPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = TurnWinCondPanel.RenderSize.Width / 2 };
  }
  private async void UpdateTurnLogUI(GameState game) {
    DateTime startTime = DateTime.Now;
    TimeSpan startAnimationDelay = TimeSpan.FromSeconds(6);
    int transparentFrameCount = 60;
    StackPanel panel = new StackPanel() {
      Background = Windows.UI.Color.FromArgb(187,255,255,255),Height = game.TrunNewLog.Count * BasicStyle.textHeight,IsHitTestVisible = false
    }.MySetChildren([.. game.TrunNewLog.Select(logText => new TextBlock() { Text = logText })]);
    game.TrunNewLog.Clear();
    TurnLogPanel.Children.Add(panel);
    ResizeTurnLogUI();
    await Task.Run(async () => {
      await Task.Delay(startAnimationDelay);
      await Enumerable.Range(1,transparentFrameCount).MyAsyncForEachSequential(async v => {
        double nextWaitSeconds = UIUtil.nextStepDelaySeconds * v - (DateTime.Now - startTime).TotalSeconds;
        if(nextWaitSeconds <= 0) return;
        await Task.Delay(TimeSpan.FromSeconds(nextWaitSeconds));
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => panel.Opacity = 1 - (double)v / transparentFrameCount);
      });
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,()=>TurnLogPanel.Children.Remove(panel));
      await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,ResizeTurnLogUI);
    });
    void ResizeTurnLogUI() => TurnLogPanel.Height = TurnLogPanel.Children.OfType<FrameworkElement>().Sum(v => v.Height) * UIUtil.GetScaleFactor(MainGrid.RenderSize) *GetZoomFactor();
  }
  private async void UpdateTurnWinCondUI(GameState game) {
    DateTime startTime = DateTime.Now;
    TimeSpan startAnimationDelay = TimeSpan.FromSeconds(6);
    int transparentFrameCount = 60;
    double shadowWidth = 0.7;
    Dictionary<string,bool?> winCondMap = game.PlayCountry?.MyApplyF(v => game.NowScenario?.MyApplyF(ScenarioBase.GetScenarioData)?.WinConditionMap.GetValueOrDefault(v))?.ProgressExplainFunc(game) ?? [];
    StackPanel panel = new StackPanel() { Background = Windows.UI.Color.FromArgb(187,255,255,255),IsHitTestVisible = false }.MySetChildren([
      new TextBlock() { Text = Text.WinCondCaptionText(game) },
        .. winCondMap.Select(winCond => new StackPanel(){ Orientation = Orientation.Horizontal }.MySetChildren([
          new Grid(){ Width = BasicStyle.fontsize }.MySetChildren(winCond.Value is bool isClearCond? [
            ..UIUtil.CreateMargin(shadowWidth).Select(margin => CreateWinCondCheckText(isClearCond).MyApplyA(v => v.Margin = margin)),CreateWinCondCheckText(isClearCond)
          ]:[]),
          new TextBlock() { Text = winCond.Key }
        ]))
    ]);
    TurnWinCondPanel.MySetChildren([panel]);
    RescaleTurnWinCondPanelUI(UIUtil.GetScaleFactor(MainGrid.RenderSize) * GetZoomFactor());
    await Task.Run(async () => {
      await Task.Delay(startAnimationDelay);
      await Enumerable.Range(1,transparentFrameCount).MyAsyncForEachSequential(async v => {
        double nextWaitSeconds = UIUtil.nextStepDelaySeconds * v - (DateTime.Now - startTime).TotalSeconds;
        if(nextWaitSeconds <= 0) return;
        await Task.Delay(TimeSpan.FromSeconds(nextWaitSeconds));
        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => panel.Opacity = 1 - (double)v / transparentFrameCount);
      });
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => TurnWinCondPanel.Children.Remove(panel));
    });
    static TextBlock CreateWinCondCheckText(bool isClear) => new() { Text = isClear ? "✓" : "✗",Foreground = isClear ? Colors.Green : Colors.Red };
  }
  private void ShowMessage(string[] messages) {
    StackPanel panel = new StackPanel() {
      Background = Windows.UI.Color.FromArgb(187,255,255,255),
      Height = messages.Length * BasicStyle.textHeight,
      IsHitTestVisible = false
    }.MySetChildren([.. messages.Select(logText => new TextBlock() { Text = logText })]).MyApplyA(async elem => {
      elem.Opacity = 1;
      await Task.Delay(6000);
      await Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).MyAsyncForEachSequential(async v => {
        await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => { elem.Opacity = 1 - v; });
        await Task.Delay(15);
      });
      TurnLogPanel.Children.Remove(elem);
      ResizeTurnLogUI();
    });
    TurnLogPanel.Children.Add(panel);
    ResizeTurnLogUI();
    void ResizeTurnLogUI() => TurnLogPanel.Height = TurnLogPanel.Children.OfType<FrameworkElement>().Sum(v => v.Height) * UIUtil.GetScaleFactor(MainGrid.RenderSize) * GetZoomFactor();
  }
  private static double GetZoomFactor() => Math.Pow(1.1,zoomLevel);
  private GameState PutPersonPanel(GameState game) {
    if(pick != null) {
      GameState newGameState = game.MyApplyF(SwapPerson).MyApplyF(PutPerson);
      MovePersonCanvas.MySetChildren([]);
      pick = null;
      UpdateCountryInfoPanel(newGameState);
      return newGameState;
    } else {
      return game;
    }
    GameState SwapPerson(GameState game) {
      KeyValuePair<PersonId,PersonData>? maybeDestPersonInfo = game.PersonMap.MyNullable().FirstOrDefault(v => Person.GetPersonCountry(game,v?.Key ?? new(string.Empty)) == game.PlayCountry && v?.Value.Post == pointerover?.post);
      return UpdateGame.PutPersonFromUI(game,maybeDestPersonInfo?.Key,pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == maybeDestPersonInfo?.Key)?.MyApplyF(destPersonInfo => pick?.panel.MySetChildren([CreatePersonPanel(game,destPersonInfo)])));
    }
    GameState PutPerson(GameState game) {
      return UpdateGame.PutPersonFromUI(game,pick?.person,pointerover?.post ?? pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == pick?.person)?.MyApplyF(putPersonInfo => (pointerover?.panel ?? pick?.panel)?.MySetChildren([CreatePersonPanel(game,putPersonInfo)])));
    }
  }
  private void UpdateCountryPostPanelZIndex(ERole toActiveRole) {
    if(activePanelRole == countryPostPanelMap.MyNullable().FirstOrDefault(v => v?.Value == CountryPostsPanel.Children.LastOrDefault() as Grid)?.Key) {
      List<Grid> resetZIndexPanels = GetResetZIndexPanels((activePanelRole, toActiveRole) switch {
        (ERole.Central, ERole.Affair) => [ERole.Affair],
        (ERole.Central, ERole.Defense) => [ERole.Affair,ERole.Defense],
        (ERole.Central, ERole.Attack) => [ERole.Affair,ERole.Defense,ERole.Attack],
        (ERole.Affair, ERole.Central) => [ERole.Central],
        (ERole.Affair, ERole.Defense) => [ERole.Defense],
        (ERole.Affair, ERole.Attack) => [ERole.Defense,ERole.Attack],
        (ERole.Defense, ERole.Central) => [ERole.Affair,ERole.Central],
        (ERole.Defense, ERole.Affair) => [ERole.Affair],
        (ERole.Defense, ERole.Attack) => [ERole.Attack],
        (ERole.Attack, ERole.Central) => [ERole.Defense,ERole.Affair,ERole.Central],
        (ERole.Attack, ERole.Affair) => [ERole.Defense,ERole.Affair],
        (ERole.Attack, ERole.Defense) => [ERole.Defense],
        _ => []
      });
      if(resetZIndexPanels.Count != 0) {
        resetZIndexPanels.ToList().ForEach(v => {
          CountryPostsPanel.Children.Remove(v);
          CountryPostsPanel.Children.Add(v);
        });
        activePanelRole = toActiveRole;       
      }
    }
    static List<Grid> GetResetZIndexPanels(ERole[] resetZIndexRoles) => resetZIndexRoles.Select(countryPostPanelMap.GetValueOrDefault).MyNonNull();
  }
  private void ShowCharacterRemark(GameState game) => CharacterRemark.Show(CharacterRemarkPanel,game);
  private static class PushArea {
    private static EArea? pushArea = null;
    internal static void Push(EArea area) => pushArea = area;
    internal static void Exit() => pushArea = null;
    internal static GameState Release(Game page,GameState game,EArea area) {
      ECountry? areaCountry = game.AreaMap.GetValueOrDefault(area)?.Country;
      return pushArea != area ? game : game.Phase == Phase.Starting ? ShowSelectPlayCountryPanel(game,areaCountry) : Area.IsPlayerSelectable(game,area) ? SelectTarget(game,areaCountry != game.PlayCountry ? area : null) : game;
      GameState ShowSelectPlayCountryPanel(GameState game,ECountry? pushCountry) {
        string title = $"{Text.CountryText(pushCountry)}陣営";
        List<TextBlock> contents = [
          .. (game.NowScenario?.MyApplyF(ScenarioBase.GetScenarioData)?.MyApplyF(scenario=>$"シナリオ:{game.NowScenario.Value}({scenario.StartYear}年開始 {scenario.EndYear}年終了)")).MyMaybeToList().Select(Make),
            Make($"[陣営初期情報]"),
            Make($"首都:{Country.GetCapitalArea(game, pushCountry)?.ToString()??"(なし)"}"),
            Make($"資金:{Country.GetFund(game, pushCountry):0.####}"),
            Make($"内政力:{Country.GetAffairPower(game, pushCountry):0.####}"),
            Make($"内政難度:{Country.GetAffairDifficult(game, pushCountry):0.####}"),
            Make($"総内政値:{Country.GetTotalAffair(game, pushCountry):0.####}"),
            Make($"支出:{Country.GetOutFund(game, pushCountry):0.####}"),
            Make($"収入:{Country.GetInFund(game, pushCountry):0.####}"),
            Make("[勝利条件]"),
            .. (pushCountry?.MyApplyF(country=>game.NowScenario?.MyApplyF(ScenarioBase.GetScenarioData)?.WinConditionMap.GetValueOrDefault(country)?.Messages.MyApplyF(v =>new List<string>([..v.Basic??[],..v.Extra??[]])))).MyApplyF(v=>v is null?["選べません(勝利条件なし)"]:v.Prepend("以下の条件を全て満たす")).Select(Make),
            Make($"[初期人物]"),
            .. (pushCountry is null?[]:Enum.GetValues<ERole>().SelectMany(role=>Person.GetInitPersonMap(game,pushCountry.Value,role).Keys.OrderBy(v=>Person.GetPersonBirthYear(game,v)).Select(v=>PersonInfoText(game,v)))).MyApplyF(v=>v.MyIsEmpty()?["(人物はいません)"]:v).Select(Make),
          ];
        string okButtonText = pushCountry is ECountry.漢 or null ? $"({Text.CountryText(pushCountry)}陣営は選べません)" : "プレイする";
        Action? okButtonAction = pushCountry is ECountry.漢 or null ? null : () => GameData.game = ClickOkButtonAction(GameData.game,pushCountry.Value);
        Ask.SetElems(page.AskPanel,title,contents,okButtonText,okButtonAction,true);
        return game;
        static TextBlock Make(string text) => new() { Text = text,HorizontalAlignment = HorizontalAlignment.Center };
        static string PersonInfoText(GameState game,PersonId personId) => $"{personId.Value}  {Text.RoleToText(Person.GetPersonRole(game,personId))} ランク{Person.GetPersonRank(game,personId)} 齢{Turn.GetYear(game) - Person.GetPersonBirthYear(game,personId)}";
        GameState ClickOkButtonAction(GameState game,ECountry playCountry) {
          GameState newGameState = SelectPlayCountry(game,playCountry).MyApplyF(StartGame).MyApplyF(game => UpdateGame.AppendLogMessage(game,[Text.TurnHeadLogText(game)])).MyApplyF(game => UpdateGame.AppendStartPlanningRemark(game,[.. Text.StartPlanningCharacterRemarkTexts(game)]));
          newGameState.MyApplyA(page.UpdateCountryInfoPanel).MyApplyA(page.ShowCharacterRemark);
          return newGameState;
        }
        GameState SelectPlayCountry(GameState game,ECountry playCountry) => UpdateGame.AttachGameStartData(game,playCountry).MyApplyA( page.UpdateCountryPosts);
        GameState StartGame(GameState game) {
          GameState newGameState = (game with { Phase = Phase.Planning }).MyApplyF(UpdateGame.AppendGameStartLog);
          newGameState.MyApplyA(page.UpdateAreaPanels).MyApplyA(GameLog.UpdateLogMessageUI).MyApplyA(page.UpdateTurnLogUI).MyApplyA(page.UpdateTurnWinCondUI);
          page.MyApplyA(page => UIUtil.SetVisibility(page.CountryPostsPanel,true));
          return newGameState;
        }
      }
      GameState SelectTarget(GameState game,EArea? area) => game.PlayCountry?.MyApplyF(playCountry => game.Phase == Phase.Planning && !Country.IsSleep(game,playCountry) ? (game with { ArmyTargetMap = game.ArmyTargetMap.MyAdd(playCountry,null).MyUpdate(playCountry,(_,_) => area) ?? game.ArmyTargetMap }).MyApplyA(page.UpdateCountryInfoPanel) : null) ?? game;
    }
  }
}