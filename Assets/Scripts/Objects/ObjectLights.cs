using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class ObjectLights : Object
{
    public List<LampLight> lampLights = new List<LampLight>();
    public float offToOnDelay = 2.0f; // 조명 다시 켜기 전 대기 시간
    
    void Start()
    {
        // LampLight 추가
        foreach (Transform child in transform)
        {
            LampLight lamp = child.GetComponent<LampLight>();
            if (lamp != null)
            {
                lampLights.Add(lamp);
            }
        }
    }
    
    public override void ExecuteRandomAction()
    {
        int actionIndex = Random.Range(0,2);
        switch (actionIndex)
        {
            case 0:
                OffLights();
                hasChanged = false;
                break;
            case 1:
                NoChange();
                hasChanged = true;
                break;
        }
    }

    private void OffLights()
    {
        StartCoroutine(OffLightsCoroutine());
        AchievementManager.Instance.IncreseAchievement(EAchievementCode.Lighting);
    }

    private IEnumerator OffLightsCoroutine()
    {
        foreach (LampLight lamp in lampLights)
        {
            lamp.TurnOff();
            yield return new WaitForSeconds(0.3f); // 끄는 간격
        }
        
        // 조명 끈 후 대기 시간
        yield return new WaitForSeconds(offToOnDelay);

        // 모든 조명 다시 켜기
        TurnOnAllLights();
    }
    
    private void TurnOnAllLights()
    {
        foreach (LampLight lamp in lampLights)
        {
            lamp.TurnOn(); // 모든 조명 켜기
        }
    }
    
    public void NoChange()
    {
        return;
    }
}


