using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class ChangeLog:UserControl {
  internal ChangeLog() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      SizeChanged += (_,_) => ResizeElem();
      void ResizeElem() {
        double scaleFactor = UIUtil.GetScaleFactor(RenderSize with { Height = 0 });
        double contentWidth = RenderSize.Width / scaleFactor - 5;
        ContentPanel.Width = contentWidth;
        ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
        ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
        Scroll.Width = RenderSize.Width;
      }
    }
  }
}