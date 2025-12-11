using HisouSangokushiZero2_1_Uno.Data.Scenario;
using HisouSangokushiZero2_1_Uno.MyUtil;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using CommanderType = HisouSangokushiZero2_1_Uno.Code.DefType.Commander;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Text {
  private record ArmyTextInfo(string Country,string Commander,string Rank);
  private record InvadeText(ArmyTextInfo Attack,ArmyTextInfo Defense,EArea Field,AttackJudge Judge,bool DefenseSideFocusDefense);
  internal static string CountryText(ECountry? country,Lang lang) => Ja.CountryText(country);
  private static string CommanderToText(CommanderType commander,Lang lang) => Ja.CommanderToText(commander);
  internal static string AreaInvadeText(Army attack,Army defense,EArea target,AttackJudge judge,bool defenseSideFocusDefense,Lang lang) => CalcAreaInvadeText(ArmyInfoToText(attack,lang),ArmyInfoToText(defense,lang),target,judge,defenseSideFocusDefense,lang);
  internal static string CountryInvadeText(Army attack,Army defense,EArea target,AttackJudge judge,bool defenseSideFocusDefense,Lang lang) => CalcCountryInvadeText(ArmyInfoToText(attack,lang),ArmyInfoToText(defense,lang),target,judge,defenseSideFocusDefense,lang);
  private static ArmyTextInfo ArmyInfoToText(Army army,Lang lang) => new(CountryText(army.Country,lang),CommanderToText(army.Commander,lang),army.Rank.ToString());
  private static string CalcCountryInvadeText(ArmyTextInfo src,ArmyTextInfo dest,EArea target,AttackJudge judge,bool defenseSideFocusDefense,Lang lang) => Ja.CalcCountryInvadeText(new(src,dest,target,judge,defenseSideFocusDefense));
  private static string CalcAreaInvadeText(ArmyTextInfo src,ArmyTextInfo dest,EArea target,AttackJudge judge,bool defenseSideFocusDefense,Lang lang) => Ja.CalcAreaInvadeText(new(src,dest,target,judge,defenseSideFocusDefense));
  internal static string GetAttackJudgeText(AttackJudge attackJudge,Lang lang) => Ja.GetAttackJudgeText(attackJudge);
  internal static string BattleDeathCommanderPersonText(ERole role,List<PersonId> deathPersons,ECountry? battleCountry,Lang lang) => Ja.BattleDeathCommanderPersonText(role,deathPersons,battleCountry);
  internal static string RoleToText(ERole? role,Lang lang) => Ja.RoleToText(role);
  internal static string? EndPhaseButtonText(Phase phase,Lang lang) => Ja.EndPhaseButtonText(phase);
  internal static string? AppearPersonText(ECountry country,List<PersonId> appearPersons) => Ja.AppearPersonText(country,appearPersons);
  internal static string? FindPersonText(ECountry country,List<PersonId> findPersons) => Ja.FindPersonText(country,findPersons);
  internal static string? NaturalDeathPersonText(ECountry country,List<PersonId> deathPersons,Lang lang) => Ja.NaturalDeathPersonText(country,deathPersons);
  internal static string? WarDeathBureaucracyPersonText(EArea area,List<PersonId> deathPersons,Lang lang) => Ja.WarDeathBureaucracyPersonText(area,deathPersons);
  internal static string DefenseText(ECountry country,bool isTryAttack,Lang lang) => Ja.DefenseText(country,isTryAttack);
  internal static string RestText(ECountry country,int remainRestTurn,Lang lang) => Ja.RestText(country,remainRestTurn);
  internal static string ChangeHasCountryText(ECountry attackSide,ECountry? defenseSide,EArea targetArea,Lang lang) => Ja.ChangeHasCountryText(attackSide,defenseSide,targetArea);
  internal static string? FallCapitalText(ECountry? country,Lang lang) => Ja.FallCapitalText(country);
  internal static string? PerishCountryText(ECountry? country,Lang lang) => Ja.PerishCountryText(country);
  internal static string AppendUpdateMaxAreaNumLog(int? updatedMaxAreaNum,ECountry? defenseSide,EArea targetArea,Lang lang) => Ja.AppendUpdateMaxAreaNumLog(updatedMaxAreaNum,defenseSide,targetArea);
  internal static string TurnHeadLogText(GameState game,Lang lang) => Ja.TurnHeadLogText(game);
  internal static string? FallPlayerCapitalText(EArea? capital,Lang lang) => Ja.FallPlayerCapitalText(capital);
  internal static string? FallPlayerCapitalDeathPersonText(List<PersonId> deathPersons,Lang lang) => Ja.FallPlayerCapitalDeathPersonText(deathPersons);
  internal static string? BattleDeathPersonCharacterRemarkText(ERole role,List<PersonId> deathPersons,ECountry? enemy,Lang lang) => Ja.BattleDeathPersonCharacterRemarkText(role,deathPersons,enemy);
  internal static string? FallPlayerCapitalCharacterRemarkText(EArea? capital,Lang lang) => Ja.FallPlayerCapitalCharacterRemarkText(capital);
  internal static string? FallPlayerCapitalDeathPersonCharacterRemarkText(List<PersonId> deathPersons,Lang lang) => Ja.FallPlayerCapitalDeathPersonCharacterRemarkText(deathPersons);
  internal static string? ChangeCapitalCharacterRemarkText(EArea prevCapital,EArea newCapial,Lang lang) => Ja.ChangeCapitalCharacterRemarkText(prevCapital,newCapial);
  internal static string? WarDeathBureaucracyPersonCharacterRemarkText(EArea area,List<PersonId> deathPersons,Lang lang) => Ja.WarDeathBureaucracyPersonCharacterRemarkText(area,deathPersons);
  internal static string[] StartPlanningCharacterRemarkTexts(GameState game,Lang lang) => Ja.StartPlanningCharacterRemarkTexts(game);
  internal static string GetRemarkPersonName(ECountry? country,bool isAliveCharacter,Lang lang) => Ja.GetRemarkPersonName(country,isAliveCharacter);
  internal static string NoBasicWinCondText(Lang lang) => Ja.NoBasicWinCondText();
  private static class Ja {
    internal static string CountryText(ECountry? country) => country?.ToString() ?? "自治";
    internal static string CommanderToText(CommanderType commander) => commander.MainPerson == null && commander.SubPerson == null ? "無名武官" : $"{commander.MainPerson?.Value ?? "無名武官"}と{commander.SubPerson?.Value ?? "無名武官"}";
    internal static string CalcCountryInvadeText(InvadeText textInfo) => $"{textInfo.Attack.Country}の{textInfo.Attack.Commander}(ランク{textInfo.Attack.Rank})が{textInfo.Field}にて{(textInfo.DefenseSideFocusDefense ? "防衛専念の" : null)}{textInfo.Defense.Country}の中央軍の{textInfo.Defense.Commander}(ランク{textInfo.Defense.Rank})に攻撃して{GetAttackJudgeText(textInfo.Judge)}";
    internal static string CalcAreaInvadeText(InvadeText textInfo) => $"{textInfo.Attack.Country}の{textInfo.Attack.Commander}(ランク{textInfo.Attack.Rank})が{(textInfo.DefenseSideFocusDefense ? "防衛専念の" : null)}{textInfo.Defense.Country}領の{textInfo.Defense.Commander}(ランク{textInfo.Defense.Rank})が守備する{textInfo.Field}に侵攻して{GetAttackJudgeText(textInfo.Judge)}";
    internal static string GetAttackJudgeText(AttackJudge attackJudge) => attackJudge switch { AttackJudge.crush => "大勝", AttackJudge.win => "辛勝", AttackJudge.lose => "惜敗", AttackJudge.rout => "大敗" };
    internal static string BattleDeathCommanderPersonText(ERole role,List<PersonId> deathPersons,ECountry? battleCountry) => $"{(role == ERole.attack ? $"{CountryText(battleCountry)}領に侵攻" : $"{CountryText(battleCountry)}軍の侵攻を守備")}した{string.Join("と",deathPersons.Select(v => v.Value))}が退却できず戦死";
    internal static string RoleToText(ERole? role) => role switch { ERole.central => "中枢", ERole.affair => "内政", ERole.defense => "防衛", ERole.attack => "攻撃", _ => string.Empty };
    internal static string? EndPhaseButtonText(Phase phase) => phase switch { Phase.Starting => null, Phase.Planning => "軍議終了", Phase.Execution => "確認", Phase.PerishEnd or Phase.TurnLimitOverEnd or Phase.WinEnd or Phase.OtherWinEnd => "ゲームログを表示" };
    internal static string? AppearPersonText(ECountry country,List<PersonId> appearPersons) => appearPersons.Count != 0 ? $"{country}に{string.Join("と",appearPersons.Select(v => v.Value))}が登場" : null;
    internal static string? FindPersonText(ECountry country,List<PersonId> findPersons) => findPersons.Count != 0 ? $"{country}が{string.Join("と",findPersons.Select(v => v.Value))}を登用" : null;
    internal static string? NaturalDeathPersonText(ECountry country,List<PersonId> deathPersons) => deathPersons.Count != 0 ? $"{country}の{string.Join("と",deathPersons.Select(v => v.Value))}が死去" : null;
    internal static string? WarDeathBureaucracyPersonText(EArea area,List<PersonId> deathPersons) => deathPersons.Count != 0 ? $"{area}にいた{string.Join("と",deathPersons.Select(v => v.Value))}が戦死" : null;
    internal static string DefenseText(ECountry country,bool isTryAttack) => $"{(isTryAttack ? "(資金不足で攻撃中止)" : null)}{country}は防衛に専念";
    internal static string RestText(ECountry country,int remainRestTurn) => $"{country}は国力回復中(残り{remainRestTurn}ターン)";
    internal static string ChangeHasCountryText(ECountry attackSide,ECountry? defenseSide,EArea targetArea) => $"{CountryText(defenseSide)}領の{targetArea}が{CountryText(attackSide)}領に";
    internal static string FallCapitalText(ECountry? country) => $"{CountryText(country)}の首都が陥落";
    internal static string PerishCountryText(ECountry? country) => $"{CountryText(country)}が滅亡";
    internal static string AppendUpdateMaxAreaNumLog(int? updatedMaxAreaNum,ECountry? defenseSide,EArea targetArea) => $"{CountryText(defenseSide)}領の{targetArea}を攻略して最大領土数を{updatedMaxAreaNum}に更新";
    internal static string TurnHeadLogText(GameState game) => $"------------{Turn.GetCalendarText(game) ?? ""}------------";
    internal static string? FallPlayerCapitalText(EArea? capital) => capital != null ? $"首都の{capital}が陥落" : null;
    internal static string? FallPlayerCapitalDeathPersonText(List<PersonId> deathPersons) => !deathPersons.MyIsEmpty() ? $"首都の陥落により{string.Join("と",deathPersons.Select(v => v.Value))}が死亡" : null;
    internal static string? BattleDeathPersonCharacterRemarkText(ERole role,List<PersonId> deathPersons,ECountry? enemy) => !deathPersons.MyIsEmpty() ? $"{(role == ERole.attack ? $"{CountryText(enemy)}領に侵攻" : $"{CountryText(enemy)}軍の侵攻を守備")}した{string.Join("と",deathPersons.Select(v => v.Value))}が戦死しました" : null;
    internal static string? FallPlayerCapitalCharacterRemarkText(EArea? capital) => capital != null ? $"我々の首都の{capital}が陥落しました" : null;
    internal static string? FallPlayerCapitalDeathPersonCharacterRemarkText(List<PersonId> deathPersons) => !deathPersons.MyIsEmpty() ? $"首都の陥落により{string.Join("と",deathPersons.Select(v => v.Value))}が死亡しました" : null;
    internal static string? ChangeCapitalCharacterRemarkText(EArea prevCapital,EArea newCapial) => prevCapital != newCapial ? $"首都が{newCapial}に移りました" : null;
    internal static string? WarDeathBureaucracyPersonCharacterRemarkText(EArea area,List<PersonId> deathPersons) => deathPersons.Count != 0 ? $"{area}にいた{string.Join("と",deathPersons.Select(v => v.Value))}が戦死しました" : null;
    internal static string[] StartPlanningCharacterRemarkTexts(GameState game) {
      return game.PlayTurn == 0 ? [
          "ゲームの説明を聞きますか？\n勝利条件・敗北条件と\n戦闘のルールの説明があります",
          $"勝利条件は2種類あります\n陣営毎の勝利条件を一番乗り達成で達成勝利\nまたは{game.NowScenario?.MyApplyF(ScenarioBase.GetScenarioData)?.EndYear}年春まで存続で存続勝利\nとなります、どちらも勝利ではありますが\n区別があることに留意してください",
          "敗北条件は2種類で\n陣営毎の勝利条件を他陣営に先に達成される\nおよび全領土失陥となります\nまた、我々の命運も共にありますので\nどうかできるだけ多くの生存者を\n後まで導いて頂きますよう",
          "戦闘のうち攻撃は首都から行軍可能な範囲ででき\n中央所属のうち攻撃の筆頭と次席のポスト\nの者が指揮し、能力差が勝敗率に影響します\n侵攻先が防衛に専念していると分が悪くなります\n無暗に攻撃することばかりが手段ではありません",
          "戦闘のうち防衛は地域毎に行われ\n侵攻された所を担当している者が防衛にあたり\n首都が侵攻され首都地域の防衛が破られると\n中央所属の防衛の筆頭と次席のポストの者が\n首都中央で応戦します\n我らが防衛に専念していると分が良くなります",
          "もし首都が落ちると仕えている我々に死者が出\n運営に支障をきたすことも考えられます\n首都の防衛は一層のご考慮が必要になる\nことでしょう",
          "今回は以上です、ご武運を！"
        ] : game.PlayTurn == 1 ? [
          "次の説明を聞きますか？\n内政値と収支の説明があります",
          "内政値は地域ごとにあり\n中央所属の内政の筆頭と次席のポストと\n地域の内政担当に配置した人物の能力により\n上昇させることができます\nただし、支配領域が広いほど\n上昇量にデバフがかかります",
          "内政担当を配置し内政値を上昇させるには\n少々支出がありますが\nその地域で戦闘があるなど\n内政値は減少する余地があるので\n適宜上げてゆかないと回らなくなります",
          "収支は収入と支出があり、うち収入は\n地域ごとの内政値と\n中央所属の内政の筆頭と次席のポストの\n者の能力が影響します\nただし支配領域が広いと\n収入効率にデバフがかかります",
          "収支のうち支出は\n支配領域が広いとより掛かる民政費\n地域の内政担当の内政のために掛かる内政費\n仕える人物への俸禄へ掛かる俸禄費\nがあります",
          "細かい計算式を把握する必要は\n基本的にありませんが\n領土を増やしたからといって\n収支に好影響なことばかりではない\nということはご理解ください",
          "今回は以上です、ご武運を"
        ] : game.PlayTurn == 2 ? [
          "次の説明を聞きますか？\n人物の加入・脱落と\n寿命の説明があります",
          "有名人物の加入は陣営ごとに決まっており\n時期が来れば人物が自動で加入します\n無名人物の加入は不確定要素で\n中央所属のうち中枢の筆頭と次席のポスト\nの者の能力により登用が行われます",
          "人物の脱落は不確定要素で\n戦死と寿命経過の要因があり\n戦死は首都陥落するか、もしくは\n防衛失敗と攻撃失敗のうち\n大敗すると可能性があります",
          "寿命は人物ごとにありますが\n必ずその時に死去ではなく\n寿命を過ぎるごとに確率が高くなり\n生存していればその時点での確率による\n死去判定があります",
          "詳しいことは\n情報のルール詳細に\n記してあります\nご武運を・・"
        ] : game.PlayTurn == 3 ? [
          $"今まで説明に来ていた{GetRemarkPersonName(game.PlayCountry,true)}が亡くなりました"
        ] : [];
    }
    internal static string GetRemarkPersonName(ECountry? country,bool isAliveCharacter) => isAliveCharacter ? country switch { ECountry.魏 => "杜畿", ECountry.呉 => "韓当", ECountry.蜀漢 => "簡雍", _ => "武官" } : "文官";
    internal static string NoBasicWinCondText() => "選べません(勝利条件なし)";
  }
}