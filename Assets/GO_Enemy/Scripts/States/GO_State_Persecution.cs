using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_State_Persecution : GO_State
{
    
    private float timeSincePlayerLost = 0f; // tiempo desde que el enemigo dejó de ver al jugador
    [SerializeField]
    private float persecutionDuration = 6f;
    
    protected override void Awake()
    {
        base.Awake();
    }
    
    private void OnEnable()
    {
        enemy.navMeshAgent.speed = enemy.runSpeed;
        enemy.UpdateAnimation("Run");
        timeSincePlayerLost = 0f;
    }

    void Update()
    {
        Transform armTransform;
        if (!enemy.hasArm && enemy.visionController.SeeTheArm(out armTransform))
        {
            // Si ve un brazo y no tiene uno, cambiar al estado de recogerlo
            GO_State_PickUpArm pickUpArmState = GetComponent<GO_State_PickUpArm>();
            pickUpArmState.SetArmTransform(armTransform);
            stateMachine.ActivateState(pickUpArmState);
            return;
        }

        Transform playerTransform;
        if (enemy.visionController.SeeThePlayer(out playerTransform))
        {
            enemy.navMeshController.followObjective = playerTransform;
            enemy.navMeshController.UpdateDestinationPoint();
            
            timeSincePlayerLost = 0f;
        }
        else
        {
            Debug.Log("tiempo que lleva: "+ timeSincePlayerLost );
            // Si el enemigo no ve al jugador:
            timeSincePlayerLost += Time.deltaTime; // incrementar el tiempo perdido de vista
            
            if (timeSincePlayerLost >= persecutionDuration)
            {
                stateMachine.ActivateState(GetComponent<GO_State_Patrol>());
                return;
            }

            // Continuar persiguiendo al jugador con la última posición conocida 
            if (enemy.navMeshController.followObjective != null)
            {
                enemy.navMeshController.UpdateDestinationPoint(enemy.navMeshController.followObjective.position);
            }
            else
            {
                stateMachine.ActivateState(GetComponent<GO_State_Patrol>());
            }
        }
    }
}