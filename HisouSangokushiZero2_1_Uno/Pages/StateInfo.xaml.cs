using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
namespace HisouSangokushiZero2_1_Uno.Pages;

internal sealed partial class StateInfo:UserControl {
  private Action nextButtonAction = () => { };
  private readonly static double minheightScale = 0.03;
  private const double minWidth = 750;
  private const double maxWidth = 1000;
  internal static readonly double defaultHeight = UIUtil.fixModeMaxWidth * minheightScale * maxWidth / minWidth;
  internal static double contentHeight = defaultHeight;
  internal StateInfo() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(StateInfo page) {
      page.Content.Height = defaultHeight;
      page.NextButton.Click += (_,_) => page.nextButtonAction();
    }
  }
  internal static void Show(StateInfo page,List<UIElement> InfoContents,string? buttonText,Action buttonAction) {
    page.InfoContentsPanel.MySetChildren([.. InfoContents]);
    if(buttonText is not null) {
      page.NextButton.Visibility = Visibility.Visible;
      page.NextButtonText.Text = buttonText;
      page.nextButtonAction = buttonAction;
    } else {
      page.NextButton.Visibility = Visibility.Collapsed;
    }
    RefreshNextButton(page);
  }
  private static void RefreshNextButton(StateInfo page) {
    if(page.NextButton.Visibility == Visibility.Visible) {
      page.NextButton.Width = page.RenderSize.Width / CalcScaleX(page) * 0.2;
    } else {
      page.NextButton.Width = 0;
    }
  }
  private static double CalcContentHeight(StateInfo page) => Math.Max(defaultHeight,page.RenderSize.Width * minheightScale);
  private static double CalcScaleX(StateInfo page) => page.RenderSize.Width switch { <= minWidth => page.RenderSize.Width / minWidth, >= maxWidth => page.RenderSize.Width / maxWidth, _ => 1 };
  private static double CalcScaleY(StateInfo page) => page.RenderSize.Width switch { >= maxWidth => page.RenderSize.Width / maxWidth, _ => 1 };
  internal static void ResizeElem(StateInfo page) {
    contentHeight = CalcContentHeight(page);
    page.Content.RenderTransform = new ScaleTransform() { ScaleX = CalcScaleX(page),ScaleY = CalcScaleY(page) };
    page.Content.Margin = new(0,0,page.RenderSize.Width * (1 - 1 / CalcScaleX(page)),page.RenderSize.Height * (1 - 1 / CalcScaleY(page)));
    RefreshNextButton(page);
  }
}