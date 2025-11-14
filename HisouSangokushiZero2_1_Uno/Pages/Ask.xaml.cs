using HisouSangokushiZero2_1_Uno.Code;
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
    page.Margin = new(UIUtil.infoFrameWidth.Value * scaleFactor,UIUtil.infoFrameWidth.Value * scaleFactor,UIUtil.infoFrameWidth.Value * scaleFactor,0);
    page.Width = parentSize.Width - page.Margin.Left - page.Margin.Right;
    page.TitleTextBlock.Margin = new(0,5 * scaleFactor,0,BasicStyle.textHeight * (scaleFactor - 1) + 5 * scaleFactor);
    page.TitleTextBlock.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * titleTextScaleFactor,ScaleY = scaleFactor * titleTextScaleFactor,CenterX = UIUtil.CalcFullWidthTextLength(page.TitleTextBlock.Text) * BasicStyle.fontsize * titleTextScaleFactor / 2 };
    page.ContentsView.Width = Math.Min(page.Width - 5 * scaleFactor,(maxWidth + 5 / scaleFactor) * scaleFactor);
    page.ContentsView.Height = Math.Min(parentSize.Height * 0.6,parentSize.Height - (UIUtil.infoFrameWidth.Value * 2 + page.TitleTextBlock.RenderSize.Height + page.OkButton.RenderSize.Height + 10) * scaleFactor - 20);
    page.ContentsPanel.Width = page.ContentsView.Width / scaleFactor - 15;
    page.ContentsPanel.Children.ToList().ForEach(child => child.Measure(new(page.ContentsPanel.Width,double.PositiveInfinity)));
    page.ContentsPanel.Height = page.ContentsPanel.Children.Sum(v => Math.Max(BasicStyle.textHeight,v.DesiredSize.Height));
    page.ContentsPanel.Margin = new(page.ContentsPanel.Width * (scaleFactor - 1) / 2,0,page.ContentsPanel.Width * (scaleFactor - 1) / 2,page.ContentsPanel.Height * (scaleFactor - 1));
    page.ContentsPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.ContentsPanel.Width / 2 };
    page.OkButton.Width = Math.Min((page.Width - 5) / 2 / scaleFactor,maxWidth / 2);
    page.OkButton.Margin = new(page.OkButton.Width * (scaleFactor - 1) / 2,0,page.OkButton.Width * (scaleFactor - 1) / 2,page.OkButton.Height * (scaleFactor - 1));
    page.OkButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.OkButton.Width / 2 };
    page.CloseButton.Width = Math.Min((page.Width - 5) / 2 / scaleFactor,maxWidth / 2);
    page.CloseButton.Margin = new(page.CloseButton.Width * (scaleFactor - 1) / 2,0,page.CloseButton.Width * (scaleFactor - 1) / 2,page.CloseButton.Height * (scaleFactor - 1));
    page.CloseButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.CloseButton.Width / 2 };
  }
}