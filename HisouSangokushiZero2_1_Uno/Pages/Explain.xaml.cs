using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Linq;
using Windows.UI;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Explain:UserControl {
  private static readonly double attackJudgePointSize = 10;
  private static readonly SolidColorBrush crushThresholdFill = new(Color.FromArgb(255,240,190,190));
  private static readonly SolidColorBrush winThresholdFill = new(Color.FromArgb(255,235,235,160));
  private static readonly SolidColorBrush loseThresholdFill = new(Color.FromArgb(255,175,240,175));
  private static readonly SolidColorBrush routThresholdFill = new(Color.FromArgb(255,190,190,240));
  private static readonly SolidColorBrush crushThresholdEdge = new(Color.FromArgb(255,240,135,135));
  private static readonly SolidColorBrush winThresholdEdge = new(Color.FromArgb(255,230,230,65));
  private static readonly SolidColorBrush loseThresholdEdge = new(Color.FromArgb(255,105,225,105));
  private static readonly SolidColorBrush routThresholdEdge = new(Color.FromArgb(255,135,135,240));
  private static readonly SolidColorBrush maxThresholdEdge = new(Color.FromArgb(255,165,165,165));
  internal Explain() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(Explain page) {
      SetUIElements(page);
      page.Measure(new(double.PositiveInfinity,double.PositiveInfinity));
      page.Width = 1320;
      page.Height = page.DesiredSize.Height;
      static void SetUIElements(Explain page) {
        page.AttackCrushFillColor.Background = ThresholdFillBrush(AttackJudge.crush);
        page.AttackCrushEdgeColor.Background = ThresholdEdgeBrush(AttackJudge.crush);
        page.AttackWinFillColor.Background = ThresholdFillBrush(AttackJudge.win);
        page.AttackWinEdgeColor.Background = ThresholdEdgeBrush(AttackJudge.win);
        page.AttackLoseFillColor.Background = ThresholdFillBrush(AttackJudge.lose);
        page.AttackLoseEdgeColor.Background = ThresholdEdgeBrush(AttackJudge.lose);
        page.AttackRoutFillColor.Background = ThresholdFillBrush(AttackJudge.rout);
        page.AttackRoutEdgeColor.Background = ThresholdEdgeBrush(AttackJudge.rout);
        page.AttackCrushShape.MyApplyA(v => v.Fill = ThresholdFillBrush(AttackJudge.crush)).MyApplyA(v => v.Points = [.. GetJudgeShapeCrds(AttackJudge.crush),.. GetJudgeShapeCrds(AttackJudge.win).Reverse()]);
        page.AttackWinShape.MyApplyA(v => v.Fill = ThresholdFillBrush(AttackJudge.win)).MyApplyA(v => v.Points = [.. GetJudgeShapeCrds(AttackJudge.win),.. GetJudgeShapeCrds(AttackJudge.lose).Reverse()]);
        page.AttackLoseShape.MyApplyA(v => v.Fill = ThresholdFillBrush(AttackJudge.lose)).MyApplyA(v => v.Points = [.. GetJudgeShapeCrds(AttackJudge.lose),.. GetJudgeShapeCrds(AttackJudge.rout).Reverse()]);
        page.AttackRoutShape.MyApplyA(v => v.Fill = ThresholdFillBrush(AttackJudge.rout)).MyApplyA(v => v.Points = [.. GetJudgeShapeCrds(AttackJudge.rout),.. GetJudgeShapeCrds(null).Reverse()]);
        page.AttackJudgePointVisualPanel.MySetChildren([.. CreateRects(null),.. CreateRects(AttackJudge.crush),.. CreateRects(AttackJudge.win),.. CreateRects(AttackJudge.lose),.. CreateRects(AttackJudge.rout),.. CreateTexts(AttackJudge.win),.. CreateTexts(AttackJudge.lose),.. CreateTexts(AttackJudge.rout)]);
        page.AttackRankDiffTextPanel.MySetChildren([.. CreateRankDiffTexts()]);
        static SolidColorBrush ThresholdEdgeBrush(AttackJudge? attackJudge) => attackJudge switch { AttackJudge.crush => crushThresholdEdge, AttackJudge.win => winThresholdEdge, AttackJudge.lose => loseThresholdEdge, AttackJudge.rout => routThresholdEdge, _ => maxThresholdEdge };
        static SolidColorBrush ThresholdFillBrush(AttackJudge attackJudge) => attackJudge switch { AttackJudge.crush => crushThresholdFill, AttackJudge.win => winThresholdFill, AttackJudge.lose => loseThresholdFill, AttackJudge.rout => routThresholdFill };
        static TextBlock[] CreateTexts(AttackJudge? attackJudge) => [.. GetJudgePoints(attackJudge).Select(crd => new TextBlock { Text = crd.Y.ToString() }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd),new(UIUtil.CalcFullWidthTextLength(crd.Y.ToString()) * BasicStyle.fontsize,BasicStyle.textHeight))))];
        static Rectangle[] CreateRects(AttackJudge? attackJudge) => [.. GetJudgePoints(attackJudge).Select(crd => new Rectangle { Width = attackJudgePointSize,Height = attackJudgePointSize,Fill = ThresholdEdgeBrush(attackJudge) }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd),new(attackJudgePointSize,attackJudgePointSize))))];
        static TextBlock[] CreateRankDiffTexts() => [.. new double[] { -5,-4,-3,-2,-1,0,1,2,3,4,5 }.Select(i => GetJudgePoint(null,i).MyApplyF(crd => new TextBlock { Text = i.ToString() }.MyApplyA(elem => SetJudgePointCrds(elem,CookPoint(crd with { Y = 0 }),new(UIUtil.CalcFullWidthTextLength(i.ToString()) * BasicStyle.fontsize,BasicStyle.textHeight)))))];
        static Windows.Foundation.Point GetJudgePoint(AttackJudge? attackJudge,double rankDiff) => new(rankDiff * 9 + 50,Battle.GetThreshold(attackJudge,rankDiff));
        static Windows.Foundation.Point[] GetJudgeShapeCrds(AttackJudge? attackJudge) => [.. new double[] { -5.5,-5,-4.5,-4,-3.5,-3,-2.5,-2,-1,1,2,2.5,3,3.5,4,4.5,5,5.5 }.Select(i => CookPoint(GetJudgePoint(attackJudge,i)))];
        static Windows.Foundation.Point[] GetJudgePoints(AttackJudge? attackJudge) => [.. new double[] { -5,-4,-3,-2,-1,0,1,2,3,4,5 }.Select(i => GetJudgePoint(attackJudge,i))];
        static Windows.Foundation.Point CookPoint(Windows.Foundation.Point point) => point with { X = point.X * 11.5,Y = point.Y * 6 };
        static UIElement SetJudgePointCrds(UIElement elem,Windows.Foundation.Point crd,Size size) => elem.MyApplyA(elem => { Canvas.SetLeft(elem,crd.X - size.Width / 2); Canvas.SetTop(elem,crd.Y - size.Height / 2); });
      }
    }
  }
}