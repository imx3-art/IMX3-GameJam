using UnityEngine;

public interface GO_IInteractable
{
    void Interact(); // M�todo que se llama al interactuar.
    GameObject GetVisualHint(); // Nuevo m�todo para obtener la ayuda visual.
}