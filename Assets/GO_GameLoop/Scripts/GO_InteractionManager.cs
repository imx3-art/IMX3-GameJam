using StarterAssets;
using UnityEngine;

public class GO_InteractionManager : MonoBehaviour
{
    public static GO_InteractionManager Instance;
    private GO_InputsPlayer inputsPlayer; // Referencia al sistema de inputs.
    private GO_IInteractable currentInteractable;

    private void Start()
    {
        // Busca el componente GO_InputsPlayer dentro del Player.
        inputsPlayer = GetComponentInParent<GO_InputsPlayer>();
        if (inputsPlayer == null)
        {
            Debug.LogError("No se encontró GO_InputsPlayer en el jugador. Asegúrate de que esté configurado.");
            return;
        }

        // Suscribirse al evento de interacción.
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
        // Solo ejecuta la interacción si hay algo con lo que interactuar.
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTRASTE A UN AREA DE INTERACCIÓN");
        // Verifica si el objeto tiene un área de interacción y es interactuable.
        if (other.CompareTag("InteractionArea")) // Asegúrate de usar este tag para las áreas de interacción.
        {
            GO_IInteractable interactable = other.GetComponentInParent<GO_IInteractable>();
            if (interactable != null)
            {
                currentInteractable = interactable;
                Debug.Log($"Objeto interactuable detectado: {other.name}");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("SALISTE DE UN AREA DE INTERACCIÓN");
        // Si el jugador sale del área de interacción, limpia la referencia.
        if (other.CompareTag("InteractionArea") && currentInteractable != null)
        {
            GO_IInteractable interactable = other.GetComponentInParent<GO_IInteractable>();
            if (interactable == currentInteractable)
            {
                currentInteractable = null;
                Debug.Log($"Saliste del área de interacción: {other.name}");
            }
        }
    }
}
