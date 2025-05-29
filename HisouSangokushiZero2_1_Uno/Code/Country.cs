using HisouSangokushiZero2_1_Uno.MyUtil;
using Windows.UI;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class Country {
    internal static Color GetCountryColor(GameState game,ECountry? country) => country?.MyApplyF(game.CountryMap.GetValueOrDefault)?.ViewColor ?? Color.FromArgb(255,240,240,240);
		internal static decimal GetTotalAffair(GameState game,ECountry country) => game.AreaMap.Where(v => v.Value.Country==country).Sum(v => v.Value.AffairParam.AffairNow*(v.Key==game.CountryMap.GetValueOrDefault(country)?.CapitalArea ? 1.5m : 1m));
		internal static decimal GetAffairPower(GameState game,ECountry country) => Commander.GetAffairsCommander(game,country).MyApplyF(v => Commander.CommanderRank(game,v,ERole.affair)).MyApplyF(affairsRank => affairsRank/5m+1);
		internal static decimal GetAffairDifficult(GameState game,ECountry country) => Math.Round((decimal)Math.Pow(GetAreaNum(game,country),0.5),4);
		internal static decimal GetInFund(GameState game,ECountry country) => GetAreaNum(game,country)==0 ? 0 : Math.Round(GetTotalAffair(game,country)*GetAffairPower(game,country)/GetAffairDifficult(game,country)+10m/GetAreaNum(game,country),4);
		internal static decimal GetOutFund(GameState game,ECountry country) {
			List<PersonParam> deployedPersonParams = [..Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role).ExceptBy(Person.GetWaitPostPersonMap(game,country,role).Keys,v => v.Key).Select(v => v.Value))];
			decimal backCost = GetAreaNum(game,country)==0 ? 0 : Math.Round((decimal)(1-Math.Pow(0.9,(double)GetAffairDifficult(game,country)))*GetTotalAffair(game,country)/GetAffairDifficult(game,country),4);
			decimal roleCost = deployedPersonParams.Sum(v => v.Post?.PostKind.MaybeHead==PostHead.main ? 1 : v.Post?.PostKind.MaybeHead==PostHead.sub ? 0.5m : v.Post?.PostKind.MaybeArea!=null ? 0.5m : 0);
			decimal affairCost = deployedPersonParams.Sum(v => v.Post?.PostKind.MaybeArea!=null&&v.Post?.PostRole==ERole.affair ? v.Rank*2 : 0);
			decimal personCost = deployedPersonParams.Sum(v => v.Rank/10m);
			return backCost+roleCost+affairCost+personCost;
		}
		internal static EArea? GetTargetArea(GameState game,ECountry? counry) => counry?.MyApplyF(game.ArmyTargetMap.GetValueOrDefault);
    internal static List<EArea> GetAreas(GameState game,ECountry country) => [.. game.AreaMap.Where(v => v.Value.Country == country).Select(v => v.Key)];
    internal static int GetAreaNum(GameState game,ECountry country) => GetAreas(game,country).Count;
    internal static ECountry? GetAreaCountry(GameState game,EArea area) => game.AreaMap.GetValueOrDefault(area)?.Country;
		internal static int? SuccessFindPersonRank(GameState game,ECountry country) {
			decimal mainRank = Person.GetPostPerson(game,country,new(ERole.central,new(PostHead.main)))?.Value.MyApplyF(v => Person.CalcRank(v,ERole.central))??0;
			decimal subRank = Person.GetPostPerson(game,country,new(ERole.central,new(PostHead.sub)))?.Value.MyApplyF(v => Person.CalcRank(v,ERole.central))??0;
			decimal findPersonRank = mainRank+subRank/2;
			return !MyRandom.RandomJudge((double)(findPersonRank+1)/30) ? null : MyRandom.RandomJudge((double)findPersonRank/100) ? 2 : 1;
		}
    internal static bool IsFocusDefense(GameState game,ECountry? country) => game.Phase == Phase.Execution && country != null && game.ArmyTargetMap.TryGetValue(country.Value,out EArea? target) && target == null;
    internal static decimal CalcAttackFund(GameState game,ECountry country) => Commander.GetAttackCommander(game,country).MyApplyF(v => Commander.CommanderRank(game,v,ERole.attack)).MyApplyF(attackRank => (attackRank+1)*50);
		internal static bool CanPayAttackFund(GameState game, ECountry country) => CalcAttackFund(game,country)<= game.CountryMap.GetValueOrDefault(country)?.Fund;
    internal static int GetSleepTurn(GameState game, ECountry? country) => country?.MyApplyF(game.CountryMap.GetValueOrDefault)?.SleepTurnNum??0;
		internal static bool IsSleep(GameState game, ECountry? country) => GetSleepTurn(game,country) > 0;
    internal static bool SuccessAttack(GameState game,ECountry country) => CanPayAttackFund(game,country) && !IsSleep(game,country);
    internal static decimal? GetFund(GameState game,ECountry country) =>game.CountryMap.GetValueOrDefault(country)?.Fund;
    internal static ECountry? GetPerishFrom(GameState game,ECountry country) => game.CountryMap.GetValueOrDefault(country)?.PerishFrom;
    internal static bool IsPerish(GameState game,ECountry country) => GetPerishFrom(game,country) != null;
    internal static int HasAreaCount(GameState game,ECountry country,EArea[] areas) => GetAreas(game,country).Intersect(areas).Count();
    internal static bool HasAreas(GameState game,ECountry country,EArea[] areas) => HasAreaCount(game,country,areas) == areas.Length;
  }
}