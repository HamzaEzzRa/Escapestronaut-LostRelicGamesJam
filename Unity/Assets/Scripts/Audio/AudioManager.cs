using UnityEngine;
using System;
using System.Collections;
using TMPro;

public class AudioManager : MonoBehaviour
{
    public Sound CurrentVoice => currentVoice;

    public static AudioManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI subtitleMesh;
    [SerializeField] private Sound[] sounds;

    private LTDescr subtitleDescr;

    private Sound currentVoice;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        foreach (Sound sound in sounds)
        {
            if (sound.clip != null)
            {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;

                sound.source.volume = sound.volume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;

                sound.source.outputAudioMixerGroup = sound.mixerGroup;
            }
        }
    }

    private void Start()
    {
        Play("Main Menu");
    }

    private void Update()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.clip != null)
            {
                sound.source.clip = sound.clip;

                sound.source.volume = sound.volume;
                sound.source.pitch = sound.pitch;
                sound.source.loop = sound.loop;
            }
        }
    }

    public void Play(string name)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound == null || (Time.time - sound.lastPlayedTime) < sound.refractionTime)
        {
            return;
        }

        if (sound.mixerGroup.name == "Voice")
        {
            currentVoice = sound;
        }
        sound.lastPlayedTime = Time.time;
        sound.source.Play();

        if (GameManager.Instance.SubtitlesOn && sound.subtitle != "")
        {
            if (subtitleDescr != null)
            {
                LeanTween.cancel(subtitleDescr.uniqueId);
            }

            subtitleMesh.SetText(sound.subtitle);
            subtitleDescr = LeanTween.delayedCall(sound.clip.length + 0.5f, () =>
            {
                subtitleMesh.SetText("");
            });
        }
    }

    public void WaitForAudio(string[] names, float offset)
    {
        StartCoroutine(WaitForAudioCoroutine(names, offset));
    }

    private IEnumerator WaitForAudioCoroutine(string[] names, float offset)
    {
        yield return new WaitForSeconds(currentVoice.clip.length + offset);
        foreach (string name in names)
        {
            Play(name);
            yield return new WaitForSeconds(currentVoice.clip.length + offset);
        }
    }

    public void WaitForAudio(string[] names, float offset, Action callback)
    {
        StartCoroutine(WaitForAudioCoroutine(names, offset, callback));
    }

    private IEnumerator WaitForAudioCoroutine(string[] names, float offset, Action callback)
    {
        yield return new WaitForSeconds(currentVoice.clip.length + offset);
        foreach (string name in names)
        {
            Play(name);
            yield return new WaitForSeconds(currentVoice.clip.length + offset);
        }

        callback?.Invoke();
    }

    public void Stop(string name)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null && sound.source.isPlaying)
        {
            sound.source.Stop();
        }

        if (currentVoice != null && currentVoice.name == name)
        {
            currentVoice = null;
        }
    }

    public void StopCurrentVoice()
    {
        if (currentVoice == null)
        {
            return;
        }

        currentVoice.source.Stop();
        if (currentVoice.subtitle != "")
        {
            if (subtitleDescr != null)
            {
                LeanTween.cancel(subtitleDescr.uniqueId);
            }

            subtitleMesh.SetText("");
        }

        currentVoice = null;
    }

    public void ChangeVolume(string name, float volume)
    {
        Sound sound = Array.Find(sounds, s => s.name == name);
        if (sound != null)
        {
            sound.volume = volume;
            sound.source.volume = volume;
        }
    }
}
