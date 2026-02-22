using System.Collections;
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

    public bool IsFadingOut { get; private set; }

    private float MusicVol => profile != null ? profile.musicVolume : 1f;
    private float SfxVol => profile != null ? profile.sfxVolume : 1f;

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

        musicSource.playOnAwake = false;
        musicSource2.playOnAwake = false;
        ambienceSource.playOnAwake = false;

        musicSource.volume = MusicVol;
        musicSource2.volume = MusicVol;
    }

    public void SetRealm(RealmType newRealm)
    {
        currentRealm = newRealm;
    }

    public void PlayMainTheme(float fadeInTime = 0.35f)
    {
        if (IsFadingOut) return;
        if (profile == null || profile.mainTheme == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        StopMusicImmediate();

        musicSource.clip = profile.mainTheme;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();

        fadeRoutine = StartCoroutine(FadeVolume(musicSource, 0f, MusicVol, fadeInTime));
        currentState = MusicState.None;
    }

    public void PlayDeathTheme(float fadeOutTime = 0.15f, float fadeInTime = 0.15f)
    {
        if (IsFadingOut) return;
        if (profile == null || profile.deathTheme == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(PlayDeathThemeRoutine(fadeOutTime, fadeInTime));
        currentState = MusicState.None;
    }

    private IEnumerator PlayDeathThemeRoutine(float fadeOutTime, float fadeInTime)
    {
        bool p1 = musicSource != null && musicSource.isPlaying;
        bool p2 = musicSource2 != null && musicSource2.isPlaying;

        if ((p1 || p2) && fadeOutTime > 0f)
        {
            float startVol1 = p1 ? musicSource.volume : 0f;
            float startVol2 = p2 ? musicSource2.volume : 0f;

            float t = 0f;
            while (t < fadeOutTime)
            {
                t += Time.unscaledDeltaTime;
                float a = Mathf.Clamp01(t / fadeOutTime);
                float k = 1f - a;

                if (p1) musicSource.volume = startVol1 * k;
                if (p2) musicSource2.volume = startVol2 * k;

                yield return null;
            }
        }

        StopMusicImmediate();

        musicSource.clip = profile.deathTheme;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();

        if (fadeInTime > 0f)
            yield return FadeVolume(musicSource, 0f, MusicVol, fadeInTime);

        musicSource.volume = MusicVol;
    }

    private AudioClip GetMusicClip(MusicState state)
    {
        if (profile == null) return null;

        switch (currentRealm)
        {
            case RealmType.CreteValley:
                switch (state)
                {
                    case MusicState.Intro: return profile.creteValley != null && profile.creteValley.Length > 0 ? profile.creteValley[0] : null;
                    case MusicState.LoopA: return profile.creteValley != null && profile.creteValley.Length > 1 ? profile.creteValley[1] : null;
                }
                break;

            case RealmType.MountOthrys:
                switch (state)
                {
                    case MusicState.Intro: return profile.mtOthrys != null && profile.mtOthrys.Length > 0 ? profile.mtOthrys[0] : null;
                    case MusicState.LoopA: return profile.mtOthrys != null && profile.mtOthrys.Length > 1 ? profile.mtOthrys[1] : null;
                }
                break;

            case RealmType.cutsceneRealm:
                switch (state)
                {
                    case MusicState.Intro: return profile.cutsceneRealm != null && profile.cutsceneRealm.Length > 0 ? profile.cutsceneRealm[0] : null;
                }
                break;
        }

        return null;
    }

    public void SetMusicState(MusicState newState)
    {
        if (IsFadingOut) return;
        if (newState == currentState) return;
        currentState = newState;

        AudioClip clipToPlay = GetMusicClip(newState);
        if (clipToPlay == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        musicSource2.Stop();
        musicSource2.clip = null;

        musicSource.Stop();
        musicSource.clip = clipToPlay;
        musicSource.loop = true;
        musicSource.volume = MusicVol;
        musicSource.Play();
    }

    public void CrossfadeTo(MusicState newState, float fadeTime = 1f)
    {
        if (IsFadingOut) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeMusic(newState, fadeTime));
    }

    private IEnumerator FadeMusic(MusicState newState, float fadeTime)
    {
        AudioClip newClip = GetMusicClip(newState);
        if (newClip == null) yield break;

        float startVolume = (musicSource != null && musicSource.isPlaying) ? musicSource.volume : 0f;

        if (musicSource != null && musicSource.isPlaying && fadeTime > 0f)
            yield return FadeVolume(musicSource, startVolume, 0f, fadeTime);

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();

        if (fadeTime > 0f)
            yield return FadeVolume(musicSource, 0f, MusicVol, fadeTime);

        musicSource.volume = MusicVol;
    }

    public void PlayIntroThenLoop(MusicState intro, MusicState loop)
    {
        if (IsFadingOut) return;

        AudioClip introClip = GetMusicClip(intro);
        AudioClip loopClip = GetMusicClip(loop);
        if (introClip == null || loopClip == null) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        StopMusicImmediate();

        double startTime = AudioSettings.dspTime + 0.1;
        double loopStartTime = startTime + introClip.length;

        musicSource.clip = introClip;
        musicSource2.clip = loopClip;

        musicSource.volume = MusicVol;
        musicSource2.volume = MusicVol;

        musicSource.PlayScheduled(startTime);
        musicSource2.PlayScheduled(loopStartTime);

        musicSource.SetScheduledEndTime(loopStartTime);
        musicSource2.loop = true;
    }

    public void FadeOutMusic(float fadeTime = 1.5f)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        IsFadingOut = true;
        fadeRoutine = StartCoroutine(FadeOutRoutineWrapper(fadeTime));
    }

    public IEnumerator FadeOutCoroutine(float fadeTime)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        IsFadingOut = true;
        yield return FadeOutRoutine(fadeTime);
        IsFadingOut = false;
        fadeRoutine = null;
    }

    private IEnumerator FadeOutRoutineWrapper(float fadeTime)
    {
        yield return FadeOutRoutine(fadeTime);
        IsFadingOut = false;
        fadeRoutine = null;
    }

    private IEnumerator FadeOutRoutine(float fadeTime)
    {
        bool p1 = musicSource != null && musicSource.isPlaying;
        bool p2 = musicSource2 != null && musicSource2.isPlaying;

        if (!p1 && !p2)
            yield break;

        float startVol1 = p1 ? musicSource.volume : 0f;
        float startVol2 = p2 ? musicSource2.volume : 0f;

        if (fadeTime <= 0f)
        {
            StopMusicImmediate();
            yield break;
        }

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / fadeTime);
            float k = 1f - a;

            if (p1) musicSource.volume = startVol1 * k;
            if (p2) musicSource2.volume = startVol2 * k;

            yield return null;
        }

        StopMusicImmediate();
    }

    private void StopMusicImmediate()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = null;
        }

        if (musicSource2 != null)
        {
            musicSource2.Stop();
            musicSource2.clip = null;
        }
    }

    private IEnumerator FadeVolume(AudioSource src, float from, float to, float time)
    {
        if (src == null) yield break;

        if (time <= 0f)
        {
            src.volume = to;
            yield break;
        }

        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(from, to, Mathf.Clamp01(t / time));
            yield return null;
        }

        src.volume = to;
    }

    public void PlaySFX(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;
        ambienceSource.PlayOneShot(clip, SfxVol * volumeMultiplier);
    }

    public void PlaySwordSounds()
    {
        if (profile == null || profile.swordSounds == null || profile.swordSounds.Length == 0) return;
        int index = Random.Range(0, profile.swordSounds.Length);
        ambienceSource.PlayOneShot(profile.swordSounds[index], SfxVol);
    }
}