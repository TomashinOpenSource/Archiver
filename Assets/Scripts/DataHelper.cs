using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ICSharpCode.SharpZipLib.Core;
//using ICSharpCode.SharpZipLib.Zip;
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
    /*
    public static void CreateZipArchiveWithPasswords(string archiveName, string[] files, string[] passwords)
    {
        using (ZipOutputStream zipStream = new ZipOutputStream(File.Create(archiveName)))
        {
            for (int i = 0; i < files.Length; i++)
            {
                ZipEntry entry = new ZipEntry(Path.GetFileName(files[i]));
                entry.CompressionMethod = CompressionMethod.Deflated;

                // Установка пароля для текущего файла
                entry.AESKeySize = 256;
                entry.Encryption = EncryptionAlgorithm.WinZipAes256;
                entry.CompressionMethod = CompressionMethod.Deflate;
                entry.CompressionLevel = CompressionLevel.BestCompression;
                entry.SetPassword("mypassword");

                zipStream.PutNextEntry(entry);
                byte[] buffer = new byte[4096];
                using (FileStream streamReader = File.OpenRead(files[i]))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            zipStream.Finish();
        }
    }
    */
    public static void CreateZipWithDifferentPasswords(string[] filesToArchive, string password)
    {
        //string[] filesToArchive = new string[] { "file1.txt", "file2.txt", "file3.txt" };
        //string[] passwords = new string[] { "password1", "password2", "password3" };
    
        using (ZipFile zip = new ZipFile())
        {
            for (int i = 0; i < filesToArchive.Length; i++)
            {
                string file = filesToArchive[i];
                //string password = passwords[i];
    
                // Добавляем файл в архив и устанавливаем ему пароль
                zip.AddFile(file);
                ZipEntry entry = zip[file];
                entry.Password = password;
            }
    
            // Сохраняем архив в файл
            zip.Save(Path.Combine(Application.persistentDataPath, "archive.zip"));
        }
    }
}
