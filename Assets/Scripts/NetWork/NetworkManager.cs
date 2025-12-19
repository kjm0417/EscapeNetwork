using ChatClient2;
using CSBaseLib;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

enum CLIENT_STATE
{
    NONE = 0,
    CONNECTED = 1,
    LOGIN = 2,
    ROOM = 3
}

public enum LOG_LEVEL
{
    TRACE,
    DEBUG,
    INFO,
    WARN,
    ERROR,
    DISABLE
}

struct PacketData
{
    public Int16 DataSize;
    public Int16 PacketID;
    public SByte Type;
    public byte[] BodyData;
}

public sealed class NetworkManager
{
    private static readonly Lazy<NetworkManager> _instance = new(() => new NetworkManager());
    public static NetworkManager Instance => _instance.Value;

    // 로그인, 회원가입
    public static event Action<string> OnLoginSuccess;
    public static event Action<string> OnLoginFailed;
    public static event Action OnRegisterSuccess;
    public static event Action<string> OnRegisterFailed;

    //업데이트, 삭제
    public static event Action OnUpdateSuccess;
    public static event Action<string> OnUpdateFailed;
    public static event Action OnDeleteAccountSuccess;
    public static event Action<string> OnDeleteAccountFailed;

    //채팅관련 코드
    public static event Action<int> OnRoomEnterSuccess;
    public static event Action<string> OnRoomEnterFailed;

    public static event Action<List<string>> OnRoomUserList;
    public static event Action<string> OnRoomNewUser;
    public static event Action<string> OnRoomLeaveUser;

    public static event Action<string, string> OnRoomChat; 
    public static event Action<string, string, string> OnRoomWhisper; 

    //Ranking
    public static event Action<List<RankingItem>> OnRankingTopReceived;
    public static event Action<string> OnRankingError;

    private bool _isInitialized = false;

    CLIENT_STATE ClientState = CLIENT_STATE.NONE;

    ClientSimpleTcp Network = new ClientSimpleTcp();

    bool IsNetworkThreadRunning = false;
    bool IsBackGroundProcessRunning = false;

    Thread NetworkReadThread = null;
    Thread NetworkSendThread = null;
    Thread BackGroundProcessThread = null;

    PacketBufferManager PacketBuffer = new PacketBufferManager();
    Queue<PacketData> RecvPacketQueue = new Queue<PacketData>();
    Queue<byte[]> SendPacketQueue = new Queue<byte[]>();

    public string CurrentUserID { get; private set; } //현재 로그인 되어 있는 ID
    public int CurrentRoomNumber { get; private set; } = PacketDef.INVALID_ROOM_NUMBER;

    /// <summary>
    /// 외부에서 인스턴스 생성 불가
    /// </summary>
    private NetworkManager()
    {
    }

