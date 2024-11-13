using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_State_Persecution : GO_State
{
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void OnEnable()
    {
        enemy.navMeshAgent.speed = enemy.runSpeed;
        enemy.UpdateAnimation("Run");
    }

    void Update()
    {
        Transform playerTransform;

        if (enemy.visionController.SeeThePlayer(out playerTransform))
        {
            enemy.navMeshController.followObjective = playerTransform;
            enemy.navMeshController.UpdateDestinationPoint();
        }
        else
        {
            enemy.navMeshController.followObjective = null;
            stateMachine.ActivateState(GetComponent<GO_State_Patrol>());
        }
    }
}