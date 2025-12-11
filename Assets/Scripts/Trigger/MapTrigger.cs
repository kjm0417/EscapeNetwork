using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class MapTrigger : ITriggerAction
{
    private List<Object> allObjects; // Object를 상속받은 오브젝트를 담는 리스트

    public MapTrigger()
    {
        // 게임 시작 시 모든 Object를 상속받은 오브젝트를 리스트에 추가
        allObjects = new List<Object>(GameObject.FindObjectsOfType<Object>(true));
    }

    public void ExecuteTrigger(Transform playerTransform, TriggerZone triggerZone)
    {
        foreach (var obj in allObjects)
        {
            obj.ResetToOriginalState(); // 비활성화된 오브젝트도 다시 초기화
        }

        if (allObjects.Count > 0)
        {
            // 무작위로 하나의 ObjectScript를 선택
            int randomIndex = Random.Range(0, allObjects.Count);
            Object selectedObject = allObjects[randomIndex];

            // 선택된 ObjectScript의 기능 실행
            selectedObject.ExecuteRandomAction();

            // 변화 여부에 따라 isCorrectMap 설정 (변화가 없으면 True로 설정)
            TriggerManager.Instance.isCorrectMap = selectedObject.HasChanged();
        }

    }

}