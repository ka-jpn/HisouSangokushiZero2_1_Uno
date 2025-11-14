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
using System.Web;
using Windows.UI.Core;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Commander = HisouSangokushiZero2_1_Uno.Code.Commander;
using Post = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
using Text = HisouSangokushiZero2_1_Uno.Code.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;

internal sealed partial class Game:Page {
  private enum InfoPanelState { Explain, WinCond, ParamList, ChangeLog, Setting };
  private record AreaElems(Border Back,TextBlock AreaNameText,StackPanel DefensePersonPanel,StackPanel AffairPersonPanel,Post DefensePost,Post AffairPost,TextBlock AffairText,TextBlock ExText,Grid WrapPanel);
  private static readonly Dictionary<Post,StackPanel> playerCountryPostPersonPanel = Enum.GetValues<ERole>().SelectMany(role => new List<(Post, StackPanel)>([
    .. Enum.GetValues<PostHead>().Select(headPost=>(new Post(role,new(headPost)),new StackPanel())),
    .. Enumerable.Range(0,UIUtil.capitalPieceCellNum).Select(cellNo=>(new Post(role,new(cellNo)),new StackPanel()))
  ])).ToDictionary();
  private static readonly List<TaskToken> taskTokens = [];
  private static Dictionary<EArea,AreaElems> areaElemsMap = [];
  internal static UIElement? contentPanel = null;
  public Game() {
    InitializeComponent();
    MyInit(this);
    void MyInit(Game page) {
      contentPanel = page.ContentGrid;
      UIUtil.SwitchViewModeActions.Add(() => RefreshViewMode(page));
      UIUtil.ChangeScaleActions.Add(() => ScalingElements(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize)));
      UIUtil.SaveGameActions.Add(() => {
        ShowMessage(page,["セーブ中.."]);
        Task.Run(async () => {
          await Storage.WriteStorageData(GameData.game,1);
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage(page,["セーブ完了"]));
        });
      });
      UIUtil.LoadGameActions.Add(() => {
        ShowMessage(page,["ロード中.."]);
        Task.Run(async () => {
          (Storage.ReadSaveFile, GameState?) read = await Storage.ReadStorageData(1);
          read.Item2?.MyApplyA(async readGame => await InitGame(page,readGame));
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage(page,[read.Item2 != null ? "ロード完了" : read.Item1 == Storage.ReadSaveFile.Read ? "ロード失敗：ファイルが破損しています" : "ロード失敗：ファイルが見つかりません"]));
        });
      });
      UIUtil.InitGameActions.Add(() => {
        ShowMessage(page,["ゲームを初期化しています.."]);
        Task.Run(async () => {
          await InitGame(page,GetGame.GetInitGameScenario(GameData.game.NowScenario));
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,()=> ScalingElements(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize)));
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ShowMessage(page,["ゲームを初期化しました"]));
        });
      });
      LoadPage(page,GameData.game);
      void LoadPage(Game page,GameState startGame) {
        Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => RefreshViewMode(page));
          await Task.Yield();
          await SetUIElements(page);
          await Task.Yield();
          await InitGame(page,startGame);
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => SwitchInfoButton(page,InfoPanelState.Explain));
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => AttachEvent(page));
          await Task.Yield();
          await InitMapCanvas();
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ScalingElements(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize)));
          await Task.Yield();
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => page.LoadingImagePanel.Visibility = Visibility.Collapsed);
        });
        async Task InitMapCanvas() {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => {
            MapCanvas.Width = UIUtil.mapSize.Width;
            MapCanvas.Height = UIUtil.mapSize.Height;
            MapCanvas.UpdateLayout();
          });
          await Task.Delay(500);
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,MapCanvas.Invalidate);
        }
        async Task SetUIElements(Game page) {
          Dictionary<ERole,Color> countryRolePanelColorMap = new([new(ERole.central,new Color(255,240,240,210)),new(ERole.affair,new Color(255,240,240,240)),new(ERole.defense,new Color(255,210,210,240)),new(ERole.attack,new Color(255,240,210,210))]);
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => {
            infoPanelMap.Clear();
            countryPostPanelMap.Clear();
            page.CountryPostsPanel.Children.Clear();
          });
          await countryRolePanelColorMap.Reverse().MyAsyncForEachSequential(async v =>
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => {
              StackPanel panel = CreateCountryPostPanel(page,v.Key,v.Value);
              countryPostPanelMap.Add(v.Key,panel);
              page.CountryPostsPanel.Children.Add(panel);
            })
          );
        }
      }
    }
  }
  private async Task InitGame(Game page,GameState newGameState) {
    GameData.game = newGameState;
    await SetInitUI(page,newGameState);
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateAreaPanels(page,newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateCountryPosts(page,newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateCountryInfoPanel(page,newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => CleanUI(page,newGameState));
    await Task.Yield();
    await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateLogMessageUI(page,newGameState));
    void CleanUI(Game page,GameState game) {
      page.MovePersonCanvas.MySetChildren([]);
      page.LogContentPanel.MySetChildren([]);
      page.AskPanel.Visibility = Visibility.Collapsed;
      page.CharacterRemarkPanel.Visibility = Visibility.Collapsed;
      page.MapAnimationElementsCanvas.MySetChildren([]);
    }
    async Task SetInitUI(Game page,GameState game) {
      areaElemsMap = game.AreaMap.ToDictionary(v => v.Key,v => CreateAreaElems(page,game,v.Key));
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,page.MapElementsCanvas.Children.Clear);
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.RoadConnections.ToList().ForEach(road => page.MapElementsCanvas.Children.Add(MaybeCreateRoadLine(game,road))));
      await Task.Yield();
      await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => areaElemsMap.ToList().ForEach(v => page.MapElementsCanvas.Children.Add(CreateAreaPanelFromAreaElems(page,game,v))));
      static Line? MaybeCreateRoadLine(GameState game,Road road) {
        return Area.GetAreaPoint(game,road.From,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth.Value) is Point from && Area.GetAreaPoint(game,road.To,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth.Value) is Point to ? CreateRoadLine(road,from,to) : null;
        static Line CreateRoadLine(Road road,Point from,Point to) => new() { X1 = from.X,Y1 = from.Y,X2 = to.X,Y2 = to.Y,Stroke = (road.Kind switch { RoadKind.land => UIUtil.landRoadColor, RoadKind.water => UIUtil.waterRoadColor }).ToBrush(),StrokeThickness = 5 * Math.Pow(road.Easiness,1.7) / 2 + 10 };
      }
      static AreaElems CreateAreaElems(Game page,GameState game,EArea area) {
        Border areaBackBorder = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height,CornerRadius = UIUtil.areaCornerRadius,BorderBrush = Colors.Red };
        TextBlock areaNameText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,1,0,-1) };
        StackPanel areaDefensePersonPanel = [];
        StackPanel areaAffairPersonPanel = [];
        Post areaDefensePost = new(ERole.defense,new(area));
        Post areaAffairPost = new(ERole.affair,new(area));
        TextBlock affairText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,-1) };
        TextBlock exText = new() { HorizontalAlignment = HorizontalAlignment.Center,Margin = new(0,-1) };
        Grid areaWrapPanel = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height };
        return new(areaBackBorder,areaNameText,areaDefensePersonPanel,areaAffairPersonPanel,areaDefensePost,areaAffairPost,affairText,exText,areaWrapPanel);
      }
      static Grid CreateAreaPanelFromAreaElems(Game page,GameState game,KeyValuePair<EArea,AreaElems> areaElemInfo) {
        Grid areaPanel = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height,CornerRadius = UIUtil.areaCornerRadius };
        StackPanel personPutAreaPanel = new() {
          HorizontalAlignment = HorizontalAlignment.Center,Orientation = Orientation.Horizontal,
          BorderBrush = GetPostFrameColor(game,areaElemInfo.Key).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth),Margin = new(0,-1)
        };
        areaPanel.PointerPressed += (_,_) => PushArea.Push(areaElemInfo.Key);
        areaPanel.PointerExited += (_,_) => PushArea.Exit();
        areaPanel.PointerReleased += (_,_) => GameData.game = PushArea.Release(page,GameData.game,areaElemInfo.Key);
        Area.GetAreaPoint(game,areaElemInfo.Key,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth.Value)?.MyApplyA(v => Canvas.SetLeft(areaPanel,v.X - UIUtil.areaSize.Width / 2)).MyApplyA(v => Canvas.SetTop(areaPanel,v.Y - UIUtil.areaSize.Height / 2));
        return areaPanel.MySetChildren([
          areaElemInfo.Value.Back,
          new StackPanel{ Width = UIUtil.areaSize.Width,VerticalAlignment = VerticalAlignment.Center }.MySetChildren([
            areaElemInfo.Value.AreaNameText,
            personPutAreaPanel.MySetChildren([
              CreatePersonPutPanel(game,areaElemInfo.Value.DefensePost,"防",areaElemInfo.Value.DefensePersonPanel),
              CreatePersonPutPanel(game,areaElemInfo.Value.AffairPost,"政",areaElemInfo.Value.AffairPersonPanel)
            ]),
            areaElemInfo.Value.AffairText,
            areaElemInfo.Value.ExText
          ]),
          areaElemInfo.Value.WrapPanel
        ]);
      }
    }
  }
  private static void UpdateAreaPanels(Game page,GameState game) {
    double capitalBorderWidth = 1.75;
    areaElemsMap.ToList().ForEach(areaElems => {
      AreaData? areaData = game.AreaMap.GetValueOrDefault(areaElems.Key);
      areaElems.Value.Back.BorderThickness = new(game.CountryMap.Values.Select(v => v.CapitalArea).Contains(areaElems.Key) ? capitalBorderWidth : 0);
      areaElems.Value.Back.Background = Country.GetCountryColor(game,areaData?.Country).ToBrush();
      areaElems.Value.AreaNameText.Text = $"{areaElems.Key} {areaData?.Country?.ToString() ?? $"自治"}領";
      areaElems.Value.AreaNameText.RenderTransform = new ScaleTransform { ScaleX = Math.Min(1,5 / UIUtil.CalcFullWidthTextLength(areaElems.Value.AreaNameText.Text)),CenterX = UIUtil.CalcFullWidthTextLength(areaElems.Value.AreaNameText.Text) * BasicStyle.fontsize / 2 };
      areaElems.Value.DefensePersonPanel.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == areaElems.Value.DefensePost)?.MyApplyF(param => CreatePersonPanel(page,game,param)).MyMaybeToList() ?? []]);
      areaElems.Value.AffairPersonPanel.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == areaElems.Value.AffairPost)?.MyApplyF(param => CreatePersonPanel(page,game,param)).MyMaybeToList() ?? []]);
      areaElems.Value.AffairText.Text = $"{Math.Floor(areaData?.AffairParam.AffairNow ?? 0)}/{Math.Floor(areaData?.AffairParam.AffairsMax ?? 0)}";
      areaElems.Value.ExText.Text = areaData?.Country?.MyApplyF(country => (Country.IsSleep(game,country) ? $"休み {Country.GetSleepTurn(game,country)}" : null) + (Country.IsFocusDefense(game,country) ? "(防)" : null)); ;
      areaElems.Value.WrapPanel.Background = Area.IsPlayerSelectable(game,areaElems.Key) ? null : UIUtil.grayoutColor.ToBrush();
    });
  }
  private static void UpdateCountryPosts(Game page,GameState game) {
    playerCountryPostPersonPanel.ToList().ForEach(countryPostInfo => countryPostInfo.Value.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Country == game.PlayCountry && v?.Value.Post == countryPostInfo.Key)?.MyApplyF(param => CreatePersonPanel(page,game,param)).MyMaybeToList() ?? []]));
    page.CountryPostsPanel.Visibility = game.Phase is Phase.Planning or Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd ? Visibility.Visible : Visibility.Collapsed;
  }
  private void UpdateCountryInfoPanel(Game page,GameState game) {
    List<UIElement> contents = game.Phase switch {
      Phase.Starting => ShowSelectScenario(page,game),
      Phase.Planning or Phase.Execution => ShowCountryInfo(game),
      Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => ShowEndGameInfo(page,game)
    };
    string? buttonText = Text.EndPhaseButtonText(game.Phase,Lang.ja);
    StateInfo.Show(page.StateInfoPanel,contents,buttonText,() => GameData.game = buttonAction(page,GameData.game));
    GameState buttonAction(Game page,GameState game) {
      taskTokens.Clear();
      return game.Phase switch {
        Phase.Starting => game,
        Phase.Planning => EndPlanningPhase(page,game).MyApplyA(game => UpdateCountryInfoPanel(page,game)),
        Phase.Execution => EndExecutionPhase(page,game).MyApplyA(game => UpdateCountryInfoPanel(page,game)),
        Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => game.MyApplyA(game => ShowGameEndLogButtonClick(page,game))
      };
    }
    List<UIElement> ShowSelectScenario(Game page,GameState game) => [
      new Grid().MySetChildren([
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,0)),
        new TextBlock { Text="シナリオ",VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,2)),
        new ComboBox {
          Width=100, VerticalAlignment=VerticalAlignment.Stretch,
          SelectedIndex=BaseData.scenarios.MyGetIndex(v => v==game.NowScenario)??0,
          Foreground=Colors.Black, Background=Colors.White, Padding=new(10,0,0,0), Margin=new(0,1),
          ItemContainerStyle = new Style(typeof(ComboBoxItem)).MyApplyA(style=>style.Setters.Add(new Setter(FontSizeProperty,BasicStyle.fontsize*UIUtil.GetScaleFactor(page.ContentGrid.RenderSize)))),
        }.MyApplyA(elem => BaseData.scenarios.Select(v =>v.Value).ToList().ForEach(elem.Items.Add)).MyApplyA(v=>
          v.SelectionChanged+=(_,_) => (v.SelectedItem as string)?.MyApplyA(async text =>await InitGame(page,GetGame.GetInitGameScenario(new(text))))
        ).MyApplyA(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,4)),
        new TextBlock{ Text=game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.MyApplyF(v=>$"{v.StartYear}年開始"),VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,5)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,6)),
        new TextBlock{ Text=game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.MyApplyF(v=>$"{v.EndYear}年終了"),VerticalAlignment=VerticalAlignment.Center}.MyApplyA(v=>Grid.SetColumn(v,7)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,8)),
        new TextBlock{ Text=$"マップをクリックしてプレイ勢力を選択",VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,9)),
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
          new TextBlock{ Text=Turn.GetCalendarText(game) },
          new TextBlock{ Text=$"プレイ勢力:{game.PlayCountry}" },
        ]).MyApplyA(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,2)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=$"首都:{Country.GetCapitalArea(game,game.PlayCountry)}" },
          new TextBlock{ Text=$"資金:{Country.GetFund(game,game.PlayCountry):0.####}" },
        ]).MyApplyA(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,4)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=$"領地数:{Country.GetAreaNum(game,game.PlayCountry)}" },
          new TextBlock{ Text=$"内政難度:{Country.GetAffairDifficult(game,game.PlayCountry):0.####}" },
        ]).MyApplyA(v=>Grid.SetColumn(v,5)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,6)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center }.MySetChildren([
          new TextBlock{ Text=$"内政力:{Country.GetAffairPower(game,game.PlayCountry):0.####}" },
          new TextBlock{ Text=$"総内政値:{Country.GetTotalAffair(game, game.PlayCountry) :0.####}" },
        ]).MyApplyA(v=>Grid.SetColumn(v,7)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,8)),
        new StackPanel{ VerticalAlignment=VerticalAlignment.Center}.MySetChildren([
          new TextBlock{ Text=$"支出:{Country.GetOutFund(game, game.PlayCountry) :0.####}" },
          new TextBlock{ Text=$"収入:{Country.GetInFund(game, game.PlayCountry) :0.####}" },
        ]).MyApplyA(v=>Grid.SetColumn(v,9)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,10)),
        new TextBlock{ Text=$"侵攻:{Country.GetTargetArea(game,game.PlayCountry)?.ToString()??"なし"}",VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,11)),
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
    static List<UIElement> ShowEndGameInfo(Game page,GameState game) => [
      new Grid().MySetChildren([
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,0)),
        new TextBlock{ Text=Turn.GetCalendarText(game),VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,1)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,2)),
        new TextBlock{ Text=$"プレイ勢力:{game.PlayCountry}",VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,3)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,4)),
        new TextBlock{ Text=$"ゲーム終了",VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,5)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,6)),
        new TextBlock{ Text=$"結果:{game.Phase switch { Phase.PerishEnd=>"滅亡敗北",Phase.TurnLimitOverEnd=>"存続勝利",Phase.WinEnd=>"条件勝利",Phase.OtherWinEnd=>"他陣営条件勝利敗北",_ =>string.Empty }}",VerticalAlignment=VerticalAlignment.Center }.MyApplyA(v=>Grid.SetColumn(v,7)),
        new StackPanel().MyApplyA(v=>Grid.SetColumn(v,8)),
      ]).MyApplyA(v=>new List<ColumnDefinition>([
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(1, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(10, GridUnitType.Star) },
        new() { Width = GridLength.Auto },
        new() { Width = new GridLength(10, GridUnitType.Star) },
      ]).ForEach(v.ColumnDefinitions.Add))
    ];
    static void ShowGameEndLogButtonClick(Game page,GameState game) {
      string title = $"ゲームログ";
      List<TextBlock> contents = [.. game.GameLog.Select(log => new TextBlock { Text = log })];
      string okButtonText = "ゲームコメントを投稿する";
      Ask.SetElems(page.AskPanel,title,contents,okButtonText,() => LogButtonClick(game),false,page.ContentGrid.RenderSize);
      static void LogButtonClick(GameState game) {
        string url = $"https://karintougames.com/siteContents/gameComment.php?caption={BaseData.name.Value} ver.{BaseData.version.Value}&comment={HttpUtility.UrlEncode(string.Join('\n',game.GameLog))}";
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS($"top.location.href='{url}';");
#else
        _ = Windows.System.Launcher.LaunchUriAsync(new Uri(url));
#endif
      }
    }
    GameState EndPlanningPhase(Game page,GameState game) {
      return game.MyApplyA(_ => ResetTurnUI(page)).MyApplyF(game => UpdateGame.AutoPutPostCPU(game,[ECountry.漢])).MyApplyF(CalcArmyTarget).MyApplyF(game => game with { Phase = Phase.Execution }).MyApplyA(game => UpdateAreaPanels(page,game)).MyApplyA(game => ExecutionMoveFlag(page,game)).MyApplyF(ArmyAttack).MyApplyA(game => UpdateLogMessageUI(page,game)).MyApplyA(game => CharacterRemark.SetElems(page.CharacterRemarkPanel,game.PlayCountry,Text.CharacterRemarkTexts(game,Lang.ja),page.ContentGrid,game.PlayTurn <= 2));
      static void ResetTurnUI(Game page) => page.MyApplyA(page => page.CharacterRemarkPanel.Visibility = Visibility.Collapsed).MyApplyA(page => page.CountryPostsPanel.Visibility = Visibility.Collapsed).MyApplyA(page => page.ScalingElements(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize)));
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
      void ExecutionMoveFlag(Game page,GameState game) {
        game.ArmyTargetMap.Where(v => v.Value != null && Country.SuccessAttack(game,v.Key)).ToDictionary(attackInfo => GetFlag(game,attackInfo.Key),attackInfo => CalcFlagMovePos(game,attackInfo)).MyApplyA(flagMap => page.MapAnimationElementsCanvas.MySetChildren([.. flagMap.Keys])).MyApplyA(flagMap => MoveFlags(page,flagMap));
        static Grid GetFlag(GameState game,ECountry attackCountry) {
          return CreateFlag(game,attackCountry).MyApplyF(v => AttachFlag(game,v,attackCountry));
          static Grid CreateFlag(GameState game,ECountry attackCountry) {
            double flagTextMaxLength = 2.2;
            double flagTextScale = 1.6;
            string flagText = attackCountry.ToString();
            double flagTextLength = UIUtil.CalcFullWidthTextLength(flagText);
            TextBlock flagTextBlock = new() {
              Text = flagText,Width = Math.Min(flagTextMaxLength,flagTextLength) * BasicStyle.fontsize * flagTextScale,
              HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Center,
              RenderTransform = new ScaleTransform { ScaleX = Math.Min(1,flagTextMaxLength / flagTextLength) * flagTextScale,ScaleY = flagTextScale,CenterY = BasicStyle.textHeight / 2 }
            };
            Grid flagPanel = new() { Width = BasicStyle.fontsize * 4,Height = BasicStyle.fontsize * 3,Background = Country.GetCountryColor(game,attackCountry).ToBrush(),HorizontalAlignment = HorizontalAlignment.Left,VerticalAlignment = VerticalAlignment.Top };
            Image attackImage = new() { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/army.svg")),Width = BasicStyle.fontsize * 4,Height = BasicStyle.fontsize * 4,HorizontalAlignment = HorizontalAlignment.Right };
            Grid attackImagePanel = new Grid() { Width = BasicStyle.fontsize * 6,Height = BasicStyle.fontsize * 5 }.MySetChildren([attackImage,flagPanel.MySetChildren([flagTextBlock])]);
            return attackImagePanel;
          }
          static Grid AttachFlag(GameState game,Grid rawFlag,ECountry attackCountry) {
            double fontsize = 21;
            double lineheight = 27;
            double shadowWidth = 1.5;
            List<Thickness> margins = [new(0,-shadowWidth,0,shadowWidth),new(shadowWidth,-shadowWidth,-shadowWidth,shadowWidth),new(shadowWidth,0,-shadowWidth,0),new(shadowWidth,shadowWidth,-shadowWidth,-shadowWidth),new(0,shadowWidth,0,-shadowWidth),new(-shadowWidth,shadowWidth,shadowWidth,-shadowWidth),new(-shadowWidth,0,shadowWidth,0),new(-shadowWidth,-shadowWidth,shadowWidth,shadowWidth)];
            decimal attackRank = Commander.CommanderRank(game,Commander.GetAttackCommander(game,attackCountry),ERole.attack);
            Grid attackRankPanel = new Grid { HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Bottom,Margin = new(0,-shadowWidth,0,shadowWidth) }.MySetChildren([
              .. margins.Select(margin=>new TextBlock{ Text=$"Rank{attackRank}",FontSize=fontsize,LineHeight=lineheight,Margin=margin}),
              new TextBlock{ Text=$"Rank{attackRank}",FontSize=fontsize,LineHeight=lineheight,Foreground=Colors.White }
            ]);
            return new Grid() { Width = BasicStyle.fontsize * 6,Height = BasicStyle.fontsize * 5,Visibility = Visibility.Collapsed }.MySetChildren([rawFlag,attackRankPanel]);
          }
        }
        static List<Point> CalcFlagMovePos(GameState game,KeyValuePair<ECountry,EArea?> attackInfo) {
          EArea[] route = attackInfo.Value?.MyApplyF(target => Route.SolveAtackArmyRoute(game,attackInfo.Key,target)) ?? [];
          List<(Point from, Point to)> routeAreaPoints = [.. route.Select(v => AreaToPoint(game,v)).MyNonNull().MyAdjacentCombinations()];
          List<Point> posList = [.. routeAreaPoints.SelectMany(v => Enumerable.Range(0,60).Select(index => new Point(double.Lerp(v.from.X,v.to.X,index / 60d),double.Lerp(v.from.Y,v.to.Y,index / 60d)))).MyApplyF(v => v.Chunk(v.Count() / 60).Select(v => v.LastOrDefault()).Append(routeAreaPoints.LastOrDefault().to).MyNonNull())];
          return posList;
          static Point? AreaToPoint(GameState game,EArea area) => Area.GetAreaPoint(game,area,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth.Value);
        }
        void MoveFlags(Game page,Dictionary<Grid,List<Point>> flags) {
          TaskToken token = new TaskToken().MyApplyA(taskTokens.Add);
          Task.Run(async () => {
            await flags.MyAsyncForEachConcurrent(async v => {
              await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => {
                Canvas.SetLeft(v.Key,v.Value.FirstOrDefault()?.X - v.Key.Width / 2 ?? 0);
                Canvas.SetTop(v.Key,v.Value.FirstOrDefault()?.Y - v.Key.Height / 2 ?? 0);
                v.Key.Visibility = Visibility.Visible;
              });
              await v.Value.Skip(1).MyAsyncForEachSequential(async pos => {
                await Task.Delay(15);
                if(!taskTokens.Contains(token)) {
                  return;
                }
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => {
                  Canvas.SetLeft(v.Key,pos.X - v.Key.Width / 2);
                  Canvas.SetTop(v.Key,pos.Y - v.Key.Height / 2);
                });
              });
            });
            await Task.Yield();
            await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => UpdateAreaPanels(page,GameData.game));
          });
        }
      }
      static GameState ArmyAttack(GameState game) {
        return game.CountryMap.Keys.OrderBy(country => Country.GetTotalAffair(game,country)).Aggregate(game,(game,country) => {
          return game.ArmyTargetMap.GetValueOrDefault(country) is EArea target ? TryAttack(game,country,target) : game.ArmyTargetMap.ContainsKey(country) ? ExeDefense(game,country) : ExeRest(game,country);
          static GameState TryAttack(GameState game,ECountry country,EArea targetArea) {
            return Country.SuccessAttack(game,country) ? ExeAttack(game,country,targetArea) : FailAttack(game,country,targetArea);
            static GameState ExeAttack(GameState game,ECountry country,EArea targetArea) => targetArea.MyApplyF(game.AreaMap.GetValueOrDefault)?.Country.MyApplyF(defeseSide => UpdateGame.Attack(game,country,targetArea,defeseSide,Country.IsFocusDefense(game,defeseSide))) ?? game;
            static GameState FailAttack(GameState game,ECountry country,EArea targetArea) => game.MyApplyF(game => UpdateGame.Defense(game,country,true)).MyApplyF(game => country == game.PlayCountry ? UpdateGame.AppendStartExecutionRemark(game,[$"{targetArea}への侵攻は\n資金不足のため中止されました"]) : game);
          }
          static GameState ExeDefense(GameState game,ECountry country) => game.MyApplyF(game => UpdateGame.Defense(game,country,false));
          static GameState ExeRest(GameState game,ECountry country) => game.MyApplyF(game => UpdateGame.Rest(game,country));
        });
      }
    }
    static GameState EndExecutionPhase(Game page,GameState game) {
      page.MapAnimationElementsCanvas.MySetChildren([]);
      return game.MyApplyA(_ => ResetTurnUI(page)).MyApplyF(UpdateGame.GameEndJudge).MyApplyF(game => game.Phase is Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd ? game : game.MyApplyF(game => NextTurn(page,game)));
      static void ResetTurnUI(Game page) => page.MyApplyA(page => page.TurnWinCondPanel.MySetChildren([])).MyApplyA(page => page.CharacterRemarkPanel.Visibility = Visibility.Collapsed).MyApplyA(page => page.CountryPostsPanel.Visibility = Visibility.Visible).MyApplyA(page => page.ScalingElements(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize)));
      static GameState NextTurn(Game page,GameState game) => game.MyApplyF(UpdateGame.NextTurn).MyApplyF(v => v with { Phase = Phase.Planning }).MyApplyA(game => UpdateCountryPosts(page,game)).MyApplyA(game => UpdateAreaPanels(page,game)).MyApplyA(game => page.UpdateTurnLogUI(page,game)).MyApplyA(async game => await page.UpdateTurnWinCondUI(page,game,true)).MyApplyA(game => UpdateLogMessageUI(page,game)).MyApplyF(UpdateGame.GameEndJudge).MyApplyA(game => CharacterRemark.SetElems(page.CharacterRemarkPanel,game.PlayCountry,Text.CharacterRemarkTexts(game,Lang.ja),page.ContentGrid,game.PlayTurn <= 2)).MyApplyF(ResetParam);
      static GameState ResetParam(GameState game) => game with { ArmyTargetMap = [],StartPlanningCharacterRemark = [],StartExecutionCharacterRemark = [] };
    }
  }
  private static Grid CreatePersonPutPanel(GameState game,Post post,string backText,StackPanel personPutInnerPanel) {
    Grid personPutPanel = new() { Width = UIUtil.personPutSize.Width,Height = UIUtil.personPutSize.Height,BorderBrush = GetPostFrameColor(game,post.PostKind.MaybeArea).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth),Background = Colors.Transparent };
    TextBlock personPutBackText = new() {
      Text = backText,Foreground = Windows.UI.Color.FromArgb(100,100,100,100),HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Center,
      RenderTransform = new ScaleTransform() { ScaleX = UIUtil.personPutFontScale,ScaleY = UIUtil.personPutFontScale,CenterX = UIUtil.CalcFullWidthTextLength(backText) * BasicStyle.fontsize / 2,CenterY = BasicStyle.textHeight / 2 }
    };
    personPutPanel.PointerEntered += (_,_) => EnterPersonPutPanel(GameData.game,personPutInnerPanel,post);
    personPutPanel.PointerExited += (_,_) => ExitPersonPutPanel(personPutInnerPanel);
    return personPutPanel.MySetChildren([personPutBackText,personPutInnerPanel]);
    static void EnterPersonPutPanel(GameState game,StackPanel personPutInnerPanel,Post post) {
      if(game.Phase != Phase.Starting && (post.PostKind.MaybeArea?.MyApplyF(area => game.AreaMap.GetValueOrDefault(area)?.Country == game.PlayCountry) ?? true)) {
        personPutInnerPanel.Background = Windows.UI.Color.FromArgb(150,255,255,255);
        pointerover = (personPutInnerPanel, post);
      }
    }
    static void ExitPersonPutPanel(StackPanel personPutInnerPanel) {
      if(pointerover != null) {
        personPutInnerPanel.Background = Colors.Transparent;
        pointerover = null;
      }
    }
  }
  private static Grid CreatePersonPanel(Game page,GameState game,KeyValuePair<PersonId,PersonData> person) {
    double minFullWidthLength = 2.25;
    double margin = page.FontSize * (UIUtil.CalcFullWidthTextLength(person.Key.Value) - 2);
    Grid panel = new Grid { Width = UIUtil.personPutSize.Width,Height = UIUtil.personPutSize.Height,Background = Country.GetCountryColor(game,Person.GetPersonCountry(game,person.Key)).ToBrush() }.MySetChildren([
      new StackPanel { HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,Background=Windows.UI.Color.FromArgb((byte)(20*Person.GetPersonRank(game,person.Key)),0,0,0) }.MySetChildren([
        GetRankPanel(page,game,person),
        new TextBlock { Text=person.Key.Value,TextAlignment=TextAlignment.Center,Margin=new(-margin/2,0),RenderTransform=new ScaleTransform{ ScaleX=minFullWidthLength/Math.Max(minFullWidthLength,UIUtil.CalcFullWidthTextLength(person.Key.Value))*UIUtil.personNameFontScale,ScaleY=UIUtil.personNameFontScale,CenterX=UIUtil.personPutSize.Width/2+margin/minFullWidthLength  }  }
      ])
    ]);
    panel.PointerPressed += (_,e) => PickPersonPanel(page,GameData.game,e,panel,person.Key);
    return panel;
    static StackPanel GetRankPanel(Game page,GameState game,KeyValuePair<PersonId,PersonData> person) {
      int postRank = Person.CalcRoleRank(game,person.Key,person.Value.Post?.PostRole);
      int personRank = Person.GetPersonRank(game,person.Key);
      return new StackPanel { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center,RenderTransform = new ScaleTransform() { ScaleX = UIUtil.personRankFontScale,ScaleY = UIUtil.personRankFontScale,CenterX = page.FontSize / 2 } }.MySetChildren(GetRankTextBlock(game,person.Key,postRank,personRank == postRank));
      static List<UIElement> GetRankTextBlock(GameState game,PersonId person,int rank,bool isMatchRole) => [
        new TextBlock() { Margin = new(0,-1.25,0,0),Text = rank.ToString(),Foreground = isMatchRole ? Colors.Black : Colors.Red },
        .. (isMatchRole?null:new Image { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{Person.GetPersonRole(game,person)}.svg")),VerticalAlignment = VerticalAlignment.Top,Width = BasicStyle.textHeight * 0.75,Height = BasicStyle.textHeight * 0.75 }).MyMaybeToList()
      ];
    }
    static void PickPersonPanel(Game page,GameState game,PointerRoutedEventArgs e,Panel personPanel,PersonId person) {
      if(game.Phase != Phase.Starting && Person.GetPersonCountry(game,person) == game.PlayCountry && personPanel.Parent is Panel parentPanel) {
        personPanel.IsHitTestVisible = false;
        parentPanel.MySetChildren([]);
        page.MovePersonCanvas.MySetChildren([personPanel]);
        MovePerson(page,personPanel,e);
        pick = (parentPanel, person);
      }
    }
  }
  private static void MovePerson(Game page,UIElement personPanel,PointerRoutedEventArgs e) {
    Canvas.SetLeft(personPanel,e.GetCurrentPoint(page.MovePersonCanvas).Position.X - UIUtil.personPutSize.Width / 2);
    Canvas.SetTop(personPanel,e.GetCurrentPoint(page.MovePersonCanvas).Position.Y - UIUtil.personPutSize.Height / 2);
  }


  private static ERole activePanelRole = ERole.central;
  private static Dictionary<InfoPanelState,UserControl> infoPanelMap = [];
  private static readonly Color buttonBackground = new(175,255,255,255);
  private static (Panel panel, Post post)? pointerover = null;
  private static (Panel panel, PersonId person)? pick = null;
  private static Dictionary<ERole,StackPanel> countryPostPanelMap = [];
  private static StackPanel CreateCountryPostPanel(Game page,ERole role,Color backColor) {
    return new StackPanel() { Padding = new(1.5,0),Background = backColor.ToBrush() }.MySetChildren([
      new StackPanel() { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center }.MySetChildren([
          new TextBlock { Text = Text.RoleToText(role,Lang.ja) },
          new Image { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{role}.svg")),Width = BasicStyle.textHeight,Height = BasicStyle.textHeight,VerticalAlignment = VerticalAlignment.Center }
        ]),
        CreateCountryPosts(page,GameData.game,role)
    ]);
    static StackPanel CreateCountryPosts(Game page,GameState game,ERole role) {
      return new StackPanel().MySetChildren([
        new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
            CreatePersonHeadPostPanel(game,role),
            CreateAutoPutPersonButton(page,game,role)
          ]),
          CreatePersonPostPanelElems(game,role)
      ]);
      static Button CreateAutoPutPersonButton(Game page,GameState game,ERole role) {
        Button autoPutPersonButton = new Button { Width = UIUtil.personPutSize.Width * 3,VerticalAlignment = VerticalAlignment.Stretch,Background = Windows.UI.Color.FromArgb(100,100,100,100) }.MyApplyA(v => v.Content = new TextBlock { Text = "オート配置" });
        autoPutPersonButton.Click += (_,_) => GameData.game = AutoPutPersonButtonClick(page,GameData.game);
        return autoPutPersonButton;
        GameState AutoPutPersonButtonClick(Game page,GameState game) => game.PlayCountry?.MyApplyF(country => Code.Post.GetAutoPutPost(game,country,role)).MyApplyF(postMap => UpdateGame.SetPersonPost(game,postMap)).MyApplyA(v => UpdateAreaPanels(page,v)).MyApplyA(game => UpdateCountryPosts(page,game)) ?? game;
      }
      static StackPanel CreatePersonHeadPostPanel(GameState game,ERole role) {
        return new StackPanel { Orientation = Orientation.Horizontal,BorderBrush = GetPostFrameColor(game,null).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) }.MySetChildren([
          .. Enum.GetValues<PostHead>().Select(v=> new Post(role,new(v)).MyApplyF(post=> CreatePersonPutPanel(game,post,GetPlayerCountryPostBackString(post.PostKind),playerCountryPostPersonPanel.GetValueOrDefault(post)??[]))),
          ]);
      }
      static StackPanel CreatePersonPostPanelElems(GameState game,ERole role) {
        return new StackPanel { BorderBrush = GetPostFrameColor(game,null).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) }.MySetChildren([.. Enumerable.Range(0,UIUtil.capitalPieceRowNum).Select(row => GetPersonPostLinePanel(game,role,row,game.PersonMap.Where(v => Person.GetPersonCountry(game,v.Key) == game.PlayCountry).ToDictionary()))]);
        static StackPanel GetPersonPostLinePanel(GameState game,ERole role,int rowNo,Dictionary<PersonId,PersonData> personMap) => new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
          .. Enumerable.Range(0,UIUtil.capitalPieceColumnNum).Select(i => new Post(role,new(rowNo * UIUtil.capitalPieceColumnNum + i)).MyApplyF(post=> CreatePersonPutPanel(game,post,GetPlayerCountryPostBackString(post.PostKind),playerCountryPostPersonPanel.GetValueOrDefault(post) ?? [])))
        ]);
      }
      static string GetPlayerCountryPostBackString(PostKind postKind) => postKind.MaybeHead switch { PostHead.main => "筆頭", PostHead.sub => "次席", _ => $"{postKind.MaybePostNo + 1}" };
    }
  }
  private static Color GetPostFrameColor(GameState game,EArea? area) => area != null && (game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.ChinaAreas ?? []).Contains(area.Value) ? new Color(150,100,100,30) : new Color(120,0,0,0);
  private static void ResizeLogMessageUI(Game page,double scaleFactor) {
    page.LogContentPanel.MaxWidth = page.LogScrollPanel.ActualWidth / scaleFactor;
    page.LogContentPanel.Height = page.LogContentPanel.Children.Sum(v => v.DesiredSize.Height);
    page.LogContentPanel.Margin = new(0,0,page.LogContentPanel.DesiredSize.Width * (scaleFactor - 1),page.LogContentPanel.Height * (scaleFactor - 1));
    page.LogContentPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
  }
  private static void ResizeCountryPostUI(Game page,double scaleFactor) {
    double PostPanelLeftUnit = (page.ContentGrid.RenderSize.Width / scaleFactor - countryPostPanelMap.Values.Sum(v => v.ActualWidth) / 4) / 3;
    countryPostPanelMap.Values.Reverse().Select((elem,index) => (elem, index)).ToList().ForEach(v => Canvas.SetLeft(v.elem,PostPanelLeftUnit * v.index));
  }
  private static void ResizeTurnWinCondPanelUI(Game page,double scaleFactor) {
    page.TurnWinCondPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.TurnWinCondPanel.RenderSize.Width / 2 };
  }
  private static void UpdateLogMessageUI(Game page,GameState game) {
    game.NewLog.Select(logText => new TextBlock() { Text = logText }).ToList().ForEach(page.LogContentPanel.Children.Add);
    game.NewLog.Clear();
  }
  private void UpdateTurnLogUI(Game page,GameState game) {
    StackPanel panel = new StackPanel() {
      Background = Windows.UI.Color.FromArgb(187,255,255,255),
      Height = game.TrunNewLog.Count * BasicStyle.textHeight,
      IsHitTestVisible = false
    }.MySetChildren([.. game.TrunNewLog.Select(logText => new TextBlock() { Text = logText })]).MyApplyA(async elem => {
      game.TrunNewLog.Clear();
      elem.Opacity = 1;
      await Task.Delay(6000);
      await Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).MyAsyncForEachSequential(async v => {
        await page.Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => elem.Opacity = 1 - v);
        await Task.Delay(15);
      });
      page.TurnLogPanel.Children.Remove(elem);
      ResizeTurnLogUI(page);
    });
    page.TurnLogPanel.Children.Add(panel);
    ResizeTurnLogUI(page);
    static void ResizeTurnLogUI(Game page) => page.TurnLogPanel.Height = page.TurnLogPanel.Children.OfType<FrameworkElement>().Sum(v => v.Height) * UIUtil.GetScaleFactor(page.ContentGrid.RenderSize);
  }
  private async Task UpdateTurnWinCondUI(Game page,GameState game,bool outputCheckString) {
    Dictionary<string,bool?> winCondMap = game.PlayCountry?.MyApplyF(v => game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.WinConditionMap.GetValueOrDefault(v))?.ProgressExplainFunc(game) ?? [];
    StackPanel panel = new StackPanel() {
      Background = Windows.UI.Color.FromArgb(187,255,255,255),
      IsHitTestVisible = false
    }.MySetChildren([
      new TextBlock() { Text = $"勝利条件({Turn.GetCalendarText(game)})" },
        .. winCondMap.Select(winCond => new StackPanel(){ Orientation = Orientation.Horizontal }.MySetChildren([
          ..((outputCheckString&&winCond.Value.HasValue)?new Image() { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{(winCond.Value.Value? "checkOK.svg" :!winCond.Value.Value? "checkNG.svg":string.Empty)}")),Width = BasicStyle.fontsize ,Height = BasicStyle.fontsize }:null).MyMaybeToList(),
          new TextBlock() { Text = winCond.Key }
        ]))
    ]).MyApplyA(async elem => {
      elem.Opacity = 1;
      await Task.Delay(6000);
      await Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).MyAsyncForEachSequential(async v => {
        await page.Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => elem.Opacity = 1 - v);
        await Task.Delay(15);
      });
      page.TurnWinCondPanel.Children.Remove(elem);
    });
    page.TurnWinCondPanel.Children.Add(panel);
    await page.Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => ResizeTurnWinCondPanelUI(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize)));
  }
  private static void ShowMessage(Game page,string[] messages) {
    StackPanel panel = new StackPanel() {
      Background = Windows.UI.Color.FromArgb(187,255,255,255),
      Height = messages.Length * BasicStyle.textHeight,
      IsHitTestVisible = false
    }.MySetChildren([.. messages.Select(logText => new TextBlock() { Text = logText })]).MyApplyA(async elem => {
      elem.Opacity = 1;
      await Task.Delay(6000);
      await Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).MyAsyncForEachSequential(async v => {
        await page.Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => { elem.Opacity = 1 - v; });
        await Task.Delay(15);
      });
      page.TurnLogPanel.Children.Remove(elem);
      ResizeTurnLogUI(page);
    });
    page.TurnLogPanel.Children.Add(panel);
    ResizeTurnLogUI(page);
    static void ResizeTurnLogUI(Game page) => page.TurnLogPanel.Height = page.TurnLogPanel.Children.OfType<FrameworkElement>().Sum(v => v.Height) * UIUtil.GetScaleFactor(page.ContentGrid.RenderSize);
  }
  private static void RefreshViewMode(Game page) {
    page.SwitchViewModeButtonText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "▼" : "▲";
    page.ContentGrid.MaxWidth = UIUtil.viewMode == UIUtil.ViewMode.fix ? UIUtil.fixModeMaxWidth : double.MaxValue;
  }
  private static void SwitchInfoButton(Game page,InfoPanelState clickButtonInfoPanelState) {
    Dictionary<InfoPanelState,Button> buttonMap = new() { { InfoPanelState.Explain,page.ExplainButton },{ InfoPanelState.WinCond,page.WinCondButton },{ InfoPanelState.ParamList,page.PersonDataButton },{ InfoPanelState.ChangeLog,page.ChangeLogButton },{ InfoPanelState.Setting,page.SettingButton } };
    buttonMap.ToList().ForEach(v => v.Value.Background = v.Key == clickButtonInfoPanelState ? Colors.LightGray : Colors.WhiteSmoke);
    infoPanelMap.TryAdd(clickButtonInfoPanelState,CreateInfoPanel(page,clickButtonInfoPanelState));
    infoPanelMap.GetValueOrDefault(clickButtonInfoPanelState)?.MyApplyF(elem => page.InfoContentPanel.MySetChildren([elem]));
    static UserControl CreateInfoPanel(Game page,InfoPanelState state) => state switch { InfoPanelState.Explain => new Explain(page.ContentGrid), InfoPanelState.WinCond => new WinCond(page.ContentGrid), InfoPanelState.ParamList => new ParamList(page.ContentGrid), InfoPanelState.ChangeLog => new ChangeLog(page.ContentGrid), InfoPanelState.Setting => new Setting(page.ContentGrid) };
  }
  private void ScalingElements(Game page,double scaleFactor) {
    StateInfo.ResizeElem(page.StateInfoPanel);
    page.CountryPostsPanel.Height = UIUtil.countryPersonPutPanelHeight * scaleFactor;
    page.CountryPostsPanel.Width = page.RenderSize.Width;
    page.CountryPostsPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.MapPanel.Width = UIUtil.mapSize.Width * scaleFactor;
    page.MapPanel.Height = UIUtil.mapSize.Height * scaleFactor;
    page.MapInnerPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.MovePersonCanvas.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.InfoLayoutPanel.Height = page.ContentGrid.RenderSize.Height - (page.CountryPostsPanel.Visibility == Visibility.Visible ? page.CountryPostsPanel.Height : 0) - StateInfo.contentHeight;
    double infoInnerButtonMargin = UIUtil.infoButtonHeight.Value * (scaleFactor - 1);
    page.InfoButtonsPanel.Margin = new(0,0,-page.ContentGrid.RenderSize.Width / scaleFactor + page.ContentGrid.RenderSize.Width - infoInnerButtonMargin,infoInnerButtonMargin);
    page.InfoButtonsPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    double infoFramebuttonMargin = UIUtil.infoFrameWidth.Value * (scaleFactor - 1);
    page.OpenLogButton.Margin = new(0,0,-page.ContentGrid.RenderSize.Width / scaleFactor + page.ContentGrid.RenderSize.Width - infoFramebuttonMargin,infoFramebuttonMargin);
    page.OpenLogButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.OpenInfoButton.Margin = new(0,0,infoFramebuttonMargin,-page.InfoLayoutPanel.Height / scaleFactor + page.InfoLayoutPanel.Height - infoFramebuttonMargin);
    page.OpenInfoButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.TopSwitchViewModeButton.Margin = new(0,0,infoFramebuttonMargin,infoFramebuttonMargin);
    page.TopSwitchViewModeButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.TurnLogPanel.Margin = new(UIUtil.infoFrameWidth.Value * scaleFactor,0,0,0);
    page.TurnLogPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.TurnWinCondPanel.Margin = new(UIUtil.infoFrameWidth.Value * scaleFactor,UIUtil.infoFrameWidth.Value * scaleFactor,0,0);
    Ask.ResizeElem(page.AskPanel,page.ContentGrid.RenderSize,scaleFactor);
    CharacterRemark.ResizeElem(page.CharacterRemarkPanel,page.ContentGrid.RenderSize,scaleFactor);
    ResizeTurnWinCondPanelUI(page,scaleFactor);
    ResizeCountryPostUI(page,scaleFactor);
  }
  private static void MovePersonPanel(Game page,PointerRoutedEventArgs e) {
    if(pick != null) {
      page.MovePersonCanvas.Children.SingleOrDefault()?.MyApplyA(personPanel => MovePerson(page,personPanel,e));
    }
  }
  private void PutPersonPanel(Game page) {
    if(pick != null) {
      GameData.game = GameData.game.MyApplyF(game => swapPerson(page,game)).MyApplyF(game => putPerson(page,game));
      page.MovePersonCanvas.MySetChildren([]);
      pick = null;
      UpdateCountryInfoPanel(page,GameData.game);
    }
    static GameState swapPerson(Game page,GameState game) {
      KeyValuePair<PersonId,PersonData>? maybeDestPersonInfo = game.PersonMap.MyNullable().FirstOrDefault(v => Person.GetPersonCountry(game,v?.Key ?? new(string.Empty)) == game.PlayCountry && v?.Value.Post == pointerover?.post);
      return UpdateGame.PutPersonFromUI(game,maybeDestPersonInfo?.Key,pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == maybeDestPersonInfo?.Key)?.MyApplyF(destPersonInfo => pick?.panel.MySetChildren([CreatePersonPanel(page,game,destPersonInfo)])));
    }
    static GameState putPerson(Game page,GameState game) {
      return UpdateGame.PutPersonFromUI(game,pick?.person,pointerover?.post ?? pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == pick?.person)?.MyApplyF(putPersonInfo => (pointerover?.panel ?? pick?.panel)?.MySetChildren([CreatePersonPanel(page,game,putPersonInfo)])));
    }
  }
  private static async Task UpdateCountryPostPanelZIndex(Game page,ERole toActiveRole) {
    if(activePanelRole == countryPostPanelMap.MyNullable().FirstOrDefault(v => v?.Value == page.CountryPostsPanel.Children.LastOrDefault() as StackPanel)?.Key) {
      List<StackPanel> resetZIndexPanels = GetResetZIndexPanels((activePanelRole, toActiveRole) switch {
        (ERole.central, ERole.affair) => [ERole.affair],
        (ERole.central, ERole.defense) => [ERole.affair,ERole.defense],
        (ERole.central, ERole.attack) => [ERole.affair,ERole.defense,ERole.attack],
        (ERole.affair, ERole.central) => [ERole.central],
        (ERole.affair, ERole.defense) => [ERole.defense],
        (ERole.affair, ERole.attack) => [ERole.defense,ERole.attack],
        (ERole.defense, ERole.central) => [ERole.affair,ERole.central],
        (ERole.defense, ERole.affair) => [ERole.affair],
        (ERole.defense, ERole.attack) => [ERole.attack],
        (ERole.attack, ERole.central) => [ERole.defense,ERole.affair,ERole.central],
        (ERole.attack, ERole.affair) => [ERole.defense,ERole.affair],
        (ERole.attack, ERole.defense) => [ERole.defense],
        _ => []
      });
      if(resetZIndexPanels.Count != 0) {
        resetZIndexPanels.ToList().ForEach(v => {
          page.CountryPostsPanel.Children.Remove(v);
          page.CountryPostsPanel.Children.Add(v);
        });
        await page.Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => activePanelRole = toActiveRole);
      }
    }
    static List<StackPanel> GetResetZIndexPanels(ERole[] resetZIndexRoles) => resetZIndexRoles.Select(countryPostPanelMap.GetValueOrDefault).MyNonNull();
  }
  private void AttachEvent(Game page) {
    page.MapCanvas.PaintSurface += (_,e) => UIUtil.MapCanvas_PaintSurface(e);
    page.OpenLogButton.Click += (_,_) => ClickOpenLogButton(page);
    page.OpenInfoButton.Click += (_,_) => ClickOpenInfoButton(page);
    page.ContentGrid.SizeChanged += (_,_) => ScalingElements(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize));
    page.LogScrollPanel.SizeChanged += (_,_) => ResizeLogMessageUI(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize));
    page.Page.PointerMoved += (_,e) => MovePersonPanel(page,e);
    page.Page.PointerReleased += (_,e) => PutPersonPanel(page);
    page.ExplainButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.Explain);
    page.WinCondButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.WinCond);
    page.PersonDataButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.ParamList);
    page.ChangeLogButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.ChangeLog);
    page.SettingButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.Setting);
    page.TopSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
    countryPostPanelMap.ToList().ForEach(v => v.Value.PointerEntered += async (_,_) => await UpdateCountryPostPanelZIndex(page,v.Key));
    static void ClickOpenLogButton(Game page) => (page.LogScrollPanel.Visibility = page.LogScrollPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible).MyApplyA(v => page.InfoPanel.Visibility = Visibility.Collapsed);
    static void ClickOpenInfoButton(Game page) => (page.InfoPanel.Visibility = page.InfoPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible).MyApplyA(v => page.LogScrollPanel.Visibility = Visibility.Collapsed);
  }
  private static class PushArea {
    private static EArea? pushArea = null;
    internal static void Push(EArea area) => pushArea = area;
    internal static void Exit() => pushArea = null;
    internal static GameState Release(Game page,GameState game,EArea area) {
      ECountry? areaCountry = game.AreaMap.GetValueOrDefault(area)?.Country;
      return pushArea != area ? game : game.Phase == Phase.Starting ? ShowSelectPlayCountryPanel(page,game,areaCountry) : Area.IsPlayerSelectable(game,area) ? SelectTarget(page,game,areaCountry != game.PlayCountry ? area : null) : game;
      static GameState ShowSelectPlayCountryPanel(Game page,GameState game,ECountry? pushCountry) {
        string title = $"{Text.CountryText(pushCountry,Lang.ja)}陣営";
        List<TextBlock> contents = [
          .. (game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.MyApplyF(scenario=>$"シナリオ:{game.NowScenario.Value}({scenario.StartYear}年開始 {scenario.EndYear}年終了)")).MyMaybeToList().Select(Make),
            Make($"[陣営初期情報]"),
            Make($"首都:{Country.GetCapitalArea(game, pushCountry)?.ToString()??"(なし)"}"),
            Make($"資金:{Country.GetFund(game, pushCountry):0.####}"),
            Make($"内政力:{Country.GetAffairPower(game, pushCountry):0.####}"),
            Make($"内政難度:{Country.GetAffairDifficult(game, pushCountry):0.####}"),
            Make($"総内政値:{Country.GetTotalAffair(game, pushCountry):0.####}"),
            Make($"支出:{Country.GetOutFund(game, pushCountry):0.####}"),
            Make($"収入:{Country.GetInFund(game, pushCountry):0.####}"),
            Make("[勝利条件]"),
            .. (pushCountry?.MyApplyF(country=>game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.WinConditionMap.GetValueOrDefault(country)?.Messages?.SelectMany(v => v))).MyApplyF(v=>v is null?["選べません(勝利条件なし)"]:v.Prepend("以下の条件を全て満たす")).Select(Make),
            Make($"[初期人物]"),
            .. (pushCountry is null?[]:Enum.GetValues<ERole>().SelectMany(role=>Person.GetInitPersonMap(game,pushCountry.Value,role).Keys.OrderBy(v=>Person.GetPersonBirthYear(game,v)).Select(v=>PersonInfoText(game,v)))).MyApplyF(v=>v.MyIsEmpty()?["(人物はいません)"]:v).Select(Make),
          ];
        string okButtonText = pushCountry is ECountry.漢 or null ? $"({Text.CountryText(pushCountry,Lang.ja)}陣営は選べません)" : "プレイする";
        Action? okButtonAction = pushCountry is ECountry.漢 or null ? null : () => ClickOkButtonAction(page,game,pushCountry.Value);
        Ask.SetElems(page.AskPanel,title,contents,okButtonText,okButtonAction,true,page.ContentGrid.RenderSize);
        return game;
        static TextBlock Make(string text) => new() { Text = text,HorizontalAlignment = HorizontalAlignment.Center };
        static string PersonInfoText(GameState game,PersonId personId) => $"{personId.Value}  {Text.RoleToText(Person.GetPersonRole(game,personId),Lang.ja)} ランク{Person.GetPersonRank(game,personId)} 齢{Turn.GetYear(game) - Person.GetPersonBirthYear(game,personId)}";
        static void ClickOkButtonAction(Game page,GameState game,ECountry playCountry) {
          GameData.game = SelectPlayCountry(page,game,playCountry).MyApplyF(game => StartGame(page,game)).MyApplyF(game => UpdateGame.AppendNewLog(game,[Text.TurnHeadLogText(game,Lang.ja)])).MyApplyA(game => page.UpdateCountryInfoPanel(page,game)).MyApplyA(game => CharacterRemark.SetElems(page.CharacterRemarkPanel,game.PlayCountry,Text.CharacterRemarkTexts(game,Lang.ja),page.ContentGrid,true));
        }
        static GameState SelectPlayCountry(Game page,GameState game,ECountry playCountry) => UpdateGame.AttachGameStartData(game,playCountry).MyApplyA(game => UpdateCountryPosts(page,game));
        static GameState StartGame(Game page,GameState game) => (game with { Phase = Phase.Planning }).MyApplyA(v => UpdateAreaPanels(page,v)).MyApplyF(UpdateGame.AppendGameStartLog).MyApplyA(game => UpdateLogMessageUI(page,game)).MyApplyA(game => page.UpdateTurnLogUI(page,game)).MyApplyA(async game => await page.UpdateTurnWinCondUI(page,game,true)).MyApplyA(_ => UpdateUI(page));
        static void UpdateUI(Game page) => page.MyApplyA(page => page.CountryPostsPanel.Visibility = Visibility.Visible).MyApplyA(page => page.ScalingElements(page,UIUtil.GetScaleFactor(page.ContentGrid.RenderSize)));
      }
      static GameState SelectTarget(Game page,GameState game,EArea? area) => game.PlayCountry?.MyApplyF(playCountry => game.Phase == Phase.Planning && !Country.IsSleep(game,playCountry) ? (game with { ArmyTargetMap = game.ArmyTargetMap.MyAdd(playCountry,null).MyUpdate(playCountry,(_,_) => area) ?? game.ArmyTargetMap }).MyApplyA(game => { page.UpdateCountryInfoPanel(page,game); }) : null) ?? game;
    }
  }
}