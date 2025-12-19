using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserInfoUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject loginCanvas;
    [SerializeField] private GameObject userInfoCanvas;

    [Header("UserName UI")]
    [SerializeField] private TMP_Text txtUserName;        
    

    [Header("Buttons")]
    [SerializeField] private Button btnUserUpdate;        
    [SerializeField] private Button btnUserDelete;        
    [SerializeField] private Button btnUserLogOut;        

    [Header("Optional Update Panel")]
    [SerializeField] private GameObject updatePanel;          
    [SerializeField] private TMP_InputField inputLevel;       
    [SerializeField] private Button btnUpdateConfirm;         
    [SerializeField] private Button btnUpdateCancel;        


    /// <summary>
    /// 시작 시 버튼/이벤트를 연결하고 UI를 초기화
    /// </summary>
    private void Start()
    {
        BindButtons();
        SubscribeNetworkEvents();
        RefreshUserName();

        HideUpdatePanel();
    }

    /// <summary>
    /// 버튼 리스너를 연결한다
    /// </summary>
    private void BindButtons()
    {
        if (btnUserUpdate != null) btnUserUpdate.onClick.AddListener(OnClickUpdate);
        if (btnUserDelete != null) btnUserDelete.onClick.AddListener(OnClickDelete);
        if (btnUserLogOut != null) btnUserLogOut.onClick.AddListener(OnClickLogout);

        if (btnUpdateConfirm != null) btnUpdateConfirm.onClick.AddListener(OnClickUpdateConfirm);
        if (btnUpdateCancel != null) btnUpdateCancel.onClick.AddListener(OnClickUpdateCancel);
    }

    /// <summary>
    /// NetworkManager 이벤트를 구독
    /// </summary>
    private void SubscribeNetworkEvents()
    {
        // 로그인 성공 시 UserName 갱신용
        NetworkManager.OnLoginSuccess += HandleLoginSuccess;

        // 업데이트 결과
        NetworkManager.OnUpdateSuccess += HandleUpdateSuccess;
        NetworkManager.OnUpdateFailed += HandleUpdateFailed;

        // 삭제 결과
        NetworkManager.OnDeleteAccountSuccess += HandleDeleteSuccess;
        NetworkManager.OnDeleteAccountFailed += HandleDeleteFailed;
    }

    /// <summary>
    /// NetworkManager 이벤트 구독을 해제
    /// </summary>
    private void UnsubscribeNetworkEvents()
    {
        NetworkManager.OnLoginSuccess -= HandleLoginSuccess;

        NetworkManager.OnUpdateSuccess -= HandleUpdateSuccess;
        NetworkManager.OnUpdateFailed -= HandleUpdateFailed;

        NetworkManager.OnDeleteAccountSuccess -= HandleDeleteSuccess;
        NetworkManager.OnDeleteAccountFailed -= HandleDeleteFailed;
    }

    /// <summary>
    /// 현재 로그인된 유저ID를 UI에 표시
    /// </summary>
    private void RefreshUserName()
    {
        if (txtUserName == null) 
            return;

        var id = NetworkManager.Instance != null ? NetworkManager.Instance.CurrentUserID : string.Empty;

        if (string.IsNullOrEmpty(id))
            txtUserName.text = "User : -";
        else
            txtUserName.text = $"User : {id}";
    }


    /// <summary>
    /// 업데이트 패널을 보여줌
    /// </summary>
    private void ShowUpdatePanel()
    {
        if (updatePanel != null) updatePanel.SetActive(true);
    }

    /// <summary>
    /// 업데이트 패널을 숨김
    /// </summary>
    private void HideUpdatePanel()
    {
        if (updatePanel != null) updatePanel.SetActive(false);
    }

    /// <summary>
    /// Update 버튼 클릭 처리

    /// </summary>
    private void OnClickUpdate()
    {
        RefreshUserName();

        // 업데이트 입력 패널을 쓰는 구조라면 열기
        if (updatePanel != null && inputLevel != null && btnUpdateConfirm != null)
        {
            ShowUpdatePanel();
            inputLevel.text = "";
            return;
        }
    }

    /// <summary>
    /// Delete 버튼 클릭 처리
    /// </summary>
    private void OnClickDelete()
    {
        var id = NetworkManager.Instance != null ? NetworkManager.Instance.CurrentUserID : string.Empty;

        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("로그인 상태가 아닙니다. 삭제 요청을 보낼 수 없습니다.");
            return;
        }

        NetworkManager.Instance.SendDeleteAccountRequest();
    }

    /// <summary>
    /// Logout 버튼 클릭 처리
    /// </summary>
    private void OnClickLogout()
    {
        // 서버 로그아웃 패킷을 이미 만들어두었으면 이 호출이 정상 동작
        NetworkManager.Instance.SendLogoutRequest();

        // UI 관점에서는 로그아웃 후 이름 표시도 초기화
        RefreshUserName();

        loginCanvas.SetActive(true);
        userInfoCanvas.SetActive(false);
    }

    /// <summary>
    /// 업데이트 확정 버튼 클릭 처리(레벨 입력값 검증 후 서버로 전송)
    /// </summary>
    private void OnClickUpdateConfirm()
    {
        if (inputLevel == null)
        {
            Debug.LogWarning("inputLevel이 연결되지 않았습니다.");
            return;
        }

        var id = NetworkManager.Instance != null ? NetworkManager.Instance.CurrentUserID : string.Empty;
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("로그인 상태가 아닙니다. 업데이트 요청을 보낼 수 없습니다.");
            return;
        }

        var levelText = inputLevel.text;
        if (string.IsNullOrEmpty(levelText))
        {
            Debug.LogWarning("레벨을 입력해주세요.");
            return;
        }

        NetworkManager.Instance.SendUpdateLevelRequest(id, levelText);
    }

    /// <summary>
    /// 업데이트 취소 버튼 클릭 처리
    /// </summary>
    private void OnClickUpdateCancel()
    {
        HideUpdatePanel();
    }

    /// <summary>
    /// 로그인 성공 시 유저 이름 표시를 갱신
    /// </summary>
    private void HandleLoginSuccess(string userID)
    {
        RefreshUserName();
    }

    /// <summary>
    /// 업데이트 성공 처리
    /// </summary>
    private void HandleUpdateSuccess()
    {
        Debug.Log("유저 Level 업데이트 성공");

        HideUpdatePanel();
    }

    /// <summary>
    /// 업데이트 실패 처리
    /// </summary>
    private void HandleUpdateFailed(string error)
    {
        Debug.LogError($"유저 Level 업데이트 실패: {error}");
    }

    /// <summary>
    /// 계정 삭제 성공 처리
    /// </summary>
    private void HandleDeleteSuccess()
    {
        Debug.Log("계정 삭제 성공");

        HideUpdatePanel();
        RefreshUserName();
    }

    /// <summary>
    /// 계정 삭제 실패 처리
    /// </summary>
    private void HandleDeleteFailed(string error)
    {
        Debug.LogError($"계정 삭제 실패: {error}");
    }
}
