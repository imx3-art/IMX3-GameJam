using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GO_State_Persecution : GO_State
{
    
    private float timeSincePlayerLost = 0f; // tiempo desde que el enemigo dejó de ver al jugador
    [SerializeField]
    private float persecutionDuration = 6f;
    
    [SerializeField]
    private float attackDistance = 0.5f;

    // Tiempo de espera entre ataques para evitar múltiples ataques rápidos
    [SerializeField]
    private float attackCooldown = 1f;
    private bool canAttack = true;
    
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
            GO_PlayerNetworkManager player = GO_PlayerNetworkManager.localPlayer;
            if (player != null)
            {
                if (player.CurrentPlayerState == PlayerState.Duel)
                {
                    // El jugador está en duelo, la IA lo ignora
                    return;
                }

                player.ChangePlayerState(PlayerState.Persecution);

                enemy.navMeshController.followObjective = playerTransform;
                enemy.navMeshController.UpdateDestinationPoint();

                timeSincePlayerLost = 0f;

                float distance = Vector3.Distance(enemy.transform.position, playerTransform.position);
                if (distance <= attackDistance)
                {
                    AttemptAttack();
                }
            }
        }
        else
        {
            Debug.Log("tiempo que lleva: "+ timeSincePlayerLost );
            // Si el enemigo no ve al jugador:
            timeSincePlayerLost += Time.deltaTime; // incrementar el tiempo perdido de vista
            
            if (timeSincePlayerLost >= persecutionDuration)
            {
                GO_PlayerNetworkManager player = GO_PlayerNetworkManager.localPlayer;
                if (player != null && player.CurrentPlayerState == PlayerState.Persecution)
                {
                    player.ChangePlayerState(PlayerState.Normal);
                }
                
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
    private void AttemptAttack()
    {
        if (canAttack)
        {
            AttackPlayer();
            StartCoroutine(AttackCooldownCoroutine());
        }
    }

    private void AttackPlayer()
    {
        if (GO_LevelManager.instance != null)
        {
            GO_LevelManager.instance.perderUnaVida();
            Debug.Log($"IA atacó al jugador");

            GO_State_Patrol patrolState = GetComponent<GO_State_Patrol>();
            if (patrolState != null)
            {
                GO_PlayerNetworkManager player = GO_PlayerNetworkManager.localPlayer;
                if (player != null && player.CurrentPlayerState == PlayerState.Persecution)
                {
                    player.ChangePlayerState(PlayerState.Normal);
                }
                stateMachine.ActivateState(patrolState);
            }
            else
            {
                Debug.LogError("No se encontró el componente GO_State_Patrol en el GameObject.");
            }
        }
        else
        {
            Debug.LogError("GO_LevelManager.instance es nulo. Asegúrate de que el LevelManager está correctamente inicializado.");
        }
    }

    private IEnumerator AttackCooldownCoroutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    /// <summary>
    /// Dibuja Gizmos en el editor para visualizar la distancia de ataque.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Verifica que el objeto no sea nulo
        if (this == null)
            return;

        // Color de la esfera de ataque
        Gizmos.color = Color.red;
        
        // Dibuja una esfera de alambre en la posición del enemigo con el radio de la distancia de ataque
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        
        // Opcional: Dibuja una línea hacia adelante indicando la dirección de ataque
        Vector3 forward = transform.forward * attackDistance;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + forward);
        
        // Opcional: Añadir una etiqueta de texto (solo en el editor)
#if UNITY_EDITOR
        Handles.Label(transform.position + Vector3.up * (attackDistance + 1), $"Attack Distance: {attackDistance}");
#endif
    }
}