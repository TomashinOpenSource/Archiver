using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using SevenZip;
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

    public static async Task<string> CreateArchive(string directoryPath, List<string> filePaths, string password)
    {
        var archiveName = "archive.zip";
        var archivePath = Path.Combine(directoryPath, archiveName);

        var dllName = "7z.dll";
        var dllPath = Path.Combine(Application.streamingAssetsPath, dllName);
#if UNITY_ANDROID && !UNITY_EDITOR
        // Преобразуем путь в формат, поддерживаемый Android API
        dllPath = "jar:file://" + Application.dataPath + "!/assets/" + dllName;
#endif
// Используем WWW для загрузки файла
        using (var request = UnityWebRequest.Get(dllPath))
        {
            // Отправляем запрос и ждем завершения
            await request.SendWebRequest();

            // Если запрос завершился с ошибкой, то выводим сообщение об ошибке и выходим
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load file: {request.error}");
                return null;
            }

            // Получаем данные из ответа
            dllPath = Path.Combine(Application.temporaryCachePath, dllName);
            System.IO.File.WriteAllBytes(dllPath, request.downloadHandler.data);

        }
// Теперь содержимое файла можно получить из www.text или www.bytes

        SevenZipBase.SetLibraryPath(dllPath);

        var zipCompressor = new SevenZipCompressor()
        {
            ArchiveFormat = OutArchiveFormat.Zip,
            CompressionLevel = SevenZip.CompressionLevel.Normal,
            CompressionMethod = CompressionMethod.Deflate,
            ZipEncryptionMethod = ZipEncryptionMethod.ZipCrypto,
            CompressionMode = !File.Exists(archivePath) ? CompressionMode.Create : CompressionMode.Append,
            TempFolderPath = Path.GetTempPath()
        };
        
        zipCompressor.CompressFilesEncrypted(archivePath, password, filePaths.ToArray());

        return archivePath;
    }
}
