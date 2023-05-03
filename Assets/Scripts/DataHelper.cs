using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Ionic.Zip;

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
        string archiveName = "archive.zip";
        string archivePath = Path.Combine(directoryPath, archiveName);

        using (var zipFile = new ZipFile(archivePath))
        {
            for (int i = 0; i < filePaths.Count; i++)
            {
                var zipEntry = zipFile.AddFile(filePaths[i], @"\");
                zipEntry.Password = password;
            }
            zipFile.Save();
        }
        return archivePath;
    }
}
