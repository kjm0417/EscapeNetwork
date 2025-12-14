using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UserAuthUI : MonoBehaviour
{
 
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
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button showLoginButton;

    [Header("User Info Panel")]
    [SerializeField] private GameObject userInfoPanel;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text emailText;
    [SerializeField] private Button updateInfoButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button deleteAccountButton;

    [Header("Update Panel")]
    [SerializeField] private GameObject updatePanel;
    [SerializeField] private TMP_InputField updateEmailInput;
    [SerializeField] private TMP_InputField updatePasswordInput;
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

    private void InitializeUI()
    {
        ShowLoginPanel();
    }

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
    private void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        registerPanel.SetActive(false);
        userInfoPanel.SetActive(false);
        updatePanel.SetActive(false);
    }

    private void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        userInfoPanel.SetActive(false);
        updatePanel.SetActive(false);
    }

    private void ShowUserInfoPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        userInfoPanel.SetActive(true);
        updatePanel.SetActive(false);
    }

    private void ShowUpdatePanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(false);
        userInfoPanel.SetActive(false);
        updatePanel.SetActive(true);
    }
    #endregion

    #region Button Handlers
    private void OnLoginClick()
    {
        string username = loginUsernameInput.text;
        string password = loginPasswordInput.text;

        if (ValidateLoginInput(username, password))
        {
            NetworkManager.Instance.SendLoginRequest(username, password);
        }
    }

    private void OnRegisterClick()
    {
        string username = registerUsernameInput.text;
        string password = registerPasswordInput.text;
        string email = registerEmailInput.text;

        if (ValidateRegisterInput(username, password, email))
        {
            NetworkManager.Instance.SendRegisterRequest(password, username);
        }
    }

    private void OnLogoutClick()
    {
        NetworkManager.Instance.SendLogoutRequest();
        ShowLoginPanel();
        ClearInputs();
    }

    private void OnDeleteAccountClick()
    {
        // Unity에서는 간단한 확인 로그로 대체 (실제로는 확인 다이얼로그 UI를 만들어야 함)
        Debug.Log("계정 삭제 요청 - 실제 구현시 확인 다이얼로그 필요");
        // NetworkManager.Instance.SendDeleteAccountRequest(); // 주석 처리됨
    }

    private void OnUpdateConfirmClick()
    {
        string newEmail = updateEmailInput.text;
        string newPassword = updatePasswordInput.text;

        if (ValidateUpdateInput(newEmail, newPassword))
        {
            NetworkManager.Instance.SendUpdateUserRequest(usernameText.text, newPassword);
        }
    }
    #endregion

    #region Validation
    private bool ValidateLoginInput(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("사용자명과 비밀번호를 입력해주세요.");
            return false;
        }
        return true;
    }

    private bool ValidateRegisterInput(string username, string password, string email)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(email))
        {
            Debug.LogWarning("모든 필드를 입력해주세요.");
            return false;
        }

        if (!email.Contains("@"))
        {
            Debug.LogWarning("올바른 이메일 형식을 입력해주세요.");
            return false;
        }

        return true;
    }

    private bool ValidateUpdateInput(string email, string password)
    {
        if (!string.IsNullOrEmpty(email) && !email.Contains("@"))
        {
            Debug.LogWarning("올바른 이메일 형식을 입력해주세요.");
            return false;
        }
        return true;
    }
    #endregion

    #region Public Methods for NetworkManager Callbacks
    public void OnLoginSuccess(string usesrID)
    {
        usernameText.text = usesrID;
        ShowUserInfoPanel();
        ClearInputs();
        Debug.Log("로그인 성공!");
    }

    public void OnLoginFailed(string errorMessage)
    {
        Debug.LogError($"로그인 실패: {errorMessage}");
    }

    public void OnRegisterSuccess()
    {
        ShowLoginPanel();
        ClearInputs();
        Debug.Log("회원가입 성공! 로그인해주세요.");
    }

    public void OnRegisterFailed(string errorMessage)
    {
        Debug.LogError($"회원가입 실패: {errorMessage}");
    }

    public void OnUpdateSuccess()
    {
        ShowUserInfoPanel();
        Debug.Log("정보 업데이트 성공!");
    }

    public void OnUpdateFailed(string errorMessage)
    {
        Debug.LogError($"정보 업데이트 실패: {errorMessage}");
    }

    public void OnDeleteAccountSuccess()
    {
        ShowLoginPanel();
        ClearInputs();
        Debug.Log("계정이 삭제되었습니다.");
    }
    #endregion

    private void ClearInputs()
    {
        loginUsernameInput.text = "";
        loginPasswordInput.text = "";
        registerUsernameInput.text = "";
        registerPasswordInput.text = "";
        registerEmailInput.text = "";
        updateEmailInput.text = "";
        updatePasswordInput.text = "";
    }
}