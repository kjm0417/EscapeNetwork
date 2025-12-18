using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CSBaseLib;

/// <summary>
/// ChatRoomCanvas 전용 UI를 관리
/// </summary>
public class ChatRoomUI : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private MainUI mainUI;

    [Header("Panels")]
    [SerializeField] private GameObject grpRoomNumber;   // Grp_RoomNumber
    [SerializeField] private GameObject pnlChatBox;      // Pnl_ChatBox

    [Header("Room Enter UI")]
    [SerializeField] private TMP_InputField inputRoomNumber; // Input_field_RoomNum
    [SerializeField] private Button btnEnterRoom;            // Btn_in
    [SerializeField] private TMP_Text txtRoomTitle;          // Room : {n}

    [Header("Exit UI")]
    [SerializeField] private Button btnRoomOut;              // Btn_RoomOut

    [Header("Members UI")]
    [SerializeField] private TMP_Text txtMemberList;
    [SerializeField] private TMP_Dropdown dropdownWhisperTarget;

    [Header("Chat UI")]
    [SerializeField] private TMP_InputField inputChat;
    [SerializeField] private Button btnSend;
    [SerializeField] private TMP_Text txtChatContent;

    private const string ALL_TARGET = "ALL";

    private readonly List<string> _members = new List<string>();

    /// <summary>
    /// 시작 시 버튼/이벤트를 연결하고 초기 화면 상태를 세팅한다
    /// </summary>
    private void Start()
    {
        BindButtons();
        SubscribeNetworkEvents();

        SetEnteredState(false);
        RefreshRoomText(PacketDef.INVALID_ROOM_NUMBER);
        RefreshMemberUI();
        AppendSystemMessage("룸 번호를 입력하고 입장하세요.");
    }

    /// <summary>
    /// 오브젝트 파괴 시 네트워크 이벤트 구독을 해제한다
    /// </summary>
    private void OnDestroy()
    {
        UnsubscribeNetworkEvents();
    }

    /// <summary>
    /// 버튼 이벤트를 연결한다
    /// </summary>
    private void BindButtons()
    {
        if (btnEnterRoom != null) btnEnterRoom.onClick.AddListener(OnClickRoomEnter);
        if (btnRoomOut != null) btnRoomOut.onClick.AddListener(OnClickRoomOut);
        if (btnSend != null) btnSend.onClick.AddListener(OnClickSend);
    }

    /// <summary>
    /// 네트워크 이벤트를 구독한다
    /// </summary>
    private void SubscribeNetworkEvents()
    {
        NetworkManager.OnRoomEnterSuccess += HandleRoomEnterSuccess;
        NetworkManager.OnRoomEnterFailed += HandleRoomEnterFailed;

        NetworkManager.OnRoomUserList += HandleRoomUserList;
        NetworkManager.OnRoomNewUser += HandleRoomNewUser;
        NetworkManager.OnRoomLeaveUser += HandleRoomLeaveUser;

        NetworkManager.OnRoomChat += HandleRoomChat;
        NetworkManager.OnRoomWhisper += HandleRoomWhisper;
    }

    /// <summary>
    /// 네트워크 이벤트 구독을 해제한다
    /// </summary>
    private void UnsubscribeNetworkEvents()
    {
        NetworkManager.OnRoomEnterSuccess -= HandleRoomEnterSuccess;
        NetworkManager.OnRoomEnterFailed -= HandleRoomEnterFailed;

        NetworkManager.OnRoomUserList -= HandleRoomUserList;
        NetworkManager.OnRoomNewUser -= HandleRoomNewUser;
        NetworkManager.OnRoomLeaveUser -= HandleRoomLeaveUser;

        NetworkManager.OnRoomChat -= HandleRoomChat;
        NetworkManager.OnRoomWhisper -= HandleRoomWhisper;
    }

    /// <summary>
    /// 룸 입장 전/후 UI 상태를 토글한다
    /// </summary>
    private void SetEnteredState(bool entered)
    {
        if (grpRoomNumber != null) grpRoomNumber.SetActive(!entered);
        if (pnlChatBox != null) pnlChatBox.SetActive(entered);
    }

    /// <summary>
    /// 룸 입장 버튼 클릭 처리(입력된 RoomNumber로 입장 요청)
    /// </summary>
    private void OnClickRoomEnter()
    {
        if (inputRoomNumber == null)
        {
            AppendSystemMessage("RoomNumber InputField가 연결되지 않았습니다.");
            return;
        }

        if (!int.TryParse(inputRoomNumber.text, out int roomNumber))
        {
            AppendSystemMessage("RoomNumber는 숫자만 입력해주세요.");
            return;
        }

        if (roomNumber < 0)
        {
            AppendSystemMessage("RoomNumber는 0 이상이어야 합니다.");
            return;
        }

        NetworkManager.Instance.SetPendingRoomNumber(roomNumber);
        NetworkManager.Instance.SendRoomEnterRequest(roomNumber);

        AppendSystemMessage($"룸 입장 요청: {roomNumber}");
    }

    /// <summary>
    /// 채팅 전송 버튼 클릭 처리(ALL이면 전쳇, 특정 유저면 귓속말)
    /// </summary>
    private void OnClickSend()
    {
        if (inputChat == null) return;

        string msg = inputChat.text;
        if (string.IsNullOrEmpty(msg)) return;

        string target = GetSelectedTarget();

        // ✅ 전쳇
        if (target == ALL_TARGET)
        {
            NetworkManager.Instance.SendRoomChat(msg);
            inputChat.text = "";
            return;
        }

        // ✅ 귓속말
        if (string.IsNullOrEmpty(target))
        {
            AppendSystemMessage("대상이 올바르지 않습니다.");
            return;
        }

        // TODO: 네 NetworkManager 프로퍼티명에 맞춰 통일해줘야 함 (CurrentUserId vs CurrentUserID)
        // 아래 줄에서 컴파일 에러 나면 NetworkManager 쪽 이름이 다른 거야.
        if (target == NetworkManager.Instance.CurrentUserID)
        {
            AppendSystemMessage("자기 자신에게 귓속말은 보낼 수 없습니다.");
            return;
        }

        NetworkManager.Instance.SendRoomWhisper(target, msg);
        inputChat.text = "";
    }

    /// <summary>
    /// 룸 나가기 버튼 클릭 처리(서버에 퇴장 요청 후 UI 초기화)
    /// </summary>
    private void OnClickRoomOut()
    {
        NetworkManager.Instance.SendRoomLeaveRequest();

        _members.Clear();
        RefreshMemberUI();
        RefreshRoomText(PacketDef.INVALID_ROOM_NUMBER);
        SetEnteredState(false);

        AppendSystemMessage("룸 퇴장 요청");

        if (mainUI != null)
        {
            mainUI.ShowMain();
        }
    }

    /// <summary>
    /// 룸 입장 성공 처리: 채팅 UI를 켜고 룸 번호를 표시한다
    /// </summary>
    private void HandleRoomEnterSuccess(int roomNumber)
    {
        SetEnteredState(true);
        RefreshRoomText(roomNumber);
        AppendSystemMessage($"룸 입장 성공: {roomNumber}");
    }

    /// <summary>
    /// 룸 입장 실패 처리
    /// </summary>
    private void HandleRoomEnterFailed(string error)
    {
        AppendSystemMessage($"룸 입장 실패: {error}");
    }

    /// <summary>
    /// 룸 유저 리스트 수신 처리(전체 목록 갱신)
    /// </summary>
    private void HandleRoomUserList(List<string> users)
    {
        _members.Clear();
        if (users != null) _members.AddRange(users);

        RefreshMemberUI();
        AppendSystemMessage($"현재 인원: {_members.Count}");
    }

    /// <summary>
    /// 신규 유저 입장 알림 처리(목록에 추가)
    /// </summary>
    private void HandleRoomNewUser(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;

        if (_members.Contains(userId) == false)
        {
            _members.Add(userId);
            RefreshMemberUI();
        }

        AppendSystemMessage($"{userId} 입장");
    }

    /// <summary>
    /// 유저 퇴장 알림 처리(목록에서 제거)
    /// </summary>
    private void HandleRoomLeaveUser(string userId)
    {
        if (string.IsNullOrEmpty(userId)) return;

        _members.Remove(userId);
        RefreshMemberUI();
        AppendSystemMessage($"{userId} 퇴장");
    }

    /// <summary>
    /// 방 전체 채팅 수신 처리(채팅창에 출력)
    /// </summary>
    private void HandleRoomChat(string userId, string msg)
    {
        AppendChatLine($"{userId} : {msg}");
    }

    /// <summary>
    /// 귓속말 수신 처리(보낸사람/받는사람만 오므로 그대로 출력)
    /// </summary>
    private void HandleRoomWhisper(string from, string to, string msg)
    {
        AppendChatLine($"[WHISPER] {from} -> {to} : {msg}");
    }

    /// <summary>
    /// 룸 텍스트를 갱신한다
    /// </summary>
    private void RefreshRoomText(int roomNumber)
    {
        if (txtRoomTitle == null) return;

        if (roomNumber == PacketDef.INVALID_ROOM_NUMBER)
            txtRoomTitle.text = "Room : -";
        else
            txtRoomTitle.text = $"Room : {roomNumber}";
    }

    /// <summary>
    /// 멤버 목록(Text/Dropdown)을 갱신한다
    /// Dropdown은 [ALL, user1, user2, ...] 형태로 구성한다
    /// </summary>
    private void RefreshMemberUI()
    {
        if (txtMemberList != null)
        {
            if (_members.Count == 0)
                txtMemberList.text = "Member\n-";
            else
                txtMemberList.text = "Member\n" + string.Join("\n", _members);
        }

        if (dropdownWhisperTarget != null)
        {
            dropdownWhisperTarget.ClearOptions();

            var options = new List<string>();
            options.Add(ALL_TARGET);          // ✅ 전쳇 기본값
            options.AddRange(_members);

            dropdownWhisperTarget.AddOptions(options);
            dropdownWhisperTarget.value = 0;  // ✅ 기본은 ALL
            dropdownWhisperTarget.RefreshShownValue();
        }
    }

    /// <summary>
    /// 드롭다운에서 선택된 대상(ALL 또는 유저ID)을 가져온다
    /// </summary>
    private string GetSelectedTarget()
    {
        if (dropdownWhisperTarget == null) return ALL_TARGET;
        if (dropdownWhisperTarget.options == null || dropdownWhisperTarget.options.Count == 0) return ALL_TARGET;

        int idx = dropdownWhisperTarget.value;
        if (idx < 0 || idx >= dropdownWhisperTarget.options.Count) return ALL_TARGET;

        return dropdownWhisperTarget.options[idx].text;
    }

    /// <summary>
    /// 채팅 텍스트에 한 줄을 추가한다
    /// </summary>
    private void AppendChatLine(string line)
    {
        if (txtChatContent == null) return;

        if (string.IsNullOrEmpty(txtChatContent.text))
            txtChatContent.text = line;
        else
            txtChatContent.text += "\n" + line;
    }

    /// <summary>
    /// 시스템 메시지를 채팅창에 출력한다
    /// </summary>
    private void AppendSystemMessage(string msg)
    {
        if (ShouldShowSystemMessage(msg) == false)
            return;


        AppendChatLine($"[SYSTEM] {msg}");
    }

    /// <summary>
    /// 시스템 메시지를 채팅창에 보여줄지 판단한다
    /// </summary>
    private bool ShouldShowSystemMessage(string msg)
    {
        if (string.IsNullOrEmpty(msg)) return false;

        //남기고 싶은 것들만
        // 1) 현재 인원
        if (msg.StartsWith("현재 인원:"))
            return true;

        // 2) 누가 입장/퇴장
        if (msg.EndsWith(" 입장") || msg.EndsWith(" 퇴장"))
            return true;

        return false;
    }
}
