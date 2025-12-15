using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserAuthUI : MonoBehaviour
{
    private void Awake()
    {
        NetworkManager.Instance.Initialize();
    }

    [Header("Login Panel")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField loginUsernameInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button showRegisterButton;

    [Header("Register Panel")]
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button showLoginButton;

    [Header("User Info Panel")]
    [SerializeField] private GameObject userInfoPanel;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Button updateInfoButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button deleteAccountButton;

    [Header("Update Panel")]
    [SerializeField] private GameObject updatePanel;
    [SerializeField] private TMP_InputField updateLevelInput;
    [SerializeField] private Button confirmUpdateButton;
    [SerializeField] private Button cancelUpdateButton;

    private void Start()
    {
        InitializeUI();
        SetupButtonListeners();
        SubscribeToNetworkEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromNetworkEvents();
    }

    /// <summary>
    /// 네트워크 이벤트를 구독한다
    /// </summary>
    private void SubscribeToNetworkEvents()
    {
        NetworkManager.OnLoginSuccess += OnLoginSuccess;
        NetworkManager.OnLoginFailed += OnLoginFailed;
        NetworkManager.OnRegisterSuccess += OnRegisterSuccess;
        NetworkManager.OnRegisterFailed += OnRegisterFailed;
        NetworkManager.OnUpdateSuccess += OnUpdateSuccess;
        NetworkManager.OnUpdateFailed += OnUpdateFailed;
        NetworkManager.OnDeleteAccountSuccess += OnDeleteAccountSuccess;
    }

    /// <summary>
    /// 네트워크 이벤트 구독을 해제한다
    /// </summary>
    private void UnsubscribeFromNetworkEvents()
    {
        NetworkManager.OnLoginSuccess -= OnLoginSuccess;
        NetworkManager.OnLoginFailed -= OnLoginFailed;
        NetworkManager.OnRegisterSuccess -= OnRegisterSuccess;
        NetworkManager.OnRegisterFailed -= OnRegisterFailed;
        NetworkManager.OnUpdateSuccess -= OnUpdateSuccess;
        NetworkManager.OnUpdateFailed -= OnUpdateFailed;
        NetworkManager.OnDeleteAccountSuccess -= OnDeleteAccountSuccess;
    }

    /// <summary>
    /// UI 초기 상태를 로그인 패널로 설정한다
    /// </summary>
    private void InitializeUI()
    {
        ShowLoginPanel();
    }

    /// <summary>
    /// 버튼 이벤트를 연결한다
    /// </summary>
    private void SetupButtonListeners()
    {
        // Login Panel
        loginButton.onClick.AddListener(OnLoginClick);
        showRegisterButton.onClick.AddListener(ShowRegisterPanel);

        // Register Panel
        registerButton.onClick.AddListener(OnRegisterClick);
        showLoginButton.onClick.AddListener(ShowLoginPanel);

        // User Info Panel
        updateInfoButton.onClick.AddListener(ShowUpdatePanel);
        logoutButton.onClick.AddListener(OnLogoutClick);
        deleteAccountButton.onClick.AddListener(OnDeleteAccountClick);

        // Update Panel
        confirmUpdateButton.onClick.AddListener(OnUpdateConfirmClick);
        cancelUpdateButton.onClick.AddListener(ShowUserInfoPanel);
    }

    #region Panel Management
    /// <summary>
    /// 로그인 패널을 보여준다
    /// </summary>
    private void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        userInfoPanel.SetActive(false);
        updatePanel.SetActive(false);
    }

    /// <summary>
    /// 회원가입 패널을 보여준다
    /// </summary>
    private void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        userInfoPanel.SetActive(false);
        updatePanel.SetActive(false);
    }

    /// <summary>
    /// 유저 정보 패널을 보여준다
    /// </summary>
    private void ShowUserInfoPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        userInfoPanel.SetActive(true);
        updatePanel.SetActive(false);
    }

    /// <summary>
    /// 레벨 업데이트 패널을 보여준다
    /// </summary>
    private void ShowUpdatePanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        userInfoPanel.SetActive(false);
        updatePanel.SetActive(true);
    }
    #endregion

    #region Button Handlers
    /// <summary>
    /// 로그인 버튼 클릭 처리
    /// </summary>
    private void OnLoginClick()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        if (ValidateLoginInput(username, password))
        {
            NetworkManager.Instance.SendLoginRequest(username, password);
        }
    }

    /// <summary>
    /// 회원가입 버튼 클릭 처리
    /// </summary>
    private void OnRegisterClick()
    {
        string username = registerUsernameInput.text;
        string password = registerPasswordInput.text;

        if (ValidateRegisterInput(username, password))
        {
            NetworkManager.Instance.SendRegisterRequest(password, username);
        }
    }

    /// <summary>
    /// 로그아웃 버튼 클릭 처리
    /// </summary>
    private void OnLogoutClick()
    {
        NetworkManager.Instance.SendLogoutRequest();
        ShowLoginPanel();
        ClearInputs();
    }

    /// <summary>
    /// 탈퇴 버튼 클릭 처리 (현재는 로그만)
    /// </summary>
    private void OnDeleteAccountClick()
    {
        Debug.Log("계정 삭제 요청 - 실제 구현시 확인 다이얼로그 필요");
        // TODO: NetworkManager.Instance.SendDeleteAccountRequest(usernameText.text, ???);
    }

    /// <summary>
    /// 레벨 업데이트 확정 버튼 클릭 처리
    /// </summary>
    private void OnUpdateConfirmClick()
    {
        string level = updateLevelInput.text;

        if (ValidateUpdateInput(level))
        {
            // 누가 업데이트할지 UserID가 필요하므로 usernameText(로그인 성공 후 설정됨)를 사용
            NetworkManager.Instance.SendUpdateLevelRequest(usernameText.text, level);
        }
    }
    #endregion

    #region Validation
    /// <summary>
    /// 로그인 입력값 검증
    /// </summary>
    private bool ValidateLoginInput(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("사용자명과 비밀번호를 입력해주세요.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// 회원가입 입력값 검증
    /// </summary>
    private bool ValidateRegisterInput(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("모든 필드를 입력해주세요.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 레벨 업데이트 입력값 검증
    /// </summary>
    private bool ValidateUpdateInput(string level)
    {
        // ✅ 기존 코드가 반대로 되어 있었음
        if (string.IsNullOrEmpty(level))
        {
            Debug.LogWarning("레벨을 입력해주세요.");
            return false;
        }

        // 숫자만 허용하고 싶으면 아래 검증 추가(선택)
        // if (!int.TryParse(level, out _))
        // {
        //     Debug.LogWarning("레벨은 숫자만 입력해주세요.");
        //     return false;
        // }

        return true;
    }
    #endregion

    #region Network Callbacks
    /// <summary>
    /// 로그인 성공 콜백 처리
    /// </summary>
    public void OnLoginSuccess(string userID)
    {
        usernameText.text = userID;
        ShowUserInfoPanel();
        ClearInputs();
        Debug.Log("로그인 성공!");
    }

    /// <summary>
    /// 로그인 실패 콜백 처리
    /// </summary>
    public void OnLoginFailed(string errorMessage)
    {
        Debug.LogError($"로그인 실패: {errorMessage}");
    }

    /// <summary>
    /// 회원가입 성공 콜백 처리
    /// </summary>
    public void OnRegisterSuccess()
    {
        ShowLoginPanel();
        ClearInputs();
        Debug.Log("회원가입 성공! 로그인해주세요.");
    }

    /// <summary>
    /// 회원가입 실패 콜백 처리
    /// </summary>
    public void OnRegisterFailed(string errorMessage)
    {
        Debug.LogError($"회원가입 실패: {errorMessage}");
    }

    /// <summary>
    /// 레벨 업데이트 성공 콜백 처리
    /// </summary>
    public void OnUpdateSuccess()
    {
        ShowUserInfoPanel();
        Debug.Log("레벨 업데이트 성공!");
        updateLevelInput.text = "";
    }

    /// <summary>
    /// 레벨 업데이트 실패 콜백 처리
    /// </summary>
    public void OnUpdateFailed(string errorMessage)
    {
        Debug.LogError($"레벨 업데이트 실패: {errorMessage}");
    }

    /// <summary>
    /// 계정 삭제 성공 콜백 처리
    /// </summary>
    public void OnDeleteAccountSuccess()
    {
        ShowLoginPanel();
        ClearInputs();
        Debug.Log("계정이 삭제되었습니다.");
    }
    #endregion

    /// <summary>
    /// 입력 필드를 초기화한다
    /// </summary>
    private void ClearInputs()
    {
        loginUsernameInput.text = "";
        loginPasswordInput.text = "";
        registerUsernameInput.text = "";
        registerPasswordInput.text = "";
        updateLevelInput.text = "";
    }
}
