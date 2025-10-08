using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Setting:UserControl {
  internal Setting(Grid contentGrid) {
    InitializeComponent();
    MyInit(this,contentGrid);
    static void MyInit(Setting page,Grid contentGrid) {
      page.SizeChanged += (_,_) => ResizeElem(page,UIUtil.GetScaleFactor(contentGrid.RenderSize,Game.scaleLevel));
      RefreshUIElements(page);
      AttachEvents(page);
      UIUtil.SwitchViewModeActions.Add(() => RefreshUIElements(page));
      static void RefreshUIElements(Setting page) {
        page.ViewModeText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "固定幅" : "フィット";
      }
      static void AttachEvents(Setting page) {
        page.SaveGameButton.Click += (_,_) => UIUtil.SaveGame();
        page.LoadGameButton.Click += (_,_) => UIUtil.LoadGame();
        page.InitGameButton.Click += (_,_) => UIUtil.InitGame();
        page.InnerSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
        page.BackTitleButton.Click += (_,_) => (Window.Current?.Content as Frame)?.Navigate(typeof(Title));
      }
    }
  }
  internal static void ResizeElem(Setting page,double scaleFactor) {
    double pageWidth = page.RenderSize.Width;
    double contentWidth = pageWidth / scaleFactor - 5;
    page.Scroll.Width = pageWidth;
    page.ContentPanel.Width = contentWidth;
    page.ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.InnerSwitchViewModeButton.MaxWidth = contentWidth - page.ViewModeCaption.RenderSize.Width - page.ViewModeText.Width - 10;
    page.SaveGameButton.MaxWidth = contentWidth - 10;
    page.LoadGameButton.MaxWidth = contentWidth - 10;
    page.InitGameButton.MaxWidth = contentWidth - 10;
    page.ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),page.ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
  }
}