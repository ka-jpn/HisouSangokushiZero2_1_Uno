using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;

internal sealed partial class Setting:UserControl {
  private static readonly int minZoomLevel = 0;
  private static readonly int maxZoomLevel = 5;
  internal Setting(Grid contentGrid) {
    InitializeComponent();
    MyInit(this,contentGrid);
    static void MyInit(Setting page,Grid contentGrid) {
      AttachEvents(page);
      RefreshUIElements(page);
      RefreshZoomButtonEnable(page);
      UIUtil.SwitchViewModeActions.Add(() => RefreshUIElements(page));
      static void RefreshUIElements(Setting page) {
        page.ViewModeText.Text = UIUtil.viewMode == UIUtil.ViewMode.fix ? "固定幅" : "フィット";
      }
      static void AttachEvents(Setting page) {
        page.SizeChanged += (_,_) => Game.contentPanel?.RenderSize.MyApplyA(v => ResizeElem(page,UIUtil.GetScaleFactor(v)));
        page.ZoomInButton.Click += (_,e) => { UIUtil.zoomLevel = Math.Clamp(UIUtil.zoomLevel + 1,minZoomLevel,maxZoomLevel); RefreshZoomButtonEnable(page); UIUtil.ChangeScaleActions.ForEach(v => v()); };
        page.ZoomOutButton.Click += (_,e) => { UIUtil.zoomLevel = Math.Clamp(UIUtil.zoomLevel - 1,minZoomLevel,maxZoomLevel); RefreshZoomButtonEnable(page); UIUtil.ChangeScaleActions.ForEach(v => v()); };
        page.InnerSwitchViewModeButton.Click += (_,_) => UIUtil.SwitchViewMode();
        page.SaveGameButton.Click += (_,_) => UIUtil.SaveGame();
        page.LoadGameButton.Click += (_,_) => UIUtil.LoadGame();
        page.InitGameButton.Click += (_,_) => UIUtil.InitGame();
        page.BackTitleButton.Click += (_,_) => (Window.Current?.Content as Frame)?.Navigate(typeof(Title));
      }
      static void RefreshZoomButtonEnable(Setting page) {
        page.ZoomInButton.IsEnabled = UIUtil.zoomLevel < maxZoomLevel;
        page.ZoomOutButton.IsEnabled = UIUtil.zoomLevel > minZoomLevel;
      }
    }
  }
  internal static void ResizeElem(Setting page,double parentScaleFactor) {
    double scaleFactor = parentScaleFactor / UIUtil.GetZoomFactor();
    double contentWidth = page.RenderSize.Width / scaleFactor - 5;
    page.ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),page.ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
    page.InnerSwitchViewModeButton.MaxWidth = contentWidth - page.ViewModeCaption.RenderSize.Width - page.ViewModeText.Width - 10;
    page.SaveGameButton.MaxWidth = contentWidth - 10;
    page.LoadGameButton.MaxWidth = contentWidth - 10;
    page.InitGameButton.MaxWidth = contentWidth - 10;
    page.BackTitleButton.MaxWidth = contentWidth - 10;
    page.ZoomInButton.MaxWidth = (contentWidth - page.MapZoomCaption.Width - 10) / 2;
    page.ZoomOutButton.MaxWidth = (contentWidth - page.MapZoomCaption.Width - 10) / 2;
    page.Scroll.Width = page.RenderSize.Width;
  }
}