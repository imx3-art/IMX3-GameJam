using Fusion;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class GO_NetworkObject : NetworkBehaviour
{
    public NetworkObject networkObject;
    private void Start()
    {
        networkObject = GetComponent<NetworkObject>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        GO_ThirdPersonController playerNetworkManager;

        if (collision.gameObject.TryGetComponent(out playerNetworkManager))
        {
            if (playerNetworkManager.enabled && !networkObject.HasStateAuthority)
            {
                networkObject.RequestStateAuthority();
                networkObject.AssignInputAuthority(Runner.LocalPlayer);
            }
            else
            {

            }
        }


    }




}

