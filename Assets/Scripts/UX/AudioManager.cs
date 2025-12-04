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
    private AudioSource musicSource2;
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
        musicSource2 = gameObject.AddComponent<AudioSource>();
        ambienceSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource2.loop = true;
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
                    case MusicState.Intro: return profile.creteValley[0];
                    case MusicState.LoopA: return profile.creteValley[1];
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

    public void PlayIntroThenLoop(MusicState intro, MusicState loop)
    {
        AudioClip introClip = GetMusicClip(intro);
        AudioClip loopClip = GetMusicClip(loop);

        if (introClip == null || loopClip == null)
        {
            Debug.LogWarning("Missing intro or loop clip");
            return;
        }

        double startTime = AudioSettings.dspTime + 0.1;
        double loopStartTime = startTime + introClip.length;

        musicSource.Stop();
        musicSource2.Stop();

        musicSource.clip = introClip;
        musicSource2.clip = loopClip;

        musicSource.PlayScheduled(startTime);
        musicSource2.PlayScheduled(loopStartTime);
        
        musicSource.SetScheduledEndTime(loopStartTime);
        musicSource2.loop = true;
    }

    public void FadeOutMusic(float fadeTime = 0.5f)
{
    if (fadeRoutine != null)
        StopCoroutine(fadeRoutine);

    fadeRoutine = StartCoroutine(FadeOutRoutine(fadeTime));
}

    private IEnumerator FadeOutRoutine(float fadeTime)
    {
        float startVol = musicSource.volume;
        float startVol2 = musicSource2 != null ? musicSource2.volume : 0f;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            float v = 1f - (t / fadeTime);
            musicSource.volume = startVol * v;
            if (musicSource2 != null)
                musicSource2.volume = startVol2 * v;

            yield return null;
        }

        musicSource.Stop();
        if (musicSource2 != null)
            musicSource2.Stop();

        musicSource.volume = profile.musicVolume;
        if (musicSource2 != null)
            musicSource2.volume = profile.musicVolume;
    }

}
