using ChatClient2;
using CSBaseLib;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor.PackageManager;
using UnityEngine;
using static UnityEditor.U2D.ScriptablePacker;

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
    private static readonly Lazy<NetworkManager> _instance = new (() => new NetworkManager());

    public static NetworkManager Instance => _instance.Value;

    // 이벤트 정의 (UI 싱글톤과 함께 사용)
    public static event System.Action<string> OnLoginSuccess;
    public static event System.Action<string> OnLoginFailed;
    public static event System.Action OnRegisterSuccess;
    public static event System.Action<string> OnRegisterFailed;
    public static event System.Action OnUpdateSuccess;
    public static event System.Action<string> OnUpdateFailed;
    public static event System.Action OnDeleteAccountSuccess;

    private bool _isInitialized = false;

    CLIENT_STATE ClientState = CLIENT_STATE.NONE;

    ClientSimpleTcp Network = new ClientSimpleTcp();

    bool IsNetworkThreadRunning = false;
    bool IsBackGroundProcessRunning = false;

    System.Threading.Thread NetworkReadThread = null;
    System.Threading.Thread NetworkSendThread = null;
    System.Threading.Thread BackGroundProcessThread = null;

    PacketBufferManager PacketBuffer = new PacketBufferManager();
    Queue<PacketData> RecvPacketQueue = new Queue<PacketData>();
    Queue<byte[]> SendPacketQueue = new Queue<byte[]>();


    // 외부에서 인스턴스 생성 불가
    private NetworkManager()
    {
    }

    public void Initialize()
    {
        if (_isInitialized)
        {
            Debug.LogWarning("NetworkManager is already initialized.");
            return;
        }
        // 네트워크 초기화 코드 작성
        PacketBuffer.Init((8096 * 10), CSBaseLib.PacketDef.PACKET_HEADER_SIZE, 1024);

        IsNetworkThreadRunning = true;
        NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
        NetworkReadThread.Start();
        NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
        NetworkSendThread.Start();

        IsBackGroundProcessRunning = true;
        BackGroundProcessThread = new System.Threading.Thread(this.BackGroundProcess);
        BackGroundProcessThread.Start();

        bool bNetwork = Network.Connect("127.0.0.1", 32452);
        if (bNetwork)
        {
            ClientState = CLIENT_STATE.CONNECTED;
            Debug.Log("버에 연결 성공");
        }
        else
        {
            Debug.Log("서버에 연결 실패 !!!");
        }

        Debug.Log("NetworkManager initialized.");
        _isInitialized = true;
    }

    void BackGroundProcess()
    {        
        try
        {
            while(IsBackGroundProcessRunning)
            {
                var packet = new PacketData();

                lock (((System.Collections.ICollection)RecvPacketQueue).SyncRoot)
                {
                    if (RecvPacketQueue.Count() > 0)
                    {
                        packet = RecvPacketQueue.Dequeue();
                    }
                }

                if (packet.PacketID != 0)
                {
                    PacketProcess(packet);
                }
            }
            
        }
        catch (Exception ex)
        {
            Debug.Log(string.Format("ReadPacketQueueProcess. error:{0}", ex.Message));
        }
    }

    void NetworkReadProcess()
    {
        const Int16 PacketHeaderSize = CSBaseLib.PacketDef.PACKET_HEADER_SIZE;

        while (IsNetworkThreadRunning)
        {
            if (Network.IsConnected() == false)
            {
                System.Threading.Thread.Sleep(1);
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
                    {
                        break;
                    }

                    var packet = new PacketData();
                    packet.DataSize = (short)(data.Count - PacketHeaderSize);
                    packet.PacketID = BitConverter.ToInt16(data.Array, data.Offset + 2);
                    packet.Type = (SByte)data.Array[(data.Offset + 4)];
                    packet.BodyData = new byte[packet.DataSize];
                    Buffer.BlockCopy(data.Array, (data.Offset + PacketHeaderSize), packet.BodyData, 0, (data.Count - PacketHeaderSize));
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

    void NetworkSendProcess()
    {
        while (IsNetworkThreadRunning)
        {
            System.Threading.Thread.Sleep(1);

            if (Network.IsConnected() == false)
            {
                continue;
            }

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

    public void SetDisconnectd()
    {
        ClientState = CLIENT_STATE.NONE;

        SendPacketQueue.Clear();

        //ClearUIRoomOut();
        //labelStatus.Text = "서버 연결이 끊어짐";
    }

    public void PostSendPacket(byte[] sendData)
    {
        if (Network.IsConnected() == false)
        {
            Debug.Log("서버 연결이 되어 있지 않습니다");
            return;
        }

        SendPacketQueue.Enqueue(sendData);
    }

    public void SendScoreUpdate(int score)
    {
        var request = new CSBaseLib.PKTReqUserScoreUpdate() { UserID = "Test", NewScore = score };

        var Body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_USER_SCORE_UPDATE, Body);
        PostSendPacket(sendData);
    }

    public void SendLoginRequest(string username, string password)
    {
        var request = new CSBaseLib.PKTReqLogin() { UserID = username, Password = password };
        var Body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_LOGIN, Body);
        PostSendPacket(sendData);
        Debug.Log($"로그인 요청 전송: {username}");
    }

    public void SendRegisterRequest(string password, string userID)
    {
        var request = new CSBaseLib.PKTReqUserAccession() 
        { 
            UserID = userID, 
            Password = password, 
        };
        var Body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_USER_ACCESSION, Body);
        PostSendPacket(sendData);
        Debug.Log($"회원가입 요청 전송: {userID}");
    }

    public void SendUpdateUserRequest(string userID, string newPassword)
    {
        var request = new CSBaseLib.PKTReqUserInfoUpdate() 
        { 
            UserID = userID, 
            NewPassword = newPassword 
        };
        var Body = MessagePackSerializer.Serialize(request);
        var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_USER_INFO_UPDATE, Body);
        PostSendPacket(sendData);
        Debug.Log("사용자 정보 업데이트 요청 전송");
    }

    //public void SendDeleteAccountRequest()
    //{
    //    var request = new CSBaseLib.PKTReqUserInfoDelete() { };
    //    var Body = MessagePackSerializer.Serialize(request);
    //    var sendData = CSBaseLib.PacketToBytes.Make(CSBaseLib.PACKETID.REQ_USER_INFO_DELETE, Body);
    //    PostSendPacket(sendData);
    //    Debug.Log("계정 삭제 요청 전송");
    //}

    public void SendLogoutRequest()
    {
        // 로그아웃은 클라이언트에서 상태만 변경
        ClientState = CLIENT_STATE.CONNECTED;
        Debug.Log("로그아웃 완료");
    }

    void PacketProcess(PacketData packet)
    {
        switch ((PACKETID)packet.PacketID)
        {
            case PACKETID.REQ_RES_TEST_ECHO:
                {
                    Debug.Log(string.Format("Echo 응답: {0} - {1}", packet.BodyData.Length, LOG_LEVEL.INFO));                    
                    break;
                }
            case PACKETID.RES_LOGIN:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResLogin>(packet.BodyData);

                    if (resData.Result == (short)ERROR_CODE.NONE)
                    {
                        ClientState = CLIENT_STATE.LOGIN;
                        OnLoginSuccess?.Invoke(resData.UserID);
                        Debug.Log("로그인 성공");
                    }
                    else
                    {
                        OnLoginFailed?.Invoke(((ERROR_CODE)resData.Result).ToString());
                        Debug.Log($"로그인 실패: {((ERROR_CODE)resData.Result).ToString()}");
                    }
                }
                break;

            case PACKETID.RES_ROOM_ENTER:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResRoomEnter>(packet.BodyData);

                    if (resData.Result == (short)ERROR_CODE.NONE)
                    {
                        ClientState = CLIENT_STATE.ROOM;
                        Debug.Log("방 입장 성공");
                    }
                    else
                    {
                        Debug.Log(string.Format("방 입장 실패 : {0} {1}", resData.Result, ((ERROR_CODE)resData.Result).ToString()));
                    }
                }
                break;
            case PACKETID.NTF_ROOM_USER_LIST:
                {
                    
                }
                break;
            case PACKETID.NTF_ROOM_NEW_USER:
                {
                    
                }
                break;

            case PACKETID.RES_ROOM_LEAVE:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResRoomLeave>(packet.BodyData);

                   
                }
                break;
            case PACKETID.NTF_ROOM_LEAVE_USER:
                {
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomLeaveUser>(packet.BodyData);
                   
                }
                break;

            case PACKETID.NTF_ROOM_CHAT:
                {                   
                    var ntfData = MessagePackSerializer.Deserialize<PKTNtfRoomChat>(packet.BodyData);                 
                }
                break;

            case PACKETID.RES_USER_ACCESSION:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResUserAccession>(packet.BodyData);
                    
                    if (resData.Result == (short)ERROR_CODE.NONE)
                    {
                        OnRegisterSuccess?.Invoke();
                    }
                    else
                    {
                        OnRegisterFailed?.Invoke(((ERROR_CODE)resData.Result).ToString());
                    }
                }
                break;

            case PACKETID.RES_USER_INFO_UPDATE:
                {
                    var resData = MessagePackSerializer.Deserialize<PKTResUserInfoUpdate>(packet.BodyData);
                    
                    if (resData.Result == (short)ERROR_CODE.NONE)
                    {
                        OnUpdateSuccess?.Invoke();
                    }
                    else
                    {
                        OnUpdateFailed?.Invoke(((ERROR_CODE)resData.Result).ToString());
                    }
                }
                break;

            //case PACKETID.RES_USER_INFO_DELETE:
            //    {
            //        var resData = MessagePackSerializer.Deserialize<PKTResUserInfoDelete>(packet.BodyData);
            //        var authUI = FindObjectOfType<UserAuthUI>();
                    
            //        if (resData.Result == (short)ERROR_CODE.NONE)
            //        {
            //            authUI?.OnDeleteAccountSuccess();
            //            ClientState = CLIENT_STATE.CONNECTED;
            //        }
            //        else
            //        {
            //            Debug.LogError($"계정 삭제 실패: {((ERROR_CODE)resData.Result).ToString()}");
            //        }
            //    }
            //    break;

            case PACKETID.RES_USER_SCORE_UPDATE:
                {
                    Debug.Log("점수 업데이트 응답 받음");
                }
                break;
        }
    }

}
