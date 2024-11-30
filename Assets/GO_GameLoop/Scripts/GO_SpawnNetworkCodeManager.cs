using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnCodeManager : MonoBehaviour
{
    [SerializeField] private GameObject[] codeManagerPrefabs; // Prefabs de CodeManager espec�ficos para cada nivel.
    private GO_LevelManager _levelManager;
    private static GameObject currentCodeManager; // Referencia al CodeManager actual para evitar duplicados.
    public static event Action OnCodeManagerReady;
    
    private void Start()
    {
        StartCoroutine(WaitForLevelManagerAndSpawn());
    }

    private IEnumerator WaitForLevelManagerAndSpawn()
    {
        if (currentCodeManager == null) 
        {
            while (GO_LevelManager.instance == null)
            {
                yield return null; // Espera un frame.
            }
            while (GO_LevelManager.instance.isReady == false)
            {
                yield return null; // Espera un frame.
            }
            // Asigna la referencia del LevelManager.
            _levelManager = GO_LevelManager.instance;

            // Una vez disponible, procede con el spawn.
            SpawnOrKeepCodeManager();
        }

        this.gameObject.SetActive(true);
    }

    public void SpawnOrKeepCodeManager()
    {
        int currentSceneIndex = (int)GO_SpawnPoint.currentSpawPoint.level_ID;

        // Verifica que el �ndice est� dentro del rango del array de prefabs.
        if (currentSceneIndex >= codeManagerPrefabs.Length)
        {
            Debug.LogError("No hay un CodeManager configurado para este nivel.");
            return;
        }

        // Construye un nombre �nico para identificar el CodeManager de este nivel.
        string codeManagerName = $"CodeManager_Level_{currentSceneIndex}";

        // Verifica si ya existe un CodeManager global.
        if (currentCodeManager != null && currentCodeManager.name == codeManagerName)
        {
            Debug.Log($"El CodeManager para el nivel {currentSceneIndex} ya existe y es persistente.");
            return;
        }

        // Si hay un CodeManager existente pero no pertenece a esta escena, lo destruimos.
        if (currentCodeManager != null && currentCodeManager.name != codeManagerName)
        {
            Debug.Log($"Destruyendo CodeManager anterior: {currentCodeManager.name}");
            Destroy(currentCodeManager);
        }
        if(currentCodeManager == null)
        {
            _levelManager.SpawnObjects(
            codeManagerPrefabs[currentSceneIndex],
            Vector3.zero,
            Quaternion.identity, 
            name
        );
        }
       
        OnCodeManagerReady?.Invoke();
    }
}
