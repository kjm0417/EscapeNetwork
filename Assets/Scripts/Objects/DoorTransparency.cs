using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTransparency : Object
{
    public override void ExecuteRandomAction()
    {
        int actionIndex = Random.Range(0,0);
        switch (actionIndex)
        {
            case 0:
                SetActive();
                hasChanged = false;
                break;
        }
    }

    private void SetActive() //문 사라지는 코드
    {
        gameObject.SetActive(false);

    }

}