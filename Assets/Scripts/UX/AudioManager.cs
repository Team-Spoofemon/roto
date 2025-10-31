using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//create enumerated list for any structured list of music or type
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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        else if (Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        ambienceSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        ambienceSource.loop = true;
    }

    private AudioClip GetMusicClip(MusicState state)
    {
        switch (currentRealm)
        {
            //Each case representing a Realm/Level
            case RealmType.CreteValley:
                //States in the level
                switch (state)
                {
                    //case MusicState.Intro: return profile.creteIntro;
                    //case MusicState.LoopA: return profile.creteLoopA;
                    //case MusicState.LoopB: return profile.creteLoopB;
                    //case MusicState.LoopC: return profile.creteLoopC;
                }
                break;

            //Add other Realms
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

        // Stop current music and start a new one
        musicSource.Stop();
        musicSource.clip = clipToPlay;
        musicSource.volume = profile.musicVolume;
        musicSource.Play();
    }

    public void PlaySwordSounds()
    {
        int index = Random.Range(0, profile.swordSounds.Length);
        AudioClip clip = profile.swordSounds[index];
        ambienceSource.PlayOneShot(clip, profile.sfxVolume);
    }
}
