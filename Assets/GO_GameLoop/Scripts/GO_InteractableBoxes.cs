using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GO_InteractableBox : MonoBehaviour, GO_IInteractable
{
    [SerializeField] private string TextLore;
    [SerializeField] private string number;
    [SerializeField] private GameObject visualHint; // UI que queremos mostrar
    [SerializeField] private SphereCollider interactionArea; // Collider del área de interacción

    private GO_InputsPlayer inputsPlayer;
    private GO_PlayerUIManager uiplayer;

    private bool isCollected = false;

    private void EnsureInputsPlayer()
    {
        if (inputsPlayer == null)
        {
            inputsPlayer = FindObjectOfType<GO_InputsPlayer>();
            if (inputsPlayer == null)
            {
                Debug.LogError("No se encontró GO_InputsPlayer. Asegúrate de que el jugador esté en la escena.");
            }
        }
        if (uiplayer == null)
        {
            uiplayer = FindObjectOfType<GO_PlayerUIManager>();
            if (uiplayer == null)
            {
                Debug.LogError("No se encontró GO_PlayerUIManager. Asegúrate de que el jugador esté en la escena.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entra es el jugador
        if (other.CompareTag("Player"))
        {
            ShowUI();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto que sale es el jugador
        if (other.CompareTag("Player"))
        {
            HideUI();
        }
    }

    private void ShowUI()
    {
        if (visualHint != null)
        {
            visualHint.SetActive(true); // Mostrar la UI
        }
    }

    private void HideUI()
    {
        if (visualHint != null)
        {
            visualHint.SetActive(false); // Ocultar la UI
        }
    }

    public GameObject GetVisualHint()
    {
        return visualHint;
    }

    public void playInteractSound()
    {
        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayUISound("GO_Interacts_Sound");
        }
    }

    public void Interact()
    {
        // Implementación del método requerido por la interfaz
        Debug.Log("Interacción realizada con la caja.");
    }

    private void OnEnable()
    {
        GO_LevelManager.OnPlayerDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        GO_LevelManager.OnPlayerDied -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        EnsureInputsPlayer();
        HideUI(); // Ocultar UI si el jugador muere
        GO_InputsPlayer.IsPause = false;
        inputsPlayer.SetCursorState(true);
    }
}
