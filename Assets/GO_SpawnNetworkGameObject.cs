using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GO_SpawnNetworkGameObject : MonoBehaviour
{
    [SerializeField] public GameObject spawnedObject;
    
    private IEnumerator Start()
    {   
        yield return new WaitWhile(() => GO_LevelManager.instance == null);
        Debug.Log("Paso la instance");
        yield return new WaitWhile(() => GO_LevelManager.instance.isReady == false);
        Debug.Log("Paso el is ready");

        
        GO_LevelManager.instance.SpawnObjects(spawnedObject, transform.position, transform.rotation, name);
        Debug.Log("SPAWNEO EL PF_CODEMANAGER");
        
    }
}
