using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Area {
  private static ScenarioData.Road[]? GetRoads(GameState game) => game.NowScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.RoadConnections;
  private static Point ConvertPointFromAreaPosition(Point areaPosition,Size mapSize,Size areaSize,Point mapGridCount,double infoFrameWidth) => new(areaPosition.X * (mapSize.Width - areaSize.Width - infoFrameWidth) / (mapGridCount.X - 1) + infoFrameWidth + areaSize.Width / 2,areaPosition.Y * (mapSize.Height - areaSize.Height - infoFrameWidth) / (mapGridCount.Y - 1) + infoFrameWidth + areaSize.Height / 2);
  internal static Point? GetAreaPoint(GameState game,EArea areaName,Size mapSize,Size areaSize,Point mapGridCount,double infoFrameWidth) => game.NowScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.AreaMap.GetValueOrDefault(areaName)?.Position.MyApplyF(areaPos => ConvertPointFromAreaPosition(areaPos,mapSize,areaSize,mapGridCount,infoFrameWidth));
  internal static Dictionary<EArea,AreaInfo> GetCountryAreaInfoMap(GameState game,ECountry country) => game.AreaMap.Where(v => v.Value.Country == country).ToDictionary();
  internal static List<EArea> GetAdjacentAnotherCountryAllAreas(GameState game,ECountry country) => [.. GetCountryAreaInfoMap(game,country).SelectMany(areaInfo => GetAdjacentAnotherCountryAreas(game,country,areaInfo.Key)).Distinct()];
  internal static bool IsPlayerSelectable(GameState game,EArea? area) => area == null || (game.PlayCountry?.MyApplyF(playCountry => GetAdjacentAnotherCountryAllAreas(game,playCountry).Concat(GetCountryAreaInfoMap(game,playCountry).Keys)).Contains(area.Value) ?? true);
  internal static List<EArea> GetAdjacentAnotherCountryAreas(GameState game,ECountry country,EArea area) => [.. GetAdjacentAreas(area,GetRoads(game)).Except(GetCountryAreaInfoMap(game,country).Keys)];
  internal static List<EArea> GetAdjacentTargetCountryAreas(GameState game,ECountry country,EArea area) => [.. GetAdjacentAreas(area,GetRoads(game)).Intersect(GetCountryAreaInfoMap(game,country).Keys)];
  private static List<EArea> GetAdjacentAreas(EArea area,ScenarioData.Road[]? roads) => [.. roads?.Where(v => v.From == area).Select(v => v.To) ?? [],.. roads?.Where(v => v.To == area).Select(v => v.From) ?? []];
  internal static int? GetAreaDistance(GameState game,ECountry country,EArea src,EArea dst) => new List<EArea[]>([[src]]).MyApplyF(v => RecGetAreaDistances(game,country,v).ToList()).MyGetIndex(v => v.Contains(dst));
  private static List<EArea[]> RecGetAreaDistances(GameState game,ECountry country,List<EArea[]> confirm) => GetMoreOneDistanceAreas(game,country,confirm).MyApplyF(v => v.Length == 0 ? confirm : RecGetAreaDistances(game,country,[.. confirm,v]));
  private static EArea[] GetMoreOneDistanceAreas(GameState game,ECountry country,List<EArea[]> map) => [.. map.Last().SelectMany(v => GetAdjacentTargetCountryAreas(game,country,v)).Distinct().Except(map.SelectMany(v => v))];
  internal static List<EArea> CalcOrdDefenseAreas(GameState game,ECountry country) {
    return [.. GetCountryAreaInfoMap(game,country).OrderByDescending(v => ComputeAreaPressure(game,v)).Select(v => v.Key)];
    static decimal ComputeAreaPressure(GameState game,KeyValuePair<EArea,AreaInfo> area) => ComputeAdjacentAnotherCountryAreas(game,area).Select(adjacent => AdjacentAreaPersonRank(game,adjacent) + 1).Sum();
    static decimal AdjacentAreaPersonRank(GameState game,EArea adjacent) => Country.GetAreaCountry(game,adjacent)?.MyApplyF(v => Person.GetPostPerson(game,v,new(ERole.defense,new(adjacent)))?.Value)?.MyApplyF(v => Person.CalcRank(v,ERole.defense)) ?? 0m;
    static List<EArea> ComputeAdjacentAnotherCountryAreas(GameState game,KeyValuePair<EArea,AreaInfo> area) => area.Value.Country?.MyApplyF(country => GetAdjacentAnotherCountryAreas(game,country,area.Key)) ?? [];
  }
  internal static List<EArea> CalcOrdAffairAreas(GameState game,ECountry country) {
    return [.. GetCountryAreaInfoMap(game,country).OrderByDescending(v => ComputeAreaRemAffairs(game,v)).Select(v => v.Key)];
    static decimal ComputeAreaRemAffairs(GameState game,KeyValuePair<EArea,AreaInfo> area) => area.Value.AffairParam.MyApplyF(v => v.AffairsMax - v.AffairNow);
  }
  internal static EArea[] GetChinaAreas(int scenarioNo)=>BaseData.scenarios.ElementAtOrDefault(scenarioNo)?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.ChinaAreas??[];
}