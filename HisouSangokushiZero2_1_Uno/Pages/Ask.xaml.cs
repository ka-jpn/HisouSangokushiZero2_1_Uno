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
  private static readonly double titleTextScaleFactor = 1.25;
  private static Action OkButtonAction = () => { };
  private static List<TextBlock> contentTextBlocks = [];
  internal static readonly Ask page = new();
  internal Ask() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(Ask page) {
      page.OkButton.Click += (_,_) => ExeOkButtonAction();
      page.CloseButton.Click += (_,_) => page.Visibility = Visibility.Collapsed;
      static void ExeOkButtonAction() => OkButtonAction();
    }
  }
  internal static void SetElems(string titleText,List<TextBlock> contents,string okButtonText,bool isOkButtonEnabled,Action okButtonAction,bool isOkButtonClickClose,Size parentSize,double scaleFactor) {
    contentTextBlocks = contents;
    page.TitleTextBlock.Text = titleText;
    page.ContentsPanel.MySetChildren([.. contents]);
    OkButtonAction = () => { okButtonAction(); page.Visibility = isOkButtonClickClose ? Visibility.Collapsed : Visibility.Visible; };
    page.OkButtonText.Text = okButtonText;
    page.OkButton.IsEnabled = isOkButtonEnabled;
    page.Visibility = Visibility.Visible;
    ResizeElem(parentSize,scaleFactor);
  }
  internal static void ResizeElem(Size parentSize,double scaleFactor) {
    page.Margin = new(UIUtil.infoFrameWidth.Value * scaleFactor);
    page.Width = parentSize.Width - page.Margin.Left - page.Margin.Right;
    page.Height = parentSize.Height - page.Margin.Top - page.Margin.Bottom;
    page.TitleTextBlock.Margin = new(0,5 * scaleFactor,0,BasicStyle.textHeight * (scaleFactor - 1) + 5 * scaleFactor);
    page.TitleTextBlock.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * titleTextScaleFactor,ScaleY = scaleFactor * titleTextScaleFactor,CenterX = UIUtil.CalcFullWidthTextLength(page.TitleTextBlock.Text) * BasicStyle.fontsize * titleTextScaleFactor / 2 };
    page.ContentsView.Width = Math.Min(page.Width - 20 * scaleFactor,(600 * 2 + 10 / scaleFactor) * scaleFactor);
    page.ContentsView.Height = page.Height * 0.5;
    page.ContentsPanel.Width = page.ContentsView.Width / scaleFactor - 15;
    page.ContentsPanel.Height = contentTextBlocks.Sum(v => Math.Max(BasicStyle.textHeight,v.RenderSize.Height));
    page.ContentsPanel.Margin = new(page.ContentsPanel.Width * (scaleFactor - 1) / 2,0,page.ContentsPanel.Width * (scaleFactor - 1) / 2,page.ContentsPanel.Height * (scaleFactor - 1));
    page.ContentsPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.ContentsPanel.Width / 2 };
    page.OkButton.Width = Math.Min((page.Width / 2 - 5 - 5d / 2) / scaleFactor,600);
    page.OkButton.Margin = new(page.OkButton.Width * (scaleFactor - 1) / 2,0,page.OkButton.Width * (scaleFactor - 1) / 2,page.OkButton.Height * (scaleFactor - 1));
    page.OkButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.OkButton.Width / 2 };
    page.CloseButton.Width = Math.Min((page.Width / 2 - 5 - 5d / 2) / scaleFactor,600);
    page.CloseButton.Margin = new(page.CloseButton.Width * (scaleFactor - 1) / 2,0,page.CloseButton.Width * (scaleFactor - 1) / 2,page.CloseButton.Height * (scaleFactor - 1));
    page.CloseButton.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor,CenterX = page.CloseButton.Width / 2 };
  }
}