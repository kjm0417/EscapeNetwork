using System.Collections;
using UnityEngine;

public class Enermy : Object
{
    public override void ExecuteRandomAction()
    {
        int actionIndex = Random.Range(0, 3);
        switch (actionIndex)
        {
            case 0:
                SetActive();
                hasChanged = false;
                break;
            case 1:
                Sound();
                hasChanged = false;
                break;
            default:
                NoChange();
                hasChanged = true;
                break;
        }
    }

    private void SetActive()
    {
        gameObject.SetActive(false);
        
    }

    private void Sound()
    {
        EventBus.Publish("ManagerSound");
        StartCoroutine(PlayLongSound());
        
    }

    public void NoChange()
    {
        return;
    }

    private IEnumerator PlayLongSound()
    {
        yield return new WaitForSeconds(2f); 
        EventBus.Publish("ManagerLongSound");
        yield return new WaitForSeconds(3f); 
        AchievementManager.Instance.IncreseAchievement(EAchievementCode.AudioClip);
        EventBus.Publish("ManagerSound");
        yield return new WaitForSeconds(2f); 
        EventBus.Publish("ManagerLongSound");
       
    }
}
