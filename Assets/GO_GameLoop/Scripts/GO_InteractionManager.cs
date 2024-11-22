using StarterAssets;
using UnityEngine;

public class GO_InteractionManager : MonoBehaviour
{
    public static GO_InteractionManager Instance;
    private GO_InputsPlayer inputsPlayer; // Referencia al sistema de inputs.
    private GO_IInteractable currentInteractable;
    private GameObject currentVisualHint; // Referencia al objeto visual de ayuda.

    private void Start()
    {
        // Busca el componente GO_InputsPlayer dentro del Player.
        inputsPlayer = GetComponentInParent<GO_InputsPlayer>();
        if (inputsPlayer == null)
        {
            Debug.LogError("No se encontr� GO_InputsPlayer en el jugador. Aseg�rate de que est� configurado.");
            return;
        }

        // Suscribirse al evento de interacci�n.
        inputsPlayer.onInteract += HandleInteract;
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar fugas de memoria.
        if (inputsPlayer != null)
        {
            inputsPlayer.onInteract -= HandleInteract;
        }
    }

    private void HandleInteract()
    {
        // Solo ejecuta la interacci�n si hay algo con lo que interactuar.
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto tiene un �rea de interacci�n y es interactuable.
        if (other.CompareTag("InteractionArea")) // Aseg�rate de usar este tag para las �reas de interacci�n.
        {
            GO_IInteractable interactable = other.GetComponentInParent<GO_IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;

                // Mostrar la ayuda visual
                ShowVisualHint(interactable);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Si el jugador sale del �rea de interacci�n, limpia la referencia.
        if (other.CompareTag("InteractionArea") && currentInteractable != null)
        {
            GO_IInteractable interactable = other.GetComponentInParent<GO_IInteractable>();
            if (interactable == currentInteractable)
            {
                currentInteractable = null;

                // Ocultar la ayuda visual
                HideVisualHint();
            }
        }
    }

    private void ShowVisualHint(GO_IInteractable interactable)
    {
        // Si el interactable tiene una ayuda visual asociada, act�vala.
        currentVisualHint = interactable.GetVisualHint(); // M�todo en el interactuable.
        if (currentVisualHint != null)
        {
            currentVisualHint.SetActive(true);
        }
    }

    private void HideVisualHint()
    {
        // Oculta la ayuda visual actual si existe.
        if (currentVisualHint != null)
        {
            currentVisualHint.SetActive(false);
            currentVisualHint = null;
        }
    }
}
