using UnityEngine;

public class TimeAttack : MonoBehaviour
{
    [Header("Time")]
    private float elapsedTime; // 경과 시간(초)

    /// <summary>
    /// 게임 시작 시 클리어 이벤트를 구독한다
    /// </summary>
    private void Start()
    {
        GameManager.OnGameClear += GameClear;
    }

    /// <summary>
    /// 매 프레임 경과 시간을 누적하고 UI에 반영한다
    /// </summary>
    private void Update()
    {
        elapsedTime += Time.deltaTime;
        UITimeUpdate();
    }

    /// <summary>
    /// 분/초로 변환해서 UI에 표시한다
    /// </summary>
    private void UITimeUpdate()
    {
        int minutes = (int)(elapsedTime / 60f);
        int seconds = (int)(elapsedTime % 60f);

        UIManager.Instance.UpdateTime(minutes, seconds);
    }

    /// <summary>
    /// 게임 클리어 시 업적 처리 + 랭킹 제출을 수행한다
    /// </summary>
    public void GameClear()
    {
        // 업적
        AchievementManager.Instance.IncreseAchievement(EAchievementCode.TimeAttack, elapsedTime);

        // 랭킹 제출용: ms(정수)로 변환
        int clearTimeMs = Mathf.RoundToInt(elapsedTime * 1000f);

        // 네트워크로 랭킹 제출(패킷 구현 후 연결)
       NetworkManager.Instance.SendRankingSubmit(clearTimeMs);
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }
}
