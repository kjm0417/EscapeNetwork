using System;
using Unity.VisualScripting;
using UnityEngine;

//게임매니저는 게임 시작, 일시 정지, 종료 관리
public class GameManager : Singleton<GameManager>
{
    public void GamStart()
    {
        //시작 씬 관련 
    }



    public delegate void GameStartHandler();
    public static event GameStartHandler OnGameStart;

    public static Action OnGameClear;

    private void Start()
    {
        // 게임 시작 시 이벤트 호출
        OnGameStart?.Invoke();
        OnGameClear?.Invoke();
    }
}
