using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class StateInfo:UserControl {
  private static Action nextButtonAction = () => { };
  private static StateInfo page = null!;
  internal StateInfo() {
    page = this;
    InitializeComponent();
    MyInit(this);
    static void MyInit(StateInfo page) {
      page.NextButton.Click += (_,_)=> ClickNextButton();
      static void ClickNextButton() => nextButtonAction();
    }
  }
  internal static void Show(List<UIElement> InfoContents,string buttonText,double? buttonHeight,Action buttonAction) {
    page.InfoContentsPanel.MySetChildren([..InfoContents]);
    page.NextButtonText.Text = buttonText;
    if(buttonHeight != null) {
      page.NextButtonText.Height = buttonHeight.Value;
      page.NextButton.Visibility=Visibility.Visible;
    } else {
      page.NextButton.Visibility = Visibility.Collapsed;
    }
    nextButtonAction = buttonAction;
  }
}