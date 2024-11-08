using Fusion;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_PlayerNetworkManager : NetworkBehaviour
{

    private GO_ThirdPersonController controller;

    [Networked] public string playerName { get; set; }
    [Networked] public float playerLife { get; set; }

    public NetworkTransform playerTransform { get; set; }
    public bool isLocalPlayer;
    public static List<GO_PlayerNetworkManager> PlayersList = new List<GO_PlayerNetworkManager>();
    public static GO_PlayerNetworkManager localPlayer;
    public override void Spawned()
    {
        PlayersList.Add(this);
        DontDestroyOnLoad(gameObject);
        if (Object.HasStateAuthority)
        {
            localPlayer = this;
            isLocalPlayer = true;
            playerTransform = GetComponentInChildren<NetworkTransform>();
            playerTransform.gameObject.transform.localPosition = Vector3.zero;
        }
        else
        {
            
        }
        //GetComponentInChildren<Rigidbody>().isKinematic = !isLocalPlayer;
        //(controller = GetComponentInChildren<GO_ThirdPersonController>()).enabled = isLocalPlayer;

    }
    private void Start()
    {
        (controller = GetComponentInChildren<GO_ThirdPersonController>()).enabled = isLocalPlayer;
        
    }
    public void teleportPlayer(Vector3 _pos, Quaternion _rot)
    {
        playerTransform.Teleport(_pos, _rot);
    }
}
