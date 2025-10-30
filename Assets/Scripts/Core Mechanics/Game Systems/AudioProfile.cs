using UnityEngine;

public enum RealmType
{
   CreteValley,
   MountOthrys

}

[CreateAssetMenu(fileName = "NewAudioProfile", menuName = "Audio/AudioProfile")]
public class AudioProfile : ScriptableObject
{
   [Header("Non-gameplay Music")]
   public AudioClip mainTheme;

   [Header("Global SFX")]
   public AudioClip[] swordSounds;
   public AudioClip[] ambienceSounds;

   [Header("Cutscene Music")]
   public AudioClip maternaLamenta;

   [Header("Crete Valley")]
   public AudioClip creteValley;
   public AudioClip[] bladedSpirits;

   //[Header("Mt. Othrys")]

   [Header("Volume Settings")]
   [Range(0f, 1f)] public float musicVolume = 1f;
   [Range(0f, 1f)] public float sfxVolume = 1f;
}
