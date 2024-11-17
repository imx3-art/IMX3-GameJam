using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GO_LevelManager : NetworkBehaviour
{
    public enum Level
    {
        L_GO_Level1,
        L_GO_Level2,
        Nivel2,
        Nivel3,
        Nivel4,
        Nivel5,
        Nivel6
    }


    //[Networked] public List<Level> levelsLoaded { get; set; } = new List<Level>();

    public List<string> objectsSpawned = new List<string>();
    public bool isReady;
    public short id;
    public static GO_LevelManager instance;
    public GameObject popupManagerPrefab;

    public int totalLives = 3;
    public bool DidSabotage = false;

    //private int _currentLives;

    //private GameObject _playerInstance;
    public GO_PlayerNetworkManager _playerInstance;
    private GameObject _playerPrefab;

    private Level _currentLevel = Level.L_GO_Level1;

    private Transform _spawnPoint;
    private Transform _endPoint;

    private bool isChangingScene = false;

    [SerializeField] GameObject prefabNetworkObjects;




    public override void Spawned()
    {
        Debug.Log("Iniciando " + instance);

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (Runner.IsSharedModeMasterClient)
            {
                SpawnObjects(prefabNetworkObjects, prefabNetworkObjects.transform.position, Quaternion.identity);
                isReady = true;
            }
            else
            {
                //objectsSpawned = RPC_GetPoolSpawnObject().ToList<string>();
                RPC_GetPoolSpawnObject();

            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {

        //_playerInstance = GameObject.FindWithTag("Player");

        while (true)
        {
            yield return new WaitForSeconds(.1f);
            Debug.Log("No player Ready");
            if(GO_PlayerNetworkManager.localPlayer != null)
            {
                _playerInstance = GO_PlayerNetworkManager.localPlayer;
                break;
            }
        }

        //RPC_setLifes(_playerInstance.playerID, (short)totalLives);
        _playerInstance.playerLives = totalLives;



        /*if (_playerInstance == null)
        {
            SpawnPlayer();
        }*/

        //_spawnPoint = GameObject.FindWithTag("Spawn")?.transform;

        /*if (_spawnPoint == null)
        {
            Debug.LogError("No se encontr� un objeto con el tag 'Spawn' en la escena.");
            return;
        }*/

        /*if (_playerInstance != null)
        {
            _playerInstance.transform.position = _spawnPoint.position;
        }*/

        _ = LoadLevelAsync(_currentLevel);
    }

    [ContextMenu("Teleport last Pont")]
    void SpawnPlayer()
    {
        if (_playerInstance != null)
        {
            (Vector3 pos, Quaternion rot) = GO_SpawnPoint.spawPointCurrent.getSpawPointPosition();
            _playerInstance.TeleportPlayer(pos, rot);
        }
    }

    private void OnEnable()
    {
        GO_AreaTrigger.OnPlayerEnterArea += HandlePlayerEnterArea;
    }

    private void OnDisable()
    {
        GO_AreaTrigger.OnPlayerEnterArea -= HandlePlayerEnterArea;
    }

    private void HandlePlayerEnterArea()
    {
        if (!isChangingScene)
        {
            //Muestra pantalla de Carga
            ChangeScene();
        }
    }
    
    [ContextMenu("PERDER VIDA")]
    public void perderUnaVida()
    {
        if(_currentLevel != Level.L_GO_Level1 || true)//REVERT
        {
            //_playerInstance.playerLives--;
            RPC_setLifes(_playerInstance.playerID, -1);
            ResetPlayerPosition();
        }
        Debug.Log(_playerInstance.playerLives);
    }

    private void ChangeScene()
    {
        switch (_currentLevel)
        {
            case Level.L_GO_Level1:
            case Level.L_GO_Level2:
            case Level.Nivel2:
            case Level.Nivel3:
                _currentLevel++;
                break;
            case Level.Nivel4:
                _playerInstance.playerLives = 3;
                //RPC_setLifes(_playerInstance.playerID, 3);
                if (DidSabotage)
                {
                    _currentLevel = Level.Nivel5;
                }
                else
                {
                    _currentLevel = Level.Nivel6;
                }
                break;
            case Level.Nivel5:
            case Level.Nivel6:
                _currentLevel = Level.L_GO_Level1;
                _playerInstance.playerLives = 3;
                //RPC_setLifes(_playerInstance.playerID, 3);
                SceneManager.LoadScene("IntroScene");
                return;
        }
        _ = LoadLevelAsync(_currentLevel);
    }

    private void ResetPlayerPosition()
    {
        if (_playerInstance.playerLives > 0)
        {
            //_spawnPoint = GameObject.FindWithTag("Spawn")?.transform;
            //_playerInstance = GameObject.FindWithTag("Player");

            /*
            if (_playerInstance == null)
            {
                Debug.LogError("playerInstance es nulo. Aseg�rate de que el jugador est� correctamente instanciado.");
                return;
            }

            if (_spawnPoint == null)
            {
                Debug.LogError("spawnPoint es nulo. Aseg�rate de que el punto de spawn est� correctamente asignado.");
                return;
            }*/
            //_playerInstance.transform.position = _spawnPoint.position;

            SpawnPlayer();
            isChangingScene = false;
        }
        else
        {
            // Instancia PopupManager si no existe
            //RPC_setLifes(_playerInstance.playerID, (short)totalLives);
            _playerInstance.playerLives = totalLives;
            _currentLevel = Level.L_GO_Level1;
            Debug.Log(_playerInstance.playerLives);
            if (GO_PopUpManager.Instance == null && popupManagerPrefab != null)
            {
                Instantiate(popupManagerPrefab);
            }
            GO_PopUpManager.Instance.ShowPopup();
        }
    }

    public async Task LoadLevelAsync(Level level)
    {
        string sceneName = level.ToString();
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                Debug.Log($"Cargando... {asyncLoad.progress * 100} %");
                await Task.Yield();
            }

            Debug.Log("*** Mando SPAWN");
            SpawnPlayer();
            Debug.Log("*** Regreso SPAWN");

        }
    }

    private void Update()
    {
        if (isChangingScene && SceneManager.GetActiveScene().name == _currentLevel.ToString())
        {
            OnSceneLoaded();
        }
    }

    public void OnSceneLoaded()
    {
        isChangingScene = false;
    }


    public NetworkObject SpawnObjects(GameObject prefabNetworkObjects, Vector3 _pos , Quaternion _rot, string _codeName = null )
    {
        Debug.Log("MANDO SPAWN " + objectsSpawned.Count);
        if (_codeName == null || !objectsSpawned.Contains(_codeName))
        {
            //Debug.Log("MANDO SPAWN un player " + objectsSpawned[0]);

            if (_codeName != null) RPC_SetPoolSpawnObject(_codeName);
            return Runner.Spawn(prefabNetworkObjects, _pos, _rot);
        }
        return null;
    }



    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetPoolSpawnObject()
    {
        RPC_SendPoolSpawnObject(objectsSpawned.ToArray());        
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendPoolSpawnObject(string[] _listSpawnedObject)
    {
        if (objectsSpawned.Count == 0 && !Object.HasStateAuthority)
        {
            objectsSpawned = _listSpawnedObject.ToList<string>();
        }
        isReady = true;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetPoolSpawnObject(string _nameObjectSpawned)
    {
        if (!objectsSpawned.Contains(_nameObjectSpawned))
        {
            objectsSpawned.Add(_nameObjectSpawned);
        }
    }
    



    //RPC
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]//, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_setLifes(short _playerID, short _value)
    {
        Debug.Log("RPC: " + _playerID + " - " + GO_PlayerNetworkManager.localPlayer.playerID);
        if(_playerID == GO_PlayerNetworkManager.localPlayer.playerID)
        {
            GO_PlayerNetworkManager.localPlayer.playerLives += _value;
        }

    }

}
