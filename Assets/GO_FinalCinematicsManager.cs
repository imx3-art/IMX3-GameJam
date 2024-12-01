using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_FinalCinematicsManager : MonoBehaviour
{
    void Start()
    {
        GO_RunnerManager.Instance._runner.Disconnect(GO_RunnerManager.Instance._runner.LocalPlayer);
        GO_LoadScene.Instance.gameObject.SetActive(false);
        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayAmbientSound("GO_Final_Track");
        }
    }
}
