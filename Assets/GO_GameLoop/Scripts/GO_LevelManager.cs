using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StarterAssets;
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
    public bool isReadyObjects;
    public short id;
    public static GO_LevelManager instance = null;
    public GameObject popupManagerPrefab;
    public GO_NetworkObject armPlayer;
    public PlayerRef CurrentPlayerRefChangeScene;//Player que es dueño del objeto, pero esta dejando la escena
    public PlayerRef newPlayerRefAuthorityChangeScene;// Posible player que queda en el nivel actual y se le puedn transferir las Autoridades

    public int totalLives = 3;
    public bool DidSabotage = false;

    //private int _currentLives;

    //private GameObject _playerInstance;
    public GO_PlayerNetworkManager _playerInstance;

    private GO_PlayerUIManager uiplayer;

    private GameObject _playerPrefab;

    [SerializeField] private Level _currentLevel;
    
    public Color[] playerColors;

    private Transform _spawnPoint;
    private Transform _endPoint;

    private bool isChangingScene = false;

    [SerializeField] GameObject prefabNetworkObjects;

    public static event Action OnPlayerChangeScene;
    
    public event Action<float> OnLivesChanged;

    public bool debug;
    
    public static event Action OnPlayerDied;

    public override void Spawned()
    {
        if(debug)Debug.Log("Iniciando " + instance);
        _currentLevel = Level.L_GO_Level0;
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
            if(debug)Debug.Log("No player Ready");
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
            if(debug)Debug.LogError("No se encontr� un objeto con el tag 'Spawn' en la escena.");
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
            uiplayer  = _playerInstance.transform.GetChild(2).GetComponent<GO_PlayerUIManager>();
            uiplayer.RemoveBooksNumber();
            CurrentPlayerRefChangeScene = Runner.LocalPlayer;
            ChangeScene();
            if (GO_PlayerNetworkManager.localPlayer.actionPlayer.CountArms() < 2)
            {
                GO_PlayerNetworkManager.localPlayer.RPC_addNewArm();
            }
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
            if(debug)Debug.Log("*** CAMBIO SCENE " + player.currentLevel_ID + " - " + player.name + " POINT: " + GO_SpawnPoint.currentSpawPoint.level_ID);

            if ((short) GO_SpawnPoint.currentSpawPoint.level_ID == player.currentLevel_ID)
            {
                if(debug)Debug.Log("*** CAMBIO SCENE " + player.currentLevel_ID + " - " + player.name);

                foreach (var playerRef in Runner.ActivePlayers)
                {
                    if(debug)Debug.Log("*** CAMBIO SCENE ACTIVE PLAYERS REF " + playerRef);

                    if (Runner.TryGetPlayerObject(playerRef, out NetworkObject playerObject))
                    {
                        if(debug)Debug.Log("*** CAMBIO SCENE PLAYER ASOCIADO " + playerObject + " - " + playerObject.GetComponent<GO_PlayerNetworkManager>().currentLevel_ID);

                        if (playerObject.GetComponent<GO_PlayerNetworkManager>().playerID == player.playerID)
                        {
                            if(debug)Debug.Log("*** CAMBIO SCENE ASIGNE E LNUEVO REFT ");
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
            if(debug)Debug.Log("---COMPARAMOS PLAYER: " + player.playerID);

            if (player != GO_PlayerNetworkManager.localPlayer)
            {
                if (player.currentLevel_ID == (short) GO_SpawnPoint.currentSpawPoint.level_ID)
                {
                    if(debug)Debug.Log("---ENTONTRAMOS PLAYER: " + player.playerID);        
                    return;
                }
            }
        }
        foreach(NetworkObject networkObject in networkObjectsSpawned)
        {
            if(debug)Debug.Log("---COMPARAMOS EL SIGUIENTE OBJETO: " + networkObject.name);
            if (networkObject.GetComponent<GO_NetworkObject>().level_ID == GO_PlayerNetworkManager.localPlayer.currentLevel_ID)
            {
                if(debug)Debug.Log("---APROBAMOS EN EL NIVEL ACUTAL EL SIGUIENTE OBJETO: " + networkObject.name);
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
                if(debug)Debug.Log("+++COMPARAMOS EL SIGUIENTE OBJETO: " + networkObject.name);

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
            if(debug)Debug.Log($"Jugador Recibio ataque, vidas restantes"+_playerInstance.playerLives);
            OnLivesChanged?.Invoke(_playerInstance.playerLives);
            OnPlayerDied?.Invoke();
            GO_ThirdPersonController control = _playerInstance.GetComponentInChildren<GO_ThirdPersonController>();
            control.RegenerateAllStamina();
            ResetPlayerPosition();
        }
        if(debug)Debug.Log(_playerInstance.playerLives);
    }
    private void ChangeScene()
    {
        _playerInstance.playerLives = totalLives;
        OnLivesChanged?.Invoke(_playerInstance.playerLives);
        if(debug)Debug.Log("SERIE " + _currentLevel);

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
        if(debug)Debug.Log("SERIE " + _currentLevel);
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
                if(debug)Debug.LogError("playerInstance es nulo. Aseg�rate de que el jugador est� correctamente instanciado.");
                return;
            }

            if (_spawnPoint == null)
            {
                if(debug)Debug.LogError("spawnPoint es nulo. Aseg�rate de que el punto de spawn est� correctamente asignado.");
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
            if(debug)Debug.Log(_playerInstance.playerLives);
            
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
        StartCoroutine(LoadLevelAsync(_currentLevel = Level.L_GO_Level0));
        GO_PlayerNetworkManager.localPlayer.RPC_ResetArms();
      
    }
    public IEnumerator LoadLevelAsync(Level level)
    {
        string sceneName = level.ToString();
        if(debug)Debug.Log("*** CARGANDO SCENE " + sceneName);

        if (!CheckPlayerInLastLevel())//
        {
            if(debug)Debug.Log("*** CAMBIO SCENE NO HAY PLAYER ACTIVOS");
        }

        OnPlayerChangeScene?.Invoke();

        while (true)
        {
            GO_NetworkObject.readyChangeScene = true;
            yield return new WaitForSeconds(1);
            if(debug)Debug.Log("ESPERANDO TRANSFERENCIA DE AUTORDADES");
            if (GO_NetworkObject.readyChangeScene)
            {
                break;
            }
        }


        if (SceneManager.GetActiveScene().name != sceneName || true)
        {
            GO_SpawnPoint.currentSpawPoint = null;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncLoad.isDone)
            {
                if(debug)Debug.Log($"***Cargando... {asyncLoad.progress * 100} % - sceneName: " + sceneName + " - SpawnPoint: " + GO_SpawnPoint.currentSpawPoint);
                yield return null;
            }

            yield return new WaitWhile(() => GO_SpawnPoint.currentSpawPoint == null);
            if(debug)Debug.Log($"***Cargando... {asyncLoad.progress * 100} % - SpawnPointB: " + GO_SpawnPoint.currentSpawPoint);

            SpawnPlayer();

            if (!CheckPlayerInLastLevel())//
            {
                if(debug)Debug.Log("CAMBIO SCENE NO HAY PLAYER ACTIVOS EN LA ESCENA ACTUAL");
                CheckPlayerInNewLevel();
            }
            OnPlayerChangeScene?.Invoke();
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
        if(debug)Debug.Log("MANDO SPAWN " + objectsSpawned.Count);

        if (_codeName == null || !objectsSpawned.Contains(_codeName))
        {
            if(debug)Debug.Log("MANDO SPAWN un player " + _codeName);

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
        int chunkSize = 18;
        if(objectsSpawned.Count < chunkSize)
        {
            RPC_SendPoolSpawnObject(objectsSpawned.ToArray(), 2);        
            return;
        }
        int totalChunks = Mathf.CeilToInt(objectsSpawned.Count / (float)chunkSize);
        List< List<string> > listListTMP = new List< List<string> >();


        for (int i = 0; i < totalChunks; i++)
        {
            List<string> chunk = objectsSpawned.GetRange(i * chunkSize, Mathf.Min(chunkSize, objectsSpawned.Count - i * chunkSize));
            listListTMP.Add( chunk );                       
        }

        for (int i = 0; i < listListTMP.Count; i++)
        {
            RPC_SendPoolSpawnObject(listListTMP[i].ToArray(), (short)(i == listListTMP.Count - 1 ? 2 : ( i == 0 ? 0 : 1 ))); //SendChunk(chunk);
        }
       // RPC_SendPoolSpawnObject(objectsSpawned.ToArray());        
    } 
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetPoolSpawnNetWorkObject()
    {
        int chunkSize = 18;
        if (networkObjectsSpawned.Count < chunkSize)
        {
            RPC_SendPoolSpawnObject(networkObjectsSpawned.ToArray(), 2);
            return;
        }
        int totalChunks = Mathf.CeilToInt(networkObjectsSpawned.Count / (float)chunkSize);
        List<List<NetworkObject>> listListTMP = new List<List<NetworkObject>>();

        for (int i = 0; i < totalChunks; i++)
        {
            List<NetworkObject> chunk = networkObjectsSpawned.GetRange(i * chunkSize, Mathf.Min(chunkSize, networkObjectsSpawned.Count - i * chunkSize));
            listListTMP.Add(chunk);
        }

        for (int i = 0; i < listListTMP.Count; i++)
        {
            RPC_SendPoolSpawnObject(listListTMP[i].ToArray(), (short)(i == listListTMP.Count - 1 ? 2 : (i == 0 ? 0 : 1)));
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendPoolSpawnObject(string[] _listSpawnedObject, short _poss)
    {
        if(debug)Debug.Log("RECIBI ESTO: " + objectsSpawned.Count + " - " + _listSpawnedObject.Length);

        if (/*(objectsSpawned.Count == 0 && _poss == 0) && */ !isReady && !Object.HasStateAuthority)
        {
            //objectsSpawned = _listSpawnedObject.ToList<string>();
            objectsSpawned.AddRange(_listSpawnedObject.ToList<string>());
            isReady = _poss == 2;
        }

    }
    
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendPoolSpawnObject(NetworkObject[] _listSpawnedObject, short _poss)
    {
        if (!isReadyObjects && !Object.HasStateAuthority)
        {
            networkObjectsSpawned.AddRange(_listSpawnedObject.ToList<NetworkObject>());
            isReadyObjects = _poss == 2;
        }
        if(debug)Debug.Log("RECIBI ESTO OBJETO: " + _listSpawnedObject.Length);
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
        if(debug)Debug.Log("RPC: " + _playerID + " - " + GO_PlayerNetworkManager.localPlayer.playerID);
        if(_playerID == GO_PlayerNetworkManager.localPlayer.playerID)
        {
            GO_PlayerNetworkManager.localPlayer.playerLives += _value;
        }

    }

    [Rpc(RpcSources.All, RpcTargets.All)]//, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_OpenFinalDoor()
    {
        StartCoroutine(GO_UIManager.Instance.OpenDoor());
    }

}
 