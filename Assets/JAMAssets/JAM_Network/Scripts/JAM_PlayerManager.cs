using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JAM_PlayerManager : NetworkBehaviour
{
    
    [Networked] public string playerName { get; set; }
    [Networked] public float playerLife { get; set; }

    public bool isLocalPlayer;
    public static List<JAM_PlayerManager> PlayersList = new List<JAM_PlayerManager>();

    public override void Spawned()
    {
        PlayersList.Add(this);
        DontDestroyOnLoad(gameObject);
        if(Object.HasStateAuthority)
        {
            isLocalPlayer = true;
        }
        else
        {

        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
