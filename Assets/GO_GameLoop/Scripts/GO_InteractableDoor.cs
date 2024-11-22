using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GO_InteractableDoor : GO_IInteractable
{
    private GO_InputsPlayer inputsPlayer;

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
    public override void Interact()
    {
        EnsureInputsPlayer();
        if (inputsPlayer.interact)
        {
            Debug.Log("Interacci�n con la puerta iniciada. Mostrando panel de c�digo.");
            GO_InputsPlayer.IsPause = true;
            inputsPlayer.SetCursorState(false);
            GO_UIManager.Instance.ShowCodePanel();
        }
        else
        {
            GO_UIManager.Instance.HideCodePanel();
            GO_InputsPlayer.IsPause = false;
            inputsPlayer.SetCursorState(true);
        }
    }
}

