using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_Enemy_State_Machine : MonoBehaviour
{
    [HideInInspector]
    public GO_State currentState;
    public MeshRenderer meshRendererIndicator;

    public void ActivateState(GO_State newState)
    {
        if (currentState != null)
        {
            currentState.enabled = false;
        }
        currentState = newState;
        currentState.enabled = true;

        if (meshRendererIndicator != null)
        {
            meshRendererIndicator.material.color = currentState.colorState;
        }
    }
    
    public GO_State GetCurrentState()
    {
        return currentState;
    }
}