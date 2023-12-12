using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class UTReadFile : VisualElement
{
    private static string uxmlPath = Path.Combine("Assets","Plugins", "ToolkitTool","Editor","CustomTool","UTReadFile.uxml");
    private VisualElement root;
 
    public Button openBtn;
    public TextField textField;
   
 

    public class Factory : UxmlFactory<UTReadFile, Train>
    {
    }
    
    public class Train:UxmlTraits
    {
        private UxmlStringAttributeDescription mDescription = new UxmlStringAttributeDescription(){name = "天上天下"};
        private UxmlIntAttributeDescription HPValue = new UxmlIntAttributeDescription(){name = "血量"};

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            UTReadFile item = ve as UTReadFile;
        }
    }

    public UTReadFile()
    {
        var ccItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        ccItem.CloneTree(this);
        Init();
    }

    void Init()
    {
        root = this.Q<VisualElement>("root");
        openBtn = this.Q<Button>("open");
        openBtn.clicked += () =>
        {
            textField.value = OpenFolder();
            ToolkitTool.Instance.SetContent(ReadFromFile(textField.value));
        };
        textField = this.Q<TextField>("pathText");
    }

    public string OpenFolder()
    {
        string filePath = EditorUtility.OpenFilePanel("Select a Log", "", ""); // 打开文件夹对话框
        if (!string.IsNullOrEmpty(filePath))
        {
            Debug.Log("Selected filePath: " + filePath); // 输出所选文件夹路径
        }
        return filePath;
    }
    
    public string ReadFromFile(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            return reader.ReadToEnd();
        }
    }
}
