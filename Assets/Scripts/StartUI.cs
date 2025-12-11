using System.Collections;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup startCanvas;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float displayDuration = 2.0f;

    private void OnEnable()
    {
        GameManager.OnGameStart += ShowStartUI;
    }

    private void OnDisable()
    {
        GameManager.OnGameStart -= ShowStartUI;
    }

    public void ShowStartUI()
    {
        StartCoroutine(FadeAndHideCanvas());
    }

    private IEnumerator FadeAndHideCanvas()
    {
        startCanvas.alpha = 1f;
        startCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(displayDuration);

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            startCanvas.alpha = 1f - (elapsedTime / fadeDuration);
            yield return null;
        }

        startCanvas.alpha = 0f;
        startCanvas.gameObject.SetActive(false);
    }
}
