using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MainCanvas(메인 메뉴)의 버튼 4개를 관리하고, 각 화면 Canvas를 켜고 끈다.
/// </summary>
public class MainUI : MonoBehaviour
{
    [Header("Canvas Groups")]
    [SerializeField] private GameObject mainCanvasRoot;
    [SerializeField] private GameObject userInfoCanvas;
    [SerializeField] private GameObject chatRoomCanvas;
    [SerializeField] private GameObject rankingCanvas;

    [Header("Main Buttons")]
    [SerializeField] private Button btnGameStart;
    [SerializeField] private Button btnUserInfo;
    [SerializeField] private Button btnChatRoom;
    [SerializeField] private Button btnRanking;

    [Header("Optional Back Buttons (각 Canvas 안의 뒤로가기 버튼)")]
    [SerializeField] private Button btnBackFromUserInfo;
    [SerializeField] private Button btnBackFromChatRoom;
    [SerializeField] private Button btnBackFromRanking;

    /// <summary>
    /// 시작 시 기본 화면을 메인으로 설정하고 버튼을 연결한다
    /// </summary>
    private void Start()
    {
        BindButtons();
        ShowMain();
    }

    /// <summary>
    /// 버튼 클릭 이벤트를 연결한다
    /// </summary>
    private void BindButtons()
    {
        if (btnGameStart != null) btnGameStart.onClick.AddListener(OnClickGameStart);
        if (btnUserInfo != null) btnUserInfo.onClick.AddListener(ShowUserInfo);
        if (btnChatRoom != null) btnChatRoom.onClick.AddListener(ShowChatRoom);
        if (btnRanking != null) btnRanking.onClick.AddListener(ShowRanking);

        if (btnBackFromUserInfo != null) btnBackFromUserInfo.onClick.AddListener(ShowMain);
        if (btnBackFromChatRoom != null) btnBackFromChatRoom.onClick.AddListener(ShowMain);
        if (btnBackFromRanking != null) btnBackFromRanking.onClick.AddListener(ShowMain);
    }

    /// <summary>
    /// 게임 시작 버튼 처리(현재는 로그만 출력)
    /// </summary>
    private void OnClickGameStart()
    {
        Debug.Log("GameStart 클릭: 여기서 씬 이동/게임 시작 로직 연결");
    }

    /// <summary>
    /// 메인 메뉴 화면을 표시한다
    /// </summary>
    public void ShowMain()
    {
        if (mainCanvasRoot != null) mainCanvasRoot.SetActive(true);
        if (userInfoCanvas != null) userInfoCanvas.SetActive(false);
        if (chatRoomCanvas != null) chatRoomCanvas.SetActive(false);
        if (rankingCanvas != null) rankingCanvas.SetActive(false);
    }

    /// <summary>
    /// 유저 정보 화면을 표시한다
    /// </summary>
    public void ShowUserInfo()
    {
        if (mainCanvasRoot != null) mainCanvasRoot.SetActive(false);
        if (userInfoCanvas != null) userInfoCanvas.SetActive(true);
        if (chatRoomCanvas != null) chatRoomCanvas.SetActive(false);
        if (rankingCanvas != null) rankingCanvas.SetActive(false);
    }

    /// <summary>
    /// 채팅방 화면을 표시한다
    /// </summary>
    public void ShowChatRoom()
    {
        if (mainCanvasRoot != null) mainCanvasRoot.SetActive(false);
        if (userInfoCanvas != null) userInfoCanvas.SetActive(false);
        if (chatRoomCanvas != null) chatRoomCanvas.SetActive(true);
        if (rankingCanvas != null) rankingCanvas.SetActive(false);
    }

    /// <summary>
    /// 랭킹 화면을 표시한다
    /// </summary>
    public void ShowRanking()
    {
        if (mainCanvasRoot != null) mainCanvasRoot.SetActive(false);
        if (userInfoCanvas != null) userInfoCanvas.SetActive(false);
        if (chatRoomCanvas != null) chatRoomCanvas.SetActive(false);
        if (rankingCanvas != null) rankingCanvas.SetActive(true);
    }
}
