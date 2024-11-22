using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GO_GameOverManager : MonoBehaviour
{
    public static GO_GameOverManager Instance { get; private set; }
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
    public GameObject popupPanel; // Arrastra aqu� el panel del popup desde el inspector

    // M�todo para mostrar el popup en un momento espec�fico
    public void ShowPopup()
    {
        Inputs.SetCursorState(false);
        popupPanel.SetActive(true);
        GO_InputsPlayer.IsPause = true;
    }
    // M�todo para ocultar el popup canvas
    public void HidePopup()
    {

        popupPanel.SetActive(false);
        GO_InputsPlayer.IsPause = false;
        Destroy(gameObject);
    }

    public void Retry()
    {

        GO_LoadScene.Instance.ShowLoadingScreen();
        _currentLevel = GO_LevelManager.Level.L_GO_Level1;  // Cambia a Nivel1 para reiniciar
        GO_LevelManager.instance.LoadLevelAsync();
        
        HidePopup();  // Oculta el popup despu�s de reiniciar el nivel
    }

    public void QuitToIntro()
    {
        SceneManager.LoadScene("IntroScene");
        HidePopup();  // Oculta el popup despu�s de regresar a la escena de introducci�n
    }

}