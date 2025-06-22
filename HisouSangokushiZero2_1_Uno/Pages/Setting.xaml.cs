using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml.Controls;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Setting:UserControl {
  internal Setting() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(Setting page) {
      RefreshUIElements(page);
      AttachEvents(page);
      page.Measure(new(double.PositiveInfinity,double.PositiveInfinity));
      page.Width = 780;
      page.Height = page.DesiredSize.Height;
      UIUtil.SwitchViewModeAction.Add(() => RefreshUIElements(page));
      static void RefreshUIElements(Setting page) {
        page.ViewModeText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "固定幅" : "フィット";
      }
      static void AttachEvents(Setting page) {
        page.InitGameButton.Click += (_,_) => UIUtil.InitGame();
        page.InnerSwitchViewModeButton.Click += (_,_) => { UIUtil.SwitchViewMode(); RefreshUIElements(page); };
      }
    }
  }
}