using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    [HideInInspector] public AudioSource source;

    public string name;

    public string subtitle = "";

    public AudioMixerGroup mixerGroup;

    public AudioClip clip;

    [Range(0, 1)] public float volume = 0.5f;

    [Range(0.1f, 3)] public float pitch = 1f;

    public float refractionTime = 0f;

    public bool loop;

    [HideInInspector] public float lastPlayedTime;
}
