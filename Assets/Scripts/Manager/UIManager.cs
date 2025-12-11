
//몇 홀 인지 나타내는 UI, 타이머 UI, 
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public TextMeshProUGUI numberText; //몇 홀인지 알려주는 텍스트
    private float numberUp;
    
    public TextMeshProUGUI timeText;
    
    public void UINumberUpdate() //층수 UI 업데이트
    {
        string floor = TriggerManager.Instance.currentfloor.ToString();
        numberText.text = floor;
    }
    
    // 시간 UI 업데이트
    public void UpdateTime(int minutes, int seconds)
    {
        timeText.text = $"{minutes:00}:{seconds:00}";
    }
}
