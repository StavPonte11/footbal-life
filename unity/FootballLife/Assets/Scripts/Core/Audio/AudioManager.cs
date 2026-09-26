using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FootballLife.Domain;

namespace FootballLife.Unity.Core.Audio
{
    /// <summary>
    /// Master Audio Manager singleton coordinating all match SFX, crowd ambience,
    /// UI haptics, and background music across scenes.
    /// Supports independent bus attenuation, ducking, and procedural clip synthesis.
    /// Zero GC allocations in update loops.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour
    {
        private const string PrefsPrefix = "FootballLife_Audio_";

        public static AudioManager? Instance { get; private set; }

        [Header("Bus Configuration")]
        [SerializeField] private AudioSettingsConfig _settings = AudioSettingsConfig.Default;

        // ── Dedicated Channels ───────────────────────────────────────────────
        private AudioSource _musicSource = null!;
        private AudioSource _ambienceSource = null!;
        private AudioSource _uiSource = null!;
        private readonly List<AudioSource> _sfxPool = new List<AudioSource>(8);
        private int _sfxPoolIndex = 0;

        // ── Clip Cache ───────────────────────────────────────────────────────
        private readonly Dictionary<SoundId, AudioClip> _clipCache = new Dictionary<SoundId, AudioClip>(16);

        // ── State ─────────────────────────────────────────────────────────────
        private Coroutine? _duckCoroutine;
        private float _musicDuckFactor = 1.0f;
        private bool _isInitialized;

        public AudioSettingsConfig Settings => _settings;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            InitializeAudioSystem();
        }

        /// <summary>
        /// Ensures an AudioManager instance exists in the current scene.
        /// </summary>
        public static AudioManager EnsureExists()
        {
            if (Instance != null) return Instance;

            var existing = FindAnyObjectByType<AudioManager>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            var go = new GameObject("[MANAGERS]_AudioManager");
            var manager = go.AddComponent<AudioManager>();
            return manager;
        }

        private void InitializeAudioSystem()
        {
            if (_isInitialized) return;

            LoadSettingsFromPrefs();

            // Create Music Channel
            var musicGo = new GameObject("Channel_Music");
            musicGo.transform.SetParent(transform, false);
            _musicSource = musicGo.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.playOnAwake = false;
            _musicSource.spatialBlend = 0.0f; // 2D

            // Create Ambience Channel
            var ambGo = new GameObject("Channel_Ambience");
            ambGo.transform.SetParent(transform, false);
            _ambienceSource = ambGo.AddComponent<AudioSource>();
            _ambienceSource.loop = true;
            _ambienceSource.playOnAwake = false;
            _ambienceSource.spatialBlend = 0.0f; // 2D

            // Create UI Channel
            var uiGo = new GameObject("Channel_UI");
            uiGo.transform.SetParent(transform, false);
            _uiSource = uiGo.AddComponent<AudioSource>();
            _uiSource.loop = false;
            _uiSource.playOnAwake = false;
            _uiSource.spatialBlend = 0.0f; // 2D

            // Create SFX Pool (6 reusable sources)
            for (int i = 0; i < 6; i++)
            {
                var sfxGo = new GameObject($"Channel_SFX_{i}");
                sfxGo.transform.SetParent(transform, false);
                var src = sfxGo.AddComponent<AudioSource>();
                src.loop = false;
                src.playOnAwake = false;
                src.spatialBlend = 0.5f; // Blend between 2D and 3D
                _sfxPool.Add(src);
            }

            // Warm up procedural sound clips
            PrewarmAudioClips();

            // Apply volumes
            ApplyBusVolumes();

            _isInitialized = true;
        }

