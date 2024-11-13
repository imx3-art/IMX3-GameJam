using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_State_Alert : GO_State
{
    [SerializeField] 
    private float speedSearch = 120f;
    [SerializeField] 
    private float durationSearch = 4f;
    
    private GO_Controller_NavMesh _controllerNavMesh;
    private GO_Controller_Vision _controllerVision;
    private float _timeSearching;

    protected override void Awake()
    {
        base.Awake();
        _controllerNavMesh = GetComponent<GO_Controller_NavMesh>();
        _controllerVision = GetComponent<GO_Controller_Vision>();
    }

   /* void Update()
    {
        //Ve al jugador
        RaycastHit hit;
        if (_controllerVision.SeeThePlayer(out hit))
        {
            _controllerNavMesh.followObjective = hit.transform;
            _enemyStateMachine.ActivateState(_enemyStateMachine.statePersecution);
            return;
        }
        
        transform.Rotate(0f, speedSearch * Time.deltaTime, 0f);
        _timeSearching += Time.deltaTime;
        if (_timeSearching >= durationSearch)
        {
            _enemyStateMachine.ActivateState(_enemyStateMachine.statePatrol);
            return;
        }
}*/
/*
    private void OnEnable()
    {
        _controllerNavMesh.StopNaveMeshAgent();
        _timeSearching = 0f;
    }*/
}
