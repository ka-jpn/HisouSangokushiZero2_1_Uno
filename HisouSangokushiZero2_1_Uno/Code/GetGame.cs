using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class GetGame {
		private static ScenarioData.ScenarioInfo? GetScenario(Scenario? newScenario) => newScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault);
		private static GameState InitGame(Scenario? scenario,ScenarioData.ScenarioInfo? scenarioInfo) => new(scenario,scenarioInfo?.AreaMap.ToDictionary()?? [],scenarioInfo?.CountryMap.ToDictionary()?? [],scenarioInfo?.PersonMap.ToDictionary()?? [],null,null,0,Phase.Starting,[],false,[],[],[],[]);
		private static GameState InitState(GameState game) => game.MyApplyF(UpdateGame.InitAlivePersonPost).MyApplyF(game => UpdateGame.AutoPutPostCPU(game,[])).MyApplyF(UpdateGame.UpdateCapitalArea).MyApplyF(UpdateGame.ClearTurnMyLog).MyApplyF(ClearLogMessage).MyApplyF(ClearGameLog);
    private static GameState ClearLogMessage(GameState game) => game with { LogMessage = [] };
    private static GameState ClearGameLog(GameState game) => game with { GameLog = [] };
    private static GameState AppendStartMessage(GameState game) => UpdateGame.AppendLogMessage(game,[$"{GameInfo.name.Value} ƒo[ƒWƒ‡ƒ“{GameInfo.version.Value}"]);
    private static GameState InitMaxAreaNum(GameState game) => game with { CountryMap = game.CountryMap.ToDictionary(v => v.Key,v => v.Value with { MaxAreaNum = Country.GetAreaNum(game,v.Key) }) };
    internal static GameState GetInitGameScenario(Scenario? scenario) => GetScenario(scenario).MyApplyF(maybeScenarioInfo => InitGame(scenario,maybeScenarioInfo)).MyApplyF(InitState).MyApplyF(AppendStartMessage).MyApplyF(InitMaxAreaNum);
	}
}