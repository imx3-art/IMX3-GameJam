using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_MainCamera : MonoBehaviour
{
    public static Camera MainCamera;

    private void Awake()
    {
        if(MainCamera == null)
        {
            MainCamera = GetComponent<Camera>();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
