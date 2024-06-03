using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerUtilsManager : MonoBehaviour
{
    private static TimerUtilsManager instance;
    private static string timeGoName = "TimerManager";

    private int timeIdPool = 0;
    public List<TimerData> TimerList = new List<TimerData>();
    public List<TimerData> waitAddTimerList = new List<TimerData>();

    public static TimerUtilsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject timer = GameObject.Find(timeGoName);
                if (timer == null)
                {
                    GameObject timeGameObject = new GameObject();
                    timeGameObject.name = timeGoName;
                    instance = timeGameObject.AddComponent<TimerUtilsManager>();
                    DontDestroyOnLoad(timeGameObject);
                }
                else
                {
                    instance = timer.GetComponent<TimerUtilsManager>();
                    if (instance == null)
                    {
                        instance = timer.AddComponent<TimerUtilsManager>();
                    }
                }

                return instance;
            }

            return instance;
        }
        set => instance = value;
    }

    private void Update()
    {
        if (waitAddTimerList.Count > 0)
        {
            for (int i = 0; i < waitAddTimerList.Count; i++)
            {
                if (!waitAddTimerList[i].TimerIsKill())
                {
                    TimerList.Add(waitAddTimerList[i]);
                }
            }

            waitAddTimerList.Clear();
        }

        if (TimerList.Count > 0)
        {
            //把没用的移除
            for (int i = TimerList.Count - 1; i >= 0; i--)
            {
                if (TimerList[i].TimerIsKill())
                {
                    TimerList.RemoveAt(i);
                }
            }

            if (TimerList.Count > 0)
            {
                for (int i = 0; i < TimerList.Count; i++)
                {
                    if (!TimerList[i].TimerIsPause())
                    {
                        TimerList[i].AddDeltaTime(Time.deltaTime);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 添加定时器任务
    /// </summary>
    /// <param name="duration">时间</param>
    /// <param name="repeat">重复次数，-1则无限重复</param>
    /// <param name="callback"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public int AddTimer(float duration, int repeat, Action<int, object> callback, object param)
    {
        if (duration <= 0)
        {
            Debug.LogError("时间小于0");
            return 0;
        }

        if (callback == null)
        {
            Debug.LogError("回调为null");
            return 0;
        }

        if (repeat == 0)
        {
            Debug.LogError("循环次数为0");
            return 0;
        }

        int id = GetTimerId();
        TimerData timer = new TimerData(id, duration, repeat, callback, param);
        waitAddTimerList.Add(timer);
        Debug.Log(string.Format("添加定时任务，id:{0},每次持续时间：{1}，重复次数：{2}", id, duration, repeat));
        return id;
    }

    /// <summary>
    /// 移除定时器任务
    /// </summary>
    /// <param name="id"></param>
    public void RemoveTimer(int id)
    {
        if (id != null)
        {
            if (waitAddTimerList.Count > 0)
            {
                for (int i = 0; i < waitAddTimerList.Count; i++)
                {
                    if (waitAddTimerList[i].id == id)
                    {
                        waitAddTimerList[i].KillTimer();
                        Debug.Log(string.Format("定时任务id为：{0}的任务停止了", id));
                        break;
                    }
                }
            }

            if (TimerList.Count > 0)
            {
                for (int i = 0; i < TimerList.Count; i++)
                {
                    if (TimerList[i].id == id)
                    {
                        TimerList[i].KillTimer();
                        Debug.Log(string.Format("定时任务id为：{0}的任务停止了", id));
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 暂停、继续定时器任务
    /// </summary>
    /// <param name="id"></param>
    /// <param name="state"></param>
    public void SetTimerPauseState(int id, bool state)
    {
        if (id != null)
        {
            if (waitAddTimerList.Count > 0)
            {
                for (int i = 0; i < waitAddTimerList.Count; i++)
                {
                    if (waitAddTimerList[i].id == id)
                    {
                        waitAddTimerList[i].SetPauseState(state);
                        Debug.Log(string.Format("定时任务id为：{0}的暂停状态改为{1}", id, state));
                        break;
                    }
                }
            }

            if (TimerList.Count > 0)
            {
                for (int i = 0; i < TimerList.Count; i++)
                {
                    if (TimerList[i].id == id)
                    {
                        TimerList[i].SetPauseState(state);
                        Debug.Log(string.Format("定时任务id为：{0}的暂停状态改为{1}", id, state));
                        break;
                    }
                }
            }
        }
    }


    /// <summary>
    /// 获取唯一id
    /// </summary>
    /// <returns></returns>
    private int GetTimerId()
    {
        timeIdPool = timeIdPool + 1;
        return timeIdPool;
    }
}

[Serializable]
public class TimerData
{
    /// <summary>
    /// 唯一id
    /// </summary>
    public int id;

    /// <summary>
    /// 倒计时
    /// </summary>
    private float duration;

    /// <summary>
    /// 重复次数，-1无限循环
    /// </summary>
    private int repeat;

    /// <summary>
    /// 回调
    /// </summary>
    private Action<int, object> callback;

    /// <summary>
    /// 回调参数
    /// </summary>
    private object param;

    /// <summary>
    /// 是否暂停
    /// </summary>
    private bool isPause;

    /// <summary>
    /// 是否已经移除
    /// </summary>
    private bool isKill;

    /// <summary>
    /// 已经过去的时间
    /// </summary>
    private float curTime;

    //当前回调次数
    private int curCallbackCount;

    public TimerData(int id, float duration, int repeat, Action<int, object> callback, object param)
    {
        Reset();
        this.id = id;
        this.duration = duration;
        this.repeat = repeat;
        this.callback = callback;
        this.param = param;
    }

    private void Reset()
    {
        isKill = false;
        isPause = false;
        curTime = 0;
        curCallbackCount = 0;
    }

    public void SetPauseState(bool state)
    {
        isPause = state;
    }

    public void KillTimer()
    {
        isKill = true;
    }

    public bool TimerIsKill()
    {
        return isKill;
    }

    public bool TimerIsPause()
    {
        return isPause;
    }

    public void AddDeltaTime(float deltaTime)
    {
        curTime = curTime + deltaTime;
        if (curTime >= duration)
        {
            curCallbackCount++;
            callback?.Invoke(curCallbackCount, param);
            curTime = curTime - duration;
            if (repeat > 0)
            {
                repeat = repeat - 1;
                if (repeat == 0)
                {
                    //移除定时器
                    isKill = true;
                }
            }
        }
    }
}