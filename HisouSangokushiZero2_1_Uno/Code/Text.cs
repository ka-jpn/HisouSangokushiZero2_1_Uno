using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using CommanderType = HisouSangokushiZero2_1_Uno.Code.DefType.Commander;
using PersonType = HisouSangokushiZero2_1_Uno.Code.DefType.Person;
namespace HisouSangokushiZero2_1_Uno.Code {
  internal static class Text {
    private record ArmyTextInfo(string Country,string Commander,string Rank);
    private record InvadeText(ArmyTextInfo Attack,ArmyTextInfo Defense,EArea Field,AttackJudge Judge,bool DefenseSideFocusDefense);
    private static string GetCountryText(ECountry? country,Lang lang) => Ja.GetCountryText(country);
    private static string CommanderToText(CommanderType commander,Lang lang) => Ja.CommanderToText(commander);
    internal static string AreaInvadeText(Army attack,Army defense,EArea target,AttackJudge judge,bool defenseSideFocusDefense,Lang lang) => CalcAreaInvadeText(ArmyInfoToText(attack,lang),ArmyInfoToText(defense,lang),target,judge,defenseSideFocusDefense,lang);
    internal static string CountryInvadeText(Army attack,Army defense,EArea target,AttackJudge judge,bool defenseSideFocusDefense,Lang lang) => CalcCountryInvadeText(ArmyInfoToText(attack,lang),ArmyInfoToText(defense,lang),target,judge,defenseSideFocusDefense,lang);
    private static ArmyTextInfo ArmyInfoToText(Army army,Lang lang) => new(GetCountryText(army.Country,lang),CommanderToText(army.Commander,lang),army.Rank.ToString());
    private static string CalcCountryInvadeText(ArmyTextInfo src,ArmyTextInfo dest,EArea target,AttackJudge judge,bool defenseSideFocusDefense,Lang lang) => Ja.CalcCountryInvadeText(new(src,dest,target,judge,defenseSideFocusDefense));
    private static string CalcAreaInvadeText(ArmyTextInfo src,ArmyTextInfo dest,EArea target,AttackJudge judge,bool defenseSideFocusDefense,Lang lang) => Ja.CalcAreaInvadeText(new(src,dest,target,judge,defenseSideFocusDefense));
    internal static string GetAttackJudgeText(AttackJudge attackJudge,Lang lang) => Ja.GetAttackJudgeText(attackJudge);
    internal static string BattleDeathPersonText(ERole role,List<PersonType> deathPersons,ECountry? battleCountry,Lang lang) => Ja.BattleDeathPersonText(role,deathPersons,battleCountry);
    internal static string RoleToText(ERole role,Lang lang) => Ja.RoleToText(role);
    internal static string EndPhaseButtonText(Phase phase,Lang lang) => Ja.EndPhaseButtonText(phase);
    internal static string? NaturalDeathPersonText(List<PersonType> deathPersons,Lang lang) => Ja.NaturalDeathPersonText(deathPersons);
    internal static string? WarDeathPersonText(List<PersonType> deathPersons,Lang lang) => Ja.WarDeathPersonText(deathPersons);
    internal static string DefenseText(ECountry country,bool isTryAttack,Lang lang) => Ja.DefenseText(country,isTryAttack);
    internal static string RestText(ECountry country,int remainRestTurn,Lang lang) => Ja.RestText(country,remainRestTurn);
    internal static string ChangeHasCountryText(ECountry attackCountry,ECountry? defenseCountry,EArea targetArea,Lang lang) => Ja.ChangeHasCountryText(attackCountry,defenseCountry,targetArea);
    internal static string? FallCapitalText(ECountry? country,Lang lang) => Ja.FallCapitalText(country);
    internal static string? PerishCountryText(ECountry? country,Lang lang) => Ja.PerishCountryText(country);
    internal static string AppendUpdateMaxAreaNumLog(int? updatedMaxAreaNum,ECountry? defenseCountry,EArea targetArea,Lang lang) => Ja.AppendUpdateMaxAreaNumLog(updatedMaxAreaNum,defenseCountry,targetArea);

    private static class Ja {
      internal static string GetCountryText(ECountry? country) => country?.ToString() ?? "自治";
      internal static string CommanderToText(CommanderType commander) => commander.MainPerson == null && commander.SubPerson == null ? "無名武官" : $"{commander.MainPerson?.Value ?? "無名武官"}と{commander.SubPerson?.Value ?? "無名武官"}";
      internal static string CalcCountryInvadeText(InvadeText textInfo) => $"{textInfo.Attack.Country}の{textInfo.Attack.Commander}(ランク{textInfo.Attack.Rank})が{textInfo.Field}にて{(textInfo.DefenseSideFocusDefense ? "防衛専念の" : null)}{textInfo.Defense.Country}の中央軍の{textInfo.Defense.Commander}(ランク{textInfo.Defense.Rank})に攻撃して{GetAttackJudgeText(textInfo.Judge)}";
      internal static string CalcAreaInvadeText(InvadeText textInfo) => $"{textInfo.Attack.Country}の{textInfo.Attack.Commander}(ランク{textInfo.Attack.Rank})が{(textInfo.DefenseSideFocusDefense ? "防衛専念の" : null)}{textInfo.Defense.Country}領の{textInfo.Defense.Commander}(ランク{textInfo.Defense.Rank})が守備する{textInfo.Field}に侵攻して{GetAttackJudgeText(textInfo.Judge)}";
      internal static string GetAttackJudgeText(AttackJudge attackJudge) => attackJudge switch { AttackJudge.crush => "大勝", AttackJudge.win => "辛勝", AttackJudge.lose => "惜敗", AttackJudge.rout => "大敗" };
      internal static string BattleDeathPersonText(ERole role,List<PersonType> deathPersons,ECountry? battleCountry) => $"{(role == ERole.attack ? $"{GetCountryText(battleCountry)}領に侵攻" : $"{GetCountryText(battleCountry)}軍の侵攻を守備")}した{string.Join("と",deathPersons.Select(v => v.Value))}が退却できず戦死";
      internal static string RoleToText(ERole role) => role switch { ERole.central => "中枢", ERole.affair => "内政", ERole.defense => "防衛", ERole.attack => "攻撃" };
      internal static string EndPhaseButtonText(Phase phase) => phase switch { Phase.Planning => "軍議終了", _ => "確認" };
      internal static string? NaturalDeathPersonText(List<PersonType> deathPersons) => deathPersons.Count != 0 ? $"{string.Join("と",deathPersons.Select(v => v.Value))}が死去" : null;
      internal static string? WarDeathPersonText(List<PersonType> deathPersons) => deathPersons.Count != 0 ? $"{string.Join("と",deathPersons.Select(v => v.Value))}が戦死" : null;
      internal static string DefenseText(ECountry country,bool isTryAttack) => $"{(isTryAttack ? "(資金不足で攻撃中止)" : null)}{country}は防衛に専念";
      internal static string RestText(ECountry country,int remainRestTurn) => $"{country}は国力回復中(残り{remainRestTurn}ターン)";
      internal static string ChangeHasCountryText(ECountry attackCountry,ECountry? defenseCountry,EArea targetArea) => $"{GetCountryText(defenseCountry)}領の{targetArea}が{GetCountryText(attackCountry)}領に";
      internal static string FallCapitalText(ECountry? country) => $"{GetCountryText(country)}の首都が陥落";
      internal static string PerishCountryText(ECountry? country) => $"{GetCountryText(country)}が滅亡";
      internal static string AppendUpdateMaxAreaNumLog(int? updatedMaxAreaNum,ECountry? defenseCountry,EArea targetArea) => $"{GetCountryText(defenseCountry)}領の{targetArea}を攻略して最大領土数を{updatedMaxAreaNum}に更新";
    }
  }
}