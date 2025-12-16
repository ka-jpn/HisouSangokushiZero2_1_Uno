using HisouSangokushiZero2_1_Uno.Code;
using HisouSangokushiZero2_1_Uno.Data;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
using static HisouSangokushiZero2_1_Uno.Code.Storage;
namespace HisouSangokushiZero2_1_Uno.Pages;
internal record SaveSlotData(int SlotIndex,string GameVersion,string? NowScenario,string? PlayCountry,string? PlayTurn,string? PlayTime,string? LastSaveDate,string? HasSaveData);
public sealed partial class SaveAndLoad:UserControl {
  internal const double minScaleFactor = 0.65;
  internal const double scrollMaxWidth = UIUtil.fixModeMaxWidth * minScaleFactor;
  private static bool isWritemode = false;
  private static Func<ReadGame?,Task> pressSlotAfterProcess = _ => Task.CompletedTask;
  private static Action pressCloseProcess = MyUtil.MyUtil.nothing;
  private static List<ReadMeta> saveSlots = [];
  internal static readonly ObservableCollection<SaveSlotData> saveSlotTexts = [];
  internal SaveAndLoad() {
    InitializeComponent();
    MyInit();
  }
  private void MyInit() {
    CloseButton.Click += (_,_) => pressCloseProcess();
    SizeChanged += (_,_) => ResizeElem(RenderSize);
  }
  private static async Task RefreshSaveSlotView() {
    saveSlots = [.. await Task.WhenAll(Enumerable.Range(0, 10).Select(async fileIndex => await ReadMetaData(IndexToFileNo(fileIndex))))];
    saveSlotTexts.Clear();
    saveSlots.Select(async (slot,index) => {
      if(slot.ReadState == ReadState.Read && slot.MaybeMeta is MetaData meta) {
        return new SaveSlotData(
          SlotIndex: index,
          GameVersion: $"ゲームバージョン：{meta.GameVersion}",
          NowScenario: $"シナリオ：{meta.NowScenario?.Value}",
          PlayCountry: $"プレイ国名：{meta.PlayCountry?.ToString() ?? "(選択前)"}",
          PlayTurn: $"ターン数：{meta.PlayTurn?.ToString() ?? "(開始前)"}",
          PlayTime: $"プレイ時間：{Math.Floor(meta.TotalPlayTime.TotalMinutes)}分",
          LastSaveDate: $"最終保存：{meta.LastSaveDate:yyyy/MM/dd HH:mm}",
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
          HasSaveData: await HasSaveData(IndexToFileNo(index)) ? "セーブデータがありますがここに表示されるメタデータがありません\n(再保存でメタデータが付加されます)" : "セーブデータなし"
        );
      }
    }).ToList().ForEach(async v=>saveSlotTexts.Add(await v));
  }
  private static int IndexToFileNo(int index) => index + 1;
  internal static async Task Show(SaveAndLoad page,bool isWrite,Func<ReadGame?,Task> afterProcess,Action closeProcess,Windows.Foundation.Size parentSize) {
    isWritemode = isWrite;
    pressSlotAfterProcess = afterProcess;
    pressCloseProcess = closeProcess;
    await RefreshSaveSlotView();
    page.Title.Text = isWrite ? "セーブデータ選択" : "ロードデータ選択";
    page.ResizeElem(parentSize);
  }
  internal void ResizeElem(Windows.Foundation.Size size) {
    double scaleFactor = CookScaleFactor(UIUtil.GetScaleFactor(size with { Height = 0 }));
    double contentWidth = RenderSize.Width / scaleFactor;
    double contentHeight = RenderSize.Height / scaleFactor;
    Content.RenderTransform = new ScaleTransform { ScaleX = scaleFactor,ScaleY = scaleFactor };
    Content.Margin = new(0,0,contentWidth * (scaleFactor - 1),contentHeight * (scaleFactor - 1));
    double CookScaleFactor(double scaleFactor) => scaleFactor switch { < minScaleFactor => scaleFactor / minScaleFactor, > 1 => scaleFactor, _ => 1 };
    }
  private static async Task PressSaveSlot(SaveSlotData slotData) {
    await (isWritemode ? WriteSlot() : ReadSlot());
    async Task WriteSlot(){
      await WriteStorageData(GameData.game,GameData.startingPlayTotalTime,IndexToFileNo(slotData.SlotIndex));
      await RefreshSaveSlotView();
      await pressSlotAfterProcess(null);
    }
    async Task ReadSlot(){
      GameData.startingPlayTotalTime = saveSlots.ElementAtOrDefault(slotData.SlotIndex)?.MaybeMeta?.TotalPlayTime ?? TimeSpan.Zero;
      await pressSlotAfterProcess(await ReadStorageData(IndexToFileNo(slotData.SlotIndex)));
    }
  }
  private async void SaveSlot_PointerPressed(object sender,PointerRoutedEventArgs e) {
    await (sender is StackPanel panel && panel.DataContext is SaveSlotData slotData ? PressSaveSlot(slotData) : Task.CompletedTask);
  }
}