using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTrigger : MonoBehaviour, ITriggerAction
{
    /// <summary>
    /// 트리거 실행 시(클리어 지점 도착)
    /// 랭킹 제출 후 EndScene으로 이동한다
    /// </summary>
    public void ExecuteTrigger(Transform playerTransform, TriggerZone triggerZone)
    {
        TriggerManager.Instance.FloorUp();
        AchievementManager.Instance.IncreseAchievement(EAchievementCode.PassSuccess);

        SubmitRankingIfPossible();

        // 패킷 송신 시간 확보 후 씬 이동
        SceneManager.LoadScene("EndScene");
    }

    /// <summary>
    /// 랭킹 제출을 시도한다 (시간이 있으면 ms로 변환하여 제출)
    /// </summary>
    private void SubmitRankingIfPossible()
    {
        // TimeAttack이 씬에 있다면 그 시간을 사용
        var timeAttack = Object.FindObjectOfType<TimeAttack>();
        if (timeAttack == null)
        {
            Debug.LogWarning("[EndTrigger] TimeAttack을 찾지 못해서 랭킹 제출을 건너뜁니다.");
            return;
        }

        // TimeAttack의 elapsedTime에 직접 접근 못하면 아래 2가지 중 하나로 처리해야 함:
        // 1) TimeAttack에 GetElapsedTime() 같은 public 메서드를 만들기
        // 2) elapsedTime을 public/properties로 노출하기
        //
        // 여기서는 (권장) TimeAttack에 GetElapsedTime()을 만든다는 전제
        float elapsedTime = timeAttack.GetElapsedTime();

        int clearTimeMs = Mathf.RoundToInt(elapsedTime * 1000f);
        Debug.Log($"[EndTrigger] Submit Ranking: {clearTimeMs}ms");

        NetworkManager.Instance.SendRankingSubmit(clearTimeMs);
    }

    /// <summary>
    /// 패킷 전송 시간을 확보하기 위해 잠깐 기다린 뒤 EndScene으로 이동한다
    /// </summary>
    private IEnumerator LoadEndSceneAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        SceneManager.LoadScene("EndScene");
    }
}
