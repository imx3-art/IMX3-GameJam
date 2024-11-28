using System;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class GO_IntroSceneManager : MonoBehaviour
{
    public void Start()
    {
        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayAmbientSound("Main_theme");
        }
    }

    public void PlayGame()
    {
        GO_LoadScene.Instance.ShowLoadingScreen();

        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("L_GO_Level1");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        GO_LoadScene.Instance.HideLoadingScreen();
    }

    public void ShowCredits()
    {

    }

    public void HideCredits()
    {

    }

    public void setCustomSessionName(TMP_InputField _input)
    {
        GO_RunnerManager._customNameSession = _input.text;
    }
}