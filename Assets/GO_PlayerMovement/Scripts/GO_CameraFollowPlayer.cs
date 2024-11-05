using UnityEngine;
using Cinemachine;

public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Cinemachine Virtual Camera that will follow the player")]
    public CinemachineVirtualCamera virtualCamera;

    [Tooltip("Transform of the player")]
    public Transform playerTransform;

    [Tooltip("Multiplier for the dynamic offset based on player's movement direction")]
    public float dynamicOffsetMultiplier = 5f;

    [Tooltip("Smooth time for camera adjustment")]
    public float smoothTime = 0.2f;

    private CinemachineFramingTransposer framingTransposer;
    private Vector3 velocity = Vector3.zero;
    private Vector3 lastPlayerPosition;

    void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera is not assigned.");
            return;
        }

        framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer == null)
        {
            Debug.LogError("Framing Transposer component is missing in the virtual camera.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned.");
            return;
        }

        lastPlayerPosition = playerTransform.position;
    }

    void LateUpdate()
    {
        Vector3 playerMovement = playerTransform.position - lastPlayerPosition;
        lastPlayerPosition = playerTransform.position;

        Vector3 movementDirection = playerMovement.normalized;

        if (movementDirection != Vector3.zero)
        {
            // Calcula el offset dinámico basado en la dirección de movimiento del jugador
            Vector3 desiredOffset = movementDirection * dynamicOffsetMultiplier;

            // Suaviza la transición del offset
            Vector3 currentOffset = framingTransposer.m_TrackedObjectOffset;
            Vector3 newOffset = Vector3.SmoothDamp(currentOffset, desiredOffset, ref velocity, smoothTime);

            framingTransposer.m_TrackedObjectOffset = newOffset;
        }
        else
        {
            // Vuelve al offset predeterminado cuando el jugador está quieto
            Vector3 defaultOffset = Vector3.zero; // Ajusta este valor según tu necesidad
            Vector3 currentOffset = framingTransposer.m_TrackedObjectOffset;
            Vector3 newOffset = Vector3.SmoothDamp(currentOffset, defaultOffset, ref velocity, smoothTime);

            framingTransposer.m_TrackedObjectOffset = newOffset;
        }
    }
}
