using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GO_IntroSceneManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("L_GO_Level1");
    }

    public void ShowCredits()
    {
        // Aqu� puedes activar el panel de cr�ditos.
    }

    public void HideCredits()
    {
        // Aqu� puedes ocultar el panel de cr�ditos.
    }
}