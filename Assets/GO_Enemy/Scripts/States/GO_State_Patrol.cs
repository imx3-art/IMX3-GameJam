using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_State_Patrol : GO_State
{
    private int _nextWaypoint;

    protected override void Awake()
    {
        base.Awake();
    }

    void Update()
    {
        Transform playerTransform;
        if (enemy.visionController.SeeThePlayer(out playerTransform))
        {
            enemy.navMeshController.followObjective = playerTransform;
            stateMachine.ActivateState(GetComponent<GO_State_Persecution>());
            return;
        }

        if (enemy.navMeshController.ArrivedPoint())
        {
            _nextWaypoint = (_nextWaypoint + 1) % enemy.GetComponent<GO_PatrollingEnemy>().waypoints.Length;
            UpdateDestinationWaypoint();
        }
    }

    private void OnEnable()
    {
        //_nextWaypoint = 0;
        enemy.navMeshAgent.speed = enemy.walkSpeed;
        UpdateDestinationWaypoint();
        enemy.UpdateAnimation("Walk");
    }

    private void UpdateDestinationWaypoint()
    {
        enemy.navMeshController.UpdateDestinationPoint(enemy.GetComponent<GO_PatrollingEnemy>().waypoints[_nextWaypoint].position);
    }
}