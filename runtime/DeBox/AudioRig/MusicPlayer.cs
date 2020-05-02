using UnityEngine;
using UnityEngine.Serialization;

namespace DeBox.AudioRig
{
    /// <summary>
    /// An addon for AudioPlayer that is focused on playing music
    /// </summary>
    [RequireComponent(typeof(AudioPlayer))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField, Tooltip("If true, this instance will be accessible via MusicPlayer.Main")]
        private bool isMain = true;        

        [FormerlySerializedAs("musicTrack")] [SerializeField, Tooltip("Music clip to play when BeginMusic is called")]
        private AudioClip defaultTrack = null;

        [SerializeField, Tooltip("Default music volume")]
        private float musicVolume = 0.5f;
        
        [SerializeField, Tooltip("If true, music will not be played in editor mode")]
        private bool silenceInEditor = true;

        [SerializeField] private float crossfadeDuration = 3;

        /// <summary>
        /// The main MusicPlayer instance
        /// </summary>
        public static MusicPlayer Main { get; private set; }

        private AudioPlayer _audioPlayer = null;

        private IAudioPlayControlPromise _currentAudioControl = null;

        private void Awake()
        {
            if (!isMain) return;
            if (Main != null)
            {
                Debug.LogError("Duplicate Main MusicPlayer", Main.gameObject);
                Destroy(gameObject);
                return;
            }
            Main = this;
        }

        private void OnDestroy()
        {
            if (Main == this)
            {
                Main = null;
            }
        }

        /// <summary>
        /// Starts playing the specified clip
        /// </summary>
        /// <param name="musicClip"></param>
        /// <param name="fade">Clip will cross-fade or fade in it true</param>
        public void BeginMusic(AudioClip musicClip, bool fade = true)
        {
            if (_currentAudioControl != null)
            {
                EndMusic();
            }
            _audioPlayer = GetComponent<AudioPlayer>();
            _currentAudioControl = _audioPlayer.Play(musicClip, musicVolume, true, 0);
            if (fade)
            {
                _currentAudioControl.FadeIn(crossfadeDuration);
            }
        }
        
        /// <summary>
        /// Start playing the default track
        /// </summary>
        public void BeginMusic()
        {
#if UNITY_EDITOR
            if (silenceInEditor)
            {
                return;
            }
#endif
            BeginMusic(defaultTrack, _currentAudioControl != null);
        }

        /// <summary>
        /// Fade the music out
        /// </summary>
        public void EndMusic()
        {
            _currentAudioControl.FadeOut(crossfadeDuration);
            _currentAudioControl = null;
        }
    }
}
