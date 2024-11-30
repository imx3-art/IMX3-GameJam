using System.Collections;
using UnityEngine;

public class GO_InteractableAutomaticDoor : MonoBehaviour
{
    [SerializeField] private bool MoveLeftUp;
    [SerializeField] private bool Horizontal;

    private float doorMoveDistance; // Distancia que las puertas deben moverse.
    private float doorMoveSpeed; // Velocidad de movimiento.

    private Vector3 StartPosition;

    private Vector3 EndPosition;

    private bool isOpen = false; // Estado de la puerta.
    private bool isMoving = false; // Para evitar m�ltiples corrutinas simult�neas.

    private void Start()
    {
        doorMoveDistance = GO_UIManager.Instance.doorMoveDistance;
        doorMoveSpeed = GO_UIManager.Instance.doorMoveSpeed;

        // Define las posiciones iniciales.
        StartPosition = transform.position;
        if (Horizontal)
        {
            if (MoveLeftUp)
            {
                EndPosition = StartPosition + Vector3.left * doorMoveDistance;
            }
            else
            {
                EndPosition = StartPosition + Vector3.right * doorMoveDistance;
            }
        }
        else
        {
            if (MoveLeftUp)
            {
                EndPosition = StartPosition + Vector3.forward * doorMoveDistance;
            }
            else
            {
                EndPosition = StartPosition + Vector3.back * doorMoveDistance;
            }
        }
        
    }

    public void OpenAutomaticDoor()
    {
        if (!isOpen && !isMoving)
        {
            GO_LevelManager.instance.RPC_PlaySound3D("GO_Open_Door_Lab", transform.position);
            StartCoroutine(MoveDoor(StartPosition, EndPosition, true));
        }
    }

    public void CloseAutomaticDoor()
    {
        if (isOpen && !isMoving)
        {
            GO_LevelManager.instance.RPC_PlaySound3D("GO_Open_Door_Lab", transform.position);
            StartCoroutine(MoveDoor(EndPosition, StartPosition, false));
        }
    }

    private IEnumerator MoveDoor(Vector3 leftStart, Vector3 leftEnd, bool opening)
    {
        isMoving = true; // Bloquear otros movimientos mientras la puerta se mueve.
        float elapsedTime = 0f;

        while (elapsedTime < doorMoveDistance / doorMoveSpeed)
        {
            transform.position = Vector3.Lerp(leftStart, leftEnd, elapsedTime * doorMoveSpeed / doorMoveDistance);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Aseg�rate de que las puertas terminen exactamente en sus posiciones finales.
        transform.position = leftEnd;

        isOpen = opening; // Actualizar el estado de la puerta.
        isMoving = false;

        Debug.Log(opening ? "�Puerta abierta!" : "�Puerta cerrada!");
    }
}
