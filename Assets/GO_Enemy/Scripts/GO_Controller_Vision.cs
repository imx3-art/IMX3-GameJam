using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GO_Controller_Vision : MonoBehaviour
{
    public Transform eyes;

    private GO_Controller_NavMesh _controllerNavMesh;
    private GO_Enemy _enemy; 

    private void Awake()
    {
        _controllerNavMesh = GetComponent<GO_Controller_NavMesh>();
        _enemy = GetComponent<GO_Enemy>();
        if (_enemy == null)
        {
            Debug.LogError("GO_Controller_Vision necesita estar en un GameObject con GO_Enemy.");
        }
    }
    
    public bool SeeThePlayer(out Transform playerTransform)
    {
        playerTransform = null;

        Collider[] playersInRange = Physics.OverlapSphere(eyes.position, _enemy.visionRange);

        foreach (Collider collider in playersInRange)
        {
            if (collider.CompareTag("Player"))
            {
                Transform currentPlayerTransform = collider.transform;

                // Vector desde los ojos del enemigo hacia el jugador
                Vector3 directionToPlayer = (currentPlayerTransform.position + _enemy.offset) - eyes.position;

                // Normalizar el vector de dirección
                Vector3 directionToPlayerNormalized = directionToPlayer.normalized;

                // Calcular el ángulo entre la dirección frontal y la dirección al jugador
                float angleToPlayer = Vector3.Angle(eyes.forward, directionToPlayerNormalized);

                // Verificar si el jugador está dentro del campo de visión
                if (angleToPlayer < _enemy.visionAngle / 2f)
                {
                    // Verificar si hay línea de visión directa al jugador
                    RaycastHit hitInfo;
                    if (Physics.Raycast(eyes.position, directionToPlayerNormalized, out hitInfo, _enemy.visionRange))
                    {
                        if (hitInfo.collider.CompareTag("Player"))
                        {
                            playerTransform = currentPlayerTransform;
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Solo dibujar Gizmos si el juego está en modo Play
        if (!Application.isPlaying)
            return;

        // Asegurarse de que 'eyes' y '_enemy' estén asignados
        if (eyes == null || _enemy == null)
            return;

        // Establecer el color de los Gizmos
        Gizmos.color = Color.yellow;

        // Posición de los ojos
        Vector3 eyePosition = eyes.position;

        // Dirección hacia adelante
        Vector3 forward = eyes.forward;

        // Calcular el ángulo medio del campo de visión
        float halfFOV = _enemy.visionAngle / 2.0f;

        // Rotaciones para los límites del cono
        Quaternion leftRayRotation = Quaternion.Euler(0, -halfFOV, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, halfFOV, 0);

        // Direcciones de los límites del cono
        Vector3 leftRayDirection = leftRayRotation * forward * _enemy.visionRange;
        Vector3 rightRayDirection = rightRayRotation * forward * _enemy.visionRange;

        // Dibujar las líneas de los límites
        Gizmos.DrawRay(eyePosition, leftRayDirection);
        Gizmos.DrawRay(eyePosition, rightRayDirection);

        // Dibujar el arco del cono de visión
        Gizmos.color = new Color(1, 1, 0, 0.2f); // Amarillo semi-transparente
        Handles.color = Gizmos.color;
        Handles.DrawSolidArc(eyePosition, Vector3.up, leftRayDirection.normalized, _enemy.visionAngle, _enemy.visionRange);
    }
#endif
}
