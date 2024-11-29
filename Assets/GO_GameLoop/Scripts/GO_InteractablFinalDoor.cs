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
                Debug.LogError("No se encontr� GO_InputsPlayer. Aseg�rate de que el jugador est� en la escena.");
            }
        }
    }
    public void Interact()
    {
        EnsureInputsPlayer();
        if (!GO_InteractionManager.IsInteracting)
        {
            GO_InteractionManager.IsInteracting = true;
            GO_InputsPlayer.IsPause = true;
            inputsPlayer.SetCursorState(false);
            visualHint.SetActive(false);
            playInteractSound();
            GO_UIManager.Instance.ShowCodePanel();
        }
        else
        {
            GO_InteractionManager.IsInteracting = false;
            playInteractSound();
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

    public void playInteractSound()
    {
        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayUISound("GO_Interacts_Sound");
        }
    }

}

