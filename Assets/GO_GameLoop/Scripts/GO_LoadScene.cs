using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GO_LoadScene : MonoBehaviour
{
    public static GO_LoadScene Instance { get; private set; }
    public GameObject loadingScreen;
    private CanvasGroup canvasGroup;
    public float fadeDuration = 1f;

    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); 
            return;
        }

        if (loadingScreen != null)
        {
            canvasGroup = loadingScreen.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = loadingScreen.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            loadingScreen.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No se ha asignado una pantalla de carga.");
        }
    }

    public void ShowLoadingScreen(Action onFadeInComplete = null)
    {
        StartCoroutine(FadeLoadingScreen(1f, onFadeInComplete));
    }

    public void HideLoadingScreen(Action onFadeOutComplete = null)
    {
        StartCoroutine(FadeLoadingScreen(0f, onFadeOutComplete));
    }

    private IEnumerator FadeLoadingScreen(float targetAlpha, Action onFadeComplete)
    {
        loadingScreen.SetActive(true);

        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;
            float curveValue = fadeCurve.Evaluate(t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (targetAlpha == 0f)
        {
            loadingScreen.SetActive(false);
        }

        onFadeComplete?.Invoke();
    }

    
}
