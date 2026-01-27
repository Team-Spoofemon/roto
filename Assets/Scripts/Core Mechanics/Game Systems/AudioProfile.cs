using UnityEngine;

/// <summary>
/// AudioProfile is a data container for all music and SFX used by the game
/// Music is assigned in the inspector, grouped by realm, cutscenes, and global sound effects.
/// AudioManager reads from this container
///
/// How to use it:
/// - Create one via Assets -> Create -> Audio -> AudioProfile.
/// - Assign all music and SFX clips here.
/// - Plug this into the AudioManager in the scene.
///
/// Adding / changing realms:
/// - Add a new RealmType
/// - Add a new clip array here for that realm
/// - Then map those clips in AudioManager.GetMusicClip()
///
/// Notes:
/// - Clip arrays usually follow a fixed order that matches MusicState
///   (ex: Intro = index 0, LoopA = index 1, etc.).
/// - Volume sliders here control global music and SFX levels.
/// </summary>

public enum RealmType{
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
   public AudioClip[] enemySpawnCalls;

   [Header("Cutscene Music")]
   public AudioClip maternaLamenta;

   [Header("Crete Valley")]
   public AudioClip[] creteValley;

   [Header("Mt. Othrys")]
   public AudioClip[] mtOthrys;

   [Header("Volume Settings")]
   [Range(0f, 1f)] public float musicVolume = 1f;
   [Range(0f, 1f)] public float sfxVolume = 1f;
}
