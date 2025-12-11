using UnityEngine;

public interface IChildAction //기능을 실행시켜주기 위한 작업
{
    void ExecuteRandomAction();
}
public class Object : MonoBehaviour, IChildAction
{
    private Vector3 originalPosition;
    protected bool hasChanged = false;
    private Quaternion originalRotation; // 초기 회전 상태 저장
    private Material originalMaterial; // 초기 머티리얼 저장
    public Renderer objectRenderer; // Renderer 참조
    private void Awake()
    {
        originalRotation = transform.rotation;
        originalPosition = transform.position;
        objectRenderer = GetComponent<MeshRenderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
    }

    public bool HasChanged()
    {
        return hasChanged;
    }
    // 인터페이스를 구현했지만 실제로 기능은 구체적인 자식 클래스들이 정의
    public virtual void ExecuteRandomAction() //object들 기능 구현을 위한 virtual
    {
        
    }
    
    public void ResetToOriginalState() //object 초기화 해주는 부분
    {
        transform.position = originalPosition;// 문고리 위치 초기화

        gameObject.SetActive(true);
        Debug.Log("set active true");
        transform.rotation = originalRotation;
        if (objectRenderer != null)
        {
            objectRenderer.material = originalMaterial;
        }
    }
}
