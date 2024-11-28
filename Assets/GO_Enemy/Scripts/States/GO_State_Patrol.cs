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
            patrollingEnemy.pickupState.SetArmTransform(armTransform);
            stateMachine.ActivateState(patrollingEnemy.pickupState);
            return;
        }

        // Si no hay brazo, comprobar si ve al jugador
        Transform playerTransform;
        if (enemy.visionController.SeeThePlayer(out playerTransform))
        {
            //GO_PlayerNetworkManager player = GO_PlayerNetworkManager.localPlayer;
            GO_PlayerNetworkManager player = playerTransform.GetComponentInParent<GO_PlayerNetworkManager>(); ;
            if(player == GO_PlayerNetworkManager.localPlayer)
            {
                GetComponent<GO_NetworkObject>().ChangeAuthority();
            }
            if (player != null && player.CurrentPlayerState != PlayerState.Duel && player.CurrentPlayerState != PlayerState.Ghost)
            {
                // Cambiar al estado de persecución si el jugador no está en duelo
                enemy.navMeshController.followObjective = playerTransform;
                patrollingEnemy.PlaySoundAlert();
                stateMachine.ActivateState(patrollingEnemy.persecutionState);
                return;
            }
        }

        // Lógica de patrulla normal
        if (enemy.navMeshController.ArrivedPoint())
        {
            if (patrollingEnemy.validWaypointCount > 0)
            {
                _nextWaypoint = (_nextWaypoint + 1) % patrollingEnemy.validWaypointCount;
                UpdateDestinationWaypoint();
            }
            else
            {
                Debug.LogWarning("No hay waypoints válidos para patrullar.");
            }
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
        Vector3 targetPosition = patrollingEnemy.WaypointsPositions.Get(_nextWaypoint);
        enemy.navMeshController.UpdateDestinationPoint(targetPosition);
    }
}