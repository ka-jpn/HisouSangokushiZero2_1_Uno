using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class LogPanel:UserControl {
  internal LogPanel() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      SizeChanged += (_,_) => ResizeElem();
    }
  }
  internal static void UpdateLogMessageUI(GameState game) {
    //game.NewLog.Select(logText => new TextBlock() { Text = logText }).ToList().ForEach(page.LogsPanel.Children.Add);
  }
  private void ResizeElem() {
    double scaleFactor = UIUtil.GetScaleFactor(RenderSize with { Height = 0 });
    LogsPanel.MaxWidth = Content.RenderSize.Width / scaleFactor;
    LogsPanel.Height = LogsPanel.Children.Sum(v => v.DesiredSize.Height);
    LogsPanel.Margin = new(0,0,LogsPanel.DesiredSize.Width * (scaleFactor - 1),LogsPanel.Height * (scaleFactor - 1));
    LogsPanel.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor };
  }
}