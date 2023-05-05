using System;
using System.IO;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

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

    public static string CreateArchive(string directoryPath, List<string> filePaths, string password)
    {
        var archiveName = "archive.zip";
        var archivePath = Path.Combine(directoryPath, archiveName);

        lzip.compress_File(5, archivePath, filePaths[0], append:true, password: password);
        lzip.compress_File(5, archivePath, filePaths[1], true);
        lzip.compress_File(5, archivePath, filePaths[2], append:true, password: password);
        
        return archivePath;
    }
}
