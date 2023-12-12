using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class ToolkitTool : EditorWindow
{
    private static string uxmlPath = Path.Combine("Assets","Plugins", "ToolkitTool","Editor","ToolkitTool.uxml");
    [MenuItem("Window/UI Toolkit/ToolkitTool _F1")]
    public static void ShowExample()
    {
        ToolkitTool wnd = GetWindow<ToolkitTool>();
        wnd.titleContent = new GUIContent("ToolkitTool");
    }

    const string Theme = "Theme";
    const string CarLight = "CarLight";
    
    public TextField content;

    public static ToolkitTool Instance;
    public ListView listView;
    
    public TextField searchText;

    public Button excute;

    public MinMaxSlider timeSlider;

    public Dictionary<string, FuncItem> parseDic = new Dictionary<string, FuncItem>();

    public FuncItem FuncItemNow;
    
    // 功能项目
    public class FuncItem
    {
        public string name;
        public string searchTxt;
        public Func<string, TimexData> callback;

        public FuncItem(string name, string searchTxt, Func<string, TimexData> callback)
        {
            this.name = name;
            this.searchTxt = searchTxt;
            this.callback = callback;
        }
    }

    public void CreateGUI()
    {
        Instance = this;
        
        parseDic.Add(Theme, new FuncItem(Theme, "setTheme:", GetThemeTimexData));
        parseDic.Add(CarLight, new FuncItem(CarLight, "cyj Do CarLight:", GetCarLightTimexData));
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
        visualTree.CloneTree(root);

        content = root.Q<TextField>("content");
        searchText = root.Q<TextField>("searchText");
        if (searchText != null)
        {
            searchText.RegisterCallback<ChangeEvent<string>>(OnTextChanged);
        }
        listView = root.Q<ListView>();
        Debug.Log(listView.name);
        listView.makeItem = () =>
        {
            var ve =  new UTCarLightItem();
            //var ve = ccItem.CloneTree();
            //ve.RegisterCallback<MouseDownEvent>((evt => Debug.Log("xx")));
            return ve;
        };

        var btn = root.Q<Button>("Theme");
        btn.clicked += () => { ChangeMode(Theme); };
        btnList.Add(btn);
        btn = root.Q<Button>("CarLight");
        btn.clicked += () => { ChangeMode(CarLight);  };
        btnList.Add(btn);
        excute = root.Q<Button>("excute");
        if (excute != null)
        {
            excute.clicked += () =>
            {
                if (UnitaskKitComponent.Instance == null)
                {
                    var go = new GameObject("UnitaskKitComponent");
                    UnitaskKitComponent.Instance = GetOrAddComponent<UnitaskKitComponent>(go);
                }
                
                List<TimexData> list = new List<TimexData>();
                int startIndex = 0;
                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].time > minV && datas[i].time < maxV)
                    {
                        list.Add(datas[i]);
                    }
                }
                TimexDataAll data = new TimexDataAll();
                data.dataArray = list.ToArray();
                UnitaskKitComponent.Instance.AddTrigger(Theme, JsonUtility.ToJson(data));
            };
        }

        timeSlider = root.Q<MinMaxSlider>("timeSlider");
        
        timeSlider.RegisterValueChangedCallback(evt =>
        {
            minV = MapValueToCount(evt.newValue.x, minTime, maxTime);
            maxV = MapValueToCount(evt.newValue.y, minTime, maxTime);
        
            InnerRefresh();
            //Debug.Log(evt.newValue.x +  " " + evt.newValue.y + " ");
        });
        ChangeMode(Theme);
    }

    void ChangeMode(string mode)
    {
        if (!parseDic.ContainsKey(mode))
        {
            return;
        }
        FuncItemNow = parseDic[mode];
        
        for (int i = 0; i < btnList.Count; i++)
        {
            var temp = "FF8555";
            if (btnList[i].name == mode)
            {
                temp = "76FF56";
            }
            int intValue = Convert.ToInt32(temp, 16);
            btnList[i].style.backgroundColor = IntToColor(intValue);
        }

        searchText.value = FuncItemNow.searchTxt;
        RefreshData();
    }
    
    Color IntToColor(int intValue)
    {
        // 将整数按照RGB分量拆分，并归一化到范围[0, 1]
        float r = ((intValue >> 16) & 0xFF) / 255.0f;
        float g = ((intValue >> 8) & 0xFF) / 255.0f;
        float b = (intValue & 0xFF) / 255.0f;

        // 创建Color对象
        return new Color(r, g, b);
    }

    Color GetColor(string hexString)
    {
        if (ColorUtility.TryParseHtmlString(hexString, out Color color))
        {
            return color;
        }
        return Color.white;
    }
 
    public static int MapValueToCount(float desiredValue,int originalMin, int originalMax)
    {
        float length = originalMax - originalMin;
        float desireLength = 50;
        float per = length * 1f / 50;
        float more = desiredValue + 10;
        // long desiredMin = -10;
        // long desiredMax = 40;
    
        return (int)(more * per + originalMin);
    }
    public static long MapValueToTime(float desiredValue,long originalMin, long originalMax)
    {
        long length = originalMax - originalMin;
        long desireLength = 50;
        long per = length / 50;
        long more = (long)desiredValue + 10;
        // long desiredMin = -10;
        // long desiredMax = 40;
    
        return more * per + originalMin;
    }

    [Serializable]
    public class  TimexDataAll
    {
        public TimexData[] dataArray;
    }
    
    [Serializable]
    public class TimexData
    {
        public long time;
        public string vv;
        public bool on;

        public TimexData()
        {
            
        }
        
        public TimexData(long time, string vv, bool on)
        {
            this.time = time;
            this.vv = vv;
            this.on = on;
        }
    }

    private void OnTextChanged(ChangeEvent<string> evt)
    {
        searchText.value = evt.newValue;
        RefreshData();
    }

    List<string> sourceList = new List<string>();
    List<string> filterList = new List<string>();
    private int minTime;
    private int maxTime;
    private int minV;
    private int maxV;
    private TimexData[] datas;
    private List<Button> btnList = new List<Button>();

    public void SetContent(string contentStr)
    {
        //Debug.Log(contentStr);
        sourceList = contentStr.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();
        searchText.value = FuncItemNow.searchTxt;
        // var array = sourceList.FindAll(s => s.Contains("setTheme:"));
        // content.value = String.Join("\n", array);
        RefreshData();
    }

    // 刷新数据
    void RefreshData()
    {
        if (string.IsNullOrEmpty(searchText.value))
        {
            filterList = sourceList;
        }
        else
        {
            filterList = sourceList.FindAll(s => s.Contains(searchText.value));
        }
        // var temp = array.ConvertAll<string>(s =>
        // {
        //     int first = s.IndexOf(" ");
        //     int second = s.IndexOf(" ",first + 1 );
        //     return s.Substring(first, second - first);
        // });
        timeSlider.value = new Vector2(-10, 40);
        SetData(filterList);
        InnerRefresh();
    }

    void InnerRefresh()
    {
        if (datas == null)
        {
            return;
        }
        var array = filterList;
        List<TimexData> list = new List<TimexData>();
        List<string> array2 = new List<string>();
        int startIndex = 0;
        for (int i = 0; i < array.Count; i++)
        {
            if (i > minV && i < maxV)
            {
                list.Add(datas[i]);
                array2.Add(array[i]);
            }
        }
        listView.itemsSource = list;
        listView.bindItem = (element, i) =>
        {
            if (i < list.Count)
            {
                (element as UTCarLightItem).SetContent(array2[i], list[i]); 
            }
        };
    }

    // 把字符串转成固定数据
    void SetData(List<string> contents)
    {
        if (!parseDic.ContainsKey(FuncItemNow.name) || contents.Count < 1)
        {
            return;
        }
        datas = new TimexData[contents.Count];
        for (int i = 0; i < contents.Count; i++)
        {
            datas[i] = FuncItemNow.callback.Invoke(contents[i]);
        }
        //Array.Sort(datas);
        Array.Sort(datas, (item1, item2) => item1.time.CompareTo(item2.time));
        //minTime = datas[0].time - 100;
        minTime = 0;
        minV = 0;
        //maxTime = datas[datas.Length - 1].time + 100;
        maxTime = datas.Length;
        maxV = datas.Length;
    }

    TimexData GetThemeTimexData(string content)
    {
        int first = content.IndexOf(" ");
        int second = content.IndexOf(" ",first + 1 );

        TimexData data = new TimexData();
        data.time = GetTimestamp2("2023-" + content.Substring(0, first) + content.Substring(first, second - first));
         
        first = content.IndexOf("setTheme:") + "setTheme:".Length;
        second = content.IndexOf("value:");
        string vv = content.Substring(first , second - first);
        data.vv = vv.Trim();

        first = content.IndexOf("value:") + "value:".Length + 1;
        vv = content.Substring(first , 1);
        data.on = vv.Trim() == "1";
        return data;
    }

    TimexData GetCarLightTimexData(string content)
    {
        int first = content.IndexOf(" ");
        int second = content.IndexOf(" ",first + 1 );

        TimexData data = new TimexData();
        data.time = GetTimestamp2("2023-" + content.Substring(0, first) + content.Substring(first, second - first));
         
        first = content.IndexOf("cyj Do CarLight:") + "cyj Do CarLight:".Length;
        second = content.IndexOf(":", first + 1);
         
        string vv = content.Substring(first , second - first);
        data.vv = vv;
        first = content.IndexOf(":", first + 1);
        var spaceIndex =  content.IndexOf(" ", first + 1);
        vv = content.Substring(first , spaceIndex - first);
        data.on = vv.Trim().ToLower() == "true";
        return data;
    }

    static string formattt = "yyyy-MM-dd HH:mm:ss.ffffff";

    public static long GetTimestamp(string dateString)
    {
        DateTime dateTime = DateTime.ParseExact(dateString, formattt, null);

        // 将DateTime转换为UTC时间（可选）
        DateTime utcDateTime = dateTime.ToUniversalTime();

        // 创建一个已知的日期（例如，1970年1月1日）
        DateTime baseDate = new DateTime(1970, 1, 1);

        // 获取毫秒时间戳
        long timestampInMilliseconds = (long)(utcDateTime - baseDate).TotalMilliseconds;


        return timestampInMilliseconds;
        // // 转换为微秒级时间戳
        // if (DateTime.TryParseExact(timeString.Trim(), formattt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
        // {
        //     // 解析成功
        //     Console.WriteLine(dateTime);
        //
        //     // 转换为毫秒级时间戳
        //     //long timestampMilliseconds = (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        //     long timestampMilliseconds = (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        //
        //
        //     Console.WriteLine("毫秒级时间戳：" + timestampMilliseconds);
        //
        //     // 转换为秒级时间戳
        //     // long timestampSeconds = (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        //     // Console.WriteLine("秒级时间戳：" + timestampSeconds);
        //     return timestampMilliseconds;
        // }
        // // 解析失败
        Console.WriteLine("时间格式不匹配");
        return 0;
    }

    public static string GetTimeStr(long timestampMilliseconds)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestampMilliseconds);

        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.ffffff");
        return formattedTime;
    }

    public static long GetTimestamp2(string dateTimeString)
    {
        // 指定日期时间格式
        string format = "yyyy-MM-dd HH:mm:ss.ffffff";

        // 解析字符串为DateTime对象
        DateTime dateTime = DateTime.ParseExact(dateTimeString, format, null);

        // 将DateTime转换为UTC时间（可选）
        DateTime utcDateTime = dateTime.ToUniversalTime();

        // 创建一个已知的日期（例如，1970年1月1日）
        DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // 获取毫秒时间戳
        long timestampInMilliseconds = (long)(utcDateTime - baseDate).TotalMilliseconds;

        // 打印结果
        Console.WriteLine($"原始日期时间字符串: {dateTimeString}");
        Console.WriteLine($"毫秒时间戳: {timestampInMilliseconds}");
        return timestampInMilliseconds;
    }
    
    public static T GetOrAddComponent<T>(GameObject node) where T : Component
    {
        T script = node.GetComponent<T>();
        if (script == null)
        {
            script = node.AddComponent<T>();
        }
        return script;
    }
}