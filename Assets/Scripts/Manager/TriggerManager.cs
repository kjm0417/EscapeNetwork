using UnityEngine;
public enum TriggerType
{
    FrontTrigger,
    BackTrigger,
    MapTrigger,
    EndTrigger
    // 필요한 만큼 더 추가 가능
}

public interface ITriggerAction
{
    void ExecuteTrigger(Transform playerTransform, TriggerZone triggerZone);
}
public class TriggerManager : Singleton<TriggerManager>
{
    public int currentfloor=0; //현재 층
    public bool isCorrectMap = true; //맵이 정상인지
    
    public void FloorUp() //층수 증가
    {
        currentfloor++;
        UIManager.Instance.UINumberUpdate();
 
    }

    public void FloorReset() //층수 초기화
    {
        currentfloor=0;
        UIManager.Instance.UINumberUpdate();
    }
}