        private void PrewarmAudioClips()
        {
            _clipCache[SoundId.KickSoft] = ProceduralAudioClipFactory.CreateKickClip(isHard: false);
            _clipCache[SoundId.KickHard] = ProceduralAudioClipFactory.CreateKickClip(isHard: true);
            _clipCache[SoundId.WoodworkHit] = ProceduralAudioClipFactory.CreateWoodworkClip();
            _clipCache[SoundId.NetRipple] = ProceduralAudioClipFactory.CreateNetRippleClip();
            _clipCache[SoundId.WhistleStart] = ProceduralAudioClipFactory.CreateWhistleClip(WhistleType.SituationStart);
            _clipCache[SoundId.WhistleGoal] = ProceduralAudioClipFactory.CreateWhistleClip(WhistleType.GoalScored);
            _clipCache[SoundId.WhistleEnd] = ProceduralAudioClipFactory.CreateWhistleClip(WhistleType.FullTime);
            _clipCache[SoundId.CrowdMurmur] = ProceduralAudioClipFactory.CreateCrowdMurmurClip();
            _clipCache[SoundId.CrowdRoar] = ProceduralAudioClipFactory.CreateCrowdRoarClip();
            _clipCache[SoundId.CrowdGasp] = ProceduralAudioClipFactory.CreateCrowdGaspClip();
            _clipCache[SoundId.UIButtonClick] = ProceduralAudioClipFactory.CreateUIClickClip();
            _clipCache[SoundId.UITabSwitch] = ProceduralAudioClipFactory.CreateUITabSwitchClip();
            _clipCache[SoundId.UIWageChime] = ProceduralAudioClipFactory.CreateUIWageChimeClip();
            _clipCache[SoundId.UIAlertPrompt] = ProceduralAudioClipFactory.CreateUIAlertClip();
            _clipCache[SoundId.MenuMusic] = ProceduralAudioClipFactory.CreateMenuMusicClip();
        }

        // ── Match Audio Public APIs ───────────────────────────────────────────

        /// <summary>
        /// Plays kick impact sound scaled by shot power.
        /// </summary>
        public void PlayKick(float power01, Vector3? position = null)
        {
            float power = Mathf.Clamp01(power01);
            SoundId id = (power > 0.5f) ? SoundId.KickHard : SoundId.KickSoft;
            float vol = Mathf.Lerp(0.65f, 1.0f, power);
            float pitch = Mathf.Lerp(0.92f, 1.12f, power);

            PlaySound(id, vol, pitch, position);
        }

        /// <summary>
        /// Plays metallic woodwork strike sound.
        /// </summary>
        public void PlayWoodwork(Vector3? position = null)
        {
            PlaySound(SoundId.WoodworkHit, 1.0f, UnityEngine.Random.Range(0.95f, 1.05f), position);
        }

        /// <summary>
        /// Plays goal net ripple sound.
        /// </summary>
        public void PlayNetRipple(Vector3? position = null)
        {
            PlaySound(SoundId.NetRipple, 0.90f, 1.0f, position);
        }

        /// <summary>
        /// Plays referee whistle sound with automatic dynamic music ducking.
        /// </summary>
        public void PlayWhistle(WhistleType type)
        {
            SoundId id = type switch
            {
                WhistleType.SituationStart => SoundId.WhistleStart,
                WhistleType.GoalScored => SoundId.WhistleGoal,
                WhistleType.FullTime => SoundId.WhistleEnd,
                _ => SoundId.WhistleStart
            };

            DuckMusic(0.20f, 1.2f);
            PlaySound(id, 0.85f, 1.0f, null);
        }

        /// <summary>
        /// Starts or resumes background stadium crowd murmur loop.
        /// </summary>
        public void StartCrowdMurmur()
        {
            if (_ambienceSource == null) return;
            if (_clipCache.TryGetValue(SoundId.CrowdMurmur, out var clip))
            {
                _ambienceSource.clip = clip;
                if (!_ambienceSource.isPlaying)
                {
                    _ambienceSource.Play();
                }
            }
        }

        /// <summary>
        /// Stops crowd murmur loop.
        /// </summary>
        public void StopCrowdMurmur()
        {
            if (_ambienceSource != null && _ambienceSource.isPlaying)
            {
                _ambienceSource.Stop();
            }
        }

        /// <summary>
        /// Triggers explosive crowd goal celebration roar.
        /// </summary>
        public void TriggerCrowdRoar()
        {
            PlaySound(SoundId.CrowdRoar, 1.0f, 1.0f, null);
            DuckMusic(0.15f, 2.8f);
        }

