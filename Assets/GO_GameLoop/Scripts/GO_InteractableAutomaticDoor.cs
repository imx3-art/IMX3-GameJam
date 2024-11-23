using System.Collections;
using UnityEngine;

public class GO_InteractableAutomaticDoor : MonoBehaviour
{
    [SerializeField] private GameObject rightUp;
    [SerializeField] private GameObject leftDown;
    [SerializeField] private bool isVertical;

    private float doorMoveDistance; // Distancia que las puertas deben moverse.
    private float doorMoveSpeed; // Velocidad de movimiento.

    private Vector3 leftDownStartPosition;
    private Vector3 rightUpStartPosition;

    private Vector3 leftDownEndPosition;
    private Vector3 rightUpEndPosition;

    private bool isOpen = false; // Estado de la puerta.
    private bool isMoving = false; // Para evitar múltiples corrutinas simultáneas.

    private void Start()
    {
        doorMoveDistance = GO_UIManager.Instance.doorMoveDistance;
        doorMoveSpeed = GO_UIManager.Instance.doorMoveSpeed;

        // Define las posiciones iniciales.
        leftDownStartPosition = leftDown.transform.position;
        rightUpStartPosition = rightUp.transform.position;

        if (isVertical)
        {
            leftDownEndPosition = leftDownStartPosition + Vector3.forward * doorMoveDistance;
            rightUpEndPosition = rightUpStartPosition + Vector3.back * doorMoveDistance;
        }
        else
        {
            leftDownEndPosition = leftDownStartPosition + Vector3.left * doorMoveDistance;
            rightUpEndPosition = rightUpStartPosition + Vector3.right * doorMoveDistance;
        }
    }

    public void ToggleDoor()
    {
        if (isMoving) return; // No permitir múltiples movimientos simultáneos.

        if (isOpen)
        {
            CloseAutomaticDoor();
        }
        else
        {
            OpenAutomaticDoor();
        }
    }

    public void OpenAutomaticDoor()
    {
        if (!isOpen && !isMoving)
        {
            StartCoroutine(MoveDoor(leftDownStartPosition, leftDownEndPosition, rightUpStartPosition, rightUpEndPosition, true));
        }
    }

    public void CloseAutomaticDoor()
    {
        if (isOpen && !isMoving)
        {
            StartCoroutine(MoveDoor(leftDownEndPosition, leftDownStartPosition, rightUpEndPosition, rightUpStartPosition, false));
        }
    }

    private IEnumerator MoveDoor(Vector3 leftStart, Vector3 leftEnd, Vector3 rightStart, Vector3 rightEnd, bool opening)
    {
        isMoving = true; // Bloquear otros movimientos mientras la puerta se mueve.
        float elapsedTime = 0f;

        while (elapsedTime < doorMoveDistance / doorMoveSpeed)
        {
            leftDown.transform.position = Vector3.Lerp(leftStart, leftEnd, elapsedTime * doorMoveSpeed / doorMoveDistance);
            rightUp.transform.position = Vector3.Lerp(rightStart, rightEnd, elapsedTime * doorMoveSpeed / doorMoveDistance);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Asegúrate de que las puertas terminen exactamente en sus posiciones finales.
        leftDown.transform.position = leftEnd;
        rightUp.transform.position = rightEnd;

        isOpen = opening; // Actualizar el estado de la puerta.
        isMoving = false;

        Debug.Log(opening ? "¡Puerta abierta!" : "¡Puerta cerrada!");
    }
}
