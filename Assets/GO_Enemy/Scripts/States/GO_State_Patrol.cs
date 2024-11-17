using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GO_State_Patrol : GO_State
{
    [Networked]
    private int _nextWaypoint { get; set; }
    
    private GO_PatrollingEnemy patrollingEnemy;

    protected override void Awake()
    {
        base.Awake();
        patrollingEnemy = enemy.GetComponent<GO_PatrollingEnemy>();
    }

    void Update()
    {
        // Primero, verificar si el enemigo ve un brazo y no tiene ya uno
        Transform armTransform;
        if (!enemy.hasArm && enemy.visionController.SeeTheArm(out armTransform))
        {
            // Si ve un brazo, cambiar al estado de recogerlo
            GO_State_PickUpArm pickUpArmState = GetComponent<GO_State_PickUpArm>();
            pickUpArmState.SetArmTransform(armTransform);
            stateMachine.ActivateState(pickUpArmState);
            return;
        }

        // Si no hay brazo, comprobar si ve al jugador
        Transform playerTransform;
        if (enemy.visionController.SeeThePlayer(out playerTransform))
        {
            enemy.navMeshController.followObjective = playerTransform;
            stateMachine.ActivateState(GetComponent<GO_State_Persecution>());
            return;
        }

        // Lógica de patrulla normal
        if (enemy.navMeshController.ArrivedPoint())
        {
            _nextWaypoint = (_nextWaypoint + 1) % patrollingEnemy.validWaypointCount;
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
        Vector3 targetPosition = patrollingEnemy.WaypointsPositions[_nextWaypoint];
        enemy.navMeshController.UpdateDestinationPoint(targetPosition);
    }
}