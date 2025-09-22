using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Size = Windows.Foundation.Size;
using Text = HisouSangokushiZero2_1_Uno.Code.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class CharacterRemark:UserControl {
  static readonly double RemarkFrameCornerRadius = 10;
  static readonly double PersonImageSize = 150;
  static string nowPersonImageName = Text.GetRemarkPersonName(null,true,Lang.ja);
  static bool resetPersonImageSource = false;
  internal CharacterRemark() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(CharacterRemark page) {
      page.CloseButton.Click += (_,_) => page.Visibility = Visibility.Collapsed;
      page.PersonImage.ImageOpened += (_,_) => { if(resetPersonImageSource) { page.Visibility = Visibility.Visible; resetPersonImageSource = false; } };
    }
  }
  internal static void SetElems(CharacterRemark page,ECountry? country,string[] contents,UIElement parent,bool isAliveCharacter) {
    if(contents.MyIsEmpty()) { return; }
    string newPersonImageName = Text.GetRemarkPersonName(country,isAliveCharacter,Lang.ja);
    page.PersonName.Text = newPersonImageName;
    page.RemarkText.Text = contents.FirstOrDefault() ?? string.Empty;
    page.buttonPanel.MySetChildren([CreateButton(page,country,[.. contents.Skip(1)],parent,isAliveCharacter)]);
    ResizeElem(page,parent.RenderSize,UIUtil.GetScaleFactor(parent.RenderSize));
    if(nowPersonImageName == newPersonImageName) {
      page.Visibility = Visibility.Visible;
    } else {
      page.PersonImage.Source = new SvgImageSource(new Uri($"ms-appx:///Assets/Svg/Person/{newPersonImageName}.svg"));
      resetPersonImageSource = true;
    }
    nowPersonImageName = newPersonImageName;
    static Button CreateButton(CharacterRemark page,ECountry? country,string[] remainContents,UIElement parent,bool isAliveCharacter) {
      return new Button { Width = 200,Height = 50,Background = Colors.FromARGB(34,0,0,0) }.MySetChild(new TextBlock { Text = remainContents.MyIsEmpty() ? "閉じる" : "次へ" }).MyApplyA(button =>
        button.Click += (_,_) => { if(remainContents.MyIsEmpty()) { page.Visibility = Visibility.Collapsed; } else { SetElems(page,country,remainContents,parent,isAliveCharacter); } }
      );
    }
  }
  internal static void ResizeElem(CharacterRemark page,Size parentSize,double scaleFactor) {
    page.Margin = new(parentSize.Width * (scaleFactor * 1.2 - 1),parentSize.Height * (scaleFactor * 1.2 - 1));
    page.Width = parentSize.Width / (scaleFactor * 1.2);
    page.Height = parentSize.Height / (scaleFactor * 1.2);
    page.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * 1.2,ScaleY = scaleFactor * 1.2,CenterX = page.Width / 2,CenterY = page.Height / 2 };
    page.RemarkText.Width = double.NaN;
    page.RemarkText.Height = double.NaN;
    page.RemarkText.Margin = new Thickness(0);
    page.RemarkText.Measure(parentSize with { Width = parentSize.Width / (scaleFactor * 1.2) - PersonImageSize - RemarkFrameCornerRadius * 3 - 20 });
    page.RemarkText.Width = page.RemarkText.DesiredSize.Width;
    page.RemarkText.Height = page.RemarkText.DesiredSize.Height;
    page.RemarkFrame.Data = RemarkFrameCornerRadius.MyApplyF(v => (page.RemarkText.Width + RemarkFrameCornerRadius * 3).MyApplyF(width => (page.RemarkText.Height + RemarkFrameCornerRadius * 2).MyApplyF(height =>
      new PathGeometry {
        Figures = [ new PathFigure {
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
        } ]
      }
    )));
    page.RemarkText.Margin = new Thickness(RemarkFrameCornerRadius,RemarkFrameCornerRadius,RemarkFrameCornerRadius * 2,RemarkFrameCornerRadius);
  }
}