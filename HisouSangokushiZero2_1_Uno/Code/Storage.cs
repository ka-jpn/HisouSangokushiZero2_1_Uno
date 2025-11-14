using HisouSangokushiZero2_1_Uno.MyUtil;
using MessagePack;
using System;
using System.IO;
using Windows.Storage;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Storage {
  internal enum ReadSaveFile { NotFind, Read };
  private static string CreateSaveFileName(int fileNo) => $"save{fileNo}.dat";
  private static async Task<StorageFolder> GetStorageFolder() {
    return await ApplicationData.Current.LocalFolder.CreateFolderAsync(BaseData.name.Value,CreationCollisionOption.OpenIfExists);
  }
  internal static async Task WriteStorageData(GameState game,int fileNo) {
    //string writePath = CreateSaveFileName(fileNo);
    string writePath = (await GetStorageFolder()).MyApplyF(myFolder => Path.Combine(myFolder.Path,CreateSaveFileName(fileNo)));
    File.WriteAllBytes(writePath,MessagePackSerializer.Serialize(game));
  }
  internal static async Task<(ReadSaveFile,GameState?)> ReadStorageData(int fileNo) {
    //string readPath = CreateSaveFileName(fileNo);
    string readPath = (await GetStorageFolder()).MyApplyF(myFolder => Path.Combine(myFolder.Path,CreateSaveFileName(fileNo)));
    return !File.Exists(readPath) ? (ReadSaveFile.NotFind, null) : (ReadSaveFile.Read,MessagePackSerializer.Deserialize<GameState?>(File.ReadAllBytes(readPath)));
  }
}