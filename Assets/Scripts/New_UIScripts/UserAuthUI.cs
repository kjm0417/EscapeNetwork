using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 로그인/회원가입 UI만 담당
/// </summary>
public class UserAuthUI : MonoBehaviour
{
    [Header("Canvas Root")]
    [SerializeField] private GameObject loginCanvas;
    [SerializeField] private GameObject mainCanvas;

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

    /// <summary>
    /// 초기 진입 시 네트워크 초기화를 보장
    /// </summary>
    private void Awake()
    {
        NetworkManager.Instance.Initialize();
    }

    /// <summary>
    /// 시작 시 UI 초기화 + 버튼 연결 + 이벤트 구독을 설정
    /// </summary>
    private void Start()
    {
        InitCanvasState();
        BindButtons();
        SubscribeNetworkEvents();
    }

    /// <summary>
    /// 오브젝트 파괴 시 이벤트 구독을 해제
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeNetworkEvents();
    }

    /// <summary>
    /// 로그인 상태에서 시작하도록 Canvas 활성/비활성을 세팅
    /// </summary>
    private void InitCanvasState()
    {
        if (loginCanvas != null) loginCanvas.SetActive(true);
        if (mainCanvas != null) mainCanvas.SetActive(false);

        ShowLoginPanel();
    }

    /// <summary>
    /// 버튼 클릭 이벤트를 연결
    /// </summary>
    private void BindButtons()
    {
        if (loginButton != null) loginButton.onClick.AddListener(OnClickLogin);
        if (showRegisterButton != null) showRegisterButton.onClick.AddListener(ShowRegisterPanel);

        if (registerButton != null) registerButton.onClick.AddListener(OnClickRegister);
        if (showLoginButton != null) showLoginButton.onClick.AddListener(ShowLoginPanel);
    }

    /// <summary>
    /// 네트워크 이벤트를 구독
    /// </summary>
    private void SubscribeNetworkEvents()
    {
        NetworkManager.OnLoginSuccess += HandleLoginSuccess;
        NetworkManager.OnLoginFailed += HandleLoginFailed;
        NetworkManager.OnRegisterSuccess += HandleRegisterSuccess;
        NetworkManager.OnRegisterFailed += HandleRegisterFailed;
    }

    /// <summary>
    /// 네트워크 이벤트 구독을 해제
    /// </summary>
    private void UnsubscribeNetworkEvents()
    {
        NetworkManager.OnLoginSuccess -= HandleLoginSuccess;
        NetworkManager.OnLoginFailed -= HandleLoginFailed;
        NetworkManager.OnRegisterSuccess -= HandleRegisterSuccess;
        NetworkManager.OnRegisterFailed -= HandleRegisterFailed;
    }

    /// <summary>
    /// 로그인 패널을 표시
    /// </summary>
    private void ShowLoginPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(true);
        if (registerPanel != null) registerPanel.SetActive(false);
    }

    /// <summary>
    /// 회원가입 패널을 표시
    /// </summary>
    private void ShowRegisterPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
    }

    /// <summary>
    /// 로그인 버튼 클릭 처리
    /// </summary>
    private void OnClickLogin()
    {
        string userId = loginUsernameInput != null ? loginUsernameInput.text : "";
        string pw = loginPasswordInput != null ? loginPasswordInput.text : "";

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(pw))
        {
            Debug.LogWarning("아이디/비밀번호를 입력해주세요.");
            return;
        }

        NetworkManager.Instance.SendLoginRequest(userId, pw);
    }

    /// <summary>
    /// 회원가입 버튼 클릭 처리
    /// </summary>
    private void OnClickRegister()
    {
        string userId = registerUsernameInput != null ? registerUsernameInput.text : "";
        string pw = registerPasswordInput != null ? registerPasswordInput.text : "";

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(pw))
        {
            Debug.LogWarning("아이디/비밀번호를 입력해주세요.");
            return;
        }

        NetworkManager.Instance.SendRegisterRequest(pw, userId);
    }

    /// <summary>
    /// 로그인 성공 시 MainCanvas로 전환
    /// </summary>
    private void HandleLoginSuccess(string userId)
    {
        ClearInputs();

        if (loginCanvas != null) loginCanvas.SetActive(false);
        if (mainCanvas != null) mainCanvas.SetActive(true);

        Debug.Log($"로그인 성공: {userId}");
    }

    /// <summary>
    /// 로그인 실패 메시지를 출력
    /// </summary>
    private void HandleLoginFailed(string error)
    {
        Debug.LogError($"로그인 실패: {error}");
    }

    /// <summary>
    /// 회원가입 성공 시 로그인 패널로 복귀
    /// </summary>
    private void HandleRegisterSuccess()
    {
        ShowLoginPanel();
        ClearInputs();
        Debug.Log("회원가입 성공! 로그인해주세요.");
    }

    /// <summary>
    /// 회원가입 실패 메시지를 출력
    /// </summary>
    private void HandleRegisterFailed(string error)
    {
        Debug.LogError($"회원가입 실패: {error}");
    }

    /// <summary>
    /// 입력 필드를 초기화
    /// </summary>
    private void ClearInputs()
    {
        if (loginUsernameInput != null) loginUsernameInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";

        if (registerUsernameInput != null) registerUsernameInput.text = "";
        if (registerPasswordInput != null) registerPasswordInput.text = "";
    }
}
