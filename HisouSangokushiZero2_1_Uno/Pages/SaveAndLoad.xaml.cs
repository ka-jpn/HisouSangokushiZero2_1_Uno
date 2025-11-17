using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.MyUtil;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using static HisouSangokushiZero2_1_Uno.MyUtil.MyUtil;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record SaveSlotData(int SlotIndex,string GameVersion,string? NowScenario,string? PlayCountry,string? PlayTurn,string? PlayTime,string? LastSaveDate,string? HasSaveData);
internal sealed partial class SaveAndLoad:UserControl {
  internal const double minScaleFactor = 0.65;
  internal const double scrollMaxWidth = UIUtil.fixModeMaxWidth * minScaleFactor;
  private static bool isWritemode = false;
  private static Func<(Storage.ReadSaveFile, GameState?)?,Task> afterProcess = _ => Task.CompletedTask;
  private static List<(Storage.ReadSaveFile, MetaData?)> saveSlots = [];
  internal static readonly ObservableCollection<SaveSlotData> saveSlotTexts = [];
  internal SaveAndLoad() {
    InitializeComponent();
    MyInit(this);
  }
  private void MyInit(SaveAndLoad page) {
    page.CloseButton.Click += (_,_) => page.Visibility = Visibility.Collapsed;
    page.SizeChanged += (_,_) => ResizeElem(page,page.GetVisualTreeParent().RenderSize);
  }
  private static void RefreshSaveSlotView() {
    saveSlots = [.. Enumerable.Range(0,10).Select(fileIndex => Storage.ReadMetaData(IndexToFileNo(fileIndex)))];
    saveSlotTexts.Clear();
    saveSlots.Select((slot,index) => {
      if(slot.Item1 == Storage.ReadSaveFile.Read && slot.Item2 is MetaData meta) {
        return new SaveSlotData(
          SlotIndex: index,
          GameVersion: $"ゲームバージョン：{meta.GameVersion}",
          NowScenario: $"シナリオ：{meta.NowScenario?.Value}",
          PlayCountry: $"プレイ国名：{meta.PlayCountry?.ToString() ?? "(選択前)"}",
          PlayTurn: $"ターン数：{meta.PlayTurn?.ToString() ?? "(開始前)"}",
          PlayTime: $"プレイ時間：{Math.Floor(meta.TotalPlayTime.TotalMinutes)}分",
          LastSaveDate: $"最終保存：{meta.LastSaveDate.ToString("yyyy/MM/dd HH:mm")}",
          HasSaveData: null
        );
      } else {
        return new SaveSlotData(
          SlotIndex: index,
          GameVersion: string.Empty,
          NowScenario: null,
          PlayCountry: null,
          PlayTurn: null,
          PlayTime: null,
          LastSaveDate: null,
          HasSaveData: Storage.HasSaveData(IndexToFileNo(index)) ? "セーブデータがありますがここに表示されるメタデータがありません\n(再保存でメタデータが付加されます)" : "セーブデータなし"
        );
      }
    }).ToList().ForEach(slotData => saveSlotTexts.Add(slotData));
  }
  private static int IndexToFileNo(int index) => index + 1;
  internal static void Show(SaveAndLoad page,bool isWrite,Func<(Storage.ReadSaveFile state, GameState? maybeGame)?,Task> actionAfterProcess,Windows.Foundation.Size parentSize) {
    isWritemode = isWrite;
    afterProcess = actionAfterProcess;
    RefreshSaveSlotView();
    page.Title.Text = isWrite ? "セーブデータ選択" : "ロードデータ選択";
    ResizeElem(page,parentSize);
    page.Visibility = Visibility.Visible;
  }
  internal static void ResizeElem(SaveAndLoad page,Windows.Foundation.Size parentSize) {
    double scaleFactor = parentSize.Width >= scrollMaxWidth ? Math.Max(parentSize.Width / UIUtil.fixModeMaxWidth,1) : parentSize.Width / UIUtil.fixModeMaxWidth / minScaleFactor;
    double contentWidth = parentSize.Width / scaleFactor;
    double contentHeight = parentSize.Height / scaleFactor;
    page.Content.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
    page.Content.Margin = new(0,0,contentWidth * (scaleFactor - 1),contentHeight * (scaleFactor - 1));
  }
  private void SaveSlot_PointerPressed(object sender,PointerRoutedEventArgs e) {
    if(sender is StackPanel panel && panel.DataContext is SaveSlotData slotData) {
      if(isWritemode) {
        Storage.WriteStorageData(GameData.game,GameData.startingPlayTotalTime,IndexToFileNo(slotData.SlotIndex));
        RefreshSaveSlotView();
        afterProcess(null);
      } else {
        GameData.startingPlayTotalTime = SaveAndLoad.saveSlots.ElementAtOrDefault(slotData.SlotIndex).Item2?.TotalPlayTime ?? TimeSpan.Zero;
        afterProcess(Storage.ReadStorageData(IndexToFileNo(slotData.SlotIndex)));
      }
    }
  }
}