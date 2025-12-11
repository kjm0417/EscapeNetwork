using UnityEngine;

public class TriggerZone : MonoBehaviour
{
    [Header("Teleport Position")]
    public Transform[] teleportPosition; // 텔레포트 할 위치
    public bool applyYRotationOffset = false; // Y축 회전을 적용할지 여부
    public bool invertZOffset = false; // Z축 반전이 필요한 트리거: Y축 회전과 세트
    public TriggerType triggerType; // 어떤 존인지 확인
    private ITriggerAction triggerAction; // 트리거 동작을 처리할 인터페이스
    private static bool isMapTriggerNext = true; // 트리거 실행 순서를 관리하는 변수
    private bool isTriggerRecentlyActivated; // 최근에 트리거가 작동되었는지 확인

    private void Start()
    {
        switch (triggerType)
        {
            case TriggerType.FrontTrigger:
                triggerAction = new FrontTrigger();
                break;
            case TriggerType.BackTrigger:
                triggerAction = new BackTrigger();
                break;
            case TriggerType.MapTrigger:
                triggerAction = new MapTrigger();
                break;
            case TriggerType.EndTrigger:
                triggerAction = new EndTrigger();
                break;
        }
    }

    private void OnTriggerEnter(Collider hitobject)
    {
        // 트리거 순서 제어 로직
        if (triggerType == TriggerType.MapTrigger && !isMapTriggerNext) return; // MapTrigger는 반드시 FrontTrigger 또는 BackTrigger 후에 실행되어야 함
        if ((triggerType == TriggerType.FrontTrigger || triggerType == TriggerType.BackTrigger) && isMapTriggerNext) return; // FrontTrigger와 BackTrigger는 반드시 MapTrigger 후에만 실행 가능

        // 트리거 실행
        if (hitobject.CompareTag("Player"))
        {
            Transform playerTransform = hitobject.transform;
            triggerAction.ExecuteTrigger(playerTransform, this);

            // 상태 변경: MapTrigger가 실행되었으면 false, Front/BackTrigger가 실행되었으면 true로 설정
            if (triggerType == TriggerType.MapTrigger)
            {
                isMapTriggerNext = false;
            }
            else if (triggerType == TriggerType.FrontTrigger || triggerType == TriggerType.BackTrigger)
            {
                isMapTriggerNext = true;
            }
        }
    }

    public void TeleportPlayer(Transform playerTransform)
    {
        // 트리거가 다시 작동될 수 있도록 일정 시간이 지난 후 상태 초기화
        
        Vector3 playerOffset = playerTransform.position - transform.position;

        if (invertZOffset)
        {
            playerOffset.z = -playerOffset.z;
        }
        // 현재 층수에 따라 다른 텔레포트 위치를 선택
        Transform targetPosition;
        if (TriggerManager.Instance.currentfloor < 6)
        {
            targetPosition = teleportPosition[0];
        }
        else if (TriggerManager.Instance.currentfloor == 6) //점수가 올라가고 텔레포트가 되어서 6으로 설정
        {
            targetPosition = teleportPosition[1];
        }
        else
        {
            return; // 층수나 설정이 잘못되었을 경우 아무 동작도 하지 않음
        }

        playerTransform.position = targetPosition.position + playerOffset;

        if (applyYRotationOffset)
        {
            playerTransform.rotation = Quaternion.Euler(
                playerTransform.rotation.eulerAngles.x,
                playerTransform.rotation.eulerAngles.y + 180f,
                playerTransform.rotation.eulerAngles.z
            );
        }
    }
}