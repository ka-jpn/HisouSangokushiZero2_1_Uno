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
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue;
using Post = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
using Text = HisouSangokushiZero2_1_Uno.Code.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class MainPage:Page {
  private enum InfoPanelState { Explain, WinCond, ParamDataView, ChangeLog, Setting };
  private record AreaElems(Border Back,TextBlock AreaNameText,StackPanel DefensePersonPanel,StackPanel AffairPersonPanel,Post DefensePost,Post AffairPost,TextBlock AffairText,TextBlock ExText,Grid WrapPanel);
  private record CentralPostElems(Dictionary<PostHead,Post> HeadPost,Post[] OtherPosts);
  private static readonly DispatcherQueue dispatcher = DispatcherQueue.GetForCurrentThread();
  private static readonly Dictionary<Post,StackPanel> playerCountryPostPersonPanel = Enum.GetValues<ERole>().SelectMany(role => new List<(Post, StackPanel)>([
    .. Enum.GetValues<PostHead>().Select(headPost=>(new Post(role,new(headPost)),new StackPanel())),
    .. Enumerable.Range(0,UIUtil.capitalPieceCellNum).Select(cellNo=>(new Post(role,new(cellNo)),new StackPanel()))
  ])).ToDictionary();
  private static Dictionary<EArea,AreaElems> areaElemsMap = [];
  private static GameState game = GetGame.GetInitGameScenario(null);
  public MainPage() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(MainPage page) {
      UIUtil.SwitchViewModeActions.Add(() => { UI.RefreshViewMode(page); UI.ScalingElements(page,game,UI.GetScaleFactor(page)); });
      UIUtil.SaveGameActions.Add(() => {
        UI.ShowMessage(page,["セーブ中.."]); 
        Task.Run(async () => { 
          await Storage.WriteStorageData(game,1); 
          dispatcher.TryEnqueue(()=> UI.ShowMessage(page,["セーブ完了"]));
        });
      });
      UIUtil.LoadGameActions.Add(() => { 
        UI.ShowMessage(page,["ロード中.."]);
        Task.Run(async () => {
          var read = await Storage.ReadStorageData(1);
          read.Item2?.MyApplyA(readGame => dispatcher.TryEnqueue(() => InitGame(page,readGame)));
          dispatcher.TryEnqueue(() => UI.ShowMessage(page,[read.Item2 != null ? "ロード完了" : read.Item1 == Storage.ReadSaveFile.Read ? "ロード失敗：ファイルが破損しています" : "ロード失敗：ファイルが見つかりません"]));
        });
      });
      UIUtil.InitGameActions.Add(() => {
        UI.ShowMessage(page,["ゲームを初期化しています.."]);
        Task.Run(() => {
          dispatcher.TryEnqueue(() => InitGame(page,GetGame.GetInitGameScenario(game.NowScenario)));
          dispatcher.TryEnqueue(() => UI.ShowMessage(page,["ゲームを初期化しました"]));
        });
      });
      page.Page.Loaded += (_,_) => LoadedPage(page);
      static void LoadedPage(MainPage page) {
        SetUIElements(page);
        InitGame(page,GetGame.GetInitGameScenario(BaseData.scenarios.FirstOrDefault()));
        UI.SwitchInfoButton(page,InfoPanelState.Explain);
        UI.RefreshViewMode(page);
        UI.ScalingElements(page,game,UI.GetScaleFactor(page));
        UI.AttachEvent(page);
        static void SetUIElements(MainPage page) {
          Dictionary<ERole,Color> countryRolePanelColorMap = new([new(ERole.central,new Color(255,240,240,210)),new(ERole.affair,new Color(255,240,240,240)),new(ERole.defense,new Color(255,210,210,240)),new(ERole.attack,new Color(255,240,210,210))]);
          UI.countryPostPanelMap = countryRolePanelColorMap.ToDictionary(v => v.Key,v => UI.CreateCountryPostPanel(page,v.Key,v.Value));
          page.CountryPostsPanel.MySetChildren([.. UI.countryPostPanelMap.Values.Reverse()]);
          page.InfoLayoutPanel.Visibility = Visibility.Visible;
        }
      }
    }
  }
  private static void InitGame(MainPage page,GameState newGameState) {
    game = newGameState;
    SetInitUI(page,newGameState);
    UpdateAreaPanels(page,newGameState);
    UpdateCountryPosts(page,newGameState);
    UpdateCountryInfoPanel(page,newGameState);
    page.MovePersonCanvas.MySetChildren([]);
    page.LogContentPanel.MySetChildren([]);
    page.MapAnimationElementsCanvas.MySetChildren([]);
    UI.countryFlagMap.ToList().ForEach(v => v.Value.Visibility = Visibility.Collapsed);
    UI.UpdateLogMessageUI(page,newGameState);
    static void SetInitUI(MainPage page,GameState game) {
      areaElemsMap = game.AreaMap.ToDictionary(v => v.Key,v => CreateAreaElems(page,game,v.Key));
      UI.infoPanelMap = new() { { InfoPanelState.Explain,page.ExplainPanel },{ InfoPanelState.WinCond,page.WinCondPanel },{ InfoPanelState.ParamDataView,page.ParamListPanel },{ InfoPanelState.ChangeLog,page.ChangeLogPanel },{ InfoPanelState.Setting,page.SettingPanel } };
      page.MapElementsCanvas.MySetChildren([.. game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.RoadConnections.Select(road => MaybeCreateRoadLine(game,road)).MyNonNull() ?? [],.. areaElemsMap.Select(v=>CreateAreaPanelFromAreaElems(page,game,v))]);
      static Line? MaybeCreateRoadLine(GameState game,Scenario.Road road) {
        return Area.GetAreaPoint(game,road.From,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth.Value) is Point from && Area.GetAreaPoint(game,road.To,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth.Value) is Point to ? CreateRoadLine(road,from,to) : null;
        static Line CreateRoadLine(Scenario.Road road,Point from,Point to) => new() { X1 = from.X,Y1 = from.Y,X2 = to.X,Y2 = to.Y,Stroke = (road.Kind switch { RoadKind.land => UIUtil.landRoadColor, RoadKind.water => UIUtil.waterRoadColor }).ToBrush(),StrokeThickness = 10 * Math.Pow(road.Easiness,1.7) / 2 + 20 };
      }
      static AreaElems CreateAreaElems(MainPage page,GameState game,EArea area) {
        Border areaBackBorder = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height,CornerRadius = UIUtil.areaCornerRadius,BorderBrush = Colors.Red };
        TextBlock areaNameText = new() { HorizontalAlignment = HorizontalAlignment.Center };
        StackPanel areaDefensePersonPanel = [];
        StackPanel areaAffairPersonPanel = [];
        Post areaDefensePost = new(ERole.defense,new(area));
        Post areaAffairPost = new(ERole.affair,new(area));
        TextBlock affairText = new() { HorizontalAlignment = HorizontalAlignment.Center };
        TextBlock exText = new() { HorizontalAlignment = HorizontalAlignment.Center };
        Grid areaWrapPanel = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height };
        return new(areaBackBorder,areaNameText,areaDefensePersonPanel,areaAffairPersonPanel,areaDefensePost,areaAffairPost,affairText,exText,areaWrapPanel);
      }
      static Grid CreateAreaPanelFromAreaElems(MainPage page,GameState game,KeyValuePair<EArea,AreaElems> areaElemInfo) {
        Grid areaPanel = new() { Width = UIUtil.areaSize.Width,Height = UIUtil.areaSize.Height,CornerRadius = UIUtil.areaCornerRadius };
        StackPanel personPutAreaPanel = new() { HorizontalAlignment = HorizontalAlignment.Center,Orientation = Orientation.Horizontal,BorderBrush = UI.GetPostFrameColor(game,areaElemInfo.Key).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) };
        areaPanel.PointerPressed += (_,_) => UI.PushArea.Push(areaElemInfo.Key);
        areaPanel.PointerExited += (_,_) => UI.PushArea.Exit();
        areaPanel.PointerReleased += (_,_) => MainPage.game = UI.PushArea.Release(page,MainPage.game,areaElemInfo.Key);
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
  private static void UpdateAreaPanels(MainPage page,GameState game) {
    double capitalBorderWidth = 3.5;
    areaElemsMap.ToList().ForEach(areaElems => {
      AreaData? areaData = game.AreaMap.GetValueOrDefault(areaElems.Key);
      areaElems.Value.Back.BorderThickness = new(game.CountryMap.Values.Select(v => v.CapitalArea).Contains(areaElems.Key) ? capitalBorderWidth : 0);
      areaElems.Value.Back.Background = Country.GetCountryColor(game,areaData?.Country).ToBrush();
      areaElems.Value.AreaNameText.Text = $"{areaElems.Key} {areaData?.Country?.ToString() ?? $"自治"}領";
      areaElems.Value.DefensePersonPanel.MySetChildren([..game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == areaElems.Value.DefensePost)?.MyApplyF(param => CreatePersonPanel(page,game,param)).MyMaybeToList()??[]]);
      areaElems.Value.AffairPersonPanel.MySetChildren([..game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post == areaElems.Value.AffairPost)?.MyApplyF(param => CreatePersonPanel(page,game,param)).MyMaybeToList()??[]]);
      areaElems.Value.AffairText.Text = $"{Math.Floor(areaData?.AffairParam.AffairNow ?? 0)}/{Math.Floor(areaData?.AffairParam.AffairsMax ?? 0)}";
      areaElems.Value.ExText.Text = areaData?.Country?.MyApplyF(country => (Country.IsSleep(game,country) ? $"休み {Country.GetSleepTurn(game,country)}" : null) + (Country.IsFocusDefense(game,country) ? "(防)" : null)); ;
      areaElems.Value.WrapPanel.Background = Area.IsPlayerSelectable(game, areaElems.Key) ? null : UIUtil.grayoutColor.ToBrush();
    });
  }
  private static void UpdateCountryPosts(MainPage page,GameState game) {
    playerCountryPostPersonPanel.ToList().ForEach(countryPostInfo => countryPostInfo.Value.MySetChildren([.. game.PersonMap.MyNullable().FirstOrDefault(v =>Person.GetPersonCountry(game,v?.Key??new(string.Empty)) == game.PlayCountry && v?.Value.Post == countryPostInfo.Key)?.MyApplyF(param => CreatePersonPanel(page,game,param)).MyMaybeToList() ?? []]));
  }
  private static void UpdateCountryInfoPanel(MainPage page,GameState game) {
    List<UIElement> contents = game.Phase switch {
      Phase.Starting => ShowSelectScenario(page,game),
      Phase.Planning or Phase.Execution => ShowCountryInfo(game),
      Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => ShowEndGameInfo(page,game)
    };
    string buttonText = Text.EndPhaseButtonText(game.Phase,Lang.ja);
    double? buttonHeight = game.Phase switch {
      Phase.Starting => null,
      Phase.Planning or Phase.Execution => BasicStyle.textHeight * 2.5,
      Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => BasicStyle.textHeight * 5.5
    };
    StateInfo.Show(page.StateInfoPanel,contents,buttonText,buttonHeight,() => buttonAction(page));
    static void buttonAction(MainPage page) {
      MainPage.game = MainPage.game.MyApplyF(game => game.Phase switch {
        Phase.Starting => game,
        Phase.Planning => EndPlanningPhase(page,game).MyApplyA(game => UpdateCountryInfoPanel(page,game)),
        Phase.Execution => EndExecutionPhase(page,game).MyApplyA(game => UpdateCountryInfoPanel(page,game)),
        Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => game.MyApplyA(game => ShowGameEndLogButtonClick(page,game))
      });
      if(MainPage.game.Phase == Phase.Execution) {
        ExecutionMoveFlag(page,MainPage.game);
      }
      static void ExecutionMoveFlag(MainPage page,GameState game) {
        game.ArmyTargetMap.Where(v => v.Value != null).ToDictionary(attackInfo => GetFlag(game,attackInfo.Key),attackInfo => CalcFlagMovePos(page,game,attackInfo)).MyApplyA(flagMap => page.MapAnimationElementsCanvas.MySetChildren([.. flagMap.Keys])).MyApplyA(flagMap => MoveFlags(page,flagMap));
        static Grid GetFlag(GameState game,ECountry attackCountry) {
          if(UI.countryFlagMap.TryGetValue(attackCountry,out Grid? flag)) {
            return flag;
          } else {
            return CreateFlag(game,attackCountry).MyApplyA(createdFlag => UI.countryFlagMap = UI.countryFlagMap.MyAdd(attackCountry,createdFlag));
          }
        }
        static Grid CreateFlag(GameState game,ECountry attackCountry) {
          string flagText = attackCountry.ToString();
          TextBlock flagTextBlock = new() { Text = flagText,RenderTransform = new ScaleTransform { ScaleX = Math.Min(1,(double)2 / flagText.Length) * 2,ScaleY = 2 },Width = Math.Min(2,flagText.Length) * BasicStyle.fontsize * 2,Height = BasicStyle.fontsize * 2 };
          Grid flagPanel = new() { Width = BasicStyle.fontsize * 4,Height = BasicStyle.fontsize * 3,Background = Country.GetCountryColor(game,attackCountry).ToBrush(),HorizontalAlignment = HorizontalAlignment.Left,VerticalAlignment = VerticalAlignment.Top };
          Image attackImage = new() { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/army.svg")),Width = BasicStyle.fontsize * 4,Height = BasicStyle.fontsize * 4,HorizontalAlignment = HorizontalAlignment.Right };
          Grid attackImagePanel = new Grid() { Width = BasicStyle.fontsize * 6,Height = BasicStyle.fontsize * 5,Visibility = Visibility.Collapsed }.MySetChildren([attackImage,flagPanel.MySetChildren([flagTextBlock])]);
          return attackImagePanel;
        }
        static List<Point> CalcFlagMovePos(MainPage page,GameState game,KeyValuePair<ECountry,EArea?> attackInfo) {
          Point? srcPoint = Country.GetCapitalArea(game,attackInfo.Key)?.MyApplyF(capital => Area.GetAreaPoint(game,capital,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth.Value));
          Point? dstPoint = attackInfo.Value?.MyApplyF(target => Area.GetAreaPoint(game,target,UIUtil.mapSize,UIUtil.areaSize,UIUtil.mapGridCount,UIUtil.infoFrameWidth.Value));
          List<Point> posList = [.. Enumerable.Range(0,60 + 1).Select(v => (double)v / 60).Select(lerpWeight => srcPoint is Point src && dstPoint is Point dst ? new Point(double.Lerp(src.X,dst.X,lerpWeight),double.Lerp(src.Y,dst.Y,lerpWeight)) : (Point?)null).MyNonNull()];
          return posList;
        }
        static void MoveFlags(MainPage page,Dictionary<Grid,List<Point>> flags) {
          _ = Task.Run(async () => {
            await flags.MyAsyncForEachConcurrent(async v => {
              dispatcher.TryEnqueue(() => {
                Canvas.SetLeft(v.Key,v.Value.FirstOrDefault()?.X - v.Key.Width / 2 ?? 0);
                Canvas.SetTop(v.Key,v.Value.FirstOrDefault()?.Y - v.Key.Height / 2 ?? 0);
                v.Key.Visibility = Visibility.Visible;
              });
              await v.Value.Skip(1).MyAsyncForEachSequential(async pos => {
                if(MainPage.game.Phase == Phase.Execution) {
                  await Task.Delay(15);
                  dispatcher.TryEnqueue(() => {
                    Canvas.SetLeft(v.Key,pos.X - v.Key.Width / 2);
                    Canvas.SetTop(v.Key,pos.Y - v.Key.Height / 2);
                  });
                }
              });
            });
            if(MainPage.game.Phase == Phase.Execution) {
              dispatcher.TryEnqueue(() => UpdateAreaPanels(page,MainPage.game));
            }
          });
        }
      }
    }
    static List<UIElement> ShowSelectScenario(MainPage page,GameState game) => [
      new StackPanel{ Height=BasicStyle.textHeight*2 },
      new TextBlock { Text="シナリオ",TextAlignment=TextAlignment.Center },
      new ComboBox {
        Height=BasicStyle.textHeight*2.5,
        HorizontalAlignment=HorizontalAlignment.Center,
        SelectedIndex=BaseData.scenarios.MyGetIndex(v => v==game.NowScenario)??0,
        Foreground=Colors.Black,
        Background=Colors.White,
        Padding=new(20,0,10,0),
        ItemContainerStyle = new Style(typeof(ComboBoxItem)).MyApplyA(style=>style.Setters.Add(new Setter(FontSizeProperty,BasicStyle.fontsize*UI.GetScaleFactor(page)))),
      }.MyApplyA(elem => BaseData.scenarios.Select(v =>v.Value).ToList().ForEach(elem.Items.Add)).MyApplyA(v=>v.SelectionChanged+=(_,_) => (v.SelectedItem as string)?.MyApplyA(text =>InitGame(page,GetGame.GetInitGameScenario(new(text))))),
      new TextBlock { Text=game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.MyApplyF(v=>$"{v.StartYear}年開始{v.EndYear}年終了"),TextAlignment=TextAlignment.Center },
      new StackPanel{ Height=BasicStyle.textHeight*2 },
      new TextBlock{ Text=$"プレイ勢力を選択後",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"情報が表示されます",TextAlignment=TextAlignment.Center },
      new StackPanel{ Height=BasicStyle.textHeight*2 }
    ];
    static List<UIElement> ShowCountryInfo(GameState game) => [
      new TextBlock{ Text=Turn.GetCalendarText(game),TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"プレイ勢力:{game.PlayCountry}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"首都:{game.PlayCountry?.MyApplyF(country=>Country.GetCapitalArea(game,country))}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"資金:{game.PlayCountry?.MyApplyF(country=>Country.GetFund(game,country)?.ToString("0.####"))}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"内政力:{game.PlayCountry?.MyApplyF(country=>Country.GetAffairPower(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"内政難度:{game.PlayCountry?.MyApplyF(country=>Country.GetAffairDifficult(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"総内政値:{game.PlayCountry?.MyApplyF(country=>Country.GetTotalAffair(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"支出:{game.PlayCountry?.MyApplyF(country=>Country.GetOutFund(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"収入:{game.PlayCountry?.MyApplyF(country=>Country.GetInFund(game,country)).ToString("0.####")}",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"侵攻:{Country.GetTargetArea(game,game.PlayCountry)?.ToString()??"なし"}",TextAlignment=TextAlignment.Center },
    ];
    static List<UIElement> ShowEndGameInfo(MainPage page,GameState game) => [
      new TextBlock{ Text=Turn.GetCalendarText(game),TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"プレイ勢力:{game.PlayCountry}",TextAlignment=TextAlignment.Center },
      new StackPanel{ Height=BasicStyle.textHeight },
      new TextBlock{ Text=$"ゲーム終了",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=$"結果",TextAlignment=TextAlignment.Center },
      new TextBlock{ Text=game.Phase switch { Phase.PerishEnd=>"滅亡敗北",Phase.TurnLimitOverEnd=>"存続勝利",Phase.WinEnd=>"条件勝利",Phase.OtherWinEnd=>"他陣営条件勝利敗北",_ =>string.Empty },TextAlignment=TextAlignment.Center },
      new StackPanel{ Height=BasicStyle.textHeight },
    ];
    static void ShowGameEndLogButtonClick(MainPage page,GameState game) {
      string title = $"ゲームログ";
      List<TextBlock> contents = [.. game.GameLog.Select(log => new TextBlock { Text = log })];
      string okButtonText = "ゲームコメントを投稿する";
      Ask.SetElems(page.AskPanel,title,contents,okButtonText,true,() => LogButtonClick(game),false,page.ContentGrid.RenderSize,UI.GetScaleFactor(page));
      static void LogButtonClick(GameState game) {
        string url = $"https://karintougames.com/siteContents/gameComment.php?caption={BaseData.name.Value} ver.{BaseData.version.Value}&comment={HttpUtility.UrlEncode(string.Join('\n',game.GameLog))}";
#if __WASM__
        Uno.Foundation.WebAssemblyRuntime.InvokeJS($"top.location.href='{url}';");
#else
      _ = Windows.System.Launcher.LaunchUriAsync(new Uri(url));
#endif
      }
    }
    static GameState EndPlanningPhase(MainPage page,GameState game) {
      return game.MyApplyF(game => UpdateGame.AutoPutPostCPU(game,[ECountry.漢])).MyApplyF(CalcArmyTarget).MyApplyF(game => game with { Phase = Phase.Execution }).MyApplyA(game => UpdateAreaPanels(page,game)).MyApplyF(ArmyAttack).MyApplyA(game => UI.UpdateLogMessageUI(page,game));
      static GameState CalcArmyTarget(GameState game) {
        Dictionary<ECountry,EArea?> playerArmyTargetMap = game.PlayCountry.MyMaybeToList().Where(country => !Country.IsSleep(game,country)).ToDictionary(v => v,v => game.ArmyTargetMap.GetValueOrDefault(v));
        Dictionary<ECountry,EArea?> NPCArmyTargetMap = game.CountryMap.Keys.Except(game.PlayCountry.MyMaybeToList()).Where(country => !Country.IsSleep(game,country)).ToDictionary(country => country,country => country == ECountry.漢 ? null : RandomSelectNPCAttackTarget(game,country));
        return game with { ArmyTargetMap = new([.. NPCArmyTargetMap,.. playerArmyTargetMap]) };
        static EArea? RandomSelectNPCAttackTarget(GameState game,ECountry country) => Area.GetAdjacentAnotherCountryAllAreas(game,country).MyNullable().Append(null).MyPickAny().MyApplyF(area => area?.MyApplyF(game.AreaMap.GetValueOrDefault)?.Country == null && MyRandom.RandomJudge(0.9) ? null : area);
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
      UI.countryFlagMap.ToList().ForEach(v => v.Value.Visibility = Visibility.Collapsed);
      return game.MyApplyF(UpdateGame.GameEndJudge).MyApplyF(game => game.Phase is Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd ? game : game.MyApplyF(game => NextTurn(page,game)).MyApplyA(game => UI.UpdateLogMessageUI(page,game)).MyApplyF(UpdateGame.GameEndJudge));
      static GameState NextTurn(MainPage page,GameState game) => game.MyApplyF(UpdateGame.NextTurn).MyApplyA(game => UpdateCountryPosts(page,game)).MyApplyF(v => v with { Phase = Phase.Planning,ArmyTargetMap = [] }).MyApplyA(game => UpdateAreaPanels(page,game)).MyApplyA(game => UI.UpdateTurnLogUI(page,game)).MyApplyA(game => UI.UpdateTurnWinCondUI(page,game,true));
    }
  }
  private static Grid CreatePersonPutPanel(GameState game,Post post,string backText,StackPanel personPutInnerPanel) {
    Grid personPutPanel = new() { Width = UIUtil.personPutSize.Width,Height = UIUtil.personPutSize.Height,BorderBrush = UI.GetPostFrameColor(game, post.PostKind.MaybeArea).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth),Background = Colors.Transparent };
    TextBlock personPutBackText = new() { Text = backText,Foreground = Windows.UI.Color.FromArgb(100,100,100,100),HorizontalAlignment = HorizontalAlignment.Center,VerticalAlignment = VerticalAlignment.Center,RenderTransform = new ScaleTransform() { ScaleX = UIUtil.personPutFontScale,ScaleY = UIUtil.personPutFontScale,CenterX = UIUtil.CalcFullWidthTextLength(backText) * BasicStyle.fontsize / 2,CenterY = BasicStyle.textHeight / 2 } };
    personPutPanel.PointerEntered += (_,_) => EnterPersonPutPanel(MainPage.game,personPutInnerPanel,post);
    personPutPanel.PointerExited += (_,_) => ExitPersonPutPanel(personPutInnerPanel);
    return personPutPanel.MySetChildren([personPutBackText,personPutInnerPanel]);
    static void EnterPersonPutPanel(GameState game,StackPanel personPutInnerPanel,Post post) {
      if(game.Phase != Phase.Starting && (post.PostKind.MaybeArea?.MyApplyF(area => game.AreaMap.GetValueOrDefault(area)?.Country == game.PlayCountry) ?? true)) {
        personPutInnerPanel.Background = Windows.UI.Color.FromArgb(150,255,255,255);
        UI.pointerover = (personPutInnerPanel, post);
      }
    }
    static void ExitPersonPutPanel(StackPanel personPutInnerPanel) {
      if(UI.pointerover != null) {
        personPutInnerPanel.Background = Colors.Transparent;
        UI.pointerover = null;
      }
    }
  }
  private static Grid CreatePersonPanel(MainPage page,GameState game,KeyValuePair<PersonId,PersonData> person) {
    double minFullWidthLength = 2.25;
    double margin = page.FontSize * (UIUtil.CalcFullWidthTextLength(person.Key.Value) - 2);
    Grid panel = new Grid { Width = UIUtil.personPutSize.Width,Height = UIUtil.personPutSize.Height,Background = Country.GetCountryColor(game,Person.GetPersonCountry(game, person.Key)).ToBrush() }.MySetChildren([
      new StackPanel { HorizontalAlignment=HorizontalAlignment.Stretch,VerticalAlignment=VerticalAlignment.Stretch,Background=Windows.UI.Color.FromArgb((byte)(20*Person.GetPersonRank(game,person.Key)),0,0,0) }.MySetChildren([
        GetRankPanel(page,game,person),
        new TextBlock { Text=person.Key.Value,TextAlignment=TextAlignment.Center,Margin=new(-margin/2,0),RenderTransform=new ScaleTransform{ ScaleX=minFullWidthLength/Math.Max(minFullWidthLength,UIUtil.CalcFullWidthTextLength(person.Key.Value))*UIUtil.personNameFontScale,ScaleY=UIUtil.personNameFontScale,CenterX=UIUtil.personPutSize.Width/2+margin/minFullWidthLength  }  }
      ])
    ]);
    panel.PointerPressed += (_,e) => PickPersonPanel(page,MainPage.game,e,panel,person.Key);
    return panel;
    static StackPanel GetRankPanel(MainPage page,GameState game,KeyValuePair<PersonId,PersonData> person) {
      int postRank = Person.CalcRoleRank(game,person.Key,person.Value.Post?.PostRole);
      int personRank = Person.GetPersonRank(game,person.Key);
      return new StackPanel { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center,RenderTransform = new ScaleTransform() { ScaleX = UIUtil.personRankFontScale,ScaleY = UIUtil.personRankFontScale,CenterX = page.FontSize / 2 } }.MySetChildren(GetRankTextBlock(game,person.Key,postRank,personRank == postRank));
      static List<UIElement> GetRankTextBlock(GameState game,PersonId person,int rank,bool isMatchRole) => [
        new TextBlock() { Margin = new(0,-3,0,3),Text=rank.ToString(),Background=isMatchRole?Colors.Transparent :Colors.Red},
        .. (isMatchRole?null:new Image { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{Person.GetPersonRole(game,person)}.svg")),VerticalAlignment = VerticalAlignment.Top,Width = BasicStyle.textHeight * 0.75,Height = BasicStyle.textHeight * 0.75 }).MyMaybeToList()
      ];
    }
    static void PickPersonPanel(MainPage page,GameState game,PointerRoutedEventArgs e,Panel personPanel,PersonId person) {
      if(game.Phase != Phase.Starting && Person.GetPersonCountry(game,person) == game.PlayCountry && personPanel.Parent is Panel parentPanel) {
        personPanel.IsHitTestVisible = false;
        parentPanel.MySetChildren([]);
        page.MovePersonCanvas.MySetChildren([personPanel]);
        MovePerson(page,personPanel,e);
        UI.pick = (parentPanel, person);
      }
    }
  }
  private static void MovePerson(MainPage page,UIElement personPanel,PointerRoutedEventArgs e) {
    Canvas.SetLeft(personPanel,e.GetCurrentPoint(page.MovePersonCanvas).Position.X - UIUtil.personPutSize.Width / 2);
    Canvas.SetTop(personPanel,e.GetCurrentPoint(page.MovePersonCanvas).Position.Y - UIUtil.personPutSize.Height / 2);
  }
  private static class UI {
    private static ERole activePanelRole = ERole.central;
    internal static Dictionary<InfoPanelState,UserControl> infoPanelMap = [];
    internal static readonly Color buttonBackground = new(175,255,255,255);
    internal static (Panel panel, Post post)? pointerover = null;
    internal static (Panel panel, PersonId person)? pick = null;
    internal static Dictionary<ERole,StackPanel> countryPostPanelMap = [];
    internal static Dictionary<ECountry,Grid> countryFlagMap = [];
    internal static StackPanel CreateCountryPostPanel(MainPage page,ERole role,Color backColor) {
      return new StackPanel() { Padding = new(1.5,0),Background = backColor.ToBrush() }.MySetChildren([
        new StackPanel() { Orientation = Orientation.Horizontal,HorizontalAlignment = HorizontalAlignment.Center }.MySetChildren([
          new TextBlock { Text = Text.RoleToText(role,Lang.ja) },
          new Image { Source = new SvgImageSource(new($"ms-appx:///Assets/Svg/{role}.svg")),Width = BasicStyle.textHeight,Height = BasicStyle.textHeight,VerticalAlignment = VerticalAlignment.Center }
        ]),
        CreateCountryPosts(page,game,role)
      ]);
      static StackPanel CreateCountryPosts(MainPage page,GameState game,ERole role) {
        return new StackPanel().MySetChildren([
          new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
            CreatePersonHeadPostPanel(game,role),
            CreateAutoPutPersonButton(page,game,role)
          ]),
          CreatePersonPostPanelElems(game,role)
        ]);
        static Button CreateAutoPutPersonButton(MainPage page,GameState game,ERole role) {
          Button autoPutPersonButton = new Button { Width = UIUtil.personPutSize.Width * 3,VerticalAlignment = VerticalAlignment.Stretch,Background = Windows.UI.Color.FromArgb(100,100,100,100) }.MyApplyA(v => v.Content = new TextBlock { Text = "オート配置" });
          autoPutPersonButton.Click += (_,_) => MainPage.game = AutoPutPersonButtonClick(page,MainPage.game);
          return autoPutPersonButton;
          GameState AutoPutPersonButtonClick(MainPage page,GameState game) => game.PlayCountry?.MyApplyF(country => Code.Post.GetAutoPutPost(game,country,role)).MyApplyF(postMap => UpdateGame.SetPersonPost(game,postMap)).MyApplyA(v => UpdateAreaPanels(page,v)).MyApplyA(game => UpdateCountryPosts(page,game)) ?? game;
        }
        static StackPanel CreatePersonHeadPostPanel(GameState game,ERole role) {
          return new StackPanel { Orientation = Orientation.Horizontal,BorderBrush = GetPostFrameColor(game, null).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) }.MySetChildren([
            .. Enum.GetValues<PostHead>().Select(v=> new Post(role,new(v)).MyApplyF(post=> CreatePersonPutPanel(game,post,GetPlayerCountryPostBackString(post.PostKind),playerCountryPostPersonPanel.GetValueOrDefault(post)??[]))),
          ]);
        }
        static StackPanel CreatePersonPostPanelElems(GameState game,ERole role) {
          return new StackPanel { BorderBrush = GetPostFrameColor(game, null).ToBrush(),BorderThickness = new(UIUtil.postFrameWidth) }.MySetChildren([.. Enumerable.Range(0,UIUtil.capitalPieceRowNum).Select(row => GetPersonPostLinePanel(game,role,row,game.PersonMap.Where(v =>Person.GetPersonCountry(game,v.Key) == game.PlayCountry).ToDictionary()))]);
          static StackPanel GetPersonPostLinePanel(GameState game,ERole role,int rowNo,Dictionary<PersonId,PersonData> personMap) => new StackPanel { Orientation = Orientation.Horizontal }.MySetChildren([
            .. Enumerable.Range(0,UIUtil.capitalPieceColumnNum).Select(i => new Post(role,new(rowNo * UIUtil.capitalPieceColumnNum + i)).MyApplyF(post=> CreatePersonPutPanel(game,post,GetPlayerCountryPostBackString(post.PostKind),playerCountryPostPersonPanel.GetValueOrDefault(post) ?? [])))
          ]);
        }
        static string GetPlayerCountryPostBackString(PostKind postKind) => postKind.MaybeHead switch { PostHead.main => "筆頭", PostHead.sub => "次席", _ => $"{postKind.MaybePostNo + 1}" };
      }
    }
    internal static double GetScaleFactor(MainPage page) => Math.Max(Math.Max(UIUtil.fixModeMaxWidth,page.ContentGrid.RenderSize.Width) / UIUtil.mapSize.Width,Math.Max(UIUtil.fixModeMaxWidth,page.ContentGrid.RenderSize.Height) / UIUtil.mapSize.Height);
    internal static Color GetPostFrameColor(GameState game,EArea? area) => area != null && (game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.ChinaAreas ?? []).Contains(area.Value) ? new Color(150,100,100,30) : new Color(150,0,0,0);
    internal static void ResizeLogMessageUI(MainPage page,double scaleFactor) {
      page.LogContentPanel.MaxWidth = page.LogScrollPanel.ActualWidth / scaleFactor;
      page.LogContentPanel.Height = page.LogContentPanel.Children.Sum(v=>v.DesiredSize.Height);
      page.LogContentPanel.Margin = new(0,0,page.LogContentPanel.DesiredSize.Width * (scaleFactor - 1),page.LogContentPanel.Height * (scaleFactor - 1));
      page.LogContentPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
    }
    internal static void ResizeCountryPostUI(MainPage page,double scaleFactor) {
      double PostPanelLeftUnit = UIUtil.mapSize.Width / 4 + (page.ContentGrid.RenderSize.Width - page.MapPanel.Width) / scaleFactor / 3;
      countryPostPanelMap.Values.Select((elem,index) => (elem, index)).ToList().ForEach(v => Canvas.SetLeft(v.elem,PostPanelLeftUnit * v.index));
    }
    internal static void ResizeTurnWinCondPanelUI(MainPage page,double scaleFactor) {
      page.TurnWinCondPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.TurnWinCondPanel.RenderSize.Width/2 };
    }
    internal static void UpdateLogMessageUI(MainPage page,GameState game) {
      game.NewLog.Select(logText => new TextBlock() { Text = logText }).ToList().ForEach(page.LogContentPanel.Children.Add);
      game.NewLog.Clear();
    }
    internal static void UpdateTurnLogUI(MainPage page,GameState game) {
      StackPanel panel = new StackPanel() {
        Background = Windows.UI.Color.FromArgb(187, 255, 255, 255),
        Height = game.TrunNewLog.Count * BasicStyle.textHeight,
        IsHitTestVisible = false
      }.MySetChildren([.. game.TrunNewLog.Select(logText => new TextBlock() { Text = logText })]).MyApplyA(async elem => {
        game.TrunNewLog.Clear();
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
      Dictionary<string,bool?> winCondMap = game.PlayCountry?.MyApplyF(v=>game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.WinConditionMap.GetValueOrDefault(v))?.ProgressExplainFunc(game) ?? [];
      StackPanel panel = new StackPanel() {
        Background = Windows.UI.Color.FromArgb(187, 255, 255, 255),
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
          dispatcher.TryEnqueue(() => { elem.Opacity = 1 - v; });
          await Task.Delay(15);
        });
        page.TurnWinCondPanel.Children.Remove(elem);
      });
      page.TurnWinCondPanel.Children.Add(panel);
      dispatcher.TryEnqueue(() => ResizeTurnWinCondPanelUI(page,GetScaleFactor(page)));
    }
    internal static void ShowMessage(MainPage page,string[] messages) {
      StackPanel panel = new StackPanel() {
        Background = Windows.UI.Color.FromArgb(187, 255, 255, 255),
        Height = messages.Length * BasicStyle.textHeight,
        IsHitTestVisible = false
      }.MySetChildren([.. messages.Select(logText => new TextBlock() { Text = logText })]).MyApplyA(async elem => {
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
    internal static void RefreshViewMode(MainPage page) {
      page.SwitchViewModeButtonText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "▼" : "▲";
      page.ContentGrid.MaxWidth = UIUtil.viewMode == UIUtil.ViewMode.fix ? UIUtil.fixModeMaxWidth : double.MaxValue;
    }
    internal static void SwitchInfoButton(MainPage page,InfoPanelState clickButtonInfoPanelState) {
      Dictionary<InfoPanelState,Button> buttonMap = new() { { InfoPanelState.Explain,page.ExplainButton },{ InfoPanelState.WinCond,page.WinCondButton },{ InfoPanelState.ParamDataView,page.PersonDataButton },{ InfoPanelState.ChangeLog,page.ChangeLogButton },{ InfoPanelState.Setting,page.SettingButton } };
      buttonMap.ToList().ForEach(v => v.Value.Background = v.Key == clickButtonInfoPanelState ? Colors.LightGray : Colors.WhiteSmoke);
      infoPanelMap.ExceptBy([clickButtonInfoPanelState],v => v.Key).ToList().ForEach(v => v.Value.Visibility = Visibility.Collapsed);
      infoPanelMap.IntersectBy([clickButtonInfoPanelState],v => v.Key).ToList().ForEach(v => v.Value.Visibility = Visibility.Visible);
    }
    internal static void ScalingElements(MainPage page,GameState game,double scaleFactor) {
      page.CountryPostsPanel.Height = UIUtil.countryPersonPutPanelHeight * scaleFactor;
      page.CountryPostsPanel.Width = page.RenderSize.Width;
      page.CountryPostsPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.MapPanel.Width = UIUtil.mapSize.Width * scaleFactor;
      page.MapPanel.Height = UIUtil.mapSize.Height * scaleFactor;
      page.MapInnerPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.MovePersonCanvas.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      page.InfoLayoutPanel.Height = page.RenderSize.Height - page.CountryPostsPanel.Height;
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
      StateInfo.ResizeElem(page.StateInfoPanel,scaleFactor);
      ResizeTurnWinCondPanelUI(page,scaleFactor);
      ResizeCountryPostUI(page,scaleFactor);
      UpdateCountryInfoPanel(page,game);
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
        KeyValuePair<PersonId,PersonData>? maybeDestPersonInfo = game.PersonMap.MyNullable().FirstOrDefault(v =>Person.GetPersonCountry(game,v?.Key??new(string.Empty)) == game.PlayCountry && v?.Value.Post == pointerover?.post);
        return UpdateGame.PutPersonFromUI(game,maybeDestPersonInfo?.Key,pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == maybeDestPersonInfo?.Key)?.MyApplyF(destPersonInfo => pick?.panel.MySetChildren([CreatePersonPanel(page,game,destPersonInfo)])));
      }
      static GameState putPerson(MainPage page,GameState game) {
        return UpdateGame.PutPersonFromUI(game,pick?.person,pointerover?.post ?? pick?.person.MyApplyF(game.PersonMap.GetValueOrDefault)?.Post).MyApplyA(game => game.PersonMap.MyNullable().FirstOrDefault(v => v?.Key == pick?.person)?.MyApplyF(putPersonInfo => (pointerover?.panel ?? pick?.panel)?.MySetChildren([CreatePersonPanel(page,game,putPersonInfo)])));
      }
    }
    internal static void UpdateCountryPostPanelZIndex(MainPage page,ERole toActiveRole) {
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
          dispatcher.TryEnqueue(() => activePanelRole = toActiveRole);
        }
      }
      static List<StackPanel> GetResetZIndexPanels(ERole[] resetZIndexRoles) => resetZIndexRoles.Select(countryPostPanelMap.GetValueOrDefault).MyNonNull();
    }
    internal class PushArea {
      internal static EArea? pushArea = null;
      internal static void Push(EArea area) => pushArea = area;
      internal static void Exit() => pushArea = null;
      internal static GameState Release(MainPage page,GameState game,EArea area) {
        ECountry? areaCountry = game.AreaMap.GetValueOrDefault(area)?.Country;
        return pushArea != area? game : game.Phase == Phase.Starting ? areaCountry?.MyApplyF(country => ShowSelectPlayCountryPanel(page,game,country)) ?? game : Area.IsPlayerSelectable(game,area) ? SelectTarget(page,game,areaCountry != game.PlayCountry ? area : null) : game;
        static GameState ShowSelectPlayCountryPanel(MainPage page,GameState game,ECountry pushCountry) {
          string title = $"{pushCountry}陣営";
          List<TextBlock> contents = [
            new TextBlock{ Text=game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.MyApplyF(scenario=>$"シナリオ:{game.NowScenario.Value}({scenario.StartYear}年開始 {scenario.EndYear}年終了)"),HorizontalAlignment=HorizontalAlignment.Center },
            new TextBlock{ Text=$"[陣営初期情報]",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"首都:{Country.GetCapitalArea(game,pushCountry)}",HorizontalAlignment=HorizontalAlignment.Center },
            new TextBlock{ Text=$"資金:{Country.GetFund(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"内政力:{Country.GetAffairPower(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"内政難度:{Country.GetAffairDifficult(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"総内政値:{Country.GetTotalAffair(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"支出:{Country.GetOutFund(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text=$"収入:{Country.GetInFund(game,pushCountry):0.####}",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text="[勝利条件]",HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock{ Text="以下の条件を全て満たす",HorizontalAlignment = HorizontalAlignment.Center },
            .. game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.WinConditionMap.GetValueOrDefault(pushCountry)?.Messages.SelectMany(v => v.Select(v => new TextBlock { Text = v, HorizontalAlignment = HorizontalAlignment.Center }))??[],
            new TextBlock{ Text=$"[初期人物]",HorizontalAlignment = HorizontalAlignment.Center  },
            .. Enum.GetValues<ERole>().SelectMany(role=>Person.GetInitPersonMap(game,pushCountry,role).OrderBy(v=>Person.GetPersonBirthYear(game,v.Key)).Select(v => new TextBlock { Text = $"{v.Key.Value}  {Text.RoleToText(Person.GetPersonRole(game,v.Key),Lang.ja)} ランク{Person.GetPersonRank(game,v.Key)} 齢{Turn.GetYear(game)-Person.GetPersonBirthYear(game,v.Key)}",HorizontalAlignment=HorizontalAlignment.Center  })),
          ];
          string okButtonText = pushCountry != ECountry.漢 ? "プレイする" : "(漢は選べません)";
          bool enableOkButton = pushCountry != ECountry.漢;
          Ask.SetElems(page.AskPanel,title,contents,okButtonText,enableOkButton,() => ClickOkButtonAction(page,game,pushCountry),true,page.ContentGrid.RenderSize,GetScaleFactor(page));
          return game;
          static void ClickOkButtonAction(MainPage page,GameState game,ECountry playCountry) {
            MainPage.game = SelectPlayCountry(page,game,playCountry).MyApplyF(game => StartGame(page,game)).MyApplyF(game=>UpdateGame.AppendNewLog(game,[Text.TurnHeadLogText(game)])).MyApplyA(game => UpdateCountryInfoPanel(page,game));
          }
          static GameState SelectPlayCountry(MainPage page,GameState game,ECountry playCountry) => UpdateGame.AttachGameStartData(game,playCountry).MyApplyA(game => UpdateCountryPosts(page,game));
          static GameState StartGame(MainPage page,GameState game) => (game with { Phase = Phase.Planning }).MyApplyA(v => UpdateAreaPanels(page,v)).MyApplyF(UpdateGame.AppendGameStartLog).MyApplyA(game => UpdateLogMessageUI(page,game)).MyApplyA(game => UpdateTurnLogUI(page,game)).MyApplyA(game => UpdateTurnWinCondUI(page,game,false));
        }
        static GameState SelectTarget(MainPage page,GameState game,EArea? area) => game.PlayCountry?.MyApplyF(playCountry => game.Phase == Phase.Planning && !Country.IsSleep(game,playCountry) ? (game with { ArmyTargetMap = game.ArmyTargetMap.MyAdd(playCountry,null).MyUpdate(playCountry,(_,_) => area) ?? game.ArmyTargetMap }).MyApplyA(game => { UpdateCountryInfoPanel(page,game); }) : null) ?? game;
      }
    }
    internal static void AttachEvent(MainPage page) {
      page.OpenLogButton.Click += (_,_) => ClickOpenLogButton(page);
      page.OpenInfoButton.Click += (_,_) => ClickOpenInfoButton(page);
      page.ContentGrid.SizeChanged += (_,_) => ScalingElements(page,game,GetScaleFactor(page));
      page.LogScrollPanel.SizeChanged += (_,_) => ResizeLogMessageUI(page,GetScaleFactor(page));
      page.Page.PointerMoved += (_,e) => MovePersonPanel(page,e);
      page.Page.PointerReleased += (_,e) => PutPersonPanel(page);
      page.ExplainButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.Explain);
      page.WinCondButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.WinCond);
      page.PersonDataButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.ParamDataView);
      page.ChangeLogButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.ChangeLog);
      page.SettingButton.Click += (_,_) => SwitchInfoButton(page,InfoPanelState.Setting);
      page.TopSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
      countryPostPanelMap.ToList().ForEach(v=>v.Value.PointerEntered += (_,_) => UpdateCountryPostPanelZIndex(page,v.Key));
      (infoPanelMap.GetValueOrDefault(InfoPanelState.Explain) as Explain)?.MyApplyA(v => v.SizeChanged += (_,_) => Explain.ResizeElem(v,GetScaleFactor(page)));
      (infoPanelMap.GetValueOrDefault(InfoPanelState.WinCond) as WinCond)?.MyApplyA(v => v.SizeChanged += (_,_) => WinCond.ResizeElem(v,GetScaleFactor(page)));
      (infoPanelMap.GetValueOrDefault(InfoPanelState.ParamDataView) as ParamList)?.MyApplyA(v => v.SizeChanged += (_,_) => ParamList.ResizeElem(v,GetScaleFactor(page)));
      (infoPanelMap.GetValueOrDefault(InfoPanelState.ChangeLog) as ChangeLog)?.MyApplyA(v => v.SizeChanged += (_,_) => ChangeLog.ResizeElem(v,GetScaleFactor(page)));
      (infoPanelMap.GetValueOrDefault(InfoPanelState.Setting) as Setting)?.MyApplyA(v => v.SizeChanged += (_,_) => Setting.ResizeElem(v,GetScaleFactor(page)));
      static void ClickOpenLogButton(MainPage page) => (page.LogScrollPanel.Visibility = page.LogScrollPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible).MyApplyA(v => page.InfoPanel.Visibility = Visibility.Collapsed);
      static void ClickOpenInfoButton(MainPage page) => (page.InfoPanel.Visibility = page.InfoPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible).MyApplyA(v => page.LogScrollPanel.Visibility = Visibility.Collapsed);
    }
  }
}