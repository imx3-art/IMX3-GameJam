using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Go_Gesture : MonoBehaviour
{
    private Coroutine _gestureCoroutine;
    [SerializeField] private CanvasGroup msjGesture;
    [SerializeField] private TextMeshProUGUI msjGestureText;
    [SerializeField] private string[] msjGestureTexts;
    private void LateUpdate()
    {
        transform.LookAt(GO_MainCamera.MainCamera.transform);
    }

    public void ShowGesture(short _msjIndex)
    {
        msjGesture.gameObject.SetActive(true);
        msjGestureText.text = msjGestureTexts[_msjIndex];
        if (_gestureCoroutine != null)
        {
            StopCoroutine(_gestureCoroutine);
            _gestureCoroutine = null;
        }
        _gestureCoroutine = StartCoroutine(ShowGestureCoroutine());
    }

    public void HideGesture()
    {
        if (_gestureCoroutine != null)
        {
            StopCoroutine(_gestureCoroutine);
            _gestureCoroutine = null;
        }
        if (gameObject.activeSelf)
        {
            _gestureCoroutine = StartCoroutine(HideGestureCoroutine());
        }
    }

    private IEnumerator ShowGestureCoroutine()
    {
        msjGesture.alpha = 0;
        while (msjGesture.alpha < .95f)
        {
            yield return null;
            msjGesture.alpha += Time.deltaTime * 4;
        }
        yield return new WaitForSeconds(4);
        StartCoroutine(HideGestureCoroutine());
    }

    private IEnumerator HideGestureCoroutine()
    {
        while (msjGesture.alpha > .05f)
        {
            yield return null;
            msjGesture.alpha -= Time.deltaTime * 4;
        }
        msjGesture.gameObject.SetActive(false);
        _gestureCoroutine = null;
    }
}
