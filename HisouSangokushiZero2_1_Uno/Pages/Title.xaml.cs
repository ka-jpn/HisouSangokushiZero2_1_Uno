using HisouSangokushiZero2_1_Uno.Code;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class Title:Page {
  public Title() {
    InitializeComponent();
    MyInit(this);
    static void MyInit(Title page) {
      page.StartButton.Click += (_,_) => { GameData.game = GetInitGameData(); NavigateToGamePage(); };
      page.LoadButton.Click += async (_,_) => { GameData.game = (await Storage.ReadStorageData(1)).Item2 ?? GetInitGameData(); NavigateToGamePage(); };
      static GameState GetInitGameData() => GetGame.GetInitGameScenario(BaseData.scenarios.FirstOrDefault());
      static void NavigateToGamePage() => (Window.Current?.Content as Frame)?.Navigate(typeof(Game));
    }
  }
}