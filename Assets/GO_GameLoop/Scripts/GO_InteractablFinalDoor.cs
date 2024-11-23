using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GO_InteractablFinalDoor :MonoBehaviour, GO_IInteractable
{
    private GO_InputsPlayer inputsPlayer;
    [SerializeField] private GameObject visualHint;

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
    }
    public void Interact()
    {
        EnsureInputsPlayer();
        if (inputsPlayer.interact)
        {
            GO_InputsPlayer.IsPause = true;
            inputsPlayer.SetCursorState(false);
            visualHint.SetActive(false);
            GO_UIManager.Instance.ShowCodePanel();
        }
        else
        {
            GO_UIManager.Instance.HideCodePanel();
            GO_InputsPlayer.IsPause = false;
            visualHint.SetActive(true);
            inputsPlayer.SetCursorState(true);
        }
    }
    public GameObject GetVisualHint()
    {
        return visualHint;
    }

}

