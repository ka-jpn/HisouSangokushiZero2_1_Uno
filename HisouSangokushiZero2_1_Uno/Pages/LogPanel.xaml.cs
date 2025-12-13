using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal sealed partial class LogPanel:UserControl {
  private static readonly ObservableCollection<string> logMessages = [];
  internal LogPanel() {
    InitializeComponent();
  }
  internal static void UpdateLogMessageUI(GameState game) {
    logMessages.Clear();
    game.LogMessage.ForEach(logMessages.Add);
  }
}