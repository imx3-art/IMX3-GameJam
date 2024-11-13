using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GO_Controller_NavMesh : MonoBehaviour
{
    public Transform followObjective;

    private NavMeshAgent _navMeshAgent;

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void UpdateDestinationPoint(Vector3 destinationPoint)
    {
        _navMeshAgent.destination = destinationPoint;
        _navMeshAgent.isStopped = false;
    }

    public void UpdateDestinationPoint()
    {
        if (followObjective != null)
        {
            UpdateDestinationPoint(followObjective.position);
        }
    }

    public void StopNavMeshAgent()
    {
        _navMeshAgent.isStopped = true;
    }

    public bool ArrivedPoint()
    {
        return _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance && !_navMeshAgent.pathPending;
    }
}