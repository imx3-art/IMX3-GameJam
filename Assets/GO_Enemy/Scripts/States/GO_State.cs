using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_State : MonoBehaviour
{
    public Color colorState;

    protected GO_Enemy enemy;
    protected GO_Enemy_State_Machine stateMachine;

    protected virtual void Awake()
    {
        enemy = GetComponent<GO_Enemy>();
        stateMachine = GetComponent<GO_Enemy_State_Machine>();
    }
}