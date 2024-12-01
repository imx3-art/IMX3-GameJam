using UnityEngine;

public class GO_CanvasController : MonoBehaviour
{
    [SerializeField] private GameObject homeCanvas;     // Canvas principal (Home)
    [SerializeField] private GameObject controlsCanvas; // Canvas de control (Controls)
    [SerializeField] private GameObject creditsCanvas;  // Canvas de créditos (Credits)

    // Método que activa el canvas Controls y desactiva el canvas Home
    public void ShowControlsCanvas()
    {
        if (homeCanvas != null && controlsCanvas != null)
        {
            homeCanvas.SetActive(false); // Oculta el canvas Home
            controlsCanvas.SetActive(true); // Activa el canvas Controls
            Debug.Log("Canvas Controls activado");
        }
        else
        {
            Debug.LogWarning("Uno o ambos canvas no están asignados en el inspector.");
        }
    }

    // Método que cierra el canvas Controls y vuelve al canvas Home
    public void CloseControlsCanvas()
    {
        if (homeCanvas != null && controlsCanvas != null)
        {
            controlsCanvas.SetActive(false); // Oculta el canvas Controls
            homeCanvas.SetActive(true); // Activa el canvas Home
            Debug.Log("Canvas Controls cerrado");
        }
        else
        {
            Debug.LogWarning("Uno o ambos canvas no están asignados en el inspector.");
        }
    }

    // Método que activa el canvas Credits y desactiva el canvas Home
    public void ShowCreditsCanvas()
    {
        if (homeCanvas != null && creditsCanvas != null)
        {
            homeCanvas.SetActive(false); // Oculta el canvas Home
            creditsCanvas.SetActive(true); // Activa el canvas Credits
            Debug.Log("Canvas Credits activado");
        }
        else
        {
            Debug.LogWarning("Uno o ambos canvas no están asignados en el inspector.");
        }
    }

    // Método que cierra el canvas Credits y vuelve al canvas Home
    public void CloseCreditsCanvas()
    {
        if (homeCanvas != null && creditsCanvas != null)
        {
            creditsCanvas.SetActive(false); // Oculta el canvas Credits
            homeCanvas.SetActive(true); // Activa el canvas Home
            Debug.Log("Canvas Credits cerrado");
        }
        else
        {
            Debug.LogWarning("Uno o ambos canvas no están asignados en el inspector.");
        }
    }
}
