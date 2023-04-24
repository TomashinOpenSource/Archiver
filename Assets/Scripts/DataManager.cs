using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;

public class DataManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private string password;
    [SerializeField] private List<SomeData> someDatas;

    private void Start()
    {
        Save();
    }

    private void Save()
    {
        string zipPath = Application.persistentDataPath;
        string folderPath = zipPath + "/Test";
        for (int i = 0; i < someDatas.Count; i++)
        {
            Write(folderPath, i, JsonUtility.ToJson(someDatas[i]));
        }
        
        Debug.Log(zipPath);
    }

    private void Write(string folderPath, int number, string data)
    {
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        File.WriteAllText(folderPath + $"/{number}.json", data);
    }

    public void CreateArchive()
    {
        string path = Application.persistentDataPath;
        string zipPath = Path.Combine(path, "test.zip");
        using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
        {
            using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                for (int i = 0; i < someDatas.Count; i++)
                {
                    string filePath = CreateFile(i, JsonUtility.ToJson(someDatas[i]), path);
                    ZipArchiveEntry entry = zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                    //entry.Enc
                }

                //zipArchive.Password = password;
            }
        }
    }

    private string CreateFile(int number, string data, string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        path = Path.Combine(path, $"{number}.json");
        File.WriteAllText(path, data);
        return path;
    }
}
