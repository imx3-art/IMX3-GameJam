using System.Collections;
using UnityEngine;

public class GO_CooperativeDoor : MonoBehaviour
{
    [Header("Configuración de la puerta")]
    [Tooltip("Distancia que las puertas deben moverse.")]
    [SerializeField] private float doorMoveDistance = 5f;
    [Tooltip("Velocidad de movimiento de la puerta.")]
    [SerializeField] private float doorMoveSpeed = 2f;
    [Tooltip("Tiempo de delay antes de cerrar la puerta.")]
    [SerializeField] private float closeDelay = 3f;

    [Header("Tags válidos")]
    [Tooltip("Tag para identificar jugadores.")]
    [SerializeField] private string playerTag = "Player";
    [Tooltip("Tag para identificar objetos agarrables.")]
    [SerializeField] private string grabbableTag = "Grabbable";

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool isOpen = false;
    private bool isMoving = false;

    private int objectsInArea = 0; // Contador de jugadores y objetos dentro del área

    private void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + Vector3.up * doorMoveDistance; // Mueve hacia arriba por defecto
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto tiene un tag válido
        if (other.CompareTag(playerTag) || other.CompareTag(grabbableTag))
        {
            objectsInArea++;
            Debug.Log($"Objetos en el área: {objectsInArea}");
            CheckDoorState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verifica si el objeto tiene un tag válido
        if (other.CompareTag(playerTag) || other.CompareTag(grabbableTag))
        {
            objectsInArea = Mathf.Max(0, objectsInArea - 1); // Asegura que no sea negativo
            Debug.Log($"Objetos en el área: {objectsInArea}");
            CheckDoorState();
        }
    }

    private void CheckDoorState()
    {
        if (objectsInArea >= 2 && !isOpen)
        {
            OpenCooperativeDoor();
        }
        else if (objectsInArea < 2 && isOpen)
        {
            Invoke(nameof(CloseCooperativeDoor), closeDelay);
        }
    }

    public void OpenCooperativeDoor()
    {
        if (!isOpen && !isMoving)
        {
            StartCoroutine(MoveDoor(startPosition, endPosition, true));
        }
    }

    public void CloseCooperativeDoor()
    {
        if (isOpen && !isMoving)
        {
            StartCoroutine(MoveDoor(endPosition, startPosition, false));
        }
    }

    private IEnumerator MoveDoor(Vector3 from, Vector3 to, bool opening)
    {
        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < doorMoveDistance / doorMoveSpeed)
        {
            transform.position = Vector3.Lerp(from, to, elapsedTime * doorMoveSpeed / doorMoveDistance);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        isOpen = opening;
        isMoving = false;

        Debug.Log(opening ? "¡Puerta abierta!" : "¡Puerta cerrada!");
    }
}
