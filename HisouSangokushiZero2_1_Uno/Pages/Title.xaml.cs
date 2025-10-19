using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Title:Page {
  public Title() {
    InitializeComponent();
    MyInit(this);
    void MyInit(Title page) {
      page.StartButton.Click += (_,_) => { GameData.game = GetInitGameData(); NavigateToGamePage(); };
      page.LoadButton.Click += async (_,_) => { GameData.game = (await Storage.ReadStorageData(1)).Item2 ?? GetInitGameData(); NavigateToGamePage(); };
      page.TopSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
      page.SizeChanged += (_,_) => ScalingElements(page,GameData.game,UIUtil.GetScaleFactor(page.RenderSize,1));
      UIUtil.SwitchViewModeActions.Add(() => { RefreshViewMode(page); ScalingElements(page,GameData.game,UIUtil.GetScaleFactor(page.RenderSize,1)); });
      RefreshViewMode(page);
      GameState GetInitGameData() => GetGame.GetInitGameScenario(BaseData.scenarios.FirstOrDefault());
      void NavigateToGamePage() => (Window.Current?.Content as Frame)?.Navigate(typeof(Game));
      void RefreshViewMode(Title page) {
        page.SwitchViewModeButtonText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "▼" : "▲";
        page.MaxWidth = UIUtil.viewMode == UIUtil.ViewMode.fix ? UIUtil.fixModeMaxWidth : double.MaxValue;
      }
      void ScalingElements(Title page,GameState game,double scaleFactor) {
        double infoFramebuttonMargin = UIUtil.infoFrameWidth.Value * (scaleFactor - 1);
        page.TopSwitchViewModeButton.Margin = new(0,0,infoFramebuttonMargin,infoFramebuttonMargin);
        page.TopSwitchViewModeButton.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
      }
    }
  }
}