    /// <summary>
    /// 네트워크 스레드/버퍼를 초기화하고 서버에 접속
    /// </summary>
    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("NetworkManager is already initialized.");
            return;
        }

        PacketBuffer.Init((8096 * 10), CSBaseLib.PacketDef.PACKET_HEADER_SIZE, 1024);

        IsNetworkThreadRunning = true;
        NetworkReadThread = new Thread(this.NetworkReadProcess);
        NetworkReadThread.Start();

        NetworkSendThread = new Thread(this.NetworkSendProcess);
        NetworkSendThread.Start();

        IsBackGroundProcessRunning = true;
        BackGroundProcessThread = new Thread(this.BackGroundProcess);
        BackGroundProcessThread.Start();

        bool bNetwork = Network.Connect("127.0.0.1", 32452);
        if (bNetwork)
        {
            ClientState = CLIENT_STATE.CONNECTED;
            Debug.Log("서버에 연결 성공");
        }
        else
        {
            Debug.Log("서버에 연결 실패 !!!");
        }

        Debug.Log("NetworkManager initialized.");
        _isInitialized = true;
    }

    public void SetPendingRoomNumber(int roomNumber)
    {
        CurrentRoomNumber = roomNumber;
    }

    /// <summary>
    /// 수신 큐(RecvPacketQueue)에 쌓인 패킷을 꺼내서 처리한다 (백그라운드 스레드)
    /// </summary>
    void BackGroundProcess()
    {
        try
        {
            while (IsBackGroundProcessRunning)
            {
                PacketData packet = new PacketData();

                lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                {
                    if (RecvPacketQueue.Count > 0)
                    {
                        packet = RecvPacketQueue.Dequeue();
                    }
                }

                if (packet.PacketID != 0)
                {
                    PacketProcess(packet);
                }

                Thread.Sleep(1);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(string.Format("ReadPacketQueueProcess. error:{0}", ex.Message));
        }
    }

    /// <summary>
    /// 서버로부터 데이터를 수신하여 PacketBuffer에 쌓고, 완전한 패킷을 RecvPacketQueue에 넣는다
    /// </summary>
    void NetworkReadProcess()
    {
        const Int16 PacketHeaderSize = CSBaseLib.PacketDef.PACKET_HEADER_SIZE;

        while (IsNetworkThreadRunning)
        {
            if (Network.IsConnected() == false)
            {
                Thread.Sleep(1);
                continue;
            }

            Thread.Sleep(1);

            var recvData = Network.Receive();

            if (recvData != null)
            {
                PacketBuffer.Write(recvData.Item2, 0, recvData.Item1);

                while (true)
                {
                    var data = PacketBuffer.Read();
                    if (data.Count < 1)
                        break;

                    PacketData packet = new PacketData();
                    packet.DataSize = (short)(data.Count - PacketHeaderSize);
                    packet.PacketID = BitConverter.ToInt16(data.Array, data.Offset + 2);
                    packet.Type = (SByte)data.Array[(data.Offset + 4)];
                    packet.BodyData = new byte[packet.DataSize];

                    Buffer.BlockCopy(
                        data.Array,
                        (data.Offset + PacketHeaderSize),
                        packet.BodyData,
                        0,
                        (data.Count - PacketHeaderSize));

                    lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                    {
                        RecvPacketQueue.Enqueue(packet);
                    }
                }
            }
            else
            {
                Network.Close();
                SetDisconnectd();
                Debug.Log(string.Format("서버 연결이 끊어짐 !!! {0}", LOG_LEVEL.INFO));
            }
        }
    }

    /// <summary>
    /// SendPacketQueue에 쌓인 패킷을 서버로 전송한다
    /// </summary>
    void NetworkSendProcess()
    {
        while (IsNetworkThreadRunning)
        {
            Thread.Sleep(1);

            if (Network.IsConnected() == false)
                continue;

            lock (((System.Collections.ICollection)SendPacketQueue).SyncRoot)
            {
                if (SendPacketQueue.Count > 0)
                {
                    var packet = SendPacketQueue.Dequeue();
                    Network.Send(packet);
                }
            }
        }
    }

    /// <summary>
    /// 서버 연결이 끊어졌을 때 상태를 초기화한다
    /// </summary>
    public void SetDisconnectd()
    {
        ClientState = CLIENT_STATE.NONE;
        CurrentRoomNumber = PacketDef.INVALID_ROOM_NUMBER;
        CurrentUserID = string.Empty;
        SendPacketQueue.Clear();    
    }

    /// <summary>
    /// 전송할 패킷을 SendPacketQueue에 넣는다
    /// </summary>
    public void PostSendPacket(byte[] sendData)
    {
        if (Network.IsConnected() == false)
        {
            Debug.Log("서버 연결이 되어 있지 않습니다");
            return;
        }

        lock (((System.Collections.ICollection)SendPacketQueue).SyncRoot)
        {
            SendPacketQueue.Enqueue(sendData);
        }
    }

    /// <summary>
    /// 로그인 요청 패킷을 전송
    /// </summary>
    public void SendLoginRequest(string username, string password)
    {
        var request = new CSBaseLib.PKTReqLogin() { UserID = username, Password = password };
        var body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_LOGIN, body);
        PostSendPacket(sendData);
        Debug.Log($"로그인 요청 전송: {username}");
    }

    /// <summary>
    /// 회원가입 요청 패킷을 전송한다
    /// </summary>
    public void SendRegisterRequest(string password, string userID)
    {
        var request = new CSBaseLib.PKTReqUserAccession()
        {
            UserID = userID,
            Password = password,
        };

        var body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_USER_ACCESSION, body);
        PostSendPacket(sendData);
        Debug.Log($"회원가입 요청 전송: {userID}");
    }

    /// <summary>
    /// (핵심) 유저 Level 업데이트 요청 패킷을 전송
    /// - CSBaseLib.PKTReqUserInfoUpdate에 Level 필드가 있어야 한다
    /// </summary>
    public void SendUpdateLevelRequest(string userID, string newLevel)
    {
        var request = new CSBaseLib.PKTReqUserInfoUpdate()
        {
            UserID = userID,
            Level = newLevel
        };

        var body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_USER_INFO_UPDATE, body);
        PostSendPacket(sendData);
        Debug.Log($"유저 Level 업데이트 요청 전송: {userID} -> Level:{newLevel}");
    }

    /// <summary>
    /// 계정 삭제 요청 패킷을 전송
    /// </summary>
    public void SendDeleteAccountRequest()
    {
        if (string.IsNullOrEmpty(CurrentUserID))
        {
            Debug.LogWarning("로그인된 유저가 없습니다.");
            return;
        }

        var request = new CSBaseLib.PKTReqUserInfoDelete()
        {
            UserID = CurrentUserID
        };

        var body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_USER_INFO_DELETE,body);

        PostSendPacket(sendData);
        Debug.Log($"계정 삭제 요청 전송: {CurrentUserID}");
    }

    /// <summary>
    /// 로그아웃 요청(클라이언트 상태만 변경)
    /// </summary>
    public void SendLogoutRequest()
    {
        CurrentUserID = string.Empty;
        ClientState = CLIENT_STATE.CONNECTED;
        Debug.Log("로그아웃 완료");
    }

    /// <summary>
    /// 룸 입장 요청을 전송
    /// </summary>
    public void SendRoomEnterRequest(int roomNumber)
    {
        var request = new CSBaseLib.PKTReqRoomEnter() { RoomNumber = roomNumber };

        var body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_ROOM_ENTER, body);
        PostSendPacket(sendData);
    }

    /// <summary>
    /// 룸 퇴장 요청을 전송
    /// </summary>
    public void SendRoomLeaveRequest()
    {
        var request = new CSBaseLib.PKTReqRoomLeave() { };
        var body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_ROOM_LEAVE, body);
        PostSendPacket(sendData);
    }

    /// <summary>
    /// 방 전체 채팅 요청을 전송
    /// </summary>
    public void SendRoomChat(string message)
    {
        var request = new CSBaseLib.PKTReqRoomChat() { ChatMessage = message };
        var body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_ROOM_CHAT, body);
        PostSendPacket(sendData);
    }

    /// <summary>
    /// 방 안 1:1 귓속말 요청을 전송
    /// </summary>
    public void SendRoomWhisper(string toUserId, string message)
    {
        var request = new CSBaseLib.PKTReqRoomWhisper() { ToUserID = toUserId, ChatMessage = message };
        var body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_ROOM_WHISPER, body);
        PostSendPacket(sendData);
    }

    /// <summary>
    /// 게임 클리어 타임(ms)을 서버에 제출
    /// </summary>
    public void SendRankingSubmit(int clearTimeMs)
    {
        var req = new PKTReqRankingSubmit
        {
            UserID = CurrentUserID,
            ClearTimeMs = clearTimeMs
        };

        var body = MessagePackSerializer.Serialize(req);
        var sendData = PacketToBytes.Make(CSBaseLib.PACKETID.REQ_RANKING_SUBMIT, body);
        PostSendPacket(sendData);
    }

    /// <summary>
    /// Top N 랭킹을 요청
    /// </summary>
    public void SendRankingGetTop(int count)
    {
        var req = new PKTReqRankingGetTop
        {
            Count = count
        };

        var body = MessagePackSerializer.Serialize(req);
        var sendData = PacketToBytes.Make(CSBaseLib.PACKETID.REQ_RANKING_GET_TOP, body);
        PostSendPacket(sendData);
    }

  

    /// <summary>
    /// 수신된 패킷을 처리
    /// </summary>
    void PacketProcess(PacketData packet)
    {
        switch ((PACKETID)packet.PacketID)
        {
            case PACKETID.RES_LOGIN:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResLogin>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        if (resData.Result == (short)ERROR_CODE.NONE)
                        {
                            ClientState = CLIENT_STATE.LOGIN;

                            CurrentUserID = resData.UserID;

                            OnLoginSuccess?.Invoke(resData.UserID);
                            Debug.Log("로그인 성공");
                        }
                        else
                        {
                            OnLoginFailed?.Invoke(((ERROR_CODE)resData.Result).ToString());
                            Debug.Log($"로그인 실패: {((ERROR_CODE)resData.Result).ToString()}");
                        }
                    });
                }
                break;

            case PACKETID.RES_USER_ACCESSION:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResUserAccession>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        if (resData.Result == (short)ERROR_CODE.NONE)
                        {
                            OnRegisterSuccess?.Invoke();
                            Debug.Log("회원가입 성공");
                        }
                        else
                        {
                            OnRegisterFailed?.Invoke(((ERROR_CODE)resData.Result).ToString());
                            Debug.Log($"회원가입 실패: {((ERROR_CODE)resData.Result).ToString()}");
                        }
                    });
                }
                break;

            case PACKETID.RES_USER_INFO_UPDATE:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResUserInfoUpdate>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        if (resData.Result == (short)ERROR_CODE.NONE)
                        {
                            OnUpdateSuccess?.Invoke();
                            Debug.Log("유저 Level 업데이트 성공");
                        }
                        else
                        {
                            OnUpdateFailed?.Invoke(((ERROR_CODE)resData.Result).ToString());
                            Debug.Log($"유저 Level 업데이트 실패: {((ERROR_CODE)resData.Result).ToString()}");
                        }
                    });
                }
                break;

            case PACKETID.RES_USER_INFO_DELETE:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResUserInfoDelete>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        if (resData.Result == (short)ERROR_CODE.NONE)
                        {
                            OnDeleteAccountSuccess?.Invoke();
                            ClientState = CLIENT_STATE.CONNECTED;
                            Debug.Log("계정 삭제 성공");
                        }
                        else
                        {
                            OnDeleteAccountFailed?.Invoke(((ERROR_CODE)resData.Result).ToString());
                            Debug.Log($"계정 삭제 실패: {((ERROR_CODE)resData.Result).ToString()}");
                        }
                    });
                }
                break;

            case PACKETID.RES_ROOM_ENTER:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResRoomEnter>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        if (resData.Result == (short)ERROR_CODE.NONE)
                        {
                            ClientState = CLIENT_STATE.ROOM;
                            OnRoomEnterSuccess?.Invoke(CurrentRoomNumber);
                            Debug.Log($"방 입장 성공. RoomNumber={CurrentRoomNumber}");
                        }
                        else
                        {
                            var err = ((ERROR_CODE)resData.Result).ToString();
                            OnRoomEnterFailed?.Invoke(err);
                            Debug.Log($"방 입장 실패: {err}");
                        }
                    });
                }
                break;

            case PACKETID.NTF_ROOM_USER_LIST:
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomUserList>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        OnRoomUserList?.Invoke(ntfData.UserIDList);
                    });
                }
                break;

            case PACKETID.NTF_ROOM_NEW_USER:
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomNewUser>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        OnRoomNewUser?.Invoke(ntfData.UserID);
                    });
                }
                break;

            case PACKETID.NTF_ROOM_LEAVE_USER:
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomLeaveUser>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        OnRoomLeaveUser?.Invoke(ntfData.UserID);
                    });
                }
                break;

            case PACKETID.NTF_ROOM_CHAT:
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomChat>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        OnRoomChat?.Invoke(ntfData.UserID, ntfData.ChatMessage);
                    });
                }
                break;

            case PACKETID.NTF_ROOM_WHISPER:
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomWhisper>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        OnRoomWhisper?.Invoke(ntfData.FromUserID, ntfData.ToUserID, ntfData.ChatMessage);
                    });
                }
                break;

            case PACKETID.RES_ROOM_LEAVE:
                {
                    // 상태 정리는 네트워크 스레드에서 해도 되지만, UI와 동시에 엮이면 Post로 통일하는 게 안전함
                    MainThreadDispatcher.Post(() =>
                    {
                        ClientState = CLIENT_STATE.LOGIN;
                        CurrentRoomNumber = PacketDef.INVALID_ROOM_NUMBER;
                        Debug.Log("방 퇴장 완료");
                    });
                }
                break;

            case PACKETID.RES_RANKING_SUBMIT:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResRankingSubmit>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        if (resData.Result == (short)ERROR_CODE.NONE)
                        {
                            Debug.Log($"랭킹 제출 성공: {resData.ClearTimeMs}ms");
                        }
                        else
                        {
                            var err = ((ERROR_CODE)resData.Result).ToString();
                            Debug.LogError($"랭킹 제출 실패: {err}");
                            OnRankingError?.Invoke(err);
                        }
                    });
                }
                break;

            case PACKETID.RES_RANKING_GET_TOP:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResRankingGetTop>(packet.BodyData);

                    MainThreadDispatcher.Post(() =>
                    {
                        if (resData.Result == (short)ERROR_CODE.NONE)
                        {
                            OnRankingTopReceived?.Invoke(resData.Items);
                        }
                        else
                        {
                            var err = ((ERROR_CODE)resData.Result).ToString();
                            Debug.LogError($"랭킹 조회 실패: {err}");
                            OnRankingError?.Invoke(err);
                        }
                    });
                }
                break;
            default:
                break;
        }
    }

}
