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

    public Material visionConeMaterial;
    [Range(3, 100)]
    public int meshSegments = 20;
    
    private GO_Controller_NavMesh _controllerNavMesh;
    private GO_Enemy _enemy; 
    
    // Referencias para el cono de visión
    private GameObject _visionConeObject;
    private MeshFilter _visionConeMeshFilter;
    private MeshRenderer _visionConeMeshRenderer;
    private Mesh _visionConeMesh;
    
    public LayerMask visionLayerMask;

    private void Awake()
    {
        _controllerNavMesh = GetComponent<GO_Controller_NavMesh>();
        _enemy = GetComponent<GO_Enemy>();
        if (_enemy == null)
        {
            Debug.LogError("GO_Controller_Vision necesita estar en un GameObject con GO_Enemy.");
        }
        
        _visionConeObject = new GameObject("VisionConeMesh");
        _visionConeObject.transform.SetParent(transform);
        _visionConeObject.transform.localPosition = Vector3.zero;
        _visionConeObject.transform.localRotation = Quaternion.identity;

        _visionConeMeshFilter = _visionConeObject.AddComponent<MeshFilter>();
        _visionConeMeshRenderer = _visionConeObject.AddComponent<MeshRenderer>();

        _visionConeMesh = new Mesh();
        _visionConeMeshFilter.mesh = _visionConeMesh;

        if (visionConeMaterial != null)
        {
            _visionConeMeshRenderer.material = visionConeMaterial;
        }
        
        visionLayerMask = ~(1 << LayerMask.NameToLayer("Enemy"));
    }
    
    private void Update()
    {
        if (_enemy == null || eyes == null) return;

        // Actualizar la geometría del cono de visión considerando obstrucciones
        UpdateVisionConeMesh(_enemy.visionRange, _enemy.visionAngle);

        // Orientar y colocar el cono en la posición y dirección de los ojos del enemigo
        _visionConeObject.transform.position = eyes.position;
        _visionConeObject.transform.rotation = Quaternion.LookRotation(eyes.forward, Vector3.up);

        // Cambiar el color del cono según el estado actual del enemigo, si es posible
        GO_State currentState = _enemy.stateMachine.GetCurrentState();
        if (currentState != null && _visionConeMeshRenderer.material.HasProperty("_Color"))
        {
            Color coneColor = currentState.colorState;
            coneColor.a = 0.5f; 
            _visionConeMeshRenderer.material.color = coneColor;
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

                GO_PlayerNetworkManager player = currentPlayerTransform.GetComponentInParent<GO_PlayerNetworkManager>();
                
                if (player != null &&
                    player.CurrentPlayerState == PlayerState.Ghost)
                {
                    return false;
                }

                

                // Vector desde los ojos del enemigo hacia el jugador ajustado con el offset
                Vector3 directionToPlayer = (currentPlayerTransform.position + _enemy.offset) - eyes.position;

                // Proyectar los vectores en el plano horizontal (ignorar Y) para calcular el ángulo
                Vector3 directionToPlayerFlat = new Vector3(directionToPlayer.x, 0, directionToPlayer.z).normalized;
                Vector3 eyesForwardFlat = new Vector3(eyes.forward.x, 0, eyes.forward.z).normalized;

                // Calcular el ángulo entre la dirección frontal y la dirección al jugador en el plano horizontal
                float angleToPlayer = Vector3.Angle(eyesForwardFlat, directionToPlayerFlat);

                // Dibujar el Raycast para visualización en el Editor
                Debug.DrawRay(eyes.position, directionToPlayer, Color.blue);

                // Verificar si el jugador está dentro del campo de visión
                if (angleToPlayer < _enemy.visionAngle / 2f)
                {
                    // Verificar si hay línea de visión directa al jugador
                    RaycastHit hitInfo;
                    if (Physics.Raycast(eyes.position, directionToPlayer, out hitInfo, _enemy.visionRange, visionLayerMask))
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

    public bool SeeTheArm(out Transform armTransform)
    {
        armTransform = null;

        if (_enemy.hasArm)
        {
            return false;
        }

        Collider[] objectsInRange = Physics.OverlapSphere(eyes.position, _enemy.visionRange);

        foreach (Collider collider in objectsInRange)
        {
            if (collider.CompareTag("Arm"))
            {
                Transform currentArmTransform = collider.transform;

                Vector3 directionToArm = currentArmTransform.position - eyes.position;

                float angleToArm = Vector3.Angle(eyes.forward, directionToArm);

                if (angleToArm < _enemy.visionAngle / 2f)
                {
                    RaycastHit hitInfo;
                    if (Physics.Raycast(eyes.position, directionToArm.normalized, out hitInfo, _enemy.visionRange, visionLayerMask))
                    {
                        if (hitInfo.collider.CompareTag("Arm"))
                        {
                            armTransform = currentArmTransform;

                            Debug.DrawLine(eyes.position, currentArmTransform.position, Color.green, 1.0f);

                            return true;
                        }
                        else
                        {
                            Debug.DrawLine(eyes.position, hitInfo.point, Color.red, 1.0f);
                        }
                    }
                }
            }
        }

        return false;
    }
    
    private void UpdateVisionConeMesh(float range, float angle)
    {
        _visionConeMesh.Clear();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        vertices.Add(Vector3.zero);

        float halfAngle = angle / 2.0f;
        float angleIncrement = angle / meshSegments;
        Vector3 origin = eyes.position; 

        for (int i = 0; i <= meshSegments; i++)
        {
            float currentAngle = -halfAngle + angleIncrement * i;
            float rad = currentAngle * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)).normalized;

            RaycastHit hitInfo;
            float currentRange = range;

            Vector3 globalDirection = eyes.transform.TransformDirection(direction);
            if (Physics.Raycast(origin, globalDirection, out hitInfo, range))
            {
                currentRange = hitInfo.distance; 
            }

            Vector3 vertex = direction * currentRange;
            vertices.Add(vertex);
        }

        // Construir triángulos para el sector
        for (int i = 1; i <= meshSegments; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        _visionConeMesh.SetVertices(vertices);
        _visionConeMesh.SetTriangles(triangles, 0);

        // Recalcular las normales del *mesh*
        _visionConeMesh.RecalculateNormals();
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
