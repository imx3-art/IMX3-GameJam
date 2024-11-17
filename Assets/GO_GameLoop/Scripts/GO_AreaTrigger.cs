using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_AreaTrigger : MonoBehaviour
{
    public static event Action OnPlayerEnterArea;

    private void OnTriggerEnter(Collider other)
    {
        GO_ThirdPersonController playerNetworkManager;

        if (other.gameObject.TryGetComponent(out playerNetworkManager))
        {
            if (playerNetworkManager.enabled)
            { 
                OnPlayerEnterArea?.Invoke();
                gameObject.SetActive(false);
            }
        }
    }
}