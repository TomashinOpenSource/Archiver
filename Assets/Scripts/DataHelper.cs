using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using SevenZip;
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

        SevenZipBase.SetLibraryPath(Path.Combine(Application.streamingAssetsPath, "7z.dll"));

        var zipCompressor = new SevenZipCompressor()
        {
            ArchiveFormat = OutArchiveFormat.Zip,
            CompressionLevel = SevenZip.CompressionLevel.Normal,
            CompressionMethod = CompressionMethod.Deflate,
            ZipEncryptionMethod = ZipEncryptionMethod.ZipCrypto,
            CompressionMode = CompressionMode.Append,
            TempFolderPath = Path.GetTempPath()
        };
        
        zipCompressor.CompressFilesEncrypted(archivePath, password, filePaths.ToArray());

        return archivePath;
    }
}
