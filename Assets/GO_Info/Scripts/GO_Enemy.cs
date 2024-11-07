using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float WalkSpeed = 2.0f;
    public float RunSpeed = 5.0f;

    [Header("Field of View Settings")]
    public float visionRange = 10.0f; 
    [Range(0, 360)]
    public float visionAngle = 90.0f; 

    public LayerMask playerLayer;      
    public LayerMask obstacleLayers;   

    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        Move();
        DetectPlayer();
    }

    public virtual void Move()
    {
        // Lógica de movimiento básica
    }

    public virtual void DetectPlayer()
    {
        
    }

    protected virtual void OnPlayerDetected()
    {
        // Comportamiento cuando el jugador es detectado
        
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GO_PlayerManager playerStats = other.GetComponent<GO_PlayerManager>();
            if (playerStats != null)
            {
                playerStats.LoseLife();
            }
        }
    }
}

