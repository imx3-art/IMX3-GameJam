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
    private async void Start()
    {
        while (true)
        {
            await Task.Delay(100);
            if(GO_LevelManager.instance != null)
            {
                HumanSpawned = GO_LevelManager.instance.SpawnObjects(Human, transform.GetChild(0).position, transform.GetChild(0).rotation);
                Transform[] waypoints = new Transform[transform.childCount];
                for (int i = 0; i < waypoints.Length; i++)
                {
                    waypoints[i] = transform.GetChild(i);
                }

                HumanSpawned.gameObject.GetComponent<GO_PatrollingEnemy>().waypoints = waypoints;
                break;
            }
        }
        return;
        /*HumanSpawned = Runner.Spawn(Human, transform.GetChild(0).position, transform.GetChild(0).rotation);
        HumanSpawned.gameObject.GetComponent<GO_PatrollingEnemy>().waypoints = transform.GetComponentsInChildren<Transform>();;*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
