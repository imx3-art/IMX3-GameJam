using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GO_LevelManager : MonoBehaviour
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

    public static GO_LevelManager instance;
    public GameObject popupManagerPrefab;

    public int totalLives = 3;
    public bool DidSabotage = false;

    private int _currentLives;

    private GameObject _playerInstance;
    private GameObject _playerPrefab;

    private Level _currentLevel = Level.Nivel0;

    private Transform _spawnPoint;
    private Transform _endPoint;

    private bool isChangingScene = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _currentLives = totalLives;

        _playerInstance = GameObject.FindWithTag("Player");
        if (_playerInstance == null)
        {
            SpawnPlayer();
        }

        _spawnPoint = GameObject.FindWithTag("Spawn")?.transform;
        if (_spawnPoint == null)
        {
            Debug.LogError("No se encontró un objeto con el tag 'Spawn' en la escena.");
            return;
        }

        if (_playerInstance != null)
        {
            _playerInstance.transform.position = _spawnPoint.position;
        }

        LoadLevel(_currentLevel);
    }

    void SpawnPlayer()
    {
        if (_playerInstance == null)
        {
            _playerInstance = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
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
            ChangeScene();
        }
    }

    [ContextMenu("PERDER VIDA")]
    private void perderUnaVida()
    {
        if(_currentLevel != Level.Nivel0)
        {
            _currentLives--;
            ResetPlayerPosition();
        }
        Debug.Log(_currentLives);
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
                _currentLives = 3;
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
                _currentLives = 3;
                SceneManager.LoadScene("IntroScene");
                return;
        }
        LoadLevel(_currentLevel);
    }

    private void ResetPlayerPosition()
    {
        if (_currentLives > 0)
        {
            _spawnPoint = GameObject.FindWithTag("Spawn")?.transform;
            _playerInstance = GameObject.FindWithTag("Player");

            if (_playerInstance == null)
            {
                Debug.LogError("playerInstance es nulo. Asegúrate de que el jugador esté correctamente instanciado.");
                return;
            }

            if (_spawnPoint == null)
            {
                Debug.LogError("spawnPoint es nulo. Asegúrate de que el punto de spawn esté correctamente asignado.");
                return;
            }

            _playerInstance.transform.position = _spawnPoint.position;
            isChangingScene = false;
        }
        else
        {
            // Instancia PopupManager si no existe
            _currentLives = totalLives;
            _currentLevel = Level.Nivel0;
            Debug.Log(_currentLives);
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
}
