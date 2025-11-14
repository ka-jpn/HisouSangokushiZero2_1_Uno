using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using static HisouSangokushiZero2_1_Uno.Code.Scenario;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Area {
  private static Road[]? GetRoads(GameState game) => game.NowScenario?.MyApplyF(scenarios.GetValueOrDefault)?.RoadConnections;
  private static Point ConvertPointFromAreaPosition(Point areaPosition,Size mapSize,Size areaSize,Point mapGridCount,double infoFrameWidth) => new(areaPosition.X * (mapSize.Width - areaSize.Width - infoFrameWidth) / (mapGridCount.X - 1) + infoFrameWidth + areaSize.Width / 2,areaPosition.Y * (mapSize.Height - areaSize.Height - infoFrameWidth) / (mapGridCount.Y - 1) + infoFrameWidth + areaSize.Height / 2);
  internal static Point? GetAreaPoint(GameState game,EArea areaName,Size mapSize,Size areaSize,Point mapGridCount,double infoFrameWidth) => game.AreaMap.GetValueOrDefault(areaName)?.Position.MyApplyF(areaPos => ConvertPointFromAreaPosition(areaPos,mapSize,areaSize,mapGridCount,infoFrameWidth));
  internal static List<EArea> GetCountryAreas(GameState game,ECountry country) => [.. game.AreaMap.Where(v => v.Value.Country == country).Select(v => v.Key)];
  internal static List<EArea> GetConnectCapitalCountryAreas(GameState game,ECountry country) => [.. country == ECountry.Š¿ ? GetCountryAreas(game,country) : GetAreaDistancesFromCapital(game,country).SelectMany(v => v)];
  internal static List<EArea> GetCellEachAdjacentAnotherCountryAreas(GameState game,ECountry country) => [.. GetConnectCapitalCountryAreas(game,country).SelectMany(areaInfo => GetAdjacentAnotherCountryAreas(game,country,areaInfo))];
  internal static List<EArea> GetAdjacentAnotherCountryAllAreas(GameState game,ECountry country) => [.. GetCellEachAdjacentAnotherCountryAreas(game,country).Distinct()];
  internal static bool IsPlayerSelectable(GameState game,EArea? area) => area == null || (game.PlayCountry?.MyApplyF(playCountry => GetConnectCapitalCountryAreas(game,playCountry).Concat(Country.IsSleep(game,playCountry) ? [] : GetAdjacentAnotherCountryAllAreas(game,playCountry))).Contains(area.Value) ?? true);
  internal static List<EArea> GetAdjacentAnotherCountryAreas(GameState game,ECountry country,EArea area) => [.. GetAdjacentAreas(area,GetRoads(game)).Except(GetCountryAreas(game,country))];
  internal static List<EArea> GetAdjacentTargetCountryAreas(GameState game,ECountry country,EArea area) => [.. GetAdjacentAreas(area,GetRoads(game)).Intersect(GetCountryAreas(game,country))];
  private static List<EArea> GetAdjacentAreas(EArea area,Road[]? roads) => [.. roads?.Where(v => v.From == area).Select(v => v.To) ?? [],.. roads?.Where(v => v.To == area).Select(v => v.From) ?? []];
  internal static int? GetAreaDistance(GameState game,ECountry country,EArea src,EArea dst) => new List<EArea[]>([[src]]).MyApplyF(v => RecGetAreaDistances(game,country,v)).MyGetIndex(v => v.Contains(dst));
  private static List<EArea[]> GetAreaDistancesFromCapital(GameState game,ECountry country) => RecGetAreaDistances(game,country,[[..Country.GetCapitalArea(game,country).MyMaybeToList()??[]]]);
  private static List<EArea[]> RecGetAreaDistances(GameState game,ECountry country,List<EArea[]> confirm) => GetMoreOneDistanceAreas(game,country,confirm).MyApplyF(v => v.Length == 0 ? confirm : RecGetAreaDistances(game,country,[.. confirm,v]));
  private static EArea[] GetMoreOneDistanceAreas(GameState game,ECountry country,List<EArea[]> map) => [.. map.Last().SelectMany(v => GetAdjacentTargetCountryAreas(game,country,v)).Distinct().Except(map.SelectMany(v => v))];
  internal static List<EArea> CalcOrdDefenseAreas(GameState game,ECountry country) {
    return [.. GetConnectCapitalCountryAreas(game,country).OrderByDescending(v => ComputeAreaPressure(game,v))];
    static decimal ComputeAreaPressure(GameState game,EArea area) => ComputeAdjacentAnotherCountryAreas(game,area).Select(adjacent => AdjacentAreaPersonRank(game,adjacent) + 1).Sum();
    static decimal AdjacentAreaPersonRank(GameState game,EArea adjacent) => Country.GetAreaCountry(game,adjacent)?.MyApplyF(v => Person.GetPostPerson(game,v,new(ERole.defense,new(adjacent))))?.MyApplyF(v => Person.CalcRoleRank(game,v.Key,ERole.defense)) ?? 0m;
    static List<EArea> ComputeAdjacentAnotherCountryAreas(GameState game,EArea area) => game.AreaMap.GetValueOrDefault(area)?.Country?.MyApplyF(country => GetAdjacentAnotherCountryAreas(game,country,area)) ?? [];
  }
  internal static List<EArea> CalcOrdAffairAreas(GameState game,ECountry country) {
    return [.. GetConnectCapitalCountryAreas(game,country).OrderByDescending(v => ComputeAreaRemAffairs(game,v))];
    static decimal ComputeAreaRemAffairs(GameState game,EArea area) => game.AreaMap.GetValueOrDefault(area)?.AffairParam.MyApplyF(v => v.AffairsMax - v.AffairNow)??0;
  }
  internal static EArea[] GetChinaAreas(int scenarioNo)=>BaseData.scenarios.ElementAtOrDefault(scenarioNo)?.MyApplyF(scenarios.GetValueOrDefault)?.ChinaAreas??[];
}