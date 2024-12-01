using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using StarterAssets;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class GO_CinematicManager : MonoBehaviour
{
    
    public PlayableDirector playableDirector; 
    public GameObject[] objectsToEnable;      
    public GameObject[] objectsToDisable;    
    public bool destroyOnFinish = true;  
    private static bool hasCinematicPlayed = false;
    private float elapsedTime = 0;
    
    [SerializeField] public TextMeshProUGUI totalTimeGame;


    private bool isCinematicPlaying = false;
    

    private void Start()
    {
        
        if (GO_LoadScene.Instance)
        {
            Debug.Log("Loading sCENE APAGADA");
            GO_LoadScene.Instance.HideLoadingScreen();
        }
        
        
        if (GO_LevelManager.instance != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            totalTimeGame.text = GO_LevelManager.instance.totalTime.ToString();
            totalTimeGame.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            GO_LevelManager.instance.PauseTimer();
            GO_AudioManager.Instance.PlayAmbientSound("GO_Final_Track");   
            GO_PlayerNetworkManager.localPlayer.gameObject.SetActive(false);
            Debug.Log("Desconectando jugador A");
            if (GO_LevelManager.instance._currentLevel == GO_LevelManager.Level.L_GO_ZombieHeaven)
            {
                Debug.Log("Desconectando jugador A");
                
                GO_RunnerManager.Instance._runner.Despawn(GO_PlayerNetworkManager.localPlayer.GetComponent<NetworkObject>());
                Debug.Log("se DESCONECTO despawneo");
                GO_RunnerManager.Instance._runner.Disconnect(GO_RunnerManager.Instance._runner.LocalPlayer);
                Debug.Log(("SE DESCCONECTO EL JUGADRO"));
                Destroy((GO_PlayerNetworkManager.localPlayer.gameObject));
                Debug.Log(("SE DESCONECTO DESTRUYÓ EL JUGADRO"));
                Destroy((GO_RunnerManager.Instance._runner.gameObject));
                Debug.Log(("SE DESCONECTO DESTRUYÓ EL RUNNER"));
                
                
            }
        }
        
        
        if (hasCinematicPlayed)
        {
            HandleCinematicCompletion();
            return;
        }
        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        if (GO_AudioManager.Instance != null)
        {
            GO_AudioManager.Instance.PlayAmbientSound("GO_Cinematic_Track");
        }

        playableDirector.stopped += OnPlayableDirectorStopped;
        playableDirector.Play();

        isCinematicPlaying = true;
    }



    private void Update()
    {
        if (isCinematicPlaying && Input.GetKeyDown(KeyCode.F))
        {
            SkipCinematic();
        }
    }

    private void SkipCinematic()
    {
        playableDirector.Stop();

        OnPlayableDirectorStopped(playableDirector);
    }

    private void OnDestroy()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnPlayableDirectorStopped;
        }
    }

    private void OnPlayableDirectorStopped(PlayableDirector director)
    {
        isCinematicPlaying = false;

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        GO_LoadScene.Instance.ShowLoadingScreen();

        hasCinematicPlayed = true;

        if (destroyOnFinish)
        {
            Destroy(gameObject);
        }
    }
    
    private void HandleCinematicCompletion()
    {
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

        GO_LoadScene.Instance.ShowLoadingScreen();

        if (destroyOnFinish)
        {
            Destroy(gameObject);
        }
    }


}
