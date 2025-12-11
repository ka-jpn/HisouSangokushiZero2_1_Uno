using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class StateInfo:UserControl {
  private Action nextButtonAction = () => { };
  private const double baseContentHeight = 45;
  internal StateInfo() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(StateInfo page) {
      page.Content.Height = baseContentHeight;
      page.NextButton.Click += (_,_) => page.nextButtonAction();
    }
  }
  internal static void Show(StateInfo page,List<UIElement> InfoContents,string? buttonText,Action buttonAction) {
    page.InfoContentsPanel.MySetChildren([.. InfoContents]);
    if(buttonText is {}) {
      page.NextButtonText.Text = buttonText;
      page.nextButtonAction = buttonAction;
      page.NextButton.Width = CalcNextButtonWidth(page);
      UIUtil.SetVisibility(page.NextButton,true);
    } else {
      page.NextButton.Width = 0;
      UIUtil.SetVisibility(page.NextButton,false);
    }
  }
  private static double CalcNextButtonWidth(StateInfo page) => page.Content.RenderSize.Width * 0.2;
  internal static void ResizeElem(StateInfo page,double scaleFactor) {
    double scaleX = CookScaleX(scaleFactor);
    double scaleY = CookScaleY(scaleFactor);
    page.Content.RenderTransform = new ScaleTransform() { ScaleX = scaleX,ScaleY = scaleY };
    page.Content.Margin = new(0, 0, page.RenderSize.Width * (1 - 1 / scaleX), baseContentHeight * (scaleY - 1));
    page.NextButton.Width = CalcNextButtonWidth(page);
    static double CookScaleX(double rawScale) => rawScale switch { < 0.8 => rawScale / 0.8, > 1 => double.Lerp(rawScale,1,0.5), _ => 1 };
  }
  private static double CookScaleY(double rawScale) => rawScale switch { < 0.8 => double.Lerp(rawScale/0.8,1,0.5), > 1 => double.Lerp(rawScale,1,0.5), _ => 1 };
  internal static double SolveScale(double height,Size size) {
    double initScale = UIUtil.GetScaleFactor(size);
    return UIUtil.mapSize.Width * initScale - size.Width < 0.1 ? initScale : (CaclSCale() - 0.0002);
    double CaclSCale() {
      return Average(Enumerable.Range(0, 30).Aggregate((initScale / 4, initScale * 4), (before, _) => Average(before).MyApplyF(v => GetDeviation(v) > 0 ? (before.Item1, v) : (v, before.Item2))));
      double Average((double left,double right) range) =>(range.left + range.right) / 2;
      double GetDeviation(double scale) => Math.Ceiling(UIUtil.mapSize.Height * scale) + Math.Ceiling(baseContentHeight * CookScaleY(scale)) - height;
    }
  }
  internal static double GetHeight(StateInfo page) => baseContentHeight + page.Content.Margin.Bottom;
}