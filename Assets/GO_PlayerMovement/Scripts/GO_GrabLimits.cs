using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_GrabLimits : MonoBehaviour
{
    [Header("Limites de Movimiento en Ejes")]
    public float minX = -1f;
    public float maxX = 1f;
    public float MinZ = -2.0f;
    public float MaxZ = 2.0f;

    public Vector3 ClampToLimits(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minX, maxX);
        position.z = Mathf.Clamp(position.z, MinZ, MaxZ);
        return position;
    }
}
