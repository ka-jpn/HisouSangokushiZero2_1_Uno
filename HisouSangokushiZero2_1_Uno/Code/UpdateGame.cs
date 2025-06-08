using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.Battle;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using PersonType = HisouSangokushiZero2_1_Uno.Code.DefType.Person;
using PostType = HisouSangokushiZero2_1_Uno.Code.DefType.Post;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class UpdateGame {
		internal static GameState SetPersonPost(GameState game,Dictionary<PersonType,PostType> postMap) => game with { PersonMap=game.PersonMap.ToDictionary(v => v.Key,v => postMap.TryGetValue(v.Key,out PostType? post) ? v.Value with { Post=post } : v.Value) };
		internal static GameState RemovePersonPost(GameState game,List<PersonType> removePersons) => game with { PersonMap=game.PersonMap.ToDictionary(v => v.Key,v => removePersons.Contains(v.Key) ? v.Value with { Post=null,GameDeathTurn = game.PlayTurn } : v.Value) };
		internal static GameState InitAlivePersonPost(GameState game) => game.CountryMap.Keys.SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetInitPost(game,country,role))).ToDictionary().MyApplyF(v => SetPersonPost(game,v));
		internal static GameState InitAppearPersonPost(GameState game) {
			Dictionary<ECountry,Dictionary<PersonType,PostType>> appearPersonMap = game.CountryMap.Keys.Select(country => (country, Enum.GetValues<ERole>().SelectMany(role => Post.GetInitAppearPost(game,country,role)).ToDictionary())).ToDictionary();
			Dictionary<ECountry,KeyValuePair<PersonType,PersonParam>> findPersonMap = game.CountryMap.Keys.Select(country => (appearPersonMap.GetValueOrDefault(country)?? []).Count==0&&Country.SuccessFindPersonRank(game,country) is int personRank ? Person.GenerateFindPerson(game,country,personRank) : (KeyValuePair<PersonType,PersonParam>?)null).MyNonNull().ToDictionary(v => v.Value.Country,v => v);
			return appearPersonMap.Values.SelectMany(v => v).ToDictionary().MyApplyF(v => SetPersonPost(game,v)).MyApplyF(game => game with { PersonMap=game.PersonMap.Concat(findPersonMap.Values).ToDictionary(),CountryMap=game.CountryMap.ToDictionary(v => v.Key,v => findPersonMap.ContainsKey(v.Key) ? v.Value with { AnonymousPersonNum=v.Value.AnonymousPersonNum+1 } : v.Value) });
		}
		internal static GameState PutWaitPersonPost(GameState game) => game.CountryMap.Keys.SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetPutWaitPost(game,country,role))).ToDictionary().MyApplyF(v => SetPersonPost(game,v));
    internal static GameState RemoveNaturalDeathPersonPost(GameState game) => game.CountryMap.Keys.SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Person.GetNaturalDeathPostPersonMap(game,country,role).Keys)).ToList().MyApplyF(deathPersons => RemoveDeathPersonPost(game,deathPersons,Text.NaturalDeathPersonText([.. deathPersons],Lang.ja)));
    internal static GameState RemoveWarDeathPersonPost(GameState game,List<PersonType> deathPersons) => RemoveDeathPersonPost(game,deathPersons,Text.WarDeathPersonText([.. deathPersons],Lang.ja));
    private static GameState RemoveDeathPersonPost(GameState game,List<PersonType> deathPersons,string? appendLog) => RemovePersonPost(game,deathPersons).MyApplyF(game => AppendLogMessage(game,[appendLog]));
    internal static GameState AutoPutPostCPU(GameState game,ECountry[] exceptCountrys) => game.CountryMap.Keys.Except(game.PlayCountry.MyMaybeToList().Concat(exceptCountrys)).SelectMany(country => Enum.GetValues<ERole>().SelectMany(role => Post.GetAutoPutPost(game,country,role))).ToDictionary().MyApplyF(v => SetPersonPost(game,v));
		internal static GameState PutPersonFromUI(GameState game,PersonType? putPerson,PostType? putPost) => putPerson!=null&&putPost!=null ? SetPersonPost(game,new() { { putPerson,putPost } }) : game;
		internal static GameState AttachGameStartData(GameState game,ECountry? countryName) => countryName is ECountry country ? game with { PlayCountry=country,PlayTurn=0 } : game;
    internal static GameState AppendGameStartLog(GameState game) {
      return game.MyApplyF(game => AppendLogMessage(game,[$"{game.PlayCountry}でプレイ開始"])).MyApplyF(game => AppendTurnLog(game,[$"{game.PlayCountry}でプレイ開始"]))
        .MyApplyF(game => AppendGameLog(game,[$"{game.PlayCountry}でプレイ開始 領土数{game.PlayCountry?.MyApplyF(country => Country.GetAreaNum(game,country))} {string.Join(',',Enum.GetValues<ERole>().SelectMany(role => game.PlayCountry?.MyApplyF(country => Person.GetInitPersonMap(game,country,role)).Select(v => v.Key.Value) ?? []))}"]));
    }
    internal static GameState UpdateCapitalArea(GameState game) => game with { CountryMap=game.CountryMap.ToDictionary(v => v.Key,countryInfo => countryInfo.Value with { CapitalArea=Area.ComputeCapitalArea(game,countryInfo.Key) }) };
		internal static GameState PayAttackFunds(GameState game,ECountry country) => game with { CountryMap=game.CountryMap.MyUpdate(country,(_,countryInfo) => countryInfo with { Fund=countryInfo.Fund-Country.CalcAttackFund(game,country) }) };
    internal static GameState AppendGameLog(GameState game,List<string?> appendMessages) => game with { GameLog = [.. game.GameLog,.. appendMessages.MyNonNull().Select(v=>$"{Turn.GetCalendarText(game)}:{v}")] };
    internal static GameState AppendLogMessage(GameState game,List<string?> appendMessages) => game with { LogMessage = [.. game.LogMessage,.. appendMessages.MyNonNull()] };
    internal static GameState AppendTurnLog(GameState game,List<string?> appendMessages) => game with { TrunLog = [.. game.TrunLog,.. appendMessages.MyNonNull()] };
    internal static GameState ClearTurnMyLog(GameState game) => game with { TrunLog = [] };
    internal static GameState CountryAttack(GameState game,ECountry attackSide,EArea target,Army attack,Army defense,AttackJudge judge) {
			return judge switch { AttackJudge.crush => Crush(game,attackSide,target,defense), AttackJudge.win => Win(game,attackSide,target), AttackJudge.lose => Lose(game,attackSide), AttackJudge.rout => Rout(game,attackSide,attack,defense) };
			static GameState Crush(GameState game,ECountry attackSide,EArea target,Army defense) => DeathCommander(game,defense,ERole.defense,attackSide);
			static GameState Win(GameState game,ECountry attackSide,EArea target) => SleepCountry(game,attackSide,1);
			static GameState Lose(GameState game,ECountry attackSide) => SleepCountry(game,attackSide,1);
			static GameState Rout(GameState game,ECountry attackSide,Army attack,Army defense) => SleepCountry(game,attackSide,3).MyApplyF(game => DeathCommander(game,attack,ERole.attack,defense.Country));
		}
		internal static GameState AreaAttack(GameState game,ECountry attackSide,EArea target,Army attack,Army defense,AttackJudge judge) {
			return judge switch { AttackJudge.crush => Crush(game,attackSide,target,defense), AttackJudge.win => Win(game,attackSide,target,defense), AttackJudge.lose => Lose(game,attackSide,target), AttackJudge.rout => Rout(game,attackSide,target,attack,defense) };
			static GameState Crush(GameState game,ECountry attackSide,EArea target,Army defense) => ChangeHasCountry(game,attackSide,defense.Country,target).MyApplyF(game => DeathCommander(game,defense,ERole.defense,attackSide));
			static GameState Win(GameState game,ECountry attackSide,EArea target,Army defense) => ChangeHasCountry(game,attackSide,defense.Country,target).MyApplyF(game => SleepCountry(game,attackSide,1));
			static GameState Lose(GameState game,ECountry attackSide,EArea target) => SleepCountry(game,attackSide,1).MyApplyF(game => DamageArea(game,target));
			static GameState Rout(GameState game,ECountry attackSide,EArea target,Army attack,Army defense) => SleepCountry(game,attackSide,3).MyApplyF(game => DeathCommander(game,attack,ERole.attack,defense.Country)).MyApplyF(game => DamageArea(game,target));
			static GameState ChangeHasCountry(GameState game,ECountry attackCountry,ECountry? defenseCountry,EArea targetArea) {
        return AppendChangeHasCountryLog(game,attackCountry,defenseCountry,targetArea).MyApplyF(game => UpdateAreaMap(game,attackCountry,targetArea)).MyApplyF(game => MakeEmptyPost(game,targetArea)).MyApplyF(game => IsPerishCountry(game,targetArea,defenseCountry) ? PerishCountry(game,attackCountry,defenseCountry) : IsFallCapital(game,targetArea,defenseCountry) ? FallCapital(game,defenseCountry) : game).MyApplyF(game=>FallArea(game,attackCountry,defenseCountry,targetArea));
        static GameState UpdateAreaMap(GameState game,ECountry attackCountry,EArea targetArea) => game with { AreaMap = game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { Country = attackCountry }) };
        static GameState MakeEmptyPost(GameState game,EArea targetArea) => game with { PersonMap = game.PersonMap.ToDictionary(v => v.Key,v => v.Value.Post?.PostKind != new PostKind(targetArea) ? v.Value : v.Value with { Post = null }) };
        static bool IsPerishCountry(GameState game,EArea targetArea,ECountry? defenseCountry) => defenseCountry?.MyApplyF(country => Country.GetAreaNum(game,country)) == 0;
        static bool IsFallCapital(GameState game,EArea targetArea,ECountry? defenseCountry) => defenseCountry?.MyApplyF(game.CountryMap.GetValueOrDefault)?.CapitalArea == targetArea;
        static GameState PerishCountry(GameState game,ECountry attackCountry,ECountry? defenseCountry) {
          List<PersonType> defenseCountryPerson = [.. defenseCountry?.MyApplyF(country=>Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role)).Select(v => v.Key))??[]];
          return game.MyApplyF(game => AppendTurnLog(game,[Text.PerishCountryText(defenseCountry,Lang.ja)])).MyApplyF(game => AppendLogMessage(game,[Text.PerishCountryText(defenseCountry,Lang.ja)])).MyApplyF(game => AppendGameLog(game,[Text.PerishCountryText(defenseCountry,Lang.ja)])).MyApplyF(game => RemoveWarDeathPersonPost(game,defenseCountryPerson)).MyApplyF(game => defenseCountry?.MyApplyF(country => game with { CountryMap = game.CountryMap.MyUpdate(country,(_,info) => info with { PerishFrom = attackCountry }) }) ?? game);
        }
        static GameState AppendChangeHasCountryLog(GameState game,ECountry attackCountry,ECountry? defenseCountry,EArea targetArea) {
          return AppendLogMessage(game,[Text.ChangeHasCountryText(attackCountry,defenseCountry,targetArea,Lang.ja)]).MyApplyF(game => AppendTurnLog(game,[Text.ChangeHasCountryText(attackCountry,defenseCountry,targetArea,Lang.ja)]));
        }
        static GameState FallCapital(GameState game,ECountry? defenseCountry) {
          List<PersonType> defenseCountryCapitalPerson = [.. defenseCountry?.MyApplyF(country => Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role)).Where(v => v.Value.Post?.PostKind.MaybeArea == null).Select(v => v.Key)) ?? []];
          List<PersonType> deathPerson = [.. defenseCountryCapitalPerson.Where(_ => MyRandom.RandomJudge(0.5))];
          return game.MyApplyF(game => AppendTurnLog(game,[Text.FallCapitalText(defenseCountry,Lang.ja)])).MyApplyF(game => AppendLogMessage(game,[Text.FallCapitalText(defenseCountry,Lang.ja)])).MyApplyF(game => RemoveWarDeathPersonPost(game,deathPerson));
        }
			  static GameState FallArea(GameState game,ECountry attackCountry,ECountry? defenseCountry,EArea targetArea) {
          return game.MyApplyF(game => IsPlayerUpdateMaxAreaNum(game)? AppendUpdateMaxAreaNumLog(game,defenseCountry,targetArea):game).MyApplyF(game=>FallDamageArea(game,targetArea)).MyApplyF(game=> UpdateMaxAreaNum(game,attackCountry,targetArea));
          static bool IsPlayerUpdateMaxAreaNum(GameState game) {
            return game.PlayCountry?.MyApplyF(game.CountryMap.GetValueOrDefault)?.MaxAreaNum < game.PlayCountry?.MyApplyF(country => Country.GetAreaNum(game,country));
          }
          static GameState AppendUpdateMaxAreaNumLog(GameState game,ECountry? defenseCountry,EArea targetArea) {
            return AppendGameLog(game,[Text.AppendUpdateMaxAreaNumLog(game.PlayCountry?.MyApplyF(country => Country.GetAreaNum(game,country)),defenseCountry,targetArea,Lang.ja)]);
          }
          static GameState FallDamageArea(GameState game,EArea targetArea) {
            return game with { AreaMap = game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { AffairParam = areaInfo.AffairParam with { AffairNow = Math.Round(areaInfo.AffairParam.AffairNow * 0.9m,4),AffairsMax = Math.Round(areaInfo.AffairParam.AffairsMax * 0.95m,4) } }) };
          }
          static GameState UpdateMaxAreaNum(GameState game,ECountry attackCountry,EArea targetArea) {
            return game with { CountryMap = game.CountryMap.MyUpdate(attackCountry,(_,countryInfo) => countryInfo with { MaxAreaNum = Math.Max(countryInfo.MaxAreaNum ?? 0,Country.GetAreaNum(game,attackCountry)) }) };
          }
        }
      }
			static GameState DamageArea(GameState game,EArea targetArea) {
				return game with { AreaMap=game.AreaMap.MyUpdate(targetArea,(_,areaInfo) => areaInfo with { AffairParam=areaInfo.AffairParam with { AffairNow=Math.Round(areaInfo.AffairParam.AffairNow*0.95m,4) } }) };
			}
		}
		private static GameState SleepCountry(GameState game,ECountry attackCountry,int sleepTurnNum) => game with { CountryMap=game.CountryMap.MyUpdate(attackCountry,(_,countryInfo) => countryInfo with { SleepTurnNum=sleepTurnNum }) };
    private static GameState DeathCommander(GameState game,Army army,ERole role,ECountry? battleCountry) {
      List<PersonType> deathPersons = new PersonType?[] { DeathJudge() ? army.Commander.MainPerson : null,DeathJudge() ? army.Commander.SubPerson : null }.MyNonNull();
      return deathPersons.Count == 0 ? game : AppendLogMessage(game,[Text.BattleDeathPersonText(role,deathPersons,battleCountry,Lang.ja)]).MyApplyF(game => army.Country == game.PlayCountry ? AppendGameLog(game,[Text.BattleDeathPersonText(role,deathPersons,battleCountry,Lang.ja)]) : game).MyApplyF(game => UpdatePersonMap(game,deathPersons));
      static bool DeathJudge() => MyRandom.RandomJudge(0.25);
      static GameState UpdatePersonMap(GameState game,List<PersonType> deathPersons) => game with { PersonMap = deathPersons.Aggregate(game.PersonMap,(fold,value) => fold.MyUpdate(value,(_,param) => param with { Post = null,GameDeathTurn = game.PlayTurn })) };
    }
    internal static GameState NextTurn(GameState game) {
      return game.MyApplyF(UpdateCapitalArea).MyApplyF(AddTurn).MyApplyF(InOutFunds).MyApplyF(AddAffair).MyApplyF(InitAppearPersonPost).MyApplyF(RemoveDeathPersonPost).MyApplyF(PutWaitPersonPost);
      static GameState AddTurn(GameState game) => game with { PlayTurn = game.PlayTurn + 1 };
      static GameState InOutFunds(GameState game) => game with { CountryMap = game.CountryMap.ToDictionary(v => v.Key,v => v.Value with { Fund = v.Value.Fund + Country.GetInFund(game,v.Key) - Country.GetOutFund(game,v.Key) }) };
      static GameState AddAffair(GameState game) => game with {
        AreaMap = game.AreaMap.ToDictionary(area => area.Key,area => area.Value with {
          AffairParam = area.Value.AffairParam with {
            AffairNow = Math.Clamp(area.Value.AffairParam.AffairNow + AddNowAfair(game,area),0,area.Value.AffairParam.AffairsMax),
            AffairsMax = area.Value.AffairParam.AffairsMax + Math.Round(area.Value.AffairParam.AffairsMax * 0.001m + 0.01m + AddNowAfair(game,area) * 0.05m,4)
          }
        })
      };
      static GameState RemoveDeathPersonPost(GameState game) => Turn.GetInYear(game) == BaseData.yearItems.Length / 2 ? RemoveNaturalDeathPersonPost(game) : game;
      static decimal AddNowAfair(GameState game,KeyValuePair<EArea,AreaInfo> area) {
        decimal countryAffairPower = Country.GetAffairPower(game,area.Value.Country) / Country.GetAffairDifficult(game,area.Value.Country);
        decimal personAffairPower = game.PersonMap.MyNullable().FirstOrDefault(v => v?.Value.Post?.PostRole == ERole.affair && v?.Value.Post?.PostKind == new PostKind(area.Key))?.Value.MyApplyF(v => Person.CalcRank(v,ERole.affair))??0;
        decimal areaAffairPower = 1 - area.Value.AffairParam.AffairNow / area.Value.AffairParam.AffairsMax;
        return Math.Round(countryAffairPower * personAffairPower * areaAffairPower,4);
      }
    }
    internal static GameState GameEndJudge(GameState game) {
      return SetWinCountrys(game).MyApplyF(game => IsPerish(game) ? PerishEnd(game) : IsWinEnd(game) ? WinEnd(game) : IsOtherWinEnd(game) ? OtherWinEnd(game) : IsTurnLimitOver(game) ? TurnLimitOverEnd(game) : game);
      static GameState SetWinCountrys(GameState game) => game with { WinCountrys = [.. game.CountryMap.Where(countryInfo => countryInfo.Value.WinConditionJudgeFunc(game)).Select(v => v.Key)] };
      static GameState WinEnd(GameState game) => (game with { Phase = Phase.WinEnd }).MyApplyF(game => AppendGameLog(game,[$"達成勝利\n最終陣営所属人物 {string.Join(',',game.PlayCountry?.MyApplyF(country => Enum.GetValues<ERole>().SelectMany(role => Person.GetAlivePersonMap(game,country,role))).Select(v => v.Key.Value) ?? [])}"]));
      static GameState OtherWinEnd(GameState game) => (game with { Phase = Phase.OtherWinEnd }).MyApplyF(game => AppendGameLog(game,[$"{string.Join("と",game.WinCountrys)}の勝利"])).MyApplyF(game => AppendGameLog(game,[$"他陣営達成勝利による敗北\n最終領土数 {game.PlayCountry?.MyApplyF(country => Country.GetAreaNum(game,country))}"]));
      static GameState PerishEnd(GameState game) => (game with { Phase = Phase.PerishEnd }).MyApplyF(game => AppendGameLog(game,[$"滅亡敗北\n{game.PlayCountry?.MyApplyF(game.CountryMap.GetValueOrDefault)?.PerishFrom}に滅ぼされた"]));
      static GameState TurnLimitOverEnd(GameState game) => (game with { Phase = Phase.TurnLimitOverEnd }).MyApplyF(game => AppendGameLog(game,[$"存続勝利\n最終領土数 {game.PlayCountry?.MyApplyF(country => Country.GetAreaNum(game,country))}"]));
      static bool IsWinEnd(GameState game) =>game.PlayCountry?.MyApplyF(game.WinCountrys.Contains)??false;
      static bool IsOtherWinEnd(GameState game) => !IsWinEnd(game) && game.WinCountrys.Length != 0;
      static bool IsTurnLimitOver(GameState game) => Turn.GetYear(game) >= game.NowScenario?.MyApplyF(ScenarioData.scenarios.GetValueOrDefault)?.EndYear;
      static bool IsPerish(GameState game) => game.PlayCountry?.MyApplyF(game.CountryMap.GetValueOrDefault)?.PerishFrom != null;
    }
    internal static GameState Attack(GameState game,ECountry attackCountry,EArea targetArea,ECountry? defenseCountry,bool defenseSideFocusDefense) {
			Army attackArmy = Commander.GetAttackCommander(game,attackCountry).MyApplyF(commander => new Army(attackCountry,commander,Commander.CommanderRank(game,commander,ERole.attack)));
			AttackResult? countryBattle = Battle.Country.Attack(game,defenseCountry,targetArea,attackArmy,defenseSideFocusDefense,Lang.ja);
			AttackResult areaBattle = Battle.Area.Attack(game,defenseCountry,targetArea,attackArmy,defenseSideFocusDefense,Lang.ja);
			return game.MyApplyF(game => PayAttackFunds(game,attackCountry)).MyApplyF(game => AttackSideDamage(game,attackCountry))
        .MyApplyF(game => BattleDefenseSideCentralDefense(game,countryBattle,attackCountry,targetArea,attackArmy))
				.MyApplyF(game => countryBattle?.Judge is AttackJudge.lose or AttackJudge.rout ? game : BattleDefenseSideAreaDefense(game,areaBattle,attackCountry,targetArea,attackArmy));
      static GameState AttackSideDamage(GameState game,ECountry attackCountry) => game with { AreaMap = game.AreaMap.ToDictionary(v => v.Key,v => v.Value.Country == attackCountry ? v.Value with { AffairParam = v.Value.AffairParam with { AffairNow = Math.Round(v.Value.AffairParam.AffairNow*0.99m,4) } } : v.Value) };
      static GameState BattleDefenseSideCentralDefense(GameState game,AttackResult? countryBattle,ECountry attackCountry,EArea targetArea,Army attackArmy) => countryBattle?.Judge.MyApplyF(judge => AppendLogMessage(game,[countryBattle.InvadeText]).MyApplyF(game => CountryAttack(game,attackCountry,targetArea,attackArmy,countryBattle.Defense,judge)))??game;
			static GameState BattleDefenseSideAreaDefense(GameState game,AttackResult areaBattle,ECountry attackCountry,EArea targetArea,Army attackArmy) => AppendLogMessage(game,[areaBattle.InvadeText]).MyApplyF(game => areaBattle.Judge.MyApplyF(judge => AreaAttack(game,attackCountry,targetArea,attackArmy,areaBattle.Defense,judge)));
		}
		internal static GameState Defense(GameState game,ECountry country,bool isTryAttack) => AppendLogMessage(game,[Text.DefenseText(country,isTryAttack,Lang.ja)]);
		internal static GameState Rest(GameState game, ECountry country) {
			return AppendLogMessage(game, [Text.RestText(country, Country.GetSleepTurn(game,country),Lang.ja)]) with { CountryMap = game.CountryMap.MyUpdate(country, (_, info) => info with { SleepTurnNum = info.SleepTurnNum - 1 }) };
		}
	}
}