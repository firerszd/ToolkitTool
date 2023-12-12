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

public class UTCarLightItem : VisualElement
{
    private static string uxmlPath = Path.Combine("Assets","Plugins", "ToolkitTool","Editor","CustomTool","UTCarLightItem.uxml");
    private VisualElement root;
  
    public EnumField carType;
    public Toggle check;
    public Toggle scene;
    public TextField content;
    public TextField content2;
 

    public class Factory : UxmlFactory<UTCarLightItem, Train>
    {
    }
    
    public class Train:UxmlTraits
    {
        private UxmlStringAttributeDescription mDescription = new UxmlStringAttributeDescription(){name = "天上天下"};
        private UxmlIntAttributeDescription HPValue = new UxmlIntAttributeDescription(){name = "血量"};

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            UTCarLightItem item = ve as UTCarLightItem;
        }
    }

    public UTCarLightItem()
    {
        var ccItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        ccItem.CloneTree(this);
        Init();
    }

    void Init()
    {
        root = this.Q<VisualElement>("root");
        carType = root.Q<EnumField>("CarType");
        //carType.Init(CarLightState.Default, false);
        check = root.Q<Toggle>("Check");
        scene = root.Q<Toggle>("Scene");
        content = root.Q<TextField>("Content");
        content2 = root.Q<TextField>("Content2");
    }

    public void SetContent(string word, ToolkitTool.TimexData data)
    {
        
        content.value = word;
        int first = word.IndexOf(" ");
        int second = word.IndexOf(" ",first + 1 );
        
        content2.value = data.time.ToString();
        // scene.value = data._enum == 1;
        // check.value = data.on;

        // first = word.IndexOf("setTheme:") + "setTheme:".Length;
        // second = word.IndexOf("value:");
        // string vv = word.Substring(first , second - first);
        // if (vv.Trim() == "1")
        // {
        //     scene.value = true;
        // }
        // else
        // {
        //     scene.value = false;
        // }

        // first = word.IndexOf("value:") + "value:".Length + 1;
        // vv = word.Substring(first , 1);
        // if (vv.Trim() == "1")
        // {
        //     check.value = true;
        // }
        // else
        // {
        //     check.value = false;
        // }
        // long me =  ToolkitTool.GetTimestamp(word);
        // content2.value = ToolkitTool.GetTimeStr(me);
    }
 
}
