using UnityEngine;

public class GO_PatrollingEnemy : GO_Enemy
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    

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
    
    
}