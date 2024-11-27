using System.Collections;
using System.Collections.Generic;
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

    [Header("GameObject de la puerta")]
    [Tooltip("El GameObject de la puerta que se moverá.")]
    [SerializeField] private GameObject doorObject; // Referencia al GameObject de la puerta a mover

    private Vector3 startPosition;
    private Vector3 endPosition;
    private bool isOpen = false;
    private bool isMoving = false;
    private bool isClosingPending = false; // Bandera para saber si el cierre está pendiente

    [SerializeField] private bool MoveLeftUp;
    [SerializeField] private bool Horizontal;

    private HashSet<Rigidbody> objectsInArea = new HashSet<Rigidbody>(); // Rastrea los rigidbodies únicos dentro del área

    private void Start()
    {
        // Aseguramos que el objeto puerta esté asignado
        if (doorObject == null)
        {
            doorObject = gameObject; // Si no se asignó, el propio objeto del script será el que se mueva
        }

        startPosition = doorObject.transform.position;

        if (Horizontal)
        {
            endPosition = MoveLeftUp
                ? startPosition + Vector3.left * doorMoveDistance
                : startPosition + Vector3.right * doorMoveDistance;
        }
        else
        {
            endPosition = MoveLeftUp
                ? startPosition + Vector3.forward * doorMoveDistance
                : startPosition + Vector3.back * doorMoveDistance;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si el objeto que entra en el área es válido
        Rigidbody rb = other.attachedRigidbody;
        if (IsValidObject(other) && rb != null && !objectsInArea.Contains(rb))
        {
            objectsInArea.Add(rb);
            Debug.Log($"Objeto agregado: {other.name}. Total en el área: {objectsInArea.Count}");
            CheckDoorState();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verificamos si el objeto que sale es válido y lo eliminamos de la lista
        Rigidbody rb = other.attachedRigidbody;
        if (IsValidObject(other) && rb != null && objectsInArea.Contains(rb))
        {
            objectsInArea.Remove(rb);
            Debug.Log($"Objeto removido: {other.name}. Total en el área: {objectsInArea.Count}");
            CheckDoorState();
        }
    }

    private bool IsValidObject(Collider other)
    {
        return other.CompareTag("Player") || other.CompareTag("GrabbableObject");
    }

    private void CheckDoorState()
    {
        Debug.Log($"CheckDoorState: Objetos en el área: {objectsInArea.Count}, isOpen: {isOpen}");

        if (objectsInArea.Count >= 2 && !isOpen)
        {
            OpenCooperativeDoor();
        }
        else if (objectsInArea.Count < 2 && isOpen)
        {
            if (!isClosingPending)
            {
                isClosingPending = true; // Marca que el cierre está pendiente
                CancelInvoke(nameof(CloseCooperativeDoor)); // Cancela cierres pendientes
                Invoke(nameof(CloseCooperativeDoor), closeDelay); // Inicia el cierre después de un retraso
            }
        }
    }

    public void OpenCooperativeDoor()
    {
        if (!isOpen && !isMoving)
        {
            Debug.Log("Abriendo puerta...");
            StartCoroutine(MoveDoor(startPosition, endPosition, true));
        }
    }

    public void CloseCooperativeDoor()
    {
        if (isOpen && !isMoving)
        {
            Debug.Log("Cerrando puerta...");
            StartCoroutine(MoveDoor(endPosition, startPosition, false));
        }
    }

    private IEnumerator MoveDoor(Vector3 from, Vector3 to, bool opening)
    {
        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < doorMoveDistance / doorMoveSpeed)
        {
            doorObject.transform.position = Vector3.Lerp(from, to, elapsedTime * doorMoveSpeed / doorMoveDistance);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        doorObject.transform.position = to;
        isOpen = opening;
        isMoving = false;

        Debug.Log(opening ? "Puerta abierta" : "Puerta cerrada");

        if (!isOpen) // Si la puerta está cerrada, no está pendiente el cierre
        {
            isClosingPending = false;
        }
    }
}
