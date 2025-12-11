using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampLight : MonoBehaviour
{
    private Light light;
    private MeshRenderer lamp;
    private Material lampMat;
    //private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        light = GetComponentInChildren<Light>();
        lamp = GetComponentInChildren<MeshRenderer>();
        lampMat = lamp.material;
    }

    // Light & Emission 설정
    private void SetLightAndEmission(bool state)
    {
        light.enabled = state;
        lampMat.SetColor("_EmissionColor", state ? Color.white : Color.clear);
    }
    
    // Light & Emission 활성화
    public void TurnOn()
    {
        SetLightAndEmission(true);
    }
    
    // Light & Emission 비활성화
    public void TurnOff()
    {
        SetLightAndEmission(false);
    }
}
