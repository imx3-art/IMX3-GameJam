using Fusion;
using UnityEngine;

public class GO_PatrollingEnemy : GO_Enemy
{
    
    private const int MaxWaypoints = 10; 

    [Networked, Capacity(MaxWaypoints)]
    public NetworkArray<Vector3> WaypointsPositions { get; }
    

    protected override void Start()
    {
        base.Start();

        GO_State_Patrol patrolState = GetComponent<GO_State_Patrol>();
        if (patrolState != null)
        {
            stateMachine.ActivateState(patrolState);
        }
        else
        {
            Debug.LogError("No se encontró el estado de patrulla en el enemigo.");
        }
    }
    
    public void InitializeWaypoints(Vector3[] waypoints)
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            WaypointsPositions.Set(i, waypoints[i]);
        }
    }
    
    
}