        /// <summary>
        /// Triggers collective crowd gasp on missed shot or dramatic goalkeeper save.
        /// </summary>
        public void TriggerCrowdGasp()
        {
            PlaySound(SoundId.CrowdGasp, 0.85f, UnityEngine.Random.Range(0.95f, 1.05f), null);
        }

        // ── UI Audio Public APIs ──────────────────────────────────────────────

        /// <summary>
        /// Plays crisp UI button click tap.
        /// </summary>
        public void PlayUIClick()
        {
            PlayUISound(SoundId.UIButtonClick, 0.70f);
        }

        /// <summary>
        /// Plays navigation tab switch swish.
        /// </summary>
        public void PlayUITabSwitch()
        {
            PlayUISound(SoundId.UITabSwitch, 0.65f);
        }

        /// <summary>
        /// Plays celebratory weekly wage deposit or shop item purchase chime.
        /// </summary>
        public void PlayUIWageChime()
        {
            PlayUISound(SoundId.UIWageChime, 0.90f);
        }

        /// <summary>
        /// Plays dilemma attention bell prompt.
        /// </summary>
        public void PlayUIAlert()
        {
            PlayUISound(SoundId.UIAlertPrompt, 0.85f);
        }

        private void PlayUISound(SoundId id, float volume)
        {
            if (_uiSource == null || !_clipCache.TryGetValue(id, out var clip)) return;

            float effVol = _settings.GetEffectiveLinearVolume(AudioBus.UI) * volume;
            if (effVol > 0.001f)
            {
                _uiSource.PlayOneShot(clip, effVol);
            }
        }

        // ── Music Public APIs ─────────────────────────────────────────────────

        /// <summary>
        /// Starts menu background music.
        /// </summary>
        public void StartMenuMusic()
        {
            if (_musicSource == null) return;
            if (_clipCache.TryGetValue(SoundId.MenuMusic, out var clip))
            {
                _musicSource.clip = clip;
                if (!_musicSource.isPlaying)
                {
                    _musicSource.Play();
                }
            }
        }

        /// <summary>
        /// Stops background music.
        /// </summary>
        public void StopMusic()
        {
            if (_musicSource != null && _musicSource.isPlaying)
            {
                _musicSource.Stop();
            }
        }

        /// <summary>
        /// Temporarily ducks background music during whistles or match events.
        /// </summary>
        public void DuckMusic(float duckFactor, float durationSeconds)
        {
            if (_duckCoroutine != null)
            {
                StopCoroutine(_duckCoroutine);
            }
            _duckCoroutine = StartCoroutine(DuckMusicRoutine(duckFactor, durationSeconds));
        }

        private IEnumerator DuckMusicRoutine(float duckFactor, float duration)
        {
            _musicDuckFactor = duckFactor;
            ApplyBusVolumes();

            yield return new WaitForSeconds(duration);

            // Smooth return to full music volume
            float elapsed = 0f;
            float returnDuration = 0.5f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                _musicDuckFactor = Mathf.Lerp(duckFactor, 1.0f, elapsed / returnDuration);
                ApplyBusVolumes();
                yield return null;
            }

