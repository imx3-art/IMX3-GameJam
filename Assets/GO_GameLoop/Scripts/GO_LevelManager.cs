using Fusion;
using System;
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
        L_GO_Level0,
        L_GO_Level1,
        L_GO_Level2,
        L_GO_Level3,
        L_GO_Level4,
        L_GO_Level5,
        L_GO_Level6,
    }


    //[Networked] public List<Level> levelsLoaded { get; set; } = new List<Level>();

    public List<string> objectsSpawned = new List<string>();
    public List<NetworkObject> networkObjectsSpawned = new List<NetworkObject>();
    public bool isReady;
    public short id;
    public static GO_LevelManager instance;
    public GameObject popupManagerPrefab;
    public GO_NetworkObject armPlayer;
    public PlayerRef CurrentPlayerRefChangeScene;//Player que es dueño del objeto, pero esta dejando la escena
    public PlayerRef newPlayerRefAuthorityChangeScene;// Posible player que queda en el nivel actual y se le puedn transferir las Autoridades

    public int totalLives = 3;
    public bool DidSabotage = false;

    //private int _currentLives;

    //private GameObject _playerInstance;
    public GO_PlayerNetworkManager _playerInstance;
    private GameObject _playerPrefab;

    [SerializeField] private Level _currentLevel;

    private Transform _spawnPoint;
    private Transform _endPoint;

    private bool isChangingScene = false;

    [SerializeField] GameObject prefabNetworkObjects;

    public static event Action OnPlayerChangeScene;
    
    public event Action<float> OnLivesChanged;



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
                RPC_GetPoolSpawnNetWorkObject();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Despawned(NetworkObject _object)
    {
        Runner.Despawn(_object);
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
        OnLivesChanged?.Invoke(_playerInstance.playerLives);



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

       // StartCoroutine(LoadLevelAsync(_currentLevel));
    }

    [ContextMenu("Teleport last Pont")]
    void SpawnPlayer()
    {
        if (_playerInstance != null)
        {
            (Vector3 pos, Quaternion rot) = GO_SpawnPoint.currentSpawPoint.getSpawPointPosition();
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

    public void HandlePlayerEnterArea()
    {
        if (!isChangingScene)
        {
            //Muestra pantalla de Carga
            GO_LoadScene.Instance.ShowLoadingScreen();

            CurrentPlayerRefChangeScene = Runner.LocalPlayer;


            ChangeScene();
        }
    }
    /// <summary>
    /// True: Se encontro un player para pasarle la Autoridad, False: Se despawnean los Objetos.
    /// </summary>
    /// <returns></returns>
    public bool CheckPlayerInLastLevel()
    {
        foreach(GO_PlayerNetworkManager player in GO_PlayerNetworkManager.PlayersList) 
        {
            if (player.playerID == GO_PlayerNetworkManager.localPlayer.playerID)
            {
                continue;
            }
            Debug.Log("*** CAMBIO SCENE " + player.currentLevel_ID + " - " + player.name + " POINT: " + GO_SpawnPoint.currentSpawPoint.level_ID);

            if ((short) GO_SpawnPoint.currentSpawPoint.level_ID == player.currentLevel_ID)
            {
                Debug.Log("*** CAMBIO SCENE " + player.currentLevel_ID + " - " + player.name);

                foreach (var playerRef in Runner.ActivePlayers)
                {
                    Debug.Log("*** CAMBIO SCENE ACTIVE PLAYERS REF " + playerRef);

                    if (Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObject))
                    {
                        Debug.Log("*** CAMBIO SCENE PLAYER ASOCIADO " + playerObject + " - " + playerObject.GetComponent<GO_PlayerNetworkManager>().currentLevel_ID);

                        if (playerObject.GetComponent<GO_PlayerNetworkManager>().playerID == player.playerID)
                        {
                            Debug.Log("*** CAMBIO SCENE ASIGNE E LNUEVO REFT ");
                            newPlayerRefAuthorityChangeScene = playerRef;
                            break;
                        }
                    }
                }

                return true;
            }
        }
        return false;
    }

    public void CheckPlayerInNewLevel()
    {
        foreach (GO_PlayerNetworkManager player in GO_PlayerNetworkManager.PlayersList)
        {
            Debug.Log("---COMPARAMOS PLAYER: " + player.playerID);

            if (player != GO_PlayerNetworkManager.localPlayer)
            {
                if (player.currentLevel_ID == (short) GO_SpawnPoint.currentSpawPoint.level_ID)
                {
                    Debug.Log("---ENTONTRAMOS PLAYER: " + player.playerID);        
                    return;
                }
            }
        }
        foreach(NetworkObject networkObject in networkObjectsSpawned)
        {
            Debug.Log("---COMPARAMOS EL SIGUIENTE OBJETO: " + networkObject.name);
            if (networkObject.GetComponent<GO_NetworkObject>().level_ID == GO_PlayerNetworkManager.localPlayer.currentLevel_ID)
            {
                Debug.Log("---APROBAMOS EN EL NIVEL ACUTAL EL SIGUIENTE OBJETO: " + networkObject.name);
                networkObject.RequestStateAuthority();
            }
        }
    }

    public void CheckPlayerInCurrentLevel(PlayerRef _player)
    {

        if (GO_PlayerNetworkManager.localPlayer.currentLevel_ID == (short)GO_SpawnPoint.currentSpawPoint.level_ID)
        {
            foreach (NetworkObject networkObject in networkObjectsSpawned)
            {
                Debug.Log("+++COMPARAMOS EL SIGUIENTE OBJETO: " + networkObject.name);

                if (networkObject.StateAuthority == _player)
                {
                    networkObject.RequestStateAuthority();
                }
            }
        }
    }

    [ContextMenu("PERDER VIDA")]
    public void perderUnaVida()
    {
        if(_currentLevel != Level.L_GO_Level1 || true)//REVERT
        {
            _playerInstance.playerLives--;
            Debug.Log($"Jugador Recibio ataque, vidas restantes"+_playerInstance.playerLives);
            OnLivesChanged?.Invoke(_playerInstance.playerLives);
            //RPC_setLifes(_playerInstance.playerID, -1);
            ResetPlayerPosition();
        }
        Debug.Log(_playerInstance.playerLives);
    }
    private void ChangeScene()
    {
        switch (_currentLevel)
        {
            case Level.L_GO_Level0:
            case Level.L_GO_Level1:
            case Level.L_GO_Level2:
            case Level.L_GO_Level3:
                _currentLevel++;
                break;
            case Level.L_GO_Level4:
                _playerInstance.playerLives = 3;
                OnLivesChanged?.Invoke(_playerInstance.playerLives);
                //RPC_setLifes(_playerInstance.playerID, 3);
                if (DidSabotage)
                {
                    _currentLevel = Level.L_GO_Level5;
                }
                else
                {
                    _currentLevel = Level.L_GO_Level6;
                }
                break;
            case Level.L_GO_Level5:
            case Level.L_GO_Level6:
                _currentLevel = Level.L_GO_Level1;
                _playerInstance.playerLives = 3;
                OnLivesChanged?.Invoke(_playerInstance.playerLives);
                //RPC_setLifes(_playerInstance.playerID, 3);
                SceneManager.LoadScene("IntroScene");
                return;
        }
        StartCoroutine(LoadLevelAsync(_currentLevel));
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
            if (GO_GameOverManager.Instance == null && popupManagerPrefab != null)
            {
                Instantiate(popupManagerPrefab);

                StartCoroutine(WaitForPopupManagerAndShowPopup());
            }
            else
            {
                GO_GameOverManager.Instance.ShowPopup();
            }
            
            // Instancia PopupManager si no existe
            //RPC_setLifes(_playerInstance.playerID, (short)totalLives);
            _playerInstance.playerLives = totalLives;
            OnLivesChanged?.Invoke(_playerInstance.playerLives);
            _currentLevel = Level.L_GO_Level1;
            Debug.Log(_playerInstance.playerLives);
            
        }
    }
    
    private IEnumerator WaitForPopupManagerAndShowPopup()
    {
        while (GO_GameOverManager.Instance == null)
        {
            yield return null; 
        }

        GO_GameOverManager.Instance.ShowPopup();
    }
    
    [ContextMenu("RESET LEVEL")]
    public void LoadLevelAsync()
    {
        StartCoroutine(LoadLevelAsync(_currentLevel = Level.L_GO_Level1));
    }
    public IEnumerator LoadLevelAsync(Level level)
    {
        string sceneName = level.ToString();

        if (!CheckPlayerInLastLevel())//
        {
            Debug.Log("*** CAMBIO SCENE NO HAY PLAYER ACTIVOS");
        }

        OnPlayerChangeScene?.Invoke();
        while (true) 
        {
            GO_NetworkObject.readyChangeScene = true;
            yield return new WaitForSeconds(1);
            Debug.Log("ESPERANDO TRANSFERENCIA DE AUTORDADES");
            if(GO_NetworkObject.readyChangeScene)
            {
                break;
            }
        }

        if (SceneManager.GetActiveScene().name != sceneName || true)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                Debug.Log($"Cargando... {asyncLoad.progress * 100} %");
                yield return null;
            }
            SpawnPlayer();
        }
        if (!CheckPlayerInLastLevel())//
        {
            Debug.Log("CAMBIO SCENE NO HAY PLAYER ACTIVOS EN LA ESCENA ACTUAL");
            CheckPlayerInNewLevel();
        }
        OnPlayerChangeScene?.Invoke();
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


            var NetworkObject = Runner.Spawn(prefabNetworkObjects, _pos, _rot);
            if (_codeName != null)
            {
                RPC_SetPoolSpawnObject(_codeName);
                RPC_SetPoolSpawnObject(NetworkObject);
            }


            return NetworkObject;
        }
        return null;
    }
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetPoolSpawnObject()
    {
        RPC_SendPoolSpawnObject(objectsSpawned.ToArray());        
    } 
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetPoolSpawnNetWorkObject()
    {
        RPC_SendPoolSpawnObject(networkObjectsSpawned.ToArray());        
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
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendPoolSpawnObject(NetworkObject[] _listSpawnedObject)
    {
        if (networkObjectsSpawned.Count == 0 && !Object.HasStateAuthority)
        {
            networkObjectsSpawned = _listSpawnedObject.ToList<NetworkObject>();
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
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_SetPoolSpawnObject(NetworkObject _nameObjectSpawned)
    {
        if (!networkObjectsSpawned.Contains(_nameObjectSpawned))
        {
            networkObjectsSpawned.Add(_nameObjectSpawned);
        }
    }
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
