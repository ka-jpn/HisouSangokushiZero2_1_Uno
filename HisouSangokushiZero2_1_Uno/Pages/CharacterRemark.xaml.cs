using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SkiaSharp;
using SkiaSharp.Views.Windows;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Size = Windows.Foundation.Size;
using Text = HisouSangokushiZero2_1_Uno.Code.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class CharacterRemark:UserControl {
  static readonly double RemarkFrameCornerRadius = 10;
  static readonly double PersonImageSize = 150;
  internal CharacterRemark() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(CharacterRemark page) {
      page.CloseButton.Click += (_,_) => page.Visibility = Visibility.Collapsed;
    }
  }
  internal static async void SetElems(CharacterRemark page,ECountry? country,string[] contents,UIElement parent,bool isAliveCharacter) {
    if(contents.MyIsEmpty()) { return; }
    page.PersonName.Text = Text.GetRemarkPersonName(country,isAliveCharacter,Lang.ja);
    page.RemarkText.Text = contents.FirstOrDefault() ?? string.Empty;
    page.RemarkText.Width = double.NaN;
    page.RemarkText.Height = double.NaN;
    page.RemarkText.Margin = new Thickness(0);
    page.RemarkText.Measure(parent.RenderSize);
    page.RemarkText.Width = page.RemarkText.DesiredSize.Width + RemarkFrameCornerRadius * 3;
    page.RemarkText.Height = page.RemarkText.DesiredSize.Height + RemarkFrameCornerRadius * 2;
    page.RemarkFrame.Data = RemarkFrameCornerRadius.MyApplyF(v => page.RemarkText.Width.MyApplyF(width => page.RemarkText.Height.MyApplyF(height =>
      new PathGeometry { Figures = [ new PathFigure {
        StartPoint=new(width-v,height-v),
        Segments = [
          new ArcSegment{ Point=new(width-v*2,height),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
          new LineSegment{ Point=new(v,height) },
          new ArcSegment{ Point=new(0,height-v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
          new LineSegment{ Point=new(0,v) },
          new ArcSegment{ Point=new(v,0),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
          new LineSegment{ Point=new(width-v*2,0) },
          new ArcSegment{ Point=new(width-v,v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
          new LineSegment{ Point=new(width-v,height-v*2) },
          new ArcSegment{ Point=new(width,height-v*2),Size=new(v,v) },
          new ArcSegment{ Point=new(width-v,height-v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
        ],
        IsClosed=true
      } ] }
    )));
    page.RemarkText.Margin = new Thickness(RemarkFrameCornerRadius,RemarkFrameCornerRadius,-RemarkFrameCornerRadius,-RemarkFrameCornerRadius);
    page.buttonPanel.MySetChildren([CreateButton(page,country,[.. contents.Skip(1)],parent,isAliveCharacter)]);
    page.PersonImagePanel.MySetChildren([await DrawPersonImage(country,isAliveCharacter)]);
    page.Visibility = Visibility.Visible;
    ResizeElem(page,parent.RenderSize,UIUtil.GetScaleFactor(parent.RenderSize));
    static Button CreateButton(CharacterRemark page,ECountry? country,string[] remainContents,UIElement parent,bool isAliveCharacter) {
      return new Button { Width = 200,Height = 50,Background = Colors.FromARGB(34,0,0,0) }.MySetChild(new TextBlock { Text = remainContents.MyIsEmpty() ? "閉じる" : "次へ" }).MyApplyA(button =>
        button.Click += (_,_) => { if(remainContents.MyIsEmpty()) { page.Visibility = Visibility.Collapsed; } else { SetElems(page,country,remainContents,parent,isAliveCharacter); } }
      );
    }
    static async Task<SKXamlCanvas> DrawPersonImage(ECountry? country,bool isAliveCharacter) {
      StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/Images/{Text.GetRemarkPersonName(country,isAliveCharacter,Lang.ja)}.png"));
      using Stream stream = await file.OpenStreamForReadAsync();
      using SKBitmap bitmap = SKBitmap.Decode(stream);
      SKImage image = SKImage.FromBitmap(bitmap);
      return new SKXamlCanvas { Width = PersonImageSize,Height = PersonImageSize }.MyApplyA(v => {
        v.PaintSurface += (sender,e) => {
          e.Surface.Canvas.DrawImage(image,new SKRect(0,0,(float)v.Width,(float)v.Height),new SKSamplingOptions(SKFilterMode.Nearest,SKMipmapMode.None));
        };
        v.Invalidate();
      });
    }
  }
  internal static void ResizeElem(CharacterRemark page,Size parentSize,double scaleFactor) {
    page.Margin = new(UIUtil.infoFrameWidth.Value * scaleFactor);
    page.Width = parentSize.Width - page.Margin.Left - page.Margin.Right;
    page.Height = parentSize.Height - page.Margin.Top - page.Margin.Bottom;
    page.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * 1.2,ScaleY = scaleFactor * 1.2,CenterX = page.Width / 2,CenterY = page.Height / 2 };
  }
}