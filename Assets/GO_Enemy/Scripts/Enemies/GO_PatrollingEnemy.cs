using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class GO_PatrollingEnemy : GO_Enemy
{
    
    private const int MaxWaypoints = 10; 
    [Networked]
    public NetworkBool initializedWaypoints { get; set; }
    
    [Networked]
    public int validWaypointCount { get; set; }

    [Networked, Capacity(MaxWaypoints)]
    public NetworkArray<Vector3> WaypointsPositions { get; }

    public GO_State_Patrol patrolState;
    public GO_State_Persecution persecutionState;
    public GO_State_PickUpArm pickupState;


    private void Awake()
    {
        patrolState = GetComponent<GO_State_Patrol>();
        persecutionState = GetComponent<GO_State_Persecution>();
        pickupState = GetComponent<GO_State_PickUpArm>();
    }

    protected override void Start()
    {
        base.Start();
        DontDestroyOnLoad(gameObject);
        GetComponent<NavMeshAgent>().enabled = false;
        StartCoroutine(ActivatePatrolWhenReady());
    }

    private IEnumerator ActivatePatrolWhenReady()
    {

        // Esperar hasta que los waypoints estén inicializados
        while (!initializedWaypoints)
        {
            yield return null; // Espera al siguiente frame
        }

        if (patrolState != null)
        {
            Debug.Log("Activando patrullaje");
            GetComponent<NetworkTransform>().Teleport(transform.GetChild(0).position, transform.GetChild(0).rotation);
            GetComponent<NavMeshAgent>().enabled = true;
            stateMachine.ActivateState(patrolState);
        }
        else
        {
            Debug.LogError("No se encontró el estado de patrulla en el enemigo.");
        }
    }
    
    public void InitializeWaypoints(Vector3[] waypoints)
    {
        validWaypointCount = Mathf.Min(waypoints.Length, MaxWaypoints); // Máximo hasta MaxWaypoints
        for (int i = 0; i < validWaypointCount; i++)
        {
            WaypointsPositions.Set(i, waypoints[i]);
        }
        Debug.Log("waypoints inicializados");
        initializedWaypoints = true;
    }

    public void PlaySoundAlert()
    {
        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayUISound("GO_Human_Warning");
        }
    }
    
    
}