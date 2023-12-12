using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UnityEngine;

public class UnitaskKitComponent : MonoBehaviour
{
    public static UnitaskKitComponent Instance;

    public Dictionary<string, Action<int,bool>> UniCallbackDic;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        public int _enum;
        public bool on;

        public TimexData()
        {
            
        }
        
        public TimexData(long time, int _enum, bool on)
        {
            this.time = time;
            this._enum = _enum;
            this.on = on;
        }
    }
    
    public class GenericData<T, V>
    {
        public float time;
        public bool on;
        public T Value1 { get; set; }
        public V Value2 { get; set; }

        public GenericData(T value,float time, bool on)
        {
            Value1 = value;
            this.time = time;
            this.on = on;
        }

        public GenericData(T value,V value2,float time, bool on)
        {
            Value1 = value;
            Value2 = value2;
            this.time = time;
            this.on = on;
        }
    }

    public void AddTrigger(string key, string TimexDataStr)
    {
        TimexDataAll data = JsonUtility.FromJson<TimexDataAll>(TimexDataStr);
        DoUniTask(key, data.dataArray);
    }

    public void AddCallback(string key, Action<int,bool>callback)
    {
        if (!UniCallbackDic.ContainsKey(key))
        {
            UniCallbackDic.Add(key, callback);
        }
        else
        {
            UniCallbackDic[key] += callback;
        }
    }
    
    public void RemoveCallback(string key, Action<int,bool>callback)
    {
        if (UniCallbackDic.ContainsKey(key))
        {
            UniCallbackDic[key] -= callback;
        }
    }
    
    private async void DoUniTask(string key, params TimexData[] datas)
    {
        TimexData first = datas[0];
        Debug.Log("执行:" + 0 + "-" + first.on + " ");
        // ChangeLightStateNew(first.state, first.on);
        // if (first._enum == 1)
        // {
        //     NDataManager.Instance.SetSignalValue(NStoreKey.HVACTheme, first.on.ToInt());
        // }
        // else
        // {
        //     NDataManager.Instance.SetSignalValue(NStoreKey.Theme, first.on.ToInt());
        // }
        if (UniCallbackDic.ContainsKey(key))
        {
            UniCallbackDic[key].Invoke(first._enum, first.on);
        }
        for (int i = 1; i < datas.Length; i++)
        {
            //Debug.Log("cyj TurnFlow " + i);
            var temp = datas[i];
            var temp2 = datas[i - 1];
            int index = i;
            long end = temp.time - temp2.time;
            float fend = (float)end * 0.001f;
            Debug.Log(fend);
            await TestTask(fend, ()=>
            { 
                Debug.Log("执行:" + index + "-" + temp.on + " " + DateTime.Now);
                if (UniCallbackDic.ContainsKey(key))
                {
                    UniCallbackDic[key].Invoke(first._enum, first.on);
                }
                // if (temp._enum == 1)
                // {
                //     NDataManager.Instance.SetSignalValue(NStoreKey.HVACTheme, temp.on.ToInt());
                // }
                // else
                // {
                //     NDataManager.Instance.SetSignalValue(NStoreKey.Theme, temp.on.ToInt());
                // }
            });
        }
    }
    
    public static string GetTimeStr(long timestampMilliseconds)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(timestampMilliseconds);

        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.ffffff");
        return formattedTime;
    }
    
    private async Task TestTask(float time, Action callback)
    {
        await Task.Delay(Convert.ToInt32( time * 1000));
        //Debug.Log("test task2 " + Time.time);
        callback?.Invoke();
    }
 
    public void Test()
    {
        var person10 = new TimexData((long)1701941370029,1, true);
        var person11 = new TimexData((long)1701941372096,-1,  true);
        var person12 = new TimexData((long)1701941374096, 1, true);
        var person13 = new TimexData((long)1701941375096, -1 ,true);
        var person14 = new TimexData((long)1701941376096, 1, true);
        DoUniTask("Theme", person10,person11,person12,person13,person14);
    }
}
