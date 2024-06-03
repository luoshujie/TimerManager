using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestData
{
    public float startTime;
    public int id;
    public float duration;
    public int repeat;

    public TestData(float startTime, float duration, int repeat)
    {
        this.startTime = startTime;
        this.duration = duration;
        this.repeat = repeat;
    }
}

public class TimerTest : MonoBehaviour
{
    public bool create = false;
    public float time;
    public int repeatC;

    public int cancelId;

    public bool isPauseTask = false;
    public int pauseTaskId;
    public bool isPause;

    // Update is called once per frame
    void Update()
    {
        if (isPauseTask)
        {
            isPauseTask = false;
            TimerUtilsManager.Instance.SetTimerPauseState(pauseTaskId, isPause);
        }

        if (create)
        {
            create = false;
            AddTimer(time, repeatC);
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            TimerUtilsManager.Instance.RemoveTimer(cancelId);
        }
    }

    public void AddTimer(float duration, int repeat)
    {
        TestData data = new TestData(Time.unscaledTime, duration, repeat);
        int id = TimerUtilsManager.Instance.AddTimer(duration, repeat, TimeCallback,
            data);
        data.id = id;
        Debug.LogWarning(string.Format("创建一个任务，持续时间为：{0}，循环次数为：{1}，id为：{2}", duration, repeat, id));
    }

    public void TimeCallback(int curRepeatCount, object param)
    {
        TestData data = param as TestData;
        string detail = "id:{0},创建时间：{1}，结束时间：{2},时间差：{3}，持续时间：{4}，当前循环次数：{5}，总循环次数：{6}";

        detail = string.Format(detail, data.id, data.startTime, Time.unscaledTime,
            Time.unscaledTime - data.startTime - ((curRepeatCount - 1) * data.duration),
            data.duration, curRepeatCount, data.repeat);
        Debug.Log(detail);
    }
}