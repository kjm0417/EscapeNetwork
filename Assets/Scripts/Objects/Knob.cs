using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knob : Object
{
    public override void ExecuteRandomAction()
    {
        int actionIndex = Random.Range(0, 2);
        switch (actionIndex)
        {
            case 0:
                SetActive();
                hasChanged = false;
                break;
            case 1:
                Switch();
                hasChanged = false;
                break;
        }
    }

    private void SetActive() //문고리 삭제
    {
        gameObject.SetActive(false);
        
    }

    private void Switch() //문고리 위치 변경
    {
        gameObject.SetActive(true);

        Vector3 currentPosition = transform.position;
        currentPosition = new Vector3(currentPosition.x + 1f, currentPosition.y,currentPosition.z);
        transform.position = currentPosition;
        
    }

}