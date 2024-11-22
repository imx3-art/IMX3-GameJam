using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GO_InteractableBook : GO_IInteractable
{
    public string number; // N�mero asociado al libro.

    // M�todo para asignar el n�mero al libro desde el CodeManager.
    public void SetNumber(string newNumber)
    {
        number = newNumber;
    }

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
            Debug.Log($"Interacci�n con el libro iniciada. N�mero: {number}");
            GO_InputsPlayer.IsPause = true;
            inputsPlayer.SetCursorState(false);
            GO_UIManager.Instance.ShowBookNumber(number);
        }else
        {
            GO_UIManager.Instance.HideBookPanel();
            GO_InputsPlayer.IsPause = false;
            inputsPlayer.SetCursorState(true);
        }
            
    }
}
