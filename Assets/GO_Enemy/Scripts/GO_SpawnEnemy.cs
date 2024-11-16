using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Unity.VisualScripting;
using UnityEngine;

public class GO_SpawnEnemy : MonoBehaviour
{
    public GameObject Human;
    private NetworkObject HumanSpawned;

    private void Start()
    {
        StartCoroutine(Start2());
    }

    private IEnumerator Start2()
    {
        Debug.Log("Spawn Enemy a ");
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if(GO_LevelManager.instance != null)
            {
                Debug.Log("Spawn Enemy b ");
                HumanSpawned = GO_LevelManager.instance.SpawnObjects(Human, transform.GetChild(0).position, transform.GetChild(0).rotation);
                Debug.Log("Spawn Enemy c "+HumanSpawned.transform.position);
                if (HumanSpawned == null)
                {
                    yield break;
                }
                int childCount = transform.childCount;
                Vector3[] waypoints = new Vector3[childCount];
                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypoints[i] = transform.GetChild(i).position;
                }
                Debug.Log("Spawn Enemy d "+HumanSpawned.transform.position);

                HumanSpawned.gameObject.GetComponent<GO_PatrollingEnemy>().InitializeWaypoints(waypoints);
                break;
            }
        }
        Debug.Log("Spawn Enemy e ");
        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
