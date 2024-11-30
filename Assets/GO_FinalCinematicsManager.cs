using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_FinalCinematicsManager : MonoBehaviour
{
    [SerializeField] private GameObject goodEnding;
    [SerializeField] private GameObject BadEnding;
    void Start()
    {
        GO_LevelManager.DidSabotage = true;
        if(GO_LevelManager.DidSabotage)
        {
            BadEnding.SetActive(true);
        }
        else
        {
            goodEnding.SetActive(true);
        }
    }
}
