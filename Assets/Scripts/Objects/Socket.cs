using UnityEngine;

public class Socket : Object
{
    public override void ExecuteRandomAction()
    {
        int actionIndex = Random.Range(0, 0);
        switch (actionIndex)
        {
            case 0:
                SetActive();
                hasChanged = false;
                break;
        }
    }
 
    private void SetActive() //삭제
    {
        gameObject.SetActive(false);

    }
}
