using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Linq;
using Windows.Devices.Enumeration;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using Size = Windows.Foundation.Size;
using Text = HisouSangokushiZero2_1_Uno.Code.Text;
namespace HisouSangokushiZero2_1_Uno.Pages;

internal sealed partial class CharacterRemark:UserControl {
  private static readonly double remarkFrameCornerRadius = 5;
  internal static readonly double personImageSize = 75;
  private static string nowPersonImageName = Text.GetRemarkPersonName(null,true,Lang.ja);
  private static bool resetPersonImageSource = false;
  internal CharacterRemark() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(CharacterRemark page) {
      page.CloseButton.Click += (_,_) => page.Visibility = Visibility.Collapsed;
      page.PersonImage.ImageOpened += (_,_) => { if(resetPersonImageSource) { page.Visibility = Visibility.Visible; resetPersonImageSource = false; } };
      page.SizeChanged += (_,_) => Game.contentPanel?.RenderSize.MyApplyA(v => ResizeElem(page,v,UIUtil.GetScaleFactor(v)));
      page.RemarkText.Padding = new(remarkFrameCornerRadius,remarkFrameCornerRadius,remarkFrameCornerRadius * 2,remarkFrameCornerRadius);
    }
  }
  internal static void SetElems(CharacterRemark page,GameState game,string[] contents,UIElement parent) {
    if(contents.MyIsEmpty()) { return; }
    string newPersonImageName = Text.GetRemarkPersonName(game.PlayCountry,game.PlayTurn < 3,Lang.ja);
    page.PersonName.Text = newPersonImageName;
    page.RemarkText.Text = contents.FirstOrDefault() ?? string.Empty;
    page.ButtonPanel.MySetChildren([CreateButton(page,game,[.. contents.Skip(1)],parent)]);
    ResizeElem(page,parent.RenderSize,UIUtil.GetScaleFactor(parent.RenderSize));
    if(nowPersonImageName == newPersonImageName) {
      page.Visibility = Visibility.Visible;
    } else {
      page.PersonImage.Source = new SvgImageSource(new Uri($"ms-appx:///Assets/Svg/Person/{newPersonImageName}.svg"));
      resetPersonImageSource = true;
    }
    nowPersonImageName = newPersonImageName;
    static Button CreateButton(CharacterRemark page,GameState game,string[] remainContents,UIElement parent) {
      return new Button { HorizontalAlignment = HorizontalAlignment.Stretch,VerticalAlignment = VerticalAlignment.Stretch,Background = Colors.FromARGB(34,0,0,0) }.MySetChild(new TextBlock { Text = remainContents.MyIsEmpty() ? "閉じる" : "次へ" }).MyApplyA(button =>
        button.Click += (_,_) => {
          GameState newGameState = game with {
            StartPlanningCharacterRemark = game.Phase == Phase.Planning ? remainContents : game.StartPlanningCharacterRemark,
            StartExecutionCharacterRemark = game.Phase == Phase.Execution ? remainContents : game.StartExecutionCharacterRemark,
          };
          page.Visibility = remainContents.MyIsEmpty() ? Visibility.Collapsed : Visibility.Visible;
          GameData.game = newGameState;
          SetElems(page,newGameState,remainContents,parent);
        }
      );
    }
  }
  internal static void ResizeElem(CharacterRemark page,Size parentSize,double scaleFactor) {
    double contentScale = 1.2;
    double sideMargin = UIUtil.infoFrameWidth * scaleFactor;
    double contentMaxWidth = (parentSize.Width - sideMargin * 2) / (scaleFactor * contentScale);
    double textMaxWidth = contentMaxWidth - personImageSize - 5 * 2;
    page.Margin = new(sideMargin,sideMargin,sideMargin,0);
    page.RemarkText.Measure(parentSize with { Width = textMaxWidth });
    page.Content.Width = (page.RemarkText.DesiredSize.Width + personImageSize + 5 * 2);
    page.Content.Height = (Math.Max(page.RemarkText.DesiredSize.Height,page.CloseButton.Height + personImageSize + page.PersonName.Height) + page.ButtonPanel.Height + 2.5 * 2);
    page.Content.Margin = new(page.Content.Width * (scaleFactor - 1) / 2,page.Content.Height * (scaleFactor - 1) / 2);
    page.Content.RenderTransform = new ScaleTransform { ScaleX = scaleFactor * contentScale,ScaleY = scaleFactor * contentScale,CenterX = page.Content.Width / 2,CenterY = page.Content.Height / 2 };
    page.RemarkText.MaxWidth = textMaxWidth;
    page.RemarkFrame.Data = remarkFrameCornerRadius.MyApplyF(v => page.RemarkText.DesiredSize.MyApplyF(size =>
      new PathGeometry {
        Figures = [ new PathFigure {
          StartPoint=new(size.Width-v,size.Height-v),
          Segments = [
            new ArcSegment{ Point=new(size.Width-v*2,size.Height),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
            new LineSegment{ Point=new(v,size.Height) },
            new ArcSegment{ Point=new(0,size.Height-v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
            new LineSegment{ Point=new(0,v) },
            new ArcSegment{ Point=new(v,0),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
            new LineSegment{ Point=new(size.Width-v*2,0) },
            new ArcSegment{ Point=new(size.Width-v,v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
            new LineSegment{ Point=new(size.Width-v,size.Height-v*2) },
            new ArcSegment{ Point=new(size.Width,size.Height-v*2),Size=new(v,v) },
            new ArcSegment{ Point=new(size.Width-v,size.Height-v),Size=new(v,v),SweepDirection=SweepDirection.Clockwise},
          ],
          IsClosed=true
        } ]
      }
    ));
  }
}