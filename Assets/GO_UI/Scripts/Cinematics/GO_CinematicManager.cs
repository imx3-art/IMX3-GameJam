using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class GO_CinematicManager : MonoBehaviour
{
    public PlayableDirector playableDirector; 
    public GameObject[] objectsToEnable;      
    public GameObject[] objectsToDisable;    
    public bool destroyOnFinish = true;  
    private static bool hasCinematicPlayed = false;


    private bool isCinematicPlaying = false; 

    private void Start()
    {
        if (hasCinematicPlayed)
        {
            HandleCinematicCompletion();
            return;
        }

        if (GO_LoadScene.Instance)
        {
            GO_LoadScene.Instance.HideLoadingScreen();
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
