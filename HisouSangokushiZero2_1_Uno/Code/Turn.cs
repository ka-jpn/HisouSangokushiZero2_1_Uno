using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class Turn {
		internal static int GetYear(GameState game) => (game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.StartYear??0)+(game.PlayTurn??0)/ UIUtil.yearItems.Length;
		internal static int GetInYear(GameState game) => (game.PlayTurn??0)% UIUtil.yearItems.Length;
		internal static string? GetCalendarInYear(GameState game) => UIUtil.yearItems.ElementAtOrDefault(GetInYear(game));
		internal static string? GetCalendarText(GameState game) => $"{GetYear(game)}年 {GetCalendarInYear(game)}";
	}
}
