using UnityEngine;

public class TimeAttack : MonoBehaviour
{
    [Header("Time")]
    private float elapsedTime; // 경과된 시간(초 단위)
    
    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime; // 매 프레임 경과된 시간 추가
        UITimeUpdate();
    }
    
    private void UITimeUpdate()
    {
        int minutes = (int)(elapsedTime / 60); //분
        int seconds = (int)(elapsedTime % 60); //초
        
        UIManager.Instance.UpdateTime(minutes, seconds);
        
    }
    public void GameClear()
    {
        AchievementManager.Instance.IncreseAchievement(EAchievementCode.TimeAttack,elapsedTime);
    }

    private void Start()
    {
        GameManager.OnGameClear += GameClear;
    }


}
