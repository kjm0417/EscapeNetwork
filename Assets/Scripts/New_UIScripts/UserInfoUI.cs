using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UserInfoCanvas 전용 UI 컨트롤러
/// - Update / Delete / Logout 버튼 처리
/// </summary>
public class UserInfoUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject loginCanvas;
    [SerializeField] private GameObject userInfoCanvas;

    [Header("UserName UI")]
    [SerializeField] private TMP_Text txtUserName;        // Img_UserName 안의 Text(TMP)
    

    [Header("Buttons")]
    [SerializeField] private Button btnUserUpdate;        // Btn_UserUpdate
    [SerializeField] private Button btnUserDelete;        // Btn_UserDelete
    [SerializeField] private Button btnUserLogOut;        // Btn_UserLogOut

    [Header("Optional Update Panel")]
    [SerializeField] private GameObject updatePanel;          // (선택) 레벨 입력용 패널
    [SerializeField] private TMP_InputField inputLevel;       // (선택) 레벨 입력 InputField
    [SerializeField] private Button btnUpdateConfirm;         // (선택) 업데이트 확정 버튼
    [SerializeField] private Button btnUpdateCancel;          // (선택) 업데이트 취소 버튼


    /// <summary>
    /// 시작 시 버튼/이벤트를 연결하고 UI를 초기화한다
    /// </summary>
    private void Start()
    {
        BindButtons();
        SubscribeNetworkEvents();
        RefreshUserName();

        // 기본: UserInfo 그룹은 켜져있고, 업데이트 패널은 꺼둔다(있다면)
        HideUpdatePanel();
    }

    /// <summary>
    /// 오브젝트 파괴 시 네트워크 이벤트 구독을 해제한다
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeNetworkEvents();
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
    /// NetworkManager 이벤트를 구독한다
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
    /// NetworkManager 이벤트 구독을 해제한다
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
    /// 현재 로그인된 유저ID를 UI에 표시한다
    /// </summary>
    private void RefreshUserName()
    {
        if (txtUserName == null) return;

        var id = NetworkManager.Instance != null ? NetworkManager.Instance.CurrentUserID : string.Empty;

        if (string.IsNullOrEmpty(id))
            txtUserName.text = "User : -";
        else
            txtUserName.text = $"User : {id}";
    }


    /// <summary>
    /// 업데이트 패널을 보여준다(있을 때만)
    /// </summary>
    private void ShowUpdatePanel()
    {
        if (updatePanel != null) updatePanel.SetActive(true);
    }

    /// <summary>
    /// 업데이트 패널을 숨긴다(있을 때만)
    /// </summary>
    private void HideUpdatePanel()
    {
        if (updatePanel != null) updatePanel.SetActive(false);
    }

    /// <summary>
    /// Update 버튼 클릭 처리
    /// - UpdatePanel이 연결돼 있으면 패널을 띄운다
    /// - 없으면 경고 로그만 출력한다
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

        Debug.LogWarning("UpdatePanel이 연결되어 있지 않습니다. (updatePanel/inputLevel/confirm 버튼을 연결하면 레벨 업데이트 가능)");
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
    /// - 서버에 로그아웃 패킷이 있다면 전송
    /// - 없다면(혹은 서버에서 처리 안 한다면) 클라이언트 상태/화면만 초기화하는 방식으로도 가능
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

        // 숫자만 받게 하고 싶으면 주석 해제
        // if (!int.TryParse(levelText, out _))
        // {
        //     Debug.LogWarning("레벨은 숫자만 입력해주세요.");
        //     return;
        // }

        NetworkManager.Instance.SendUpdateLevelRequest(id, levelText);
    }

    /// <summary>
    /// 업데이트 취소 버튼 클릭 처리(원래 화면으로 복귀)
    /// </summary>
    private void OnClickUpdateCancel()
    {
        HideUpdatePanel();
    }

    /// <summary>
    /// 로그인 성공 시 유저 이름 표시를 갱신한다
    /// </summary>
    private void HandleLoginSuccess(string userID)
    {
        RefreshUserName();
    }

    /// <summary>
    /// 업데이트 성공 처리(패널 닫고 원래 화면으로)
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
    /// 계정 삭제 성공 처리(로그아웃과 동일하게 초기화)
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
