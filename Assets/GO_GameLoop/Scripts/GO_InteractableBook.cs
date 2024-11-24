using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GO_InteractableBook : MonoBehaviour, GO_IInteractable
{
    [SerializeField] private string TextLore;
    [SerializeField] private GameObject visualHint;

    private string number; // N�mero del libro.
    private int positionInCode; // Posici�n del n�mero en el c�digo.
    private Color positionColor; // Color asociado a esta posici�n.

    // M�todo para asignar el n�mero al libro desde el CodeManager.
    public void SetNumber(string newNumber, int position, Color color)
    {
        number = newNumber;
        positionInCode = position;
        positionColor = color;
    }

    private GO_InputsPlayer inputsPlayer;
    private GO_PlayerUIManager uiplayer;

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
        if (uiplayer == null)
        {
            uiplayer = FindObjectOfType<GO_PlayerUIManager>();
            if (uiplayer == null)
            {
                Debug.LogError("No se encontr� GO_InputsPlayer. Aseg�rate de que el jugador est� en la escena.");
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
            GO_UIManager.Instance.ShowBookNumber(number, positionColor, TextLore);
            uiplayer.AddBookNumber(number, positionColor);
            
        }else
        {
            GO_UIManager.Instance.HideBookPanel();
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
