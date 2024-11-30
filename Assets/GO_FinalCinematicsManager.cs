using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_FinalCinematicsManager : MonoBehaviour
{
    [SerializeField] private GameObject goodEnding;
    [SerializeField] private GameObject BadEnding;
    void Start()
    {
        GO_RunnerManager.Instance._runner.Disconnect(GO_RunnerManager.Instance._runner.LocalPlayer);
        GO_LoadScene.Instance.gameObject.SetActive(false);
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
