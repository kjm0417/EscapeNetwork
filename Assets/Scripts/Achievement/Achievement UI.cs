using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    public Animator achievementAnimator;
    public TextMeshProUGUI achievementText;
    public Image achievementImage; // 업적 이미지를 표시할 UI
    public float displayDuration = 2.0f;
    

    // 업적 UI 표시
    public void ShowAchievementUI(Achievement achievement, string imagePath)
    {

        // 업적 이름과 이미지 설정
        achievementText.text = $"{achievement.name}";
        //achievementImage.sprite = image; //이미지 경로
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            achievementImage.sprite = sprite; // UI에 이미지 설정
        }

        // 애니메이션 트리거 실행
        achievementAnimator.SetTrigger("Show");

        // 일정 시간 후 UI 숨김 애니메이션 실행
        Invoke("HideAchievementUI", displayDuration);
    }

    // 업적 UI 숨김
    private void HideAchievementUI()
    {
        achievementAnimator.SetTrigger("Hide");
    }
}
