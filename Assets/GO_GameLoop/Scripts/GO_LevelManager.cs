using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GO_LevelManager : MonoBehaviour
{
    // Definimos el enum para manejar las escenas
    public enum Level
    {
        Nivel1,
        Nivel2,
        Nivel3,
        Nivel4
    }

    public static GO_LevelManager instance;

    // Variables para manejar las vidas del jugador
    public int totalLives = 3; // Número total de vidas
    private int currentLives; // Vidas actuales

    private GameObject playerInstance; // Asigna la instancia del jugador desde el Inspector
    private GameObject playerPrefab;  // Prefab del jugador

    private Level currentLevel = Level.Nivel1; // Nivel inicia
                                               // l
    private Transform spawnPoint;     // Transform del punto de spawn
    private Transform endPoint;       // Transform del punto final

    private bool isChangingScene = false; // Para evitar cambios repetidos de escena


    // Instancia estática para implementar el patrón Singleton
   

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
        currentLives = totalLives; // Inicializar las vidas

        // Asegúrate de que el playerInstance esté asignado
        playerInstance = GameObject.FindWithTag("Player");
        if (playerInstance == null)
        {
            SpawnPlayer();
        }

        // Asegúrate de que el spawnPoint esté asignado
        spawnPoint = GameObject.FindWithTag("Spawn")?.transform;
        if (spawnPoint == null)
        {
            Debug.LogError("No se encontró un objeto con el tag 'Spawn' en la escena.");
            return;
        }

        // Si ya está instanciado, mueve al jugador al punto de spawn
        if (playerInstance != null)
        {
            playerInstance.transform.position = spawnPoint.position;
        }

        LoadLevel(currentLevel);
    }


    void SpawnPlayer()
    {
        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
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
        Debug.Log("Jugador ha entrado en el área. Cambiando de escena...");

        if (!isChangingScene)
        {
            ChangeScene();
        }
    }


    [ContextMenu("PERDER  VIDA")]
    private void perderUnaVida()
    {
        currentLives = currentLives - 1;
        ResetPlayerPosition();
    }

    private void ChangeScene()
    {
        // Avanzar al siguiente nivel en el enum (circular entre los niveles)
        if (currentLevel < Level.Nivel4)
        {
            currentLevel++;
            Debug.Log(currentLevel);
        }
        else
        {
            currentLevel = Level.Nivel1; // Si llegamos al último nivel, reiniciamos al primer nivel
            currentLives = 3;
        }

        // Cargar la escena asociada con el enum actual
        LoadLevel(currentLevel);
    }


    private void ResetPlayerPosition()
    {
        Debug.Log(currentLives);
        if(currentLives > 0)
        {
            // Asegúrate de que el spawnPoint esté asignado
            spawnPoint = GameObject.FindWithTag("Spawn")?.transform;
            // Asegúrate de que el playerInstance esté asignado
            playerInstance = GameObject.FindWithTag("Player");
            if (playerInstance == null)
            {
                Debug.LogError("playerInstance es nulo. Asegúrate de que el jugador esté correctamente instanciado.");
                return;
            }

            if (spawnPoint == null)
            {
                Debug.LogError("spawnPoint es nulo. Asegúrate de que el punto de spawn esté correctamente asignado.");
                return;
            }

            Debug.Log("Jugador ha perdido una vida. Regresando al spawn del nivel actual.");
            playerInstance.transform.position = spawnPoint.position; // Regresar al punto de spawn
            isChangingScene = false; // Permitir que el jugador vuelva a entrar al área
        }
        else
        {
            currentLevel = Level.Nivel4;
            ChangeScene();
        }
    }


    private void LoadLevel(Level level)
    {
        string sceneName = level.ToString();

        if (SceneManager.GetActiveScene().name != sceneName)
        {
            Debug.Log("Cambiando a la escena: " + sceneName);
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.Log("Ya estamos en la escena " + sceneName);
        }
    }

    public void OnSceneLoaded()
    {
        Debug.Log("Escena cargada correctamente.");
        isChangingScene = false; // Resetear el flag de cambio de escena
    }

    private void Update()
    {
        if (isChangingScene && SceneManager.GetActiveScene().name == currentLevel.ToString())
        {
            OnSceneLoaded();
        }
    }
}