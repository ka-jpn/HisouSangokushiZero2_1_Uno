using HisouSangokushiZero2_1_Uno.MyUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using CommanderType = HisouSangokushiZero2_1_Uno.Code.DefType.Commander;
namespace HisouSangokushiZero2_1_Uno.Code {
	internal static class Commander {
		private static Dictionary<PostHead,PersonId?> GetPersonMap(GameState game,ECountry country,ERole role) => Enum.GetValues<PostHead>().ToDictionary(v => v,v => Person.GetPostPerson(game,country,new(role,new(v)))?.Key);
		private static Dictionary<PostHead,PersonId?> GetAreaDefensePersonMap(GameState game,ECountry defense,EArea battleArea) => new([new(PostHead.main,Person.GetPostPerson(game,defense,new(ERole.defense,new(battleArea)))?.Key)]);
		private static CommanderType PersonMapToCommander(Dictionary<PostHead,PersonId?> targetPersonMap) => new(targetPersonMap.GetValueOrDefault(PostHead.main),targetPersonMap.GetValueOrDefault(PostHead.sub));
		internal static CommanderType GetCommander(GameState game,ECountry? country,ERole role) => (country?.MyApplyF(v => GetPersonMap(game,v,role))?? []).MyApplyF(PersonMapToCommander);
		internal static CommanderType GetCentralCommander(GameState game,ECountry? country) => GetCommander(game,country,ERole.central);
		internal static CommanderType GetAffairsCommander(GameState game,ECountry? affairsCountry) => GetCommander(game,affairsCountry,ERole.affair);
		internal static CommanderType GetDefenseCommander(GameState game,ECountry? defenseCountry) => GetCommander(game,defenseCountry,ERole.defense);
		internal static CommanderType GetAttackCommander(GameState game,ECountry? attackCountry) => GetCommander(game,attackCountry,ERole.attack);
		internal static CommanderType AreaCommander(GameState game,ECountry? defenseCountry,EArea defenseArea) => (defenseCountry?.MyApplyF(v => GetAreaDefensePersonMap(game,v,defenseArea))?? []).MyApplyF(PersonMapToCommander);
		internal static decimal CommanderRank(GameState game,CommanderType commander,ERole role) => (commander.MainPerson?.MyApplyF(v => Person.CalcRoleRank(game,v,role))??0)+(commander.SubPerson?.MyApplyF(v => Person.CalcRoleRank(game,v,role)/2m)??0);
	}
}
