using UnityEngine;

namespace TopDownCharacter2D.FX
{
    /// <summary>
    ///     Manages the game's sounds
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(AudioSource))]
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;

        [SerializeField] [Range(0f, 1f)] private float soundEffectVolume;
        [SerializeField] [Range(0f, 1f)] private float soundEffectPitchVariance;
        [SerializeField] [Range(0f, 1f)] private float musicVolume;

        private AudioSource musicAudioSource;

        private void Awake()
        {
            instance = this;
            musicAudioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        ///     Play a sound effect at the given position
        /// </summary>
        /// <param name="audio"> The sound effect to play</param>
        /// <param name="position"> The position where to play the sound</param>
        public static void PlaySoundEffect(AudioClip audio, Vector3 position)
        {
            PlayClipAt(audio, position, true);
        }

        /// <summary>
        ///     Play a sound effect globally
        /// </summary>
        /// <param name="audio"> The sound effect to play</param>
        public static void PlaySoundEffect(AudioClip audio)
        {
            PlayClipAt(audio, Vector3.zero, false);
        }

        /// <summary>
        ///     Changes the background music
        /// </summary>
        /// <param name="music"> The new music to play</param>
        public static void ChangeBackGroundMusic(AudioClip music)
        {
            instance.musicAudioSource.Stop();
            instance.musicAudioSource.clip = music;
            instance.musicAudioSource.Play();
        }

        /// <summary>
        ///     Creates and play a temporary AudioSource to play a single sound
        /// </summary>
        /// <param name="clip"> The clip to play</param>
        /// <param name="pos"> The position where to play the clip</param>
        /// <param name="spatialize"> Whether to spatialize the sound or not</param>
        private static void PlayClipAt(AudioClip clip, Vector3 pos, bool spatialize)
        {
            GameObject gameObject = new GameObject("TempAudio");
            gameObject.transform.position = pos;
            AudioSource source = gameObject.AddComponent<AudioSource>(); // add an audio source
            source.clip = clip; // define the clip
            source.volume = instance.soundEffectVolume;
            source.Play(); // start the sound
            source.pitch = 1f + Random.Range(-instance.soundEffectPitchVariance, instance.soundEffectPitchVariance);
            source.spatialize = spatialize;
            Destroy(gameObject, clip.length * 2f); // destroy object after clip duration
        }
    }
}