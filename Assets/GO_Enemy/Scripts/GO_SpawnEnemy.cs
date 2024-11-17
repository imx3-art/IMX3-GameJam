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
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if(GO_LevelManager.instance != null &&
                GO_LevelManager.instance.isReady)
            {
                HumanSpawned = GO_LevelManager.instance.SpawnObjects(Human, transform.GetChild(0).position, transform.GetChild(0).rotation, name);
                if (HumanSpawned == null)
                {
                    yield break;
                }
                HumanSpawned.name = name;
                DontDestroyOnLoad(HumanSpawned);
                int childCount = transform.childCount;
                Vector3[] waypoints = new Vector3[childCount];
                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypoints[i] = transform.GetChild(i).position;
                }
                HumanSpawned.gameObject.GetComponent<GO_PatrollingEnemy>().InitializeWaypoints(waypoints);
                break;
            }
        }
        yield break;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
