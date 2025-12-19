using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndUI : MonoBehaviour
{

    private void Start()
    {
        // 5초 뒤에 자동으로 메인 메뉴로 이동
        Invoke(nameof(OnClickMain), 5f);
    }

    public void OnClickMain()
    {
        SceneManager.LoadScene("LoginScene");
    }
}
