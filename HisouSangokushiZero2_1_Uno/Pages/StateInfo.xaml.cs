using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class StateInfo:UserControl {
  private static Action nextButtonAction = () => { };
  internal StateInfo() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(StateInfo page) {
      page.NextButton.Click += (_,_)=> ClickNextButton();
      static void ClickNextButton() => nextButtonAction();
    }
  }
  internal static void Show(StateInfo page,List<UIElement> InfoContents,string buttonText,double? buttonHeight,Action buttonAction) {
    page.InfoContentsPanel.MySetChildren([..InfoContents]);
    page.NextButtonText.Text = buttonText;
    if(buttonHeight != null) {
      page.NextButton.Height = buttonHeight.Value;
      page.NextButton.Visibility=Visibility.Visible;
    } else {
      page.NextButton.Visibility = Visibility.Collapsed;
    }
    nextButtonAction = buttonAction;
  }
  internal static void ResizeElem(StateInfo page, double scaleFactor) {
    page.Margin = new(0,page.Height*(scaleFactor-1),0,0);
    page.RenderTransform = new ScaleTransform() { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.Width,CenterY = page.Height };
  }
}