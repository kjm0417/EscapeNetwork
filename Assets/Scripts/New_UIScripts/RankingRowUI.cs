using TMPro;
using UnityEngine;

public class RankingRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text txtRank;
    [SerializeField] private TMP_Text txtPlayer;
    [SerializeField] private TMP_Text txtTime;

    /// <summary>
    /// 한 줄(row) 데이터를 표시
    /// </summary>
    public void SetRow(int rank, string playerId, int clearTimeMs)
    {
        if (txtRank != null) txtRank.text = rank.ToString();
        if (txtPlayer != null) txtPlayer.text = playerId;
        if (txtTime != null) txtTime.text = FormatMs(clearTimeMs);
    }

    /// <summary>
    /// ms를 mm:ss.mmm 형태로 변환
    /// </summary>
    private string FormatMs(int ms)
    {
        if (ms < 0) ms = 0;

        int minutes = ms / 60000;
        int seconds = (ms % 60000) / 1000;
        int milli = ms % 1000;

        return $"{minutes:00}:{seconds:00}.{milli:000}";
    }
}
