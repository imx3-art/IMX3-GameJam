using Fusion;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GO_LevelManager : NetworkBehaviour
{
    public enum Level
    {
        Nivel0,
        Nivel1,
        Nivel2,
        Nivel3,
        Nivel4,
        Nivel5,
        Nivel6
    }

    public short id;
    public static GO_LevelManager instance;
    public GameObject popupManagerPrefab;

    public int totalLives = 3;
    public bool DidSabotage = false;

    //private int _currentLives;

    //private GameObject _playerInstance;
    public GO_PlayerNetworkManager _playerInstance;
    private GameObject _playerPrefab;

    private Level _currentLevel = Level.Nivel0;

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
                SpawnObjects();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {

        //_playerInstance = GameObject.FindWithTag("Player");

        while (true)
        {
            await Task.Delay(100);
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

        LoadLevel(_currentLevel);
    }

    [ContextMenu("Teleport last Pont")]
    void SpawnPlayer()
    {
        if (_playerInstance != null)
        {
            //_playerInstance = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
            
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

    private async void HandlePlayerEnterArea()
    {
        if (!isChangingScene)
        {
            ChangeScene();
            await Task.Delay(1000);
            SpawnPlayer();
            //ResetPlayerPosition();
        }
    }
    
    [ContextMenu("PERDER VIDA")]
    public void perderUnaVida()
    {
        if(_currentLevel != Level.Nivel0 || true)//REVERT
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
            case Level.Nivel0:
            case Level.Nivel1:
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
                _currentLevel = Level.Nivel0;
                _playerInstance.playerLives = 3;
                //RPC_setLifes(_playerInstance.playerID, 3);
                SceneManager.LoadScene("IntroScene");
                return;
        }
        LoadLevel(_currentLevel);
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
            _currentLevel = Level.Nivel0;
            Debug.Log(_playerInstance.playerLives);
            if (GO_PopUpManager.Instance == null && popupManagerPrefab != null)
            {
                Instantiate(popupManagerPrefab);
            }
            GO_PopUpManager.Instance.ShowPopup();
        }
    }

    public void LoadLevel(Level level)
    {
        string sceneName = level.ToString();

        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
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


    public void SpawnObjects()
    {
        Debug.Log("RESPQEN OBJETO");
        Runner.Spawn(prefabNetworkObjects, Vector3.zero + Vector3.up * 10, Quaternion.identity);
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
