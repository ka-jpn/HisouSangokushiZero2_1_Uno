using HisouSangokushiZero2_1_Uno.MyUtil;
using MessagePack;
using System;
using System.IO;
using System.Threading.Tasks;
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
    (await GetStorageFolder()).MyApplyA(myFolder => File.WriteAllBytes(Path.Combine(myFolder.Path,CreateSaveFileName(fileNo)),MessagePackSerializer.Serialize(game)));
  }
  internal static async Task<(ReadSaveFile,GameState?)> ReadStorageData(int fileNo) {
    return (await GetStorageFolder()).MyApplyF(myFolder => Path.Combine(myFolder.Path,CreateSaveFileName(fileNo)).MyApplyF(readPath => !File.Exists(readPath) ? (ReadSaveFile.NotFind, null) : File.ReadAllBytes(readPath).MyApplyF(v => (ReadSaveFile.Read,MessagePackSerializer.Deserialize<GameState?>(v)))));
  }
}