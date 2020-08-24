using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using RSG;

namespace DeBox.AudioRig
{
    /// <summary>
    /// An audio control promise.
    /// When playing a clip with AudioRig, you receive the
    /// control token for that clip 
    /// </summary>
    public interface IAudioPlayControlPromise : IPromise
    {
        /// <summary>
        /// Set/Get spatial blend
        /// </summary>
        float SpatialBlend { get; set; }

        /// <summary>
        /// Get/Set volume of the played clip
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// Get/Set the pitch of the played clip
        /// </summary>
        float Pitch { get; set; }

        /// <summary>
        /// Stop playing the clip
        /// </summary>
        void Stop();

        /// <summary>
        /// Perform a fade out, but don't stop playing
        /// </summary>
        /// <param name="duration">Fade out duration</param>
        /// <param name="stopAfter">If true, stop playing after the fade out</param>
        void FadeOut(float duration, bool stopAfter = true);

        /// <summary>
        /// Perform a fade in to full volume
        /// </summary>
        /// <param name="duration">Fade in duration</param>
        void FadeIn(float duration);

        /// <summary>
        /// Perform a fade in to the specified volume
        /// </summary>
        /// <param name="duration">Fade in duration</param>
        /// <param name="targetVolume">Target volume</param>
        void FadeIn(float duration, float targetVolume);

        /// <summary>
        /// Play the clip at a specific position
        /// </summary>
        /// <param name="position">Target position</param>
        /// <param name="spatial">Amount of spatial blend, default is 1</param>
        /// <returns>this</returns>
        IAudioPlayControlPromise PlayAt(Vector3 position, float spatial = 1);

        /// <summary>
        /// Follow a target transform around while playing the clip
        /// </summary>
        /// <param name="transform">Target transform</param>
        /// <returns>this</returns>
        IAudioPlayControlPromise Follow(Transform transform);

        /// <summary>
        /// Stop following
        /// </summary>
        /// <returns>this</returns>
        IAudioPlayControlPromise StopFollow();
    }

    /// <summary>
    /// A concrete audio control promise.
    /// When playing a clip with AudioRig, you receive the
    /// control token for that clip 
    /// </summary>
    public class AudioPlayControlPromise : Promise, IAudioPlayControlPromise
    {
        private AudioSourceManager _manager;
        private MonoBehaviour _coroutineRunner;
        private bool _isFollowing = false;
        private bool _isActive = false;

        /// <summary>
        /// Create a new audio control promise
        /// </summary>
        /// <param name="manager">The AudioSourceManager of the controlled AudioSource</param>
        /// <param name="coroutineRunner">Coroutine runner</param>
        internal AudioPlayControlPromise(AudioSourceManager manager, MonoBehaviour coroutineRunner) : base()
        {
            _manager = manager;
            _coroutineRunner = coroutineRunner;
            _isActive = true;
        }

        /// <summary>
        /// Sets the Unity AudioSource's spatialBlend to 1 and starts following the target transform
        /// </summary>
        /// <param name="transform"></param>
        /// <returns>this</returns>
        public IAudioPlayControlPromise Follow(Transform transform)
        {
            _manager.SpatialBlend = 1;
            _coroutineRunner.StartCoroutine(FollowCoroutine(transform));
            return this;
        }

        /// <summary>
        /// Stop following
        /// </summary>
        /// <returns>this</returns>
        public IAudioPlayControlPromise StopFollow()
        {
            _isFollowing = false;
            return this;
        }

        private IEnumerator FollowCoroutine(Transform transform)
        {
            _isFollowing = true;
            while (_isFollowing)
            {
                PlayAt(transform.position);
                yield return null;
            }
        }

        /// <summary>
        /// Moves the AudioSource playing the clip to target position and sets SpatialBlend to 1
        /// </summary>
        /// <param name="position"></param>
        /// <param name="spatialBlend"></param>
        /// <returns></returns>
        public IAudioPlayControlPromise PlayAt(Vector3 position, float spatialBlend = 1)
        {
            _manager.SpatialBlend = spatialBlend;
            _manager.PlaceAt(position);
            return this;
        }

        /// <summary>
        /// Perform Fade Out
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="stopAfter">Indicates we should stop playing the clip after the fadeout ends</param>
        public void FadeOut(float duration, bool stopAfter = true)
        {
            _coroutineRunner.StartCoroutine(FadeCoroutine(Volume, 0, duration, stopAfter));
        }

        /// <summary>
        /// Perform Fade in
        /// </summary>
        /// <param name="duration"></param>
        public void FadeIn(float duration)
        {
            _coroutineRunner.StartCoroutine(FadeCoroutine(0, Volume, duration, false));
        }

        /// <summary>
        /// Perform fade in to target volume
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="targetVolume"></param>
        public void FadeIn(float duration, float targetVolume)
        {
            _coroutineRunner.StartCoroutine(FadeCoroutine(0, targetVolume, duration, false));
        }

        private IEnumerator FadeCoroutine(float from, float to, float duration, bool stopAfter)
        {
            float remainingTime = duration;
            while (remainingTime > 0 && _isActive)
            {
                remainingTime = Mathf.Max(0, remainingTime - Time.deltaTime);
                Volume = Mathf.Lerp(from, to, 1f - (remainingTime / duration));
                yield return null;
            }

            if (stopAfter)
            {
                Stop();
            }
        }

        /// <summary>
        /// Set/Get pick
        /// </summary>
        public float Pitch
        {
            get => _manager.Pitch;
            set => _manager.Pitch = value;
        }

        public float SpatialBlend
        {
            get => _manager.SpatialBlend;
            set => _manager.SpatialBlend = value;
        }

        /// <summary>
        /// Set/Get volume
        /// </summary>
        public float Volume
        {
            get => _manager.Volume;
            set => _manager.Volume = value;
        }

        /// <summary>
        /// Stop playing
        /// </summary>
        public void Stop()
        {
            if (_manager == null)
            {
                Debug.LogWarning("null audio manager!!!");
                return;
            }

            _manager.Stop(this);
        }

        /// <summary>
        /// Clears stale values
        /// </summary>
        internal void Expire()
        {
            _isActive = false;
            _manager = null;
            _coroutineRunner = null;
        }
    }

    /// <summary>
    /// Controls a Unity Audio Source
    /// </summary>
    internal class AudioSourceManager : MonoBehaviour
    {
        private AudioPlayControlPromise _currentPromise = null;

        private AudioSource _audioSource = null;
        private AudioPlayer _audioPlayer = null;
        private float _lastMasterVolumeUpdate = 0;
        private float _volume = 1;

        public IAudioPlayControlPromise CurrentPromise
        {
            get { return _currentPromise; }
        }

        public float Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                _audioSource.volume = _volume * _audioPlayer.MasterVolume;
            }
        }

        public float Pitch
        {
            get { return _audioSource.pitch; }
            set { _audioSource.pitch = value; }
        }

        public bool IsPlaying
        {
            get { return _currentPromise != null; }
        }

        public float SpatialBlend
        {
            get => _audioSource.spatialBlend;
            set => _audioSource.spatialBlend = value;
        }

        public void Initialize(AudioSource audioSource, AudioPlayer audioPlayer)
        {
            _audioSource = audioSource;
            _audioPlayer = audioPlayer;
        }

        public void PlaceAt(Vector3 worldPosition)
        {
            _audioSource.transform.position = worldPosition;
        }

        public IAudioPlayControlPromise Play(AudioClip audioClip, float volume, bool loop, float spatial)
        {
            if (IsPlaying)
            {
                throw new System.Exception("Already playing");
            }

            Pitch = 1;
            _currentPromise = new AudioPlayControlPromise(this, this);
            _audioSource.clip = audioClip;
            _audioSource.spatialBlend = spatial;
            Volume = volume;
            _audioSource.loop = loop;
            _audioSource.Play();
            return _currentPromise;
        }

        public void Stop(AudioPlayControlPromise requestingControlPromise)
        {
            if (_currentPromise != requestingControlPromise)
            {
                Debug.LogError("Requesting audio controller has expired");
                return;
            }

            _audioSource.Stop();
            ResolvePromise();
        }

        private void Update()
        {
            if (!IsPlaying)
            {
                return;
            }

            if (!_audioSource.isPlaying)
            {
                ResolvePromise();
            }
            else
            {
                if (_lastMasterVolumeUpdate < _audioPlayer.LastVolumeUpdate)
                {
                    _lastMasterVolumeUpdate = _audioPlayer.LastVolumeUpdate;
                    _audioSource.volume = _volume * _audioPlayer.MasterVolume;
                }
            }
        }

        private void ResolvePromise()
        {
            var promise = _currentPromise;
            _currentPromise = null;
            promise.Expire();
            promise.Resolve();
        }
    }

    /// <summary>
    /// An AudioRig audio player. Place it in your scene on an empty game object
    /// </summary>
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField, Tooltip("Optional output mixer group")]
        private AudioMixerGroup outputGroup = null;

        [SerializeField, Tooltip("Optional AudioSource prefab to use")]
        private AudioSource audioSourcePrefab = null;

        [SerializeField, Range(0, 30), Tooltip("Maximum amount of simultaneous sounds")]
        private int sourceCount = 5;

        [SerializeField, Tooltip("If true, this instance will be accessible via `AudioPlayer.Main`")]
        private bool isMain = true;

        private AudioSourceManager[] _audioSourceManagers = null;

        private float _masterVolume = 1;
        
        internal float LastVolumeUpdate { get; private set; }

        /// <summary>
        /// Indicates the manager is initialized
        /// </summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>
        /// Global volume for this player
        /// </summary>
        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                LastVolumeUpdate = Time.time;
                _masterVolume = value;
            }
        }

        /// <summary>
        /// Returns the main AudioPlayer if such exists
        /// </summary>
        public static AudioPlayer Main { get; private set; }

        /// <summary>
        /// Unity Awake, initializes the manager
        /// </summary>
        protected virtual void Awake()
        {
            if (isMain)
            {
                if (Main != null)
                {
                    Debug.LogError("Duplicate Main AudioPlayer", Main.gameObject);
                    Destroy(gameObject);
                    return;
                }

                Main = this;
            }

            InitializeIfRequired();
        }

        private void OnDestroy()
        {
            if (Main == this)
            {
                Main = null;
            }
        }

        /// <summary>
        /// Play an audio clip at position, spatial blend will be 1
        /// </summary>
        /// <param name="audioClip"></param>
        /// <param name="position"></param>
        /// <returns>Audio Control Promise</returns>
        public IAudioPlayControlPromise Play(AudioClip audioClip, Vector3 position)
        {
            if (audioClip == null)
            {
                Debug.LogError("Requested play of null audio clip");
                return null;
            }

            var control = Play(audioClip, 1, false, 1);
            control?.PlayAt(position);
            return control;
        }

        /// <summary>
        /// Plays the audio clip in a loop
        /// </summary>
        /// <param name="clip"></param>
        /// <returns>audio control promise</returns>
        public IAudioPlayControlPromise PlayLoop(AudioClip clip)
        {
            return Play(clip, 1, true, 0);
        }

        /// <summary>
        /// Play an audio clip, spatial blend will be 0
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns>Audio Control Promise</returns>
        public IAudioPlayControlPromise Play(AudioClip audioClip)
        {
            if (audioClip == null)
            {
                Debug.LogError("Requested play of null audio clip");
                return null;
            }

            return Play(audioClip, 1);
        }

        /// <summary>
        /// Play an audio clip at a specific volume, spatial blend will be 0
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns>Audio Control Promise</returns>
        public IAudioPlayControlPromise Play(AudioClip audioClip, float volume)
        {
            return Play(audioClip, volume, false, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="volume"></param>
        /// <param name="loop"></param>
        /// <param name="spatial"></param>
        /// <returns></returns>
        public IAudioPlayControlPromise Play(AudioClip clip, float volume, bool loop, float spatial)
        {
            InitializeIfRequired();
            for (int i = 0; i < _audioSourceManagers.Length; i++)
            {
                var audioSourceManager = _audioSourceManagers[i];
                if (!audioSourceManager.IsPlaying)
                {
                    return audioSourceManager.Play(clip, volume, loop, spatial);
                }
            }

            Debug.LogError("No available audio sources");
            return null;
        }

        private AudioSource InstantiateAudioSource()
        {
            if (audioSourcePrefab != null)
            {
                return Instantiate(audioSourcePrefab);
            }

            var instanceGameObject = new GameObject("AudioSource");
            var audioSource = instanceGameObject.AddComponent<AudioSource>();
            return audioSource;
        }

        private void Initialize()
        {
            _audioSourceManagers = new AudioSourceManager[sourceCount];
            for (int i = 0; i < _audioSourceManagers.Length; i++)
            {
                var audioSource = InstantiateAudioSource();
                audioSource.transform.SetParent(transform);
                audioSource.outputAudioMixerGroup = outputGroup;
                var audioSourceManager = audioSource.gameObject.AddComponent<AudioSourceManager>();
                audioSourceManager.Initialize(audioSource, this);
                _audioSourceManagers[i] = audioSourceManager;
            }

            IsInitialized = true;
        }

        private void InitializeIfRequired()
        {
            if (IsInitialized)
            {
                return;
            }

            Initialize();
        }
    }
}