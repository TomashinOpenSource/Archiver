using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private List<SomeData> someDatas;
    
    [Header("State")]
    [SerializeField] private List<string> filePaths;

    private void Start()
    {
        CreateFiles();
    }

    public void CreateFiles()
    {
        filePaths = DataHelper.CreateFiles(Application.persistentDataPath, someDatas);
    }
}
