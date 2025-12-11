
using System.Diagnostics;
using UnityEngine.SocialPlatforms.Impl;

public enum EAchievementCode
{
    PassSuccess,  // 통과 성공  1
    PassFail,     // 통과 실패  2
    TimeAttack,   // 타임 어택  3   
    AudioClip,    // 오디오 클립과 관련된 업적 4
    Lighting      // 조명과 관련된 업적 5
}

public class Achievement 
{
    public string imagePath; // 이미지 경로
    public string name; // 화면에 띄어줄 이름
    public int value; // 업적 값
    public int curvalue = 0; // 현재 업적 값
    public float count; // 시간값
    public EAchievementCode code; // 코드
    public bool isCompleted; // 완료 여부 플래그



    public void Complete()
    {
        isCompleted = true;
    }

    public void IncrementProgress()
    {
        curvalue++;

        if (curvalue >= value)
        {
            Complete();
        }

    }

    public Achievement(string name, int value, float count, EAchievementCode code, string imagePath)
    {
        this.imagePath = imagePath;
        this.name = name;
        this.value = value;
        this.count = count;
        this.code = code;
        this.isCompleted = false; // 처음엔 완료되지 않음
    }
}

