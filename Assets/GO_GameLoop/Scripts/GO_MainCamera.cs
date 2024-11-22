using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_MainCamera : MonoBehaviour
{
    public static Camera MainCamera;
    public static CinemachineBrain cinemachineBrain;

    private void Awake()
    {
        if(MainCamera == null)
        {
            MainCamera = GetComponent<Camera>();
            cinemachineBrain = GetComponent<CinemachineBrain>();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
