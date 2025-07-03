using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using static HisouSangokushiZero2_1_Uno.Code.Scenario;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class GetGame {
		private static ScenarioData? GetScenario(ScenarioId? newScenario) => newScenario?.MyApplyF(scenarios.GetValueOrDefault);
		private static GameState InitGame(ScenarioId? scenario,ScenarioData? scenarioInfo) => new(scenario,ToAreaInfo(scenarioInfo?.AreaMap.ToDictionary()?? []),ToCountryInfo(scenarioInfo?.CountryMap.ToDictionary()?? []),ToPersonInfo(scenarioInfo?.PersonMap.ToDictionary()?? []),null,null,0,Phase.Starting,[],false,[],[],[],[],[]);
		private static GameState InitState(GameState game) => game.MyApplyF(UpdateGame.InitAlivePersonPost).MyApplyF(game => UpdateGame.AutoPutPostCPU(game,[])).MyApplyF(UpdateGame.UpdateCapitalArea).MyApplyF(ClearAllLog);
    private static GameState ClearAllLog(GameState game) => game with { GameLog = [],NewLog = [],TrunNewLog = [] };
    private static GameState AppendStartMessage(GameState game) => UpdateGame.AppendNewLog(game,[$"{BaseData.name.Value} ƒo[ƒWƒ‡ƒ“{BaseData.version.Value}"]);
    private static GameState InitMaxAreaNum(GameState game) => game with { CountryMap = game.CountryMap.ToDictionary(v => v.Key,v => v.Value with { MaxAreaNum = Country.GetAreaNum(game,v.Key) }) };
    private static Dictionary<EArea,AreaInfo> ToAreaInfo(Dictionary<EArea,AreaData> areaData) => areaData.ToDictionary(v=>v.Key,v=>new AreaInfo(v.Value.AffairParam,v.Value.Country));
    private static Dictionary<ECountry,CountryInfo> ToCountryInfo(Dictionary<ECountry,CountryData> areaData) => areaData.ToDictionary(v => v.Key,v => new CountryInfo(v.Value.Fund,v.Value.NavyLevel,v.Value.SleepTurnNum,v.Value.AnonymousPersonNum));
    private static Dictionary<PersonId,PersonInfo> ToPersonInfo(Dictionary<PersonId,PersonData> areaData) => areaData.ToDictionary(v => v.Key,v => new PersonInfo());
    internal static GameState GetInitGameScenario(ScenarioId? scenario) => GetScenario(scenario).MyApplyF(maybeScenarioInfo => InitGame(scenario,maybeScenarioInfo)).MyApplyF(InitState).MyApplyF(AppendStartMessage).MyApplyF(InitMaxAreaNum);
	}
}