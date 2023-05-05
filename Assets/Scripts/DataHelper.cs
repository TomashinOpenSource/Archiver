using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
//using SevenZip;
using UnityEngine;
using UnityEngine.Networking;

public static class DataHelper
{
    public static List<string> CreateFiles<T>(string savePath, List<T> datas)
    {
        var paths = new List<string>();
        for (int i = 0; i < datas.Count; i++)
        {
            var path = Path.Combine(savePath, $"{i}.json");
            File.WriteAllText(path, JsonUtility.ToJson(datas[i]));
            paths.Add(path);
        }
        return paths;
    }

    public static async UniTask<string> CreateArchive(string directoryPath, List<string> filePaths, string password)
    {
        var archiveName = "archive.zip";
        var archivePath = Path.Combine(directoryPath, archiveName);

        lzip.compress_File(5, archivePath, filePaths[0], append:true, password: password);
        lzip.compress_File(5, archivePath, filePaths[1], true);
        lzip.compress_File(5, archivePath, filePaths[2], append:true, password: password);
        
        return archivePath;
    }

    public static async UniTask<string> CreateArchiveSevenZip(string directoryPath, List<string> filePaths, string password)
    {
        var archiveName = "archive.zip";
        var archivePath = Path.Combine(directoryPath, archiveName);
/*
        var dllPath = await LoadZipDLL();
        //var dllPath = Resources.Load("7z.dll");
        SevenZipBase.SetLibraryPath(dllPath);
        Debug.Log("SetLibraryPath DONE");
        var zipCompressor = new SevenZipCompressor()
        {
            ArchiveFormat = OutArchiveFormat.Zip,
            CompressionLevel = SevenZip.CompressionLevel.Normal,
            CompressionMethod = CompressionMethod.Deflate,
            //ZipEncryptionMethod = ZipEncryptionMethod.Aes256,
            //CompressionMode = !File.Exists(archivePath) ? CompressionMode.Create : CompressionMode.Append,
            CompressionMode = CompressionMode.Create,
            TempFolderPath = Path.GetTempPath()
        };
        Debug.Log("new SevenZipCompressor DONE");
        
        //await zipCompressor.CompressFilesEncryptedAsync(archivePath, password, filePaths.ToArray());
        zipCompressor.CompressFiles(archivePath, filePaths.ToArray());
        Debug.Log("CompressFiles DONE");
*/
        return archivePath;
    }

    public static async UniTask<string> LoadZipDLL()
    {
        var dllName = "7z.dll";
        var dllPath = Path.Combine(Application.persistentDataPath, dllName);
        if (!File.Exists(dllPath))
        {
            var loadPath = Path.Combine(Application.streamingAssetsPath, dllName);
            if (Application.platform == RuntimePlatform.Android)
            {
                var request = UnityWebRequest.Get(loadPath);
                await request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log($"ERROR - Failed to load file: {request.error}");
                    return null;
                }
                await File.WriteAllBytesAsync(dllPath, request.downloadHandler.data);
            }
            else
            {
                File.Copy(loadPath, dllPath);
            }
        }
        Debug.Log($"Success - LoadZipDLL - {dllPath}");
        return dllPath;
    }
}
