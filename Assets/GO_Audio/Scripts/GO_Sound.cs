using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GO_Sound : MonoBehaviour
{
    public string name;
    public AudioClip clip;
    public bool loop;

    [Range(0f, 1f)]
    public float volume = 1f;

    [HideInInspector]
    public AudioSource source;
}
