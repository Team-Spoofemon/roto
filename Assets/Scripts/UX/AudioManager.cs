using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MusicState
{
    None,
    Intro,
    LoopA,
    LoopB,
    LoopC,
    Outro,
    TransitionA,
    TransitionB
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioProfile profile;
    [SerializeField] private RealmType currentRealm;

    private AudioSource musicSource;
    private AudioSource ambienceSource;
    private MusicState currentState = MusicState.None;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        ambienceSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        ambienceSource.loop = true;

        Debug.Log($"AudioManager initialized for realm: {currentRealm}");
    }

    private AudioClip GetMusicClip(MusicState state)
    {
        switch (currentRealm)
        {
            case RealmType.CreteValley:
                switch (state)
                {
                    case MusicState.Intro: return profile.genesis[0];
                    case MusicState.LoopA: return profile.genesis[1];
                    case MusicState.TransitionA: return profile.genesis[2];
                    case MusicState.LoopB: return profile.genesis[3];
                    case MusicState.Outro: return profile.genesis[4];
                }
                break;
        }

        return null;
    }

    public void SetRealm(RealmType newRealm)
    {
        currentRealm = newRealm;
    }

    public void SetMusicState(MusicState newState)
    {
        if (newState == currentState) return;
        currentState = newState;
        AudioClip clipToPlay = GetMusicClip(newState);
        if (clipToPlay == null)
        {
            Debug.LogWarning($"No clip found for {currentRealm} in {newState} state");
            return;
        }

        musicSource.Stop();
        musicSource.clip = clipToPlay;
        musicSource.volume = profile.musicVolume;
        musicSource.Play();
    }

    public void CrossfadeTo(MusicState newState, float fadeTime = 1f)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeMusic(newState, fadeTime));
    }

    private IEnumerator FadeMusic(MusicState newState, float fadeTime)
    {
        AudioClip newClip = GetMusicClip(newState);
        if (newClip == null) yield break;
        float startVolume = musicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, profile.musicVolume, t / fadeTime);
            yield return null;
        }
        musicSource.volume = profile.musicVolume;
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        ambienceSource.PlayOneShot(clip, profile.sfxVolume * volumeMultiplier);
    }

    public void PlaySwordSounds()
    {
        int index = Random.Range(0, profile.swordSounds.Length);
        AudioClip clip = profile.swordSounds[index];
        ambienceSource.PlayOneShot(clip, profile.sfxVolume);
    }
}
