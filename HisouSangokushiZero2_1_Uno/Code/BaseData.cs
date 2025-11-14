using System.Collections.Generic;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class BaseData {
  internal static readonly DefType.Text name = new("悲愴三国志Zero2 バージョン1");
  internal static readonly DefType.Text version = new("1.15");
  internal static readonly ScenarioId[] scenarios = [new("反董連合"),new("蜀之南征")];
  internal static readonly Dictionary<PersonId,string> BiographyMap=new([
    new(new("陳温"),"漢末の揚州刺史、揚州への野望を持つ袁術に殺された"),
    new(new("華歆"),"漢末の豫章太守、のち孫策、曹魏に仕え、高官に昇る"),
    new(new("盛憲"),"漢末の呉郡太守、孫策の江東進出の際孫権に殺された"),
    new(new("周昕"),"漢末の丹陽太守、袁術が揚州に進出すると王朗を頼る"),
    new(new("蔡琰"),"漢末の巴郡太守、蔡瑁の従兄弟　同名他人と混同注意"),
    new(new("朱符"),"漢末の交州刺史、朱儁の長男、不服従民の反乱で死去"),
    new(new("陸康"),"太守を歴任し、漢末に廬江太守に、孫策に敗れて死去"),
    new(new("張羨"),"零陵や桂陽で県長、長沙太守になり劉表に反旗も敗死"),
    new(new("申氏"),"名は不詳、申耽や申儀の父、上庸郡一帯を抑える豪族"),
    new(new("公孫度"),"漢末の遼東太守、遼東一帯を支配し独自の体制を築く"),
    new(new("王建"),"燕の相国、司馬懿の遼東征伐の際、交渉で斬られ死去"),
    new(new("公孫康"),"公孫度の子、遼東太守を継ぐ、曹操との敵対を避けた"),
    new(new("柳甫"),"燕の軍師、司馬懿の遼東征伐の際、交渉で斬られ死去"),
    new(new("柳毅"),"遼東太守の公孫度の側近、公孫度から野望を語られる"),
    new(new("陽儀"),"遼東太守の公孫度の側近、公孫度から野望を語られる"),
    new(new("韓忠"),"公孫康の文官、烏丸の懐柔を曹操の牽招に阻止される"),
    new(new("賈範"),"公孫淵が燕を建て魏に反旗を翻す際、諫めて殺される"),
    new(new("張敞"),"公孫康の武官、濊や馬韓などを討伐、占有域を広げる"),
    new(new("公孫晃"),"公孫恭に曹操の下に送られる、燕が背くと処刑された"),
    new(new("宿舒"),"公孫淵の武官、呉と互いに海路で往来し、誼を通じる"),
    new(new("公孫模"),"公孫康の武官、濊や馬韓などを討伐、占有域を広げる"),
    new(new("綸直"),"公孫淵が燕を建て魏に反旗を翻す際、諫めて殺される"),
    new(new("卑衍"),"燕の将軍、司馬懿の遼東征伐の際、迎撃するも敗れる"),
    new(new("関靖"),""),
   /* new(new(""),""),
    new(new(""),""),
    new(new(""),""),
    new(new(""),""),
    new(new(""),""),*/
  ]);
}