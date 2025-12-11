using UnityEngine;

public class PulpleDoor : Object
{
    public override void ExecuteRandomAction()
    {
        int actionIndex = Random.Range(0,0);
        switch (actionIndex)
        {
            case 0:
                ChangeColor();
                hasChanged = false;
                break;
            default:
                NoChange();
                hasChanged = true;
                break;
        }
    }

    private void ChangeColor() //색 교체
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.5f, 0.0f, 0.7f);
        }
    }

    public void NoChange()
    {
        return;
    }
}
