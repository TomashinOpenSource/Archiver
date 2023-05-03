using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private List<SomeData> someDatas;
    
    [Header("State")]
    [SerializeField] private List<string> filePaths;
    [SerializeField] private string archivePath;

    private void Start()
    {
        filePaths = DataHelper.CreateFiles(Application.persistentDataPath, someDatas);
        archivePath = DataHelper.CreateArchive(Application.persistentDataPath, filePaths, "1234");
    }
}
