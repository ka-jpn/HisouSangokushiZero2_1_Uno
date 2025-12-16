using HisouSangokushiZero2_1_Uno.Data;
using HisouSangokushiZero2_1_Uno.MyUtil;
using MessagePack;
using System;
using Windows.Storage;
using Windows.Storage.Streams;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;
internal static class Storage {
  internal enum ReadState { NotFind, Read };
  internal record ReadGame(ReadState ReadState,GameState? MaybeGame);
  internal record ReadMeta(ReadState ReadState,MetaData? MaybeMeta);
  private static string CreateMetaFileName(int fileNo) => $"save{fileNo}.meta";
  private static string CreateSaveFileName(int fileNo) => $"save{fileNo}.dat";
  private static async Task<StorageFile> CreateFileAsync(StorageFolder folder, string fileName) => await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
  private static async Task<StorageFile?> GetFileAsync(StorageFolder folder, string fileName) => await folder.TryGetItemAsync(fileName) as StorageFile;
  private static async Task<StorageFolder?> GetOldStorageFolder() => await ApplicationData.Current.LocalFolder.TryGetItemAsync(BaseData.name.Value) as StorageFolder;
  private static StorageFolder GetNewStorageFolder() => ApplicationData.Current.LocalFolder;
  private static async Task<byte[]> ReadAsync(StorageFile file) => await FileIO.ReadBufferAsync(file).AsTask().ContinueWith(t => new byte[t.Result.Length].MyApplyA(DataReader.FromBuffer(t.Result).ReadBytes));
  internal static async Task WriteStorageData(GameState game,TimeSpan startingPlayTotalTime,int fileNo) {
    StorageFile? metaFile = await GetNewStorageFolder().MyApplyF(myFolder => CreateFileAsync(myFolder,CreateMetaFileName(fileNo)));
    StorageFile? saveFile = await GetNewStorageFolder().MyApplyF(myFolder => CreateFileAsync(myFolder,CreateSaveFileName(fileNo)));
    TimeSpan totalPlayTime = DateTime.Now - GameData.startGameDateTime + startingPlayTotalTime;
    DateTime lastSaveTime = DateTime.Now + new TimeSpan(9, 0, 0);
    await FileIO.WriteBytesAsync(metaFile, MessagePackSerializer.Serialize(new MetaData(game.NowScenario,game.PlayCountry,game.PlayTurn,totalPlayTime,lastSaveTime,fileNo,BaseData.version.Value)));
    await FileIO.WriteBytesAsync(saveFile,MessagePackSerializer.Serialize(game));
  }
  internal static async Task<ReadGame> ReadStorageData(int fileNo) {
    StorageFile? newSaveFile = await GetNewStorageFolder().MyApplyF(myFolder => GetFileAsync(myFolder,CreateSaveFileName(fileNo)));
    StorageFile? oldSaveFile = await ((await GetOldStorageFolder())?.MyApplyF(myFolder => GetFileAsync(myFolder, CreateSaveFileName(fileNo))) ?? Task.FromResult<StorageFile?>(null));
    return newSaveFile is { } ? new(ReadState.Read, MessagePackSerializer.Deserialize<GameState?>(await ReadAsync(newSaveFile))?.MyApplyF(FillState)) :
      oldSaveFile is { } ? new(ReadState.Read, MessagePackSerializer.Deserialize<GameState?>(await ReadAsync(oldSaveFile))?.MyApplyF(FillState)) : new(ReadState.NotFind, null);
    GameState FillState(GameState game) => game with { LogMessage = game.LogMessage ?? [], StartPlanningCharacterRemark = game.StartPlanningCharacterRemark ?? [], StartExecutionCharacterRemark = game.StartExecutionCharacterRemark ?? [] };
  }
  internal static async Task<ReadMeta> ReadMetaData(int fileNo) {
    StorageFile? newMetaFile = await GetNewStorageFolder().MyApplyF(myFolder => GetFileAsync(myFolder,CreateMetaFileName(fileNo)));
    return newMetaFile is { } ? new(ReadState.Read, MessagePackSerializer.Deserialize<MetaData?>(await ReadAsync(newMetaFile))) : new(ReadState.NotFind, null);
  }
  internal static async Task DeleteStorageData(int fileNo) {
    StorageFile? oldWriteFile = await ((await GetOldStorageFolder())?.MyApplyF(myFolder => GetFileAsync(myFolder,CreateSaveFileName(fileNo))) ?? Task.FromResult<StorageFile?>(null));
    StorageFile? metaFile = await GetNewStorageFolder().MyApplyF(myFolder => GetFileAsync(myFolder,CreateMetaFileName(fileNo)));
    StorageFile? newSaveFile = await GetNewStorageFolder().MyApplyF(myFolder => GetFileAsync(myFolder,CreateSaveFileName(fileNo)));
    oldWriteFile?.DeleteAsync();
    metaFile?.DeleteAsync();
    newSaveFile?.DeleteAsync(); 
  }
  internal static async Task<bool> HasSaveData(int fileNo) {
    StorageFile? newSaveFile = await GetNewStorageFolder().MyApplyF(myFolder => GetFileAsync(myFolder,CreateSaveFileName(fileNo)));
    StorageFile? oldSaveFile = await ((await GetOldStorageFolder())?.MyApplyF(myFolder => GetFileAsync(myFolder,CreateSaveFileName(fileNo))) ?? Task.FromResult<StorageFile?>(null));
    return newSaveFile is { } || oldSaveFile is { };
  }
}