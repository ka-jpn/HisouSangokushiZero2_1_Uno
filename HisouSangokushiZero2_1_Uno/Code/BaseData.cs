using System.Collections.Generic;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class BaseData {
  internal static readonly DefType.Text name = new("悲愴三国志Zero2 バージョン1");
  internal static readonly DefType.Text version = new("1.13");
  internal static readonly ScenarioId[] scenarios = [new("反董連合"),new("蜀之南征")];
  internal static readonly Dictionary<PersonId,string> BiographyMap=new([
    new(new("陳温"),""),
    new(new(""),""),
  ]);
}