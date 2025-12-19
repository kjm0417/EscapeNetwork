using CSBaseLib;
using System.Collections.Generic;
using UnityEngine;

public class RankingUI : MonoBehaviour
{
    [Header("Scroll View")]
    [SerializeField] private Transform content;          
    [SerializeField] private RankingRowUI rowPrefab;     

    [Header("Option")]
    [SerializeField] private int topCount = 50;

    /// <summary>
    /// 시작 시 랭킹 수신 이벤트를 구독하고 Top N을 요청
    /// </summary>
    private void Start()
    {
        NetworkManager.OnRankingTopReceived += HandleRankingTopReceived;
        NetworkManager.OnRankingError += HandleRankingError;

        NetworkManager.Instance.SendRankingGetTop(topCount);
    }

    private void OnEnable()
    {
        // 랭킹 UI가 켜질 때마다 최신 Top 요청
        NetworkManager.Instance.SendRankingGetTop(20);
    }

    /// <summary>
    /// 오브젝트 파괴 시 이벤트 구독을 해제
    /// </summary>
    private void OnDestroy()
    {
        NetworkManager.OnRankingTopReceived -= HandleRankingTopReceived;
        NetworkManager.OnRankingError -= HandleRankingError;
    }

    /// <summary>
    /// 랭킹 리스트를 받아서 UI를 갱신
    /// </summary>
    private void HandleRankingTopReceived(List<RankingItem> items)
    {
        ClearRows();

        if (items == null || items.Count == 0)
        {
            return;
        }

        for (int i = 0; i < items.Count; i++)
        {
            var row = Instantiate(rowPrefab, content);
            row.SetRow(i + 1, items[i].UserID, items[i].ClearTimeMs);
        }
    }

    /// <summary>
    /// 랭킹 에러를 로그로 출력
    /// </summary>
    private void HandleRankingError(string err)
    {
        Debug.LogError($"Ranking Error: {err}");
    }

    /// <summary>
    /// Content 아래 기존 Row들을 제거
    /// </summary>
    private void ClearRows()
    {
        if (content == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
}
