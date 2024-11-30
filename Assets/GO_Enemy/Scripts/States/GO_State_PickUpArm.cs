using System.Collections;
using UnityEngine;

public class GO_State_PickUpArm : GO_State
{
    [Header("Pick Up Settings")]
    [SerializeField]
    private float pickUpTime = 4.0f;
    [SerializeField]
    private float throwTime = 2.0f; 
    [SerializeField]
    private float stopDistance = 1.0f;

    private Transform armTransform;
    private bool _processStarted = false; 
    private GO_PatrollingEnemy patrollingEnemy;

    protected override void Awake()
    {
        base.Awake();
        patrollingEnemy = enemy.GetComponent<GO_PatrollingEnemy>();
    }

    private void OnEnable()
    {
        if(GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayGameSoundByName("GO_Human_Confusion", transform.position);
        }
        enemy.navMeshAgent.speed = enemy.walkSpeed;
        enemy.UpdateAnimation("Walk");
    }

    private void Update()
    {
        // Evitar reiniciar el proceso si ya ha comenzado
        if (_processStarted) return;

        if (armTransform == null)
        {
            Debug.LogWarning("El brazo ha desaparecido; proceso de recolección cancelado.");
            SetStateAfterProcess();
            return;
        }

        // Moverse hacia el brazo
        enemy.navMeshController.UpdateDestinationPoint(armTransform.position);

        if (enemy.navMeshController.ArrivedPoint())
        {
            _processStarted = true;
            StartCoroutine(PickUpAndDisposeArmCoroutine());
        }
    }

    public void SetArmTransform(Transform arm)
    {
        armTransform = arm;
    }

    private IEnumerator PickUpAndDisposeArmCoroutine()
    {
        // --- Fase 1: Recoger el Brazo ---
        enemy.navMeshAgent.isStopped = true;
        enemy.UpdateAnimation("Idle"); 
        yield return new WaitForSeconds(pickUpTime);

        if (armTransform == null)
        {
            Debug.LogWarning("El brazo ha desaparecido durante la recolección.");
            enemy.hasArm = false;
            enemy.navMeshAgent.isStopped = false;
            SetStateAfterProcess();
            yield break;
        }

        armTransform.GetComponent<GO_NetworkObject>().Despawned();
        armTransform = null;
        enemy.hasArm = true;

        // --- Fase 2: Ir al Basurero Más Cercano ---
        enemy.navMeshAgent.isStopped = false;
        enemy.UpdateAnimation("Walk");

        Transform closestTrashCan = FindClosestTrashCan();
        if (closestTrashCan == null)
        {
            Debug.LogWarning("No hay basureros disponibles.");
            SetStateAfterProcess();
            yield break;
        }

        Vector3 direction = (closestTrashCan.position - enemy.transform.position).normalized;
        Vector3 targetPosition = closestTrashCan.position - direction * stopDistance;

        enemy.navMeshController.UpdateDestinationPoint(targetPosition);
        
        while (!enemy.navMeshController.ArrivedPoint())
        {
            yield return null;
        }

        // --- Fase 3: Desechar el Brazo ---
        enemy.navMeshAgent.isStopped = true;
        enemy.UpdateAnimation("Idle");
        
        if(GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayGameSoundByName("GO_Human_Fridge", transform.position);
        }
        yield return new WaitForSeconds(throwTime); 

        enemy.hasArm = false;

        enemy.navMeshAgent.isStopped = false;

        SetStateAfterProcess();
    }

    /// <summary>
    /// Encuentra el basurero más cercano al enemigo.
    /// </summary>
    /// <returns>El transform del basurero más cercano o null si no se encuentra ninguno.</returns>
    private Transform FindClosestTrashCan()
    {
        GameObject[] trashCans = GameObject.FindGameObjectsWithTag("TrashCan");
        if (trashCans.Length == 0)
        {
            return null;
        }

        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = enemy.transform.position;

        foreach (GameObject trashCan in trashCans)
        {
            float distance = Vector3.Distance(currentPosition, trashCan.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = trashCan.transform;
            }
        }

        return closest;
    }

    /// <summary>
    /// Establece el estado del enemigo después de recoger y desechar el brazo.
    /// </summary>
    private void SetStateAfterProcess()
    {
        _processStarted = false; 
        Transform newArmTransform;
        if (enemy.visionController.SeeTheArm(out newArmTransform) && !enemy.hasArm)
        {
            patrollingEnemy.pickupState.SetArmTransform(newArmTransform);
            stateMachine.ActivateState(patrollingEnemy.pickupState);
            return;
        }

        Transform playerTransform;
        if (enemy.visionController.SeeThePlayer(out playerTransform))
        {
            GO_PlayerNetworkManager player = playerTransform.GetComponentInParent<GO_PlayerNetworkManager>();
            if (player.isLocalPlayer)
            {
                patrollingEnemy.PlaySoundAlert();
            }
            stateMachine.ActivateState(patrollingEnemy.persecutionState);
        }
        else
        {
            stateMachine.ActivateState(patrollingEnemy.patrolState);
        }
    }
}
