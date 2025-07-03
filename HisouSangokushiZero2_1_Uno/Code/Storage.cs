using HisouSangokushiZero2_1_Uno.MyUtil;
using MessagePack;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Storage {
  private static readonly string saveFileName = "save.dat";
  private static StorageFolder? myFolder = null;
  private static async Task<StorageFolder> GetStorageFolder() {
    return await ApplicationData.Current.LocalFolder.CreateFolderAsync(BaseData.name.Value,CreationCollisionOption.OpenIfExists);
  }
  internal static async void WriteStorageData(GameState game) {
    myFolder ??= await GetStorageFolder();
    _ = Task.Run(() => myFolder?.MyApplyA(myFolder => File.WriteAllBytes(Path.Combine(myFolder.Path,saveFileName),MessagePackSerializer.Serialize(game))));
  }
  internal static async Task<GameState?> ReadStorageData() {
    myFolder ??= await GetStorageFolder();
    return myFolder?.MyApplyF(myFolder => File.Exists(Path.Combine(myFolder.Path,saveFileName)) ? File.ReadAllBytes(Path.Combine(myFolder.Path,saveFileName)).MyApplyF(v => MessagePackSerializer.Deserialize<GameState?>(v)) : null);
  }
}