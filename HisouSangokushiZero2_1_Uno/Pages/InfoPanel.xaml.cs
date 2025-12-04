using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Pages;

internal sealed partial class InfoPanel:UserControl {
  private enum InfoPanelState { Explain, WinCond, ParamList, ChangeLog, Setting };
  private static readonly Dictionary<InfoPanelState,UserControl> infoPanelMap = [];
  private static readonly Color buttonBackground = new(175,255,255,255);
  internal InfoPanel() {
    InitializeComponent();
    MyInit();
    void MyInit() {
      SizeChanged += (_,_) => ResizeElem();
      ExplainButton.Click += (_,_) => SwitchInfoButton(InfoPanelState.Explain);
      WinCondButton.Click += (_,_) => SwitchInfoButton(InfoPanelState.WinCond);
      ParamListButton.Click += (_,_) => SwitchInfoButton(InfoPanelState.ParamList);
      ChangeLogButton.Click += (_,_) => SwitchInfoButton(InfoPanelState.ChangeLog);
      SettingButton.Click += (_,_) => SwitchInfoButton(InfoPanelState.Setting);

      LoadPage();
      void LoadPage() {
        Task.Run(async () => {
          await Dispatcher.RunAsync(CoreDispatcherPriority.Low,() => SwitchInfoButton(InfoPanelState.Explain));
          await Task.Yield();
        });
      }
      void SwitchInfoButton(InfoPanelState clickButtonInfoPanelState) {
        Dictionary<InfoPanelState,Button> buttonMap = new() { { InfoPanelState.Explain,ExplainButton },{ InfoPanelState.WinCond,WinCondButton },{ InfoPanelState.ParamList,ParamListButton },{ InfoPanelState.ChangeLog,ChangeLogButton },{ InfoPanelState.Setting,SettingButton } };
        buttonMap.ToList().ForEach(v => v.Value.Background = v.Key == clickButtonInfoPanelState ? Colors.LightGray : Colors.WhiteSmoke);
        infoPanelMap.TryAdd(clickButtonInfoPanelState,CreateInfoPanel(clickButtonInfoPanelState));
        infoPanelMap.GetValueOrDefault(clickButtonInfoPanelState)?.MyApplyF(elem => InfoContentPanel.MySetChildren([elem]));
        static UserControl CreateInfoPanel(InfoPanelState state) => state switch { InfoPanelState.Explain => new Explain(), InfoPanelState.WinCond => new WinCond(), InfoPanelState.ParamList => new ParamList(), InfoPanelState.ChangeLog => new ChangeLog(), InfoPanelState.Setting => new Setting() };
      }
      void ResizeElem() {

      }
    }
  }
}
