using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class StateInfo:UserControl {
  private Action nextButtonAction = () => { };
  private const double baseContentHeight = 45;
  internal static double contentHeight = baseContentHeight;
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
  private static double CalcNextButtonWidth(StateInfo page) => page.RenderSize.Width * 0.2;
  internal static void ResizeElem(StateInfo page,double scaleFactor) {
    double scaleX = CookScaleX(Math.Min(scaleFactor,UIUtil.GetScaleFactor(page.RenderSize with { Height = 0 })));
    double scaleY = CookScaleY(Math.Min(scaleFactor,UIUtil.GetScaleFactor(page.RenderSize with { Height = 0 })));
    double height = baseContentHeight * scaleY;
    contentHeight = height;
    page.Content.Height = height;
    page.Content.RenderTransform = new ScaleTransform() { ScaleX = scaleX,ScaleY = scaleY };
    page.Content.Margin = new(0,0,page.RenderSize.Width * (1 - 1 / scaleX),height * (1 - 1 / scaleY));
    page.NextButton.Width = CalcNextButtonWidth(page);
    double CookScaleX(double rawScale)=> rawScale switch { < 0.8 => rawScale / 0.8, > 1 => double.Lerp(rawScale,1,0.5), _ => 1 };
    double CookScaleY(double rawScale) => rawScale switch { < 0.8 => double.Lerp(rawScale/0.8,1,0.8), > 1 => double.Lerp(rawScale,1,0.5), _ => 1 };
  }
}