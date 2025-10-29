using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioProfile", menuName = "Audio/AudioProfile")]
public class AudioProfile : ScriptableObject
{
   [Header("Music Tracks")]
    public AudioClip mainTheme;
    public AudioClip maternaLamenta;
    public AudioClip creteValley;
    public AudioClip[] bladedspirits;

   [Header("SFX")]
   public AudioClip[] swordSounds;
   public AudioClip[] ambienceSounds;

   [Header("Volume Settings")]
   [Range(0f, 1f)] public float musicVolume = 1f;
   [Range(0f, 1f)] public float sfxVolume = 1f;
}
