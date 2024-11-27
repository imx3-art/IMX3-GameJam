using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_LookAtCamera : MonoBehaviour
{
    private Camera targetCamera;

    private void Start()
    {
        // Solo asignamos la cámara si es el jugador local
        if (GO_PlayerNetworkManager.localPlayer)
        {
            targetCamera = Camera.main; // Asignar la cámara principal del jugador local
        }
    }

    private void Update()
    {
        // Solo ejecutar si esta instancia pertenece al jugador local
        if (GO_PlayerNetworkManager.localPlayer)
        {        
            transform.LookAt(targetCamera.transform.position);
            transform.Rotate(0, 180, 0);
        }
    }
}