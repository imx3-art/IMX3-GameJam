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
    private void Start()
    {
        StartCoroutine(WaitForAudioManagerAndPlaySound());
    }

    private IEnumerator WaitForAudioManagerAndPlaySound()
    {
        while (GO_AudioManager.Instance == null)
        {
            yield return null; 
        }
        GO_AudioManager.Instance.PlayAmbientSound("Main_theme");
    }

    public void PlayGame()
    {
        GO_LoadScene.Instance.ShowLoadingScreen();

        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        bool fadeInCompleted = false;

        GO_LoadScene.Instance.ShowLoadingScreen(() => {
            fadeInCompleted = true;
        });

        while (!fadeInCompleted)
        {
            yield return null;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("L_GO_Level0");

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        bool fadeOutCompleted = false;

        GO_LoadScene.Instance.HideLoadingScreen(() => {
            fadeOutCompleted = true;
        });

        while (!fadeOutCompleted)
        {
            yield return null;
        }
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