using HisouSangokushiZero2_1_Uno.MyUtil;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class GetGame {
		private static ScenarioData.ScenarioInfo? GetScenario(Scenario? newScenario) => newScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault);
		private static GameState InitGame(Scenario? scenario,ScenarioData.ScenarioInfo? scenarioInfo) => new(scenario,scenarioInfo?.AreaMap.ToDictionary()?? [],scenarioInfo?.CountryMap.ToDictionary()?? [],scenarioInfo?.PersonMap.ToDictionary()?? [],null,null,0,Phase.SelectScenario,[],false,[],[],[],[]);
		private static GameState InitState(GameState game) => game.MyApplyF(UpdateGame.InitAlivePersonPost).MyApplyF(UpdateGame.AutoPutPostCPU).MyApplyF(UpdateGame.UpdateCapitalArea).MyApplyF(UpdateGame.ClearTurnMyLog).MyApplyF(ClearLogMessage).MyApplyF(ClearGameLog);
    private static GameState ClearLogMessage(GameState game) => game with { LogMessage = [] };
    private static GameState ClearGameLog(GameState game) => game with { GameLog = [] };
    private static GameState AppendStartMessage(GameState game) => UpdateGame.AppendLogMessage(game,[$"{GameInfo.name.Value} ƒo[ƒWƒ‡ƒ“{GameInfo.version.Value}"]);
    internal static GameState GetInitGameScenario(Scenario? scenario) => GetScenario(scenario).MyApplyF(maybeScenarioInfo => InitGame(scenario,maybeScenarioInfo)).MyApplyF(InitState).MyApplyF(AppendStartMessage);
	}
}