using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class ChangeLog:UserControl {
  private static UIElement? parent = null;
  internal ChangeLog() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      AttachEvent();
      void AttachEvent() {
        SizeChanged += (_,_) => parent?.MyApplyA(ResizeElem);
        void ResizeElem(UIElement parent) {
          double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
          double contentWidth = RenderSize.Width / scaleFactor - 5;
          ContentPanel.Width = contentWidth;
          ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
          ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
        }
      }
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
}