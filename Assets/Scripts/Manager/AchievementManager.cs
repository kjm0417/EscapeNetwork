using System;
using System.Collections.Generic;
using UnityEngine;
public class AchievementManager : Singleton<AchievementManager>
{
    [SerializeField]
    private AchievementUI achievementUI;
    // 업적을 딕셔너리로 관리
    private Dictionary<EAchievementCode, List<Achievement>> achievements = new Dictionary<EAchievementCode, List<Achievement>>();
    // 업적이 달성 되기 위한 추가 과정
    public void IncreseAchievement(EAchievementCode code, float time = 0)
    {
        if (achievements.ContainsKey(code)) //해당 업적이 딕셔너리에 있으면
        {
            List<Achievement> list = achievements[code]; // 그 딕셔너리를 List에 넣는다.
            foreach (Achievement achievement in list) // 해당 업적 리스트를 전체 순회하면서
            {
                if (time != 0 && achievement.count >= time) // 시간이 0 이 아니거나 시간이 오버된경우
                {
                    continue; // 업적UI 비활성
                }
                // 3연속 실패 하다가 성공 나왔을 때 초기화
                if (code == EAchievementCode.PassFail) { ResetAchievement(EAchievementCode.PassSuccess); }
                // 4연속 성공하다가 실패 나왔을 때 초기화
                if (code == EAchievementCode.PassSuccess) { ResetAchievement(EAchievementCode.PassFail); }
                if (!achievement.isCompleted) // 성공하지 않았다면
                {
                    achievement.IncrementProgress(); // curvalue ++
                    if (achievement.isCompleted) // 성공 시
                    {
                        ShowAchievementUI(achievement); // UI 띄우기
                    }
                }
            }
        }
    }

    public void ResetAchievement(EAchievementCode resetCode) //연속을 위해 초기화 해주는 코드
    {
        if (!achievements.ContainsKey(resetCode))// 업적이 없으면
        {
            return; // 넘기기
        }
        List<Achievement> list = achievements[resetCode]; //업적이 있으면 리스트에 넣어주고
        foreach (Achievement achievement in list)
        {
            achievement.curvalue = 0;
        }
    }

    public void AddAchievement(Achievement achievement)
    { //새로운 업적을 딕셔너리에 추가
        if (!achievements.ContainsKey(achievement.code)) //해당 코드가 있는 딕셔너리가 없다면
        {
            achievements[achievement.code] = new List<Achievement>(); //리스트에 새롭게 추가
        }
        achievements[achievement.code].Add(achievement); //해당 코드가 있는 리스트에 추가
    }

    public void InitializeAchievements()
    {
        // 딕셔너리에 추가
        AddAchievement(new Achievement("행운은 멍청이를 싫어하는 법이지", 4, 0, EAchievementCode.PassSuccess, "Sprites/Lucky"));
        AddAchievement(new Achievement("탈출..? 응 아니야 내일 또 와야해", 9, 0, EAchievementCode.PassSuccess, "Sprites/Exit"));
        AddAchievement(new Achievement("그냥 계속 여기 있으세요", 3, 0, EAchievementCode.PassFail, "Sprites/Exitfail"));
        AddAchievement(new Achievement("이 업적 못깨면 나갈 마음이 없었던거다", 0, 15, EAchievementCode.TimeAttack, "Sprites/Time"));
        AddAchievement(new Achievement("...저 아직 12시간 못채웠어요", 1, 0, EAchievementCode.AudioClip, "Sprites/ManagerFace"));
        AddAchievement(new Achievement("불좀 꺼줄래? 나,여기서 그냥 잘라고", 1, 0, EAchievementCode.Lighting, "Sprites/Light"));
    }
    public void ShowAchievementUI(Achievement achievement)
    {
        achievementUI.ShowAchievementUI(achievement, achievement.imagePath);
    }
    // 유니티에서 시작할 때 호출
    void Start()
    {
        InitializeAchievements(); // 업적 초기화
    }
}