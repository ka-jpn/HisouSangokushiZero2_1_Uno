using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record LogMessage(string Text);
internal sealed partial class LogPanel:UserControl {
  private static readonly ObservableCollection<LogMessage> logMessages = [];
  internal LogPanel() {
    InitializeComponent();
  }
  internal static void UpdateLogMessageUI(GameState game) {
    game.NewLog.Select(v => new LogMessage(v)).ToList().ForEach(logMessages.Add);
    game.NewLog.Clear();
  }
  internal static void ClearLogMessageUI() => logMessages.Clear();
}