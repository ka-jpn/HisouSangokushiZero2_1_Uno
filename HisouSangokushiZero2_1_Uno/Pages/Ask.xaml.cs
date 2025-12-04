using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Ask:UserControl {
  internal static Thickness paddingThickness = new(5);
  internal static double paddingSize = 5;
  private const double maxWidth = 600;
  private const double titleTextScaleFactor = 1.25;
  private static Action OkButtonAction = () => { };
  private static UIElement? parent = null;
  internal Ask() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      OkButton.Click += (_,_) => ExeOkButtonAction();
      CloseButton.Click += (_,_) => UIUtil.SetVisibility(this,false);
      Page.SizeChanged += (_,_) => parent?.MyApplyA(parent => ResizeElem(parent));
      static void ExeOkButtonAction() => OkButtonAction();
    }
  }
  internal static void Init(UIElement parentElem) => parent = parentElem;
  internal static void SetElems(Ask page,string titleText,List<TextBlock> contents,string okButtonText,Action? okButtonAction,bool isOkButtonClickClose) {
    page.TitleTextBlock.Text = titleText;
    page.ContentsPanel.MySetChildren([.. contents]);
    OkButtonAction = () => { okButtonAction?.Invoke(); UIUtil.SetVisibility(page,!isOkButtonClickClose); };
    page.OkButtonText.Text = okButtonText;
    page.OkButton.IsEnabled = okButtonAction is {};
    page.Visibility = Visibility.Visible;
    parent?.MyApplyA(page.ResizeElem);
  }
  internal void ResizeElem(UIElement parent) {
    double padding = 5;
    double scaleFactor = UIUtil.GetScaleFactor(parent.RenderSize);
    double sideMargin = UIUtil.infoFrameWidth * scaleFactor;
    double contentWidth = Page.RenderSize.Width - sideMargin * 2 - padding * 2;
    Content.Margin = new(sideMargin,sideMargin,sideMargin,0);
    TitleTextBlock.Margin = new(0,padding * scaleFactor,0,BasicStyle.textHeight * (scaleFactor - 1) + padding * scaleFactor);
    TitleTextBlock.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * titleTextScaleFactor,ScaleY = scaleFactor * titleTextScaleFactor,CenterX = UIUtil.CalcFullWidthTextLength(TitleTextBlock.Text) * BasicStyle.fontsize * titleTextScaleFactor / 2 };
    ContentsView.Width = Math.Min(contentWidth,(maxWidth + padding * 2 / scaleFactor) * scaleFactor);
    ContentsView.Height = Math.Min(Page.RenderSize.Height * 0.6,Page.RenderSize.Height - (UIUtil.infoFrameWidth * 2 + TitleTextBlock.RenderSize.Height + OkButton.RenderSize.Height + padding * 2) * scaleFactor - padding * 4);
    ContentsPanel.Width = ContentsView.Width / scaleFactor - 15;
    ContentsPanel.Height = ContentsPanel.Children.MyApplyA(v => v.ToList().ForEach(child => child.Measure(new(ContentsPanel.Width,double.PositiveInfinity)))).Sum(v => Math.Max(BasicStyle.textHeight,v.DesiredSize.Height));
    ContentsPanel.Margin = new(ContentsPanel.Width * (scaleFactor - 1) / 2,0,ContentsPanel.Width * (scaleFactor - 1) / 2,ContentsPanel.Height * (scaleFactor - 1));
    ContentsPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = ContentsPanel.Width / 2 };
    ButtonGrid.Height = 45 * scaleFactor;
    OkButton.Margin = new(OkButton.RenderSize.Width * (scaleFactor - 1) / 2,0,OkButton.RenderSize.Width * (scaleFactor - 1) / 2,ButtonGrid.Height * (scaleFactor - 1));
    OkButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = OkButton.RenderSize.Width / 2 };
    CloseButton.Margin = new(CloseButton.RenderSize.Width * (scaleFactor - 1) / 2,0,CloseButton.RenderSize.Width * (scaleFactor - 1) / 2,ButtonGrid.Height * (scaleFactor - 1));
    CloseButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = CloseButton.RenderSize.Width / 2 };
    paddingThickness = new(padding * scaleFactor);
    paddingSize = padding * scaleFactor;
  }
}