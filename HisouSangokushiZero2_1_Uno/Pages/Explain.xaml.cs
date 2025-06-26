using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
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
  private static readonly Dictionary<AttackJudge,Dictionary<double,Point>> judgePointMap = Enum.GetValues<AttackJudge>().ToDictionary(v => v,v => GetJudgeShapeCrds(v));
  private static readonly Dictionary<double,Point> judgeMaxPoints = GetJudgeShapeCrds(null);
  private static Dictionary<AttackJudge,Dictionary<double,Rectangle>> judgeThresholdPointRects = [];
  private static Dictionary<double,Rectangle> judgeMaxPointRects = [];
  private static Dictionary<AttackJudge,Dictionary<double,TextBlock>> judgeThresholdPointTexts = [];
  private static Dictionary<double,TextBlock> rankDiffTexts = [];
  internal Explain() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(Explain page) {
      SetAttackJudgeExplain(page);
      static void SetAttackJudgeExplain(Explain page) {
        page.AttackCrushFillColor.Background = ThresholdFillBrush(AttackJudge.crush);
        page.AttackCrushEdgeColor.Background = ThresholdEdgeBrush(AttackJudge.crush);
        page.AttackWinFillColor.Background = ThresholdFillBrush(AttackJudge.win);
        page.AttackWinEdgeColor.Background = ThresholdEdgeBrush(AttackJudge.win);
        page.AttackLoseFillColor.Background = ThresholdFillBrush(AttackJudge.lose);
        page.AttackLoseEdgeColor.Background = ThresholdEdgeBrush(AttackJudge.lose);
        page.AttackRoutFillColor.Background = ThresholdFillBrush(AttackJudge.rout);
        page.AttackRoutEdgeColor.Background = ThresholdEdgeBrush(AttackJudge.rout);
        page.AttackCrushShape.MyApplyA(v => v.Fill = ThresholdFillBrush(AttackJudge.crush));
        page.AttackWinShape.MyApplyA(v => v.Fill = ThresholdFillBrush(AttackJudge.win));
        page.AttackLoseShape.MyApplyA(v => v.Fill = ThresholdFillBrush(AttackJudge.lose));
        page.AttackRoutShape.MyApplyA(v => v.Fill = ThresholdFillBrush(AttackJudge.rout));
        judgeThresholdPointRects = Enum.GetValues<AttackJudge>().ToDictionary(v => v,v => CreateRects(v));
        judgeMaxPointRects = CreateRects(null);
        judgeThresholdPointTexts = Enum.GetValues<AttackJudge>().Skip(1).ToDictionary(v => v,v => CreateTexts(v));
        rankDiffTexts = CreateRankDiffTexts();
        page.AttackJudgeThresholdPointPanel.MySetChildren([.. judgeMaxPointRects.Values,.. judgeThresholdPointRects.Values.SelectMany(v => v.Values),.. judgeThresholdPointTexts.Values.SelectMany(v => v.Values)]);
        page.AttackRankDiffTextPanel.MySetChildren([.. rankDiffTexts.Values]);
        static SolidColorBrush ThresholdEdgeBrush(AttackJudge? attackJudge) => attackJudge switch { AttackJudge.crush => crushThresholdEdge, AttackJudge.win => winThresholdEdge, AttackJudge.lose => loseThresholdEdge, AttackJudge.rout => routThresholdEdge, _ => maxThresholdEdge };
        static SolidColorBrush ThresholdFillBrush(AttackJudge attackJudge) => attackJudge switch { AttackJudge.crush => crushThresholdFill, AttackJudge.win => winThresholdFill, AttackJudge.lose => loseThresholdFill, AttackJudge.rout => routThresholdFill };
        static Dictionary<double,TextBlock> CreateTexts(AttackJudge? attackJudge) => (attackJudge?.MyApplyF(judgePointMap.GetValueOrDefault) ?? judgeMaxPoints).Where(v => v.Key is -5 or -4 or -3 or -2 or -1 or 0 or 1 or 2 or 3 or 4 or 5).ToDictionary(v => v.Key,v => new TextBlock { Text = v.Value.Y.ToString() });
        static Dictionary<double,Rectangle> CreateRects(AttackJudge? attackJudge) => (attackJudge?.MyApplyF(judgePointMap.GetValueOrDefault) ?? judgeMaxPoints).ToDictionary(v => v.Key,_ => new Rectangle { Width = attackJudgePointSize,Height = attackJudgePointSize,Fill = ThresholdEdgeBrush(attackJudge) });
        static Dictionary<double,TextBlock> CreateRankDiffTexts() => judgeMaxPoints.Where(v => v.Key is -5 or -4 or -3 or -2 or -1 or 0 or 1 or 2 or 3 or 4 or 5).ToDictionary(v => v.Key,v => new TextBlock { Text = $"{v.Key:0.#}" });
      }
    }
  }
  private static Dictionary<double,Point> GetJudgeShapeCrds(AttackJudge? attackJudge) {
    return Enumerable.Range(-11,23).Select(v => v * 0.5).ToDictionary(v => v,v => GetJudgePoint(attackJudge,v));
    static Point GetJudgePoint(AttackJudge? attackJudge,double rankDiff) => new(rankDiff * 0.09 + 0.5,Battle.GetThreshold(attackJudge,rankDiff));
  }
  internal static void ResizeElem(Explain page,double scaleFactor) {
    double pageWidth = page.RenderSize.Width;
    double contentWidth = pageWidth / scaleFactor - 5;
    Size judgeViewSize = new(Math.Min(page.AttackJudgeExplainPanel.Width,contentWidth - page.BattleExplainCaption.DesiredSize.Width),page.AttackJudgeExplainPanel.Height);
    page.ContentPanel.Width = contentWidth;
    page.ContentPanel.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.AttackJudgeExplainPanel.MaxWidth = judgeViewSize.Width;
    judgePointMap.GetValueOrDefault(AttackJudge.crush)?.Values.Concat(judgePointMap.GetValueOrDefault(AttackJudge.win)?.Values.Reverse() ?? [])?.Select(crd => CookPoint(judgeViewSize,crd)).MyApplyA(crds => page.AttackCrushShape.Points = [.. crds]);
    judgePointMap.GetValueOrDefault(AttackJudge.win)?.Values.Concat(judgePointMap.GetValueOrDefault(AttackJudge.lose)?.Values.Reverse() ?? [])?.Select(crd => CookPoint(judgeViewSize,crd)).MyApplyA(crds => page.AttackWinShape.Points = [.. crds]);
    judgePointMap.GetValueOrDefault(AttackJudge.lose)?.Values.Concat(judgePointMap.GetValueOrDefault(AttackJudge.rout)?.Values.Reverse() ?? [])?.Select(crd => CookPoint(judgeViewSize,crd)).MyApplyA(crds => page.AttackLoseShape.Points = [.. crds]);
    judgePointMap.GetValueOrDefault(AttackJudge.rout)?.Values.Concat(judgeMaxPoints.Values.Reverse() ?? [])?.Select(crd => CookPoint(judgeViewSize,crd)).MyApplyA(crds => page.AttackRoutShape.Points = [.. crds]);
    judgeThresholdPointRects.ToList().ForEach(judge => judge.Value.ToList().ForEach(elemMap => UpdateJudgePointCrds(judgeViewSize,elemMap.Value,judgePointMap.GetValueOrDefault(judge.Key)?.GetValueOrDefault(elemMap.Key) ?? new())));
    judgeMaxPointRects.ToList().ForEach(elemMap => UpdateJudgePointCrds(judgeViewSize,elemMap.Value,judgeMaxPoints.GetValueOrDefault(elemMap.Key)));
    judgeThresholdPointTexts.ToList().ForEach(judge => judge.Value.ToList().ForEach(elemMap => UpdateJudgePointCrds(judgeViewSize,elemMap.Value,judgePointMap.GetValueOrDefault(judge.Key)?.GetValueOrDefault(elemMap.Key) ?? new())));
    rankDiffTexts.ToList().ForEach(rankDiff => UpdateJudgePointCrds(judgeViewSize,rankDiff.Value,judgePointMap.GetValueOrDefault(AttackJudge.crush)?.GetValueOrDefault(rankDiff.Key) ?? new()));
    page.ContentPanel.Margin = new(0,0,contentWidth * (scaleFactor - 1),page.ContentPanel.Children.Sum(v => v.DesiredSize.Height) * (scaleFactor - 1));
    static void UpdateJudgePointCrds(Size size,FrameworkElement elem,Point crd) => CookPoint(size,crd).MyApplyA(cookedCrd => {
      Canvas.SetLeft(elem,cookedCrd.X - elem.DesiredSize.Width / 2); Canvas.SetTop(elem,cookedCrd.Y - elem.DesiredSize.Height / 2);
    });
    static Point CookPoint(Size size,Point point) => point with { X = point.X * size.Width,Y = point.Y / 100 * size.Height };
  }
}