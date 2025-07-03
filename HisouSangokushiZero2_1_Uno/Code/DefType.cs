using MessagePack;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.Foundation;
namespace HisouSangokushiZero2_1_Uno.Code {
  public static class DefType {
    internal record Text(string Value);
    internal record AreaData(Point Position,AffairsParam AffairParam,ECountry? Country);
    internal record CountryData(decimal Fund,int NavyLevel,SolidColorBrush ViewBrush,string[][] WinConditionMessages,Func<GameState,bool> WinConditionJudgeFunc,Func<GameState,Dictionary<string,bool?>> WinConditionProgressExplainFunc,int SleepTurnNum,int AnonymousPersonNum);
    [MessagePackObject]
    public record PersonData(
     [property: Key(0)] ERole Role,
     [property: Key(1)] int Rank,
     [property: Key(2)] int BirthYear,
     [property: Key(3)] int DeathYear,
     [property: Key(4)] ECountry Country,
     [property: Key(5)] int? GameAppearYear = null);
    [MessagePackObject] public record ScenarioId([property: Key(0)] string Value);
    [MessagePackObject] public record PersonId([property: Key(0)] string Value);
    [MessagePackObject] public record AreaInfo([property: Key(0)] AffairsParam AffairParam,[property: Key(1)] ECountry? Country);
    [MessagePackObject] public record CountryInfo([property: Key(0)] decimal Fund,[property: Key(1)] int NavyLevel,[property: Key(2)] int SleepTurnNum,[property: Key(3)] int AnonymousPersonNum,[property: Key(4)] int? MaxAreaNum = null,[property: Key(5)] EArea? CapitalArea = null,[property: Key(6)] ECountry? PerishFrom = null);
    [MessagePackObject] public record PersonInfo([property: Key(0)] int? GameDeathTurn = null,[property: Key(1)] Post? Post = null);
    [MessagePackObject] public record Post([property: Key(0)] ERole PostRole,[property: Key(1)] PostKind PostKind);
    [MessagePackObject] public record AffairsParam([property: Key(0)] decimal AffairsMax,[property: Key(1)] decimal AffairNow);
    [MessagePackObject]
    public record GameState(
      [property: Key(0)] ScenarioId? NowScenario,
      [property: Key(1)] Dictionary<EArea,AreaInfo> AreaMap,
      [property: Key(2)] Dictionary<ECountry,CountryInfo> CountryMap,
      [property: Key(3)] Dictionary<PersonId,PersonInfo> PersonMap,
      [property: Key(4)] ECountry? PlayCountry,
      [property: Key(5)] int? PlayTurn,
      [property: Key(6)] int PlayerMaxCellNum,
      [property: Key(7)] Phase Phase,
      [property: Key(8)] Dictionary<ECountry,EArea?> ArmyTargetMap,
      [property: Key(9)] bool IsTurnProcessing,
      [property: Key(10)] List<string> NewLog,
      [property: Key(11)] List<string> TrunNewLog,
      [property: Key(12)] List<string> GameLog,
      [property: Key(13)] ECountry[] WinCountrys,
      [property: Key(14)] Dictionary<PersonId,PersonData> AnonymousPersonMap);
    [MessagePackObject]
    public record PostKind([property: Key(0)] PostHead? MaybeHead,[property: Key(1)] int? MaybePostNo,[property: Key(2)] EArea? MaybeArea) {
      public PostKind(PostHead head) : this(head,null,null) { }
      public PostKind(int postNo) : this(null,postNo,null) { }
      public PostKind(EArea area) : this(null,null,area) { }
      public PostKind() : this(null,null,null) { }
    };
    internal record Commander(PersonId? MainPerson,PersonId? SubPerson);
    internal record Army(ECountry? Country,Commander Commander,decimal Rank);
    internal record AttackResult(Army Defense,AttackJudge Judge,string InvadeText);
    public enum Lang { ja };
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
}