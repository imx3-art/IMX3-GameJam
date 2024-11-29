using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GO_InteractableBook : MonoBehaviour, GO_IInteractable
{
    [SerializeField] private string TextLore;
    [SerializeField] private string number;
    [SerializeField] private GameObject visualHint;

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
            playInteractSound();
            GO_UIManager.Instance.ShowBookNumber(number, positionColor, TextLore);
            if (positionColor != Color.clear && number != null)
            {
                uiplayer.AddBookNumber(number, positionColor);
            }
            
        }else
        {
            playInteractSound();
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
    
    public void playInteractSound()
    {
        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayUISound("GO_Interacts_Sound");
        }
    }
}
