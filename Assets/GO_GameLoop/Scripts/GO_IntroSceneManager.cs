using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GO_IntroSceneManager : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Nivel1");
    }

    public void ShowCredits()
    {
        // Aquí puedes activar el panel de créditos.
    }

    public void HideCredits()
    {
        // Aquí puedes ocultar el panel de créditos.
    }
}