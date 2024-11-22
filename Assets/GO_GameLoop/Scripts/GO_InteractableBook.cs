using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GO_InteractableBook : GO_IInteractable
{
    public string number; // Número asociado al libro.

    // Método para asignar el número al libro desde el CodeManager.
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
                Debug.LogError("No se encontró GO_InputsPlayer. Asegúrate de que el jugador esté en la escena.");
            }
        }
    }
    public override void Interact()
    {
        EnsureInputsPlayer();
        if (inputsPlayer.interact)
        {
            Debug.Log($"Interacción con el libro iniciada. Número: {number}");
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
