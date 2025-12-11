using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.Data.Scenario;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI.Core;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
public sealed partial class Title:Page {
  public Title() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      RefreshViewMode();
      MapCanvas.PaintSurface += (_,e) => UIUtil.MapCanvas_PaintSurface(e);
      StartButton.Click += (_,_) => {
        GameData.game = GetInitGameData();
        NavigateToGamePage();
      };
      LoadButton.Click += (_,_) => {
        SaveAndLoad.Show(SaveDataPanel,false,maybeRead => maybeRead?.MaybeGame is GameState game ? Task.Run(async () => {
          GameData.game = game;
          await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => NavigateToGamePage());
        }) : Task.CompletedTask,() => UIUtil.SetVisibility(SaveDataPanel,false),Content.RenderSize);
        UIUtil.SetVisibility(SaveDataPanel,true);
      };
      TopSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
      Content.SizeChanged += (_,_) => ScalingElements();
      UIUtil.SwitchViewModeActions.Add(RefreshViewMode);
      GameState GetInitGameData() => GetGame.GetInitGameScenario(ScenarioBase.GetScenarioId(0));
      void NavigateToGamePage() => (Window.Current?.Content as Frame)?.Navigate(typeof(Game));
      void RefreshViewMode() {
        SwitchViewModeButtonText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "▼" : "▲";
        Content.MaxWidth = UIUtil.viewMode == UIUtil.ViewMode.fix ? UIUtil.fixModeMaxWidth : double.MaxValue;
      }
      void ScalingElements() {
        double scaleFactor = UIUtil.GetScaleFactor(Content.RenderSize with { Height = 0 });
        double infoFramebuttonMargin = UIUtil.infoFrameWidth * (scaleFactor - 1);
        TopSwitchViewModeButton.Margin = new(0,0,infoFramebuttonMargin,infoFramebuttonMargin);
        TopSwitchViewModeButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      }
    }
  }
}