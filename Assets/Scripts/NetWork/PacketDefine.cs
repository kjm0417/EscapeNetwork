using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSBaseLib
{
    // 0 ~ 9999
    public enum ERROR_CODE : short
    {
        NONE                        = 0, // 에러가 아니다

        // 서버 초기화 에라
        REDIS_INIT_FAIL             = 1,    // Redis 초기화 에러

        // 로그인 
        LOGIN_INVALID_AUTHTOKEN             = 1001, // 로그인 실패: 잘못된 인증 토큰
        ADD_USER_DUPLICATION                = 1002,
        REMOVE_USER_SEARCH_FAILURE_USER_ID  = 1003,
        USER_AUTH_SEARCH_FAILURE_USER_ID    = 1004,
        USER_AUTH_ALREADY_SET_AUTH          = 1005,
        LOGIN_ALREADY_WORKING = 1006,
        LOGIN_FULL_USER_COUNT = 1007,

        DB_LOGIN_INVALID_PASSWORD   = 1011,
        DB_LOGIN_EMPTY_USER         = 1012,
        DB_LOGIN_EXCEPTION          = 1013,

        ROOM_ENTER_INVALID_STATE = 1021,
        ROOM_ENTER_INVALID_USER = 1022,
        ROOM_ENTER_ERROR_SYSTEM = 1023,
        ROOM_ENTER_INVALID_ROOM_NUMBER = 1024,
        ROOM_ENTER_FAIL_ADD_USER = 1025,
    }

    // 1 ~ 10000
    public enum PACKETID : int
    {
        REQ_RES_TEST_ECHO = 101,
        
        // 클라이언트
        CS_BEGIN        = 1001,

        NTF_MUST_CLOSE       = 1005,

        REQ_ROOM_DEV_ALL_ROOM_START_GAME = 1091,
        RES_ROOM_DEV_ALL_ROOM_START_GAME = 1092,

        REQ_ROOM_DEV_ALL_ROOM_END_GAME = 1093,
        RES_ROOM_DEV_ALL_ROOM_END_GAME = 1094,

        REQ_USER_SEARCH = 1101,
        RES_USER_SEARCH = 1102,

        REQ_USER_SCORE_UPDATE = 1103,
        RES_USER_SCORE_UPDATE = 1104,   

        CS_END          = 1200,


        // 시스템, 서버 - 서버
        SS_START    = 8001,

        NTF_IN_CONNECT_CLIENT = 8011,
        NTF_IN_DISCONNECT_CLIENT = 8012,

        REQ_SS_SERVERINFO = 8021,
        RES_SS_SERVERINFO = 8023,

        REQ_IN_ROOM_ENTER = 8031,
        RES_IN_ROOM_ENTER = 8032,

        NTF_IN_ROOM_LEAVE = 8036,


        // DB 8101 ~ 9000
        REQ_DB_LOGIN = 8101,
        RES_DB_LOGIN = 8102,

        //====================================================
        //내가 사용할 패킷 아이디
        //로그인
        REQ_LOGIN = 1002,
        RES_LOGIN = 1003,

        //회원가입
        REQ_USER_ACCESSION = 1095,
        RES_USER_ACCESSION = 1096,

        //회원정보 수정
        REQ_USER_INFO_UPDATE = 1097,
        RES_USER_INFO_UPDATE = 1098,

        //회원탈퇴
        REQ_USER_INFO_DELETE = 1099,
        RES_USER_INFO_DELETE = 1100,

        //채팅
        //방 입장
        REQ_ROOM_ENTER = 1015,
        RES_ROOM_ENTER = 1016,

        //채팅 방에서
        REQ_ROOM_CHAT = 1026,
        NTF_ROOM_CHAT = 1027,

        //채팅 1ㄷ1 귓말
        REQ_ROOM_WHISPER = 1028,
        NTF_ROOM_WHISPER = 1029,

        //룸에 들어온 리스트
        NTF_ROOM_USER_LIST = 1017,

        //새로 들어온 유저
        NTF_ROOM_NEW_USER = 1018,

        REQ_ROOM_LEAVE = 1021,
        RES_ROOM_LEAVE = 1022,
        NTF_ROOM_LEAVE_USER = 1023,

        //Ranking클리어 타임 제출
        RES_RANKING_SUBMIT = 6102,   
        REQ_RANKING_SUBMIT = 6101,     

        REQ_RANKING_GET_TOP = 6103,   
        RES_RANKING_GET_TOP = 6104,   

    }

    
    
}
