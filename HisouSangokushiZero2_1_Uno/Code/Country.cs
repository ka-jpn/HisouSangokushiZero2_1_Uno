using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class Country {
    private static readonly Color nonCountryColor = new(255,240,240,240);
    internal static Color GetCountryColor(GameState game,ECountry? country) => country?.MyApplyF(v=>game.NowScenario?.MyApplyF(Scenario.scenarios.GetValueOrDefault)?.CountryMap.GetValueOrDefault(v))?.ViewColor ?? nonCountryColor;
		internal static decimal GetTotalAffair(GameState game,ECountry country) => game.AreaMap.Where(v => v.Value.Country==country).Sum(v => v.Value.AffairParam.AffairNow*(v.Key==game.CountryMap.GetValueOrDefault(country)?.CapitalArea ? 1.5m : 1m));
		internal static decimal GetAffairPower(GameState game,ECountry? country) => Commander.GetAffairsCommander(game,country).MyApplyF(v => Commander.CommanderRank(game,v,ERole.affair)).MyApplyF(affairsRank => affairsRank/5m+1);
		internal static decimal GetAffairDifficult(GameState game,ECountry? country) => Math.Round((decimal)Math.Pow(GetAreaNum(game,country),0.5),4);
		internal static decimal GetInFund(GameState game,ECountry country) => GetAreaNum(game,country)==0 ? 0 : Math.Round(GetTotalAffair(game,country)*GetAffairPower(game,country)/GetAffairDifficult(game,country)+10m/GetAreaNum(game,country),4);
		internal static decimal GetOutFund(GameState game,ECountry country) {
			Dictionary<PersonId,PersonData> deployedPersonMap = Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role).ExceptBy(Person.GetWaitPostPersonMap(game,country,role).Keys,v=>v.Key)).ToDictionary();
			decimal backCost = GetAreaNum(game,country)==0 ? 0 : Math.Round((decimal)(1-Math.Pow(0.9,(double)GetAffairDifficult(game,country)))*GetTotalAffair(game,country)/GetAffairDifficult(game,country),4);
			decimal roleCost = deployedPersonMap.Sum(v => v.Value.Post?.PostKind.MaybeHead==PostHead.main ? 1 : v.Value.Post?.PostKind.MaybeHead==PostHead.sub ? 0.5m : v.Value.Post?.PostKind.MaybeArea!=null ? 0.5m : 0);
      decimal affairCost = deployedPersonMap.Sum(v => v.Value.Post?.PostKind.MaybeArea != null && v.Value.Post?.PostRole == ERole.affair ? Person.CalcRoleRank(game,v.Key,ERole.affair) * 2 : 0);
			decimal personCost = deployedPersonMap.Sum(v => Person.GetPersonRank(game,v.Key)/10m);
			return backCost+roleCost+affairCost+personCost;
		}
		internal static EArea? GetTargetArea(GameState game,ECountry? counry) => counry?.MyApplyF(game.ArmyTargetMap.GetValueOrDefault);
    internal static List<EArea> GetAreas(GameState game,ECountry? country) => [.. game.AreaMap.Where(v => v.Value.Country == country).Select(v => v.Key)];
    internal static int GetAreaNum(GameState game,ECountry? country) => GetAreas(game,country).Count;
    internal static ECountry? GetAreaCountry(GameState game,EArea area) => game.AreaMap.GetValueOrDefault(area)?.Country;
		internal static int? SearchPersonRank(GameState game,ECountry country) {
			decimal mainSearchRank = Person.GetPostPerson(game,country,new(ERole.central,new(PostHead.main)))?.MyApplyF(v => Person.CalcRoleRank(game,v.Key,ERole.central))??0;
			decimal subSearchRank = Person.GetPostPerson(game,country,new(ERole.central,new(PostHead.sub)))?.MyApplyF(v => Person.CalcRoleRank(game,v.Key,ERole.central))??0;
			decimal searchPersonRank = mainSearchRank+subSearchRank/2;
			return !MyRandom.RandomJudge((double)(searchPersonRank+1)/30) ? null : MyRandom.RandomJudge((double)searchPersonRank/100) ? 2 : 1;
		}
    internal static bool IsFocusDefense(GameState game,ECountry? country) => game.Phase == Phase.Execution && country != null && game.ArmyTargetMap.TryGetValue(country.Value,out EArea? target) && target == null;
    internal static decimal CalcAttackFund(GameState game,ECountry country) => Commander.GetAttackCommander(game,country).MyApplyF(v => Commander.CommanderRank(game,v,ERole.attack)).MyApplyF(attackRank => (attackRank+1)*50);
		internal static bool CanPayAttackFund(GameState game, ECountry country) => CalcAttackFund(game,country)<= game.CountryMap.GetValueOrDefault(country)?.Fund;
    internal static int GetSleepTurn(GameState game, ECountry? country) => country?.MyApplyF(game.CountryMap.GetValueOrDefault)?.SleepTurnNum??0;
		internal static bool IsSleep(GameState game, ECountry? country) => GetSleepTurn(game,country) > 0;
    internal static bool SuccessAttack(GameState game,ECountry country) => CanPayAttackFund(game,country) && !IsSleep(game,country);
    internal static decimal? GetFund(GameState game,ECountry country) => game.CountryMap.GetValueOrDefault(country)?.Fund;
    internal static ECountry? GetPerishFrom(GameState game,ECountry country) => game.CountryMap.GetValueOrDefault(country)?.PerishFrom;
    internal static bool IsPerish(GameState game,ECountry country) => GetPerishFrom(game,country) != null;
    internal static int HasAreaCount(GameState game,ECountry country,EArea[] areas) => GetAreas(game,country).Intersect(areas).Count();
    internal static bool HasAreas(GameState game,ECountry country,EArea[] areas) => HasAreaCount(game,country,areas) == areas.Length;
    internal static EArea? GetCapitalArea(GameState game,ECountry countryName) => game.CountryMap.GetValueOrDefault(countryName)?.CapitalArea;
    internal static EArea? ComputeCapitalArea(GameState game,ECountry countryName) => countryName == ECountry.Š¿ ? null : Area.GetCountryAreaInfoMap(game,countryName).MyNullable().MaxBy(v => v?.Value.AffairParam.AffairNow)?.Key;
  }
}