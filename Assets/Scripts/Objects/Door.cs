using UnityEngine;

public class Door : Object
{
    public override void ExecuteRandomAction()
    {
        int actionIndex = Random.Range(0, 2);
        switch (actionIndex)
        {
            case 0:
                Rotate();
                hasChanged = false;
                break;
            case 1:
                ChangeColor();
                hasChanged = false;
                break;
        }
    }

    private void Rotate() //문이 열리는 내용
    {
        Vector3 currentRotation = transform.eulerAngles;
        float newRotationY = currentRotation.x - 10f;
        transform.eulerAngles = new Vector3(currentRotation.x, newRotationY, currentRotation.z);

    }

    private void ChangeColor() //색 체인지
    {
        objectRenderer.material = Resources.Load<Material>("DoorColor");
    }
}