            _musicDuckFactor = 1.0f;
            ApplyBusVolumes();
            _duckCoroutine = null;
        }

        // ── Generic Play Sound ────────────────────────────────────────────────

        public void PlaySound(SoundId sound, float volume = 1.0f, float pitch = 1.0f, Vector3? position = null)
        {
            if (!_clipCache.TryGetValue(sound, out var clip)) return;

            AudioBus targetBus = sound switch
            {
                SoundId.CrowdMurmur or SoundId.CrowdRoar or SoundId.CrowdGasp => AudioBus.Ambience,
                SoundId.UIButtonClick or SoundId.UITabSwitch or SoundId.UIWageChime or SoundId.UIAlertPrompt => AudioBus.UI,
                SoundId.MenuMusic => AudioBus.Music,
                _ => AudioBus.SFX
            };

            float effVol = _settings.GetEffectiveLinearVolume(targetBus) * volume;
            if (effVol <= 0.001f) return;

            // Fetch next pooled SFX source
            var source = _sfxPool[_sfxPoolIndex];
            _sfxPoolIndex = (_sfxPoolIndex + 1) % _sfxPool.Count;

            source.pitch = pitch;
            source.volume = effVol;

            if (position.HasValue)
            {
                source.transform.position = position.Value;
                source.spatialBlend = 0.75f;
            }
            else
            {
                source.spatialBlend = 0.0f;
            }

            source.PlayOneShot(clip, effVol);
        }

        // ── Bus Volume & Settings APIs ────────────────────────────────────────

        public void SetBusVolume(AudioBus bus, float volume)
        {
            _settings = _settings.WithVolume(bus, volume);
            ApplyBusVolumes();
            SaveSettingsToPrefs();
        }

        public void SetBusMute(AudioBus bus, bool mute)
        {
            _settings = _settings.WithMute(bus, mute);
            ApplyBusVolumes();
            SaveSettingsToPrefs();
        }

        public void ApplyBusVolumes()
        {
            if (_musicSource != null)
            {
                float musicVol = _settings.GetEffectiveLinearVolume(AudioBus.Music) * _musicDuckFactor;
                _musicSource.volume = musicVol;
            }

            if (_ambienceSource != null)
            {
                _ambienceSource.volume = _settings.GetEffectiveLinearVolume(AudioBus.Ambience);
            }

            if (_uiSource != null)
            {
                _uiSource.volume = _settings.GetEffectiveLinearVolume(AudioBus.UI);
            }
        }

        private void SaveSettingsToPrefs()
        {
            PlayerPrefs.SetFloat(PrefsPrefix + "MasterVol", _settings.MasterVolume);
            PlayerPrefs.SetFloat(PrefsPrefix + "SfxVol", _settings.SfxVolume);
            PlayerPrefs.SetFloat(PrefsPrefix + "AmbienceVol", _settings.AmbienceVolume);
            PlayerPrefs.SetFloat(PrefsPrefix + "UiVol", _settings.UiVolume);
            PlayerPrefs.SetFloat(PrefsPrefix + "MusicVol", _settings.MusicVolume);

            PlayerPrefs.SetInt(PrefsPrefix + "MasterMute", _settings.MasterMuted ? 1 : 0);
            PlayerPrefs.SetInt(PrefsPrefix + "SfxMute", _settings.SfxMuted ? 1 : 0);
            PlayerPrefs.SetInt(PrefsPrefix + "AmbienceMute", _settings.AmbienceMuted ? 1 : 0);
            PlayerPrefs.SetInt(PrefsPrefix + "UiMute", _settings.UiMuted ? 1 : 0);
            PlayerPrefs.SetInt(PrefsPrefix + "MusicMute", _settings.MusicMuted ? 1 : 0);

            PlayerPrefs.Save();
        }

        private void LoadSettingsFromPrefs()
        {
            if (!PlayerPrefs.HasKey(PrefsPrefix + "MasterVol")) return;

            _settings = new AudioSettingsConfig
            {
                MasterVolume = PlayerPrefs.GetFloat(PrefsPrefix + "MasterVol", 1.0f),
                SfxVolume = PlayerPrefs.GetFloat(PrefsPrefix + "SfxVol", 1.0f),
                AmbienceVolume = PlayerPrefs.GetFloat(PrefsPrefix + "AmbienceVol", 0.85f),
                UiVolume = PlayerPrefs.GetFloat(PrefsPrefix + "UiVol", 1.0f),
                MusicVolume = PlayerPrefs.GetFloat(PrefsPrefix + "MusicVol", 0.70f),

                MasterMuted = PlayerPrefs.GetInt(PrefsPrefix + "MasterMute", 0) == 1,
                SfxMuted = PlayerPrefs.GetInt(PrefsPrefix + "SfxMute", 0) == 1,
                AmbienceMuted = PlayerPrefs.GetInt(PrefsPrefix + "AmbienceMute", 0) == 1,
                UiMuted = PlayerPrefs.GetInt(PrefsPrefix + "UiMute", 0) == 1,
                MusicMuted = PlayerPrefs.GetInt(PrefsPrefix + "MusicMute", 0) == 1
            };
        }
    }
}
