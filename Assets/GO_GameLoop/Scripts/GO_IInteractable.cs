using UnityEngine;

public interface GO_IInteractable
{
    void Interact(); // Método que se llama al interactuar.
    GameObject GetVisualHint(); // Nuevo método para obtener la ayuda visual.
}