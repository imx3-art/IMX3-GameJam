using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using StarterAssets;
using UnityEngine;

public class GO_AudioManager : MonoBehaviour
{
    public static GO_AudioManager Instance;
    
    public GO_Sound[] uiSounds;
    public GO_Sound[] gameSounds;
    public GO_Sound[] ambientSounds;

    private AudioSource ambientSource;
    
    public float minDistance = 1.0f;
    public float maxDistance = 20.0f;
    
    public float panRange = 10.0f;
    
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    
    [Range(0f, 1f)]
    public float gameVolume = 1f;
    
    [SerializeField]
    private Transform playerTransform;

    private Dictionary<string, AudioSource> loopingSounds = new Dictionary<string, AudioSource>();
    
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAudioSources();
    }
    
    private void InitializeAudioSources()
    {
        foreach (GO_Sound s in uiSounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.loop = s.loop;
            s.source.volume = s.volume;
        }
        
        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.loop = true;
        ambientSource.volume = masterVolume;
    }
    
    public void PlayAmbientSound(string name)
    {
        GO_Sound s = Array.Find(ambientSounds, sound => sound.name == name);
        if (s != null)
        {
            if (ambientSource.clip == s.clip && ambientSource.isPlaying)
            {
                return;
            }
            ambientSource.Stop();
            ambientSource.volume = s.volume;
            ambientSource.clip = s.clip;
            ambientSource.Play();
        }
        else
        {
            Debug.LogWarning("Sonido ambiental no encontrado: " + name);
        }
    }

    public void StopAmbientSound()
    {
        if (ambientSource.isPlaying)
        {
            ambientSource.Stop();
        }
    }

    public void PlayUISound(string name)
    {
        GO_Sound s = System.Array.Find(uiSounds, sound => sound.name == name);
        if (s != null)
        {
            s.source.Play();
        }
        else
        {
            Debug.LogWarning("Sonido de UI no encontrado: " + name);
        }
    }
    
    public void PlayGameSoundByName(string name, Vector3 position,NetworkObject parentTransform = null, String SoundID = null)
    {
        GO_Sound s = Array.Find(gameSounds, sound => sound.name == name);
        if (s != null)
        {
            if (SoundID != null)
            {
                PlayGameSoundDynamicLoop(s.clip, parentTransform.transform, SoundID);
            }
            else
            {
                PlayGameSoundDynamic(s.clip, position);
            }
            
        }
        else
        {
            Debug.LogWarning($"Sonido del juego no encontrado: {name}");
        }
    }

    public void PlayGameSoundDynamic(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            GameObject soundObject = new GameObject("Sound_" + clip.name);
            soundObject.transform.position = position;
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = false; 
            source.spatialBlend = 0f; 
            source.playOnAwake = false;

            Transform player = GetPlayerTransform();

            if (player != null)
            {
                float distance = Vector3.Distance(soundObject.transform.position, player.position);
                float volume = Mathf.Clamp01(1 - ((distance - minDistance) / (maxDistance - minDistance)));
                source.volume = volume * masterVolume * gameVolume;

                float pan = Mathf.Clamp((soundObject.transform.position.x - player.position.x) / panRange, -1f, 1f);
                source.panStereo = pan;

                //Debug.Log($"Sonando sonido dinámico. Distancia:{clip.name} {distance}, Volumen: {source.volume}");
            }
            else
            {
                Debug.LogWarning("playerTransform es nulo en PlayGameSoundDynamic.");
                source.volume = masterVolume * gameVolume;
            }

            source.Play();

            StartCoroutine(Handle3DSound(source));

            Destroy(soundObject, clip.length);
        }
        else
        {
            Debug.LogWarning("Clip de sonido nulo en PlayGameSoundDynamic.");
        }
    }
    
    public void PlayGameSoundDynamicLoop(AudioClip clip, Transform parentTransform, string SoundID)
    {
        if (loopingSounds.ContainsKey(SoundID))
        {
            Debug.LogWarning($"Ya existe un sonido en loop con el ID: {SoundID}");
            return;
        }
        
        if (clip != null)
        {
            GameObject soundObject = new GameObject("SoundLoop_" + clip.name);
            soundObject.transform.SetParent(parentTransform);
            soundObject.transform.localPosition = Vector3.zero;
            AudioSource source = soundObject.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true; 
            source.spatialBlend = 0f; 
            source.playOnAwake = false;

            Transform player = GetPlayerTransform();

            if (player != null)
            {
                float distance = Vector3.Distance(soundObject.transform.position, player.position);
                float volume = Mathf.Clamp01(1 - ((distance - minDistance) / (maxDistance - minDistance)));
                source.volume = volume * masterVolume * gameVolume;

                float pan = Mathf.Clamp((soundObject.transform.position.x - player.position.x) / panRange, -1f, 1f);
                source.panStereo = pan;

                Debug.Log($"Sonando sonido Loop. Distancia:{clip.name} {distance}, Volumen: {source.volume}");
            }
            else
            {
                Debug.LogWarning("playerTransform es nulo en PlayGameSoundDynamic.");
                source.volume = masterVolume * gameVolume;
            }

            source.Play();
            loopingSounds.Add(SoundID, source);

            StartCoroutine(Handle3DSoundLoop(source, SoundID));

        }
        else
        {
            Debug.LogWarning("Clip de sonido nulo en PlayGameSoundDynamic.");
        }
    }

    private IEnumerator Handle3DSound(AudioSource source)
    {
        while (source != null && source.isPlaying)
        {
            Transform player = GetPlayerTransform();
            if (player != null)
            {
                float distance = Vector3.Distance(source.transform.position, player.position);
                float volume = Mathf.Clamp01(1 - ((distance - minDistance) / (maxDistance - minDistance)));
                source.volume = volume * masterVolume * gameVolume;

                float pan = Mathf.Clamp((source.transform.position.x - player.position.x) / panRange, -1f, 1f);
                source.panStereo = pan;

                if (volume <= 0f)
                {
                    source.Stop();
                    Destroy(source.gameObject);
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning("playerTransform es nulo en Handle3DSound.");
            }

            yield return null;
        }
    }
    
    private IEnumerator Handle3DSoundLoop(AudioSource source, string SoundID)
    {
        while (source != null && source.isPlaying)
        {
            Transform player = GetPlayerTransform();
            if (player != null)
            {
                float distance = Vector3.Distance(source.transform.position, player.position);
                float volume = Mathf.Clamp01(1 - ((distance - minDistance) / (maxDistance - minDistance)));
                source.volume = volume * masterVolume * gameVolume;

                float pan = Mathf.Clamp((source.transform.position.x - player.position.x) / panRange, -1f, 1f);
                source.panStereo = pan;

                if (volume <= 0f)
                {
                    source.Stop();
                    Destroy(source.gameObject);
                    loopingSounds.Remove(SoundID);
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning("playerTransform es nulo en Handle3DSound.");
            }

            yield return null;
        }
    }
    
    public void StopGameSoundLoop(string SoundID)
    {
        if (loopingSounds.TryGetValue(SoundID, out AudioSource source))
        {
            if (source != null)
            {
                source.Stop();
                Destroy(source.gameObject);
                loopingSounds.Remove(SoundID);
            }
        }
        else
        {
            Debug.LogWarning($"No se encontró ningún sonido en loop con el ID: {SoundID}");
        }
    }
    
    private Transform GetPlayerTransform()
    {
        if (playerTransform == null)
        {
            var localPlayer = GO_PlayerNetworkManager.localPlayer;
            if (localPlayer != null)
            {
                playerTransform = localPlayer.playerTransform.GetComponent<GO_ThirdPersonController>().transform;
            }
        }
        return playerTransform;
    }


    
    private void UpdateVolumes()
    {
        foreach (GO_Sound s in uiSounds)
        {
            if (s.source != null)
                s.source.volume = s.volume * masterVolume;
        }

        foreach (GO_Sound s in gameSounds)
        {
            if (s.source != null)
                s.source.volume = s.volume * masterVolume * gameVolume; 
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        UpdateVolumes();
    }
    
    public void SetGameVolume(float value)
    {
        gameVolume = Mathf.Clamp01(value);
        UpdateVolumes();
    }

}
