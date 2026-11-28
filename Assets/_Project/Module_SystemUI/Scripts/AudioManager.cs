using System.Collections.Generic;
using MetalRebels.Core.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MetalRebels.Core.Managers
{
    [System.Serializable]
    public class SoundEntry
    {
        public string soundName;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Nhạc nền (BGM)")]
        [SerializeField] private AudioClip mainMenuBgm;
        [SerializeField] private AudioClip bossBgm;

        [Header("Danh sách Hiệu ứng Âm thanh (SFX)")]
        [SerializeField] private List<SoundEntry> sfxList = new List<SoundEntry>();

        private Dictionary<string, SoundEntry> sfxMap;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();
            BuildSfxDictionary();

            AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        }

        private void SetupAudioSources()
        {
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
                bgmSource.volume = 0.65f;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
        }

        private void BuildSfxDictionary()
        {
            sfxMap = new Dictionary<string, SoundEntry>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var entry in sfxList)
            {
                if (!string.IsNullOrWhiteSpace(entry.soundName) && entry.clip != null)
                {
                    sfxMap[entry.soundName.Trim()] = entry;
                }
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            PlayerEvents.OnPlaySFX += PlaySFX;
            PlayerEvents.OnWeaponFired += PlayWeaponFireSFX;
            PlayerEvents.OnReloadingChanged += HandleReloadSFX;
            PlayerEvents.OnBossBattleChanged += HandleBossBattleAudio;
            PlayerEvents.OnAllLivesLost += StopBGM;
            PlayerEvents.OnMissionCompleted += StopBGM;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            PlayerEvents.OnPlaySFX -= PlaySFX;
            PlayerEvents.OnWeaponFired -= PlayWeaponFireSFX;
            PlayerEvents.OnReloadingChanged -= HandleReloadSFX;
            PlayerEvents.OnBossBattleChanged -= HandleBossBattleAudio;
            PlayerEvents.OnAllLivesLost -= StopBGM;
            PlayerEvents.OnMissionCompleted -= StopBGM;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
            {
                PlayBGM(mainMenuBgm);
            }
            else
            {
                StopBGM();
            }
        }

        public void PlayBGM(AudioClip clip)
        {
            if (clip == null)
            {
                StopBGM();
                return;
            }

            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            bgmSource.loop = true;
            bgmSource.clip = clip;
            bgmSource.Play();
        }

        public void StopBGM()
        {
            if (bgmSource == null) return;
            if (bgmSource.isPlaying) bgmSource.Stop();
            bgmSource.clip = null;
        }

        private void HandleBossBattleAudio(bool isFighting)
        {
            if (isFighting)
                PlayBGM(bossBgm);
            else
                StopBGM();
        }

        public void PlaySFX(string soundName)
        {
            if (string.IsNullOrWhiteSpace(soundName) || sfxMap == null) return;

            if (sfxMap.TryGetValue(soundName.Trim(), out SoundEntry entry) && entry.clip != null)
            {
                sfxSource.PlayOneShot(entry.clip, entry.volume);
            }
        }

        private void PlayWeaponFireSFX(string weaponName)
        {
            if (string.IsNullOrWhiteSpace(weaponName)) return;
            string key = weaponName.Trim();
            if (sfxMap != null && !sfxMap.ContainsKey(key))
            {
                if (key.Equals("Assault Rifle", System.StringComparison.OrdinalIgnoreCase))
                    key = "Rifle";
                else if (key.Equals("Handgun", System.StringComparison.OrdinalIgnoreCase))
                    key = "Pistol";
            }
            PlaySFX(key);
        }

        private void HandleReloadSFX(bool isReloading)
        {
            if (isReloading) PlaySFX("Reload");
        }
    }
}