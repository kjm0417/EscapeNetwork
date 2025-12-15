using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _jobs = new Queue<Action>();
    private static MainThreadDispatcher _instance;

    /// <summary>
    /// 씬에 디스패처를 1개만 유지하고 파괴되지 않도록 한다
    /// </summary>
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 메인 스레드에서 실행할 작업을 등록한다
    /// </summary>
    public static void Post(Action job)
    {
        if (job == null) return;

        lock (((System.Collections.ICollection)_jobs).SyncRoot)
        {
            _jobs.Enqueue(job);
        }
    }

    /// <summary>
    /// 매 프레임 메인 스레드에서 등록된 작업들을 순차 실행한다
    /// </summary>
    private void Update()
    {
        while (true)
        {
            Action job = null;

            lock (((System.Collections.ICollection)_jobs).SyncRoot)
            {
                if (_jobs.Count > 0)
                    job = _jobs.Dequeue();
            }

            if (job == null)
                break;

            job.Invoke();
        }
    }
}
