using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GO_PopUpManager : MonoBehaviour
{
    public static GO_PopUpManager Instance { get; private set; }
    public GO_InputsPlayer Inputs;
    private GO_LevelManager.Level _currentLevel;

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
        }
    }
    public GameObject popupPanel; // Arrastra aquí el panel del popup desde el inspector

    // Método para mostrar el popup en un momento específico
    public void ShowPopup()
    {
        GO_InputsPlayer.IsPause = true;
        Inputs.cursorLocked = false;
        popupPanel.SetActive(true);
    }
    // Método para ocultar el popup canvas
    public void HidePopup()
    {
        popupPanel.SetActive(false);
        GO_InputsPlayer.IsPause = false;
        Destroy(gameObject);
    }

    public void Retry()
    {
        _currentLevel = GO_LevelManager.Level.Nivel1;  // Cambia a Nivel1 para reiniciar
        GO_LevelManager.instance.LoadLevel(_currentLevel);
        HidePopup();  // Oculta el popup después de reiniciar el nivel
    }

    public void QuitToIntro()
    {
        SceneManager.LoadScene("IntroScene");
        HidePopup();  // Oculta el popup después de regresar a la escena de introducción
    }

}