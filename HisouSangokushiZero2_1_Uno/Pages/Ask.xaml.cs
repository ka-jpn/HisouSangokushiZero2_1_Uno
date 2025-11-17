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

internal sealed partial class Ask:UserControl {
  private static readonly double maxWidth = 600;
  private static readonly double titleTextScaleFactor = 1.25;
  private static Action OkButtonAction = () => { };
  internal Ask() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(Ask page) {
      page.OkButton.Click += (_,_) => ExeOkButtonAction();
      page.CloseButton.Click += (_,_) => page.Visibility = Visibility.Collapsed;
      page.SizeChanged += (_,_) => Game.contentPanel?.RenderSize.MyApplyA(v => ResizeElem(page,v,UIUtil.GetScaleFactor(v)));
      static void ExeOkButtonAction() => OkButtonAction();
    }
  }
  internal static void SetElems(Ask page,string titleText,List<TextBlock> contents,string okButtonText,Action? okButtonAction,bool isOkButtonClickClose,Size parentSize) {
    page.TitleTextBlock.Text = titleText;
    page.ContentsPanel.MySetChildren([.. contents]);
    OkButtonAction = () => { okButtonAction?.Invoke(); page.Visibility = isOkButtonClickClose ? Visibility.Collapsed : Visibility.Visible; };
    page.OkButtonText.Text = okButtonText;
    page.OkButton.IsEnabled = okButtonAction != null;
    page.Visibility = Visibility.Visible;
    ResizeElem(page,parentSize,UIUtil.GetScaleFactor(parentSize));
  }
  internal static void ResizeElem(Ask page,Size parentSize,double scaleFactor) {
    double padding = 5;
    double sideMargin = UIUtil.infoFrameWidth * scaleFactor;
    double contentWidth = parentSize.Width - sideMargin * 2 - padding * 2;
    page.Margin = new(sideMargin,sideMargin,sideMargin,0);
    page.TitleTextBlock.Margin = new(0,padding * scaleFactor,0,BasicStyle.textHeight * (scaleFactor - 1) + padding * scaleFactor);
    page.TitleTextBlock.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * titleTextScaleFactor,ScaleY = scaleFactor * titleTextScaleFactor,CenterX = UIUtil.CalcFullWidthTextLength(page.TitleTextBlock.Text) * BasicStyle.fontsize * titleTextScaleFactor / 2 };
    page.ContentsView.Width = Math.Min(contentWidth,(maxWidth + padding * 2 / scaleFactor) * scaleFactor);
    page.ContentsView.Height = Math.Min(parentSize.Height * 0.6,parentSize.Height - (UIUtil.infoFrameWidth * 2 + page.TitleTextBlock.RenderSize.Height + page.OkButton.RenderSize.Height + padding * 2) * scaleFactor - padding * 4);
    page.ContentsPanel.Width = page.ContentsView.Width / scaleFactor - 15;
    page.ContentsPanel.Height = page.ContentsPanel.Children.MyApplyA(v => v.ToList().ForEach(child => child.Measure(new(page.ContentsPanel.Width,double.PositiveInfinity)))).Sum(v => Math.Max(BasicStyle.textHeight,v.DesiredSize.Height));
    page.ContentsPanel.Margin = new(page.ContentsPanel.Width * (scaleFactor - 1) / 2,0,page.ContentsPanel.Width * (scaleFactor - 1) / 2,page.ContentsPanel.Height * (scaleFactor - 1));
    page.ContentsPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.ContentsPanel.Width / 2 };
    page.OkButton.Width = Math.Min(contentWidth / 2 / scaleFactor,maxWidth / 2);
    page.OkButton.Margin = new(page.OkButton.Width * (scaleFactor - 1) / 2,0,page.OkButton.Width * (scaleFactor - 1) / 2,page.OkButton.Height * (scaleFactor - 1));
    page.OkButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.OkButton.Width / 2 };
    page.CloseButton.Width = Math.Min(contentWidth / 2 / scaleFactor,maxWidth / 2);
    page.CloseButton.Margin = new(page.CloseButton.Width * (scaleFactor - 1) / 2,0,page.CloseButton.Width * (scaleFactor - 1) / 2,page.CloseButton.Height * (scaleFactor - 1));
    page.CloseButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.CloseButton.Width / 2 };
  }
}