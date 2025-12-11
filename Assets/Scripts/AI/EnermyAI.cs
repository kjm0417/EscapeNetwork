using UnityEngine;
using UnityEngine.AI;

public class EnermyAI : MonoBehaviour
{
    public enum AIState
    {
        Idle,
        Wandering
    }

    [Header("Stats")]
    public float speed;

    [Header("AI")]
    private AIState aiState;
    public float detectDistance;

    [Header("Wandering")]
    public float minWanderDistance;
    public float maxWanderDistance;
    public float minWanderWaitTime;
    public float maxWanderWaitTime;

    private NavMeshAgent agent;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        SetState(AIState.Wandering);
    }
    //  초기 AI 상태를 Wandering으로 설정한다.

    void Update()
    {
        switch (aiState)
        {
            case AIState.Idle:
                PassiveUpdate();
                break;
            case AIState.Wandering:
                PassiveUpdate();
                break;
        }
    }
    // switch (aiState): 현재 AI 상태에 따라 PassiveUpdate 메서드를 호출한다.

    public void SetState(AIState state)
    {
        aiState = state;

        switch (aiState) 
        {
            case AIState.Idle:
                agent.speed = speed;
                agent.isStopped = true;
                break;
            case AIState.Wandering:
                agent.speed = speed;
                agent.isStopped = false;
                break;
        }
    }
    // AI 상태를 설정하고, 상태에 따라 NavMeshAgent의 동작을 조정한다.
    // Idle 상태: 적의 속도를 설정하고, NavMeshAgent를 멈춘다.
    // Wandering 상태: 적의 속도를 설정하고, NavMeshAgent를 이동시키도록 설정한다.

    void PassiveUpdate()
    {
        if(aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }
    }
    // AI가 돌아다니는 상태에서 목적지에 도착하면 대기 상태로 전환하고, 무작위 위치로 이동하도록 설정한다.
    // SetState(AIState.Idle): AI 상태를 Idle로 전환한다.
    // Invoke: 무작위 대기 시간을 설정하고, 그 후에 WanderToNewLocation 메서드를 호출한다.

    void WanderToNewLocation()
    {
        if (aiState != AIState.Idle) return;

        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }
    // AI 상태가 대기 상태일 때 무작위 위치로 이동하도록 설정한다.
    // aiState != AIState.Idle: AI 상태가 Idle이 아니면 메서드를 종료한다.
    // SetState(AIState.Wandering): AI 상태를 Wandering으로 전환한다.
    // agent.SetDestination: NavMeshAgent의 목적지를 무작위 위치로 설정한다.

    Vector3 GetWanderLocation()
    {
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);

        int i = 0;

        while(Vector3.Distance(transform.position, hit.position) < detectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);
            i++;
            if(i == 30) break;
        }
        return hit.position;
    }
    // 무작위 위치를 생성하여 반환한다.
    // NavMesh.SamplePosition: 무작위 위치에서 NavMesh 내의 위치를 찾는다.
    // while(Vector3.Distance(transform.position, hit.position) < detectDistance): 생성된 무작위 위치가 탐지 거리 내에 있으면 새로운 위치를 찾는다. 최대 30번 반복한다.
    // return hit.position: 찾은 위치를 반환한다.
}
