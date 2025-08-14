using MessagePack;
using System;
using System.Collections.Generic;
using Windows.UI;
namespace HisouSangokushiZero2_1_Uno.Code;
public static class DefType {
  internal record Text(string Value);
  internal record Size(double Width,double Height);
  [MessagePackObject(true)] public record Point(double X,double Y);
  [MessagePackObject(true)] public record Color(byte A,byte R,byte G,byte B);
  internal record CountryWinCondition(string[][] Messages,Func<GameState,bool> JudgeFunc,Func<GameState,Dictionary<string,bool?>> ProgressExplainFunc);
  [MessagePackObject(true)] public record AreaData(Point Position,AffairsParam AffairParam,ECountry? Country);
  [MessagePackObject(true)] public record CountryData(decimal Fund,int NavyLevel,Color ViewColor,int SleepTurnNum,int AnonymousPersonNum,int? MaxAreaNum = null,EArea? CapitalArea = null,ECountry? PerishFrom = null);
  [MessagePackObject(true)] public record PersonData(ERole Role,int Rank,int BirthYear,int DeathYear,ECountry Country,int? GameAppearYear = null,int? GameDeathTurn = null,Post? Post = null);
  [MessagePackObject(true)] public record ScenarioId(string Value);
  [MessagePackObject(true)] public record PersonId(string Value);
  [MessagePackObject(true)] public record Post(ERole PostRole,PostKind PostKind);
  [MessagePackObject(true)] public record AffairsParam(decimal AffairsMax,decimal AffairNow);
  [MessagePackObject(true)] public record GameState(ScenarioId? NowScenario,Dictionary<EArea,AreaData> AreaMap,Dictionary<ECountry,CountryData> CountryMap,Dictionary<PersonId,PersonData> PersonMap,ECountry? PlayCountry,int? PlayTurn,int PlayerMaxCellNum,Phase Phase,Dictionary<ECountry,EArea?> ArmyTargetMap,bool IsTurnProcessing,List<string> NewLog,List<string> TrunNewLog,List<string> GameLog,ECountry[] WinCountrys);
  [MessagePackObject(true)] public record PostKind(PostHead? MaybeHead,int? MaybePostNo,EArea? MaybeArea) {
    public PostKind(PostHead head) : this(head,null,null) { }
    public PostKind(int postNo) : this(null,postNo,null) { }
    public PostKind(EArea area) : this(null,null,area) { }
    public PostKind() : this(null,null,null) { }
  };
  internal record Commander(PersonId? MainPerson,PersonId? SubPerson);
  internal record Army(ECountry? Country,Commander Commander,decimal Rank);
  internal record AttackResult(Army Defense,AttackJudge Judge,string InvadeText);
  internal enum Lang { ja };
  public enum PostHead { main, sub };
  public enum Phase { Starting, Planning, Execution, PerishEnd, TurnLimitOverEnd, WinEnd, OtherWinEnd };
  internal enum AttackJudge { crush, win, lose, rout };
  internal enum RoadKind { land, water };
  public enum ERole { central, affair, defense, attack };
  public enum EArea {
    襄平, 番汗, 朝鮮, 土垠, 陽楽, 薊, 代, 南皮, 濮陽, 魯, 鄴, 晋陽, 劇, 淮陰, 彭城, 陰陵, 平陽, 離石, 洛陽, 平輿, 酸棗, 長安, 金城, 武威, 居延, 敦煌, 
    宛, 襄陽, 鄂, 武昌, 臨沅, 臨湘, 泉陵, 郴, 秣陵, 建業, 舒, 鄱陽, 南城, 山陰, 東侯官, 番禺, 合浦, 龍編, 日南, 
    南鄭, 房陵, 綿竹, 葭萌, 魚復, 成都, 漢嘉, 涪陵, 朱提, 邛都, 且蘭, 滇池, 西随, 不韋, 緬甸, 哀牢, 南越, 朱崖, 
    台北, 台南, 伊吾, 湔氐, 汶江, 羌, 西海, 朔方, 臨河, 北地, 鮮卑, 弾汗, 丁零, 堅昆, 烏桓, 白狼, 沃沮, 丸都, 目支, 首里
  };
  public enum ECountry {
    漢, 董卓, 袁紹, 袁術, 曹操, 孫堅, 公孫瓚, 陶謙, 韓遂, 劉焉, 公孫度, 劉虞, 焦和, 王叡, 劉表, 蘇固, 曹寅, 張咨, 劉岱, 韓馥, 張超, 張楊,
    魏, 呉, 蜀漢, 燕, 士燮, 馬相, 黒山賊, 南蛮, 北匈奴, 羌, 氐, 南匈奴, 鮮卑, 烏丸, 高句麗, 沃沮, 濊, 馬韓, 南越, 琉球
  };
}