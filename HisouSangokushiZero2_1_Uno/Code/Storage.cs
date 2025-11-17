using HisouSangokushiZero2_1_Uno.MyUtil;
using MessagePack;
using System;
using System.IO;
using Windows.Storage;
using static HisouSangokushiZero2_1_Uno.Code.DefType;
namespace HisouSangokushiZero2_1_Uno.Code;

internal static class Storage {
  internal enum ReadSaveFile { NotFind, Read };
  private static string CreateMetaFileName(int fileNo) => $"save{fileNo}.meta";
  private static string CreateSaveFileName(int fileNo) => $"save{fileNo}.dat";
  private static string GetOldStorageFolderPath() => ApplicationData.Current.LocalFolder.Path.MyApplyF(v => Path.Combine(v,BaseData.name.Value));
  private static string GetNewStorageFolderPath() => ApplicationData.Current.LocalFolder.Path;
  internal static void WriteStorageData(GameState game,TimeSpan startingPlayTotalTime,int fileNo) {
    string metaPath = GetNewStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateMetaFileName(fileNo)));
    string writePath = GetNewStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateSaveFileName(fileNo)));
    File.WriteAllBytes(metaPath,MessagePackSerializer.Serialize(new MetaData(game.NowScenario,game.PlayCountry,game.PlayTurn,DateTime.Now - GameData.startGameDateTime + startingPlayTotalTime,DateTime.Now + new TimeSpan(9,0,0),fileNo,BaseData.version.Value)));
    File.WriteAllBytes(writePath,MessagePackSerializer.Serialize(game));
  }
  internal static (ReadSaveFile, GameState?) ReadStorageData(int fileNo) {
    string newReadPath = GetNewStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateSaveFileName(fileNo)));
    string oldReadPath = GetOldStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateSaveFileName(fileNo)));
    return File.Exists(newReadPath) ? (ReadSaveFile.Read, MessagePackSerializer.Deserialize<GameState?>(File.ReadAllBytes(newReadPath))) :
      File.Exists(oldReadPath) ? (ReadSaveFile.Read, MessagePackSerializer.Deserialize<GameState?>(File.ReadAllBytes(oldReadPath))) : (ReadSaveFile.NotFind, null);
  }
  internal static (ReadSaveFile, MetaData?) ReadMetaData(int fileNo) {
    string newMetaPath = GetNewStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateMetaFileName(fileNo)));
    return File.Exists(newMetaPath) ? (ReadSaveFile.Read, MessagePackSerializer.Deserialize<MetaData?>(File.ReadAllBytes(newMetaPath))) : (ReadSaveFile.NotFind, null);
  }
  internal static void DeleteStorageData(int fileNo) {
    string oldWritePath = GetOldStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateSaveFileName(fileNo)));
    string metaPath = GetNewStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateMetaFileName(fileNo)));
    string writePath = GetNewStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateSaveFileName(fileNo)));
    if(File.Exists(oldWritePath)) { File.Delete(oldWritePath); }
    if(File.Exists(metaPath)) { File.Delete(metaPath); }
    if(File.Exists(writePath)) { File.Delete(writePath); }
  }
  internal static bool HasSaveData(int fileNo) {
    string newPath = GetNewStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateSaveFileName(fileNo)));
    string oldPath = GetOldStorageFolderPath().MyApplyF(myFolder => Path.Combine(myFolder,CreateSaveFileName(fileNo)));
    return File.Exists(newPath) || File.Exists(oldPath);
  }
}