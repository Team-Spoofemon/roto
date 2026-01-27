using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AudioManager = manages music and audio
/// singleton, picks music based on the current Realm, and plays tracks based on MusicState.
/// swap tracks instantly, crossfade, or do an intro to loop. SFX are just one shots through the ambience source.
///
/// How I use it:
/// - Ensure script is attached to a GameObject in the first scene and assign an AudioProfile in the Inspector.
/// - Then call AudioManager.Instance from anywhere.
///
/// Music:
/// - SetRealm(newRealm): sets which realm we’re in (this controls what clips get picked).
/// - SetMusicState(state): hard switches to the clip for that state.
/// - CrossfadeTo(state, fadeTime): fades out then fades in the new track.
/// - PlayIntroThenLoop(introState, loopState): plays an intro once, then loops the main track seamlessly.
/// - FadeOutMusic(fadeTime): fades out whatever music is playing and stops it.
///
/// SFX:
/// - PlaySFX(clip, volumeMultiplier): plays a one shot.
/// - PlaySwordSounds(): randomly plays a sword sound from the profile pool.
///
/// Adding a new realm:
/// - Add the realm clips to AudioProfile and set them in the Inspector.
/// - Update GetMusicClip(): add a new case for RealmType.RealmName, then map MusicState to the right clip.
/// - If a state is not properly mapped, it’ll return null.
/// </summary>


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
