using UnityEngine;
using UnityEngine.SceneManagement;

public class EndTrigger : ITriggerAction
{
    //EndTringger면 
    public void ExecuteTrigger(Transform playerTransform, TriggerZone triggerZone)
    {
        TriggerManager.Instance.FloorUp();
        AchievementManager.Instance.IncreseAchievement(EAchievementCode.PassSuccess);
        SceneManager.LoadScene("EndScene");
    }
}
