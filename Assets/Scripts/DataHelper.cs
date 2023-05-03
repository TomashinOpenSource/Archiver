using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        return directoryPath;
    }
}
