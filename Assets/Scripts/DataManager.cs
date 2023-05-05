using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private TMP_Text textField;
    
    [Header("Parameters")]
    [SerializeField] private List<SomeData> someDatas;
    
    [Header("State")]
    [SerializeField] private List<string> filePaths;
    [SerializeField] private string archivePath;

    private void Awake()
    {
        Application.logMessageReceived += LogReceived;
        var path = Application.persistentDataPath;
    }

    private async void Start()
    {
        filePaths = DataHelper.CreateFiles(Application.persistentDataPath, someDatas);
        filePaths.ForEach(filePath => LogReceived(filePath));
        archivePath = await DataHelper.CreateArchive(Application.persistentDataPath, filePaths, "1234");
        LogReceived(archivePath);
    }
    
    private void LogReceived(string message, string stackTrace = null, LogType logType = LogType.Log)
    {
        textField.text += $"\n---\n{message}";
        if (!string.IsNullOrEmpty(stackTrace) && logType != LogType.Log)
            textField.text += $"\n{stackTrace}";
    }
}
