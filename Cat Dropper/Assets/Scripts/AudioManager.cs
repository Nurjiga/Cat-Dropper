using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Весь звук игры: эффекты, музыка, клики по кнопкам.
/// Создаётся сам при запуске и живёт во всех сценах — в сцену ничего добавлять не нужно.
/// Клипы лежат в Assets/Resources/Audio. Чтобы заменить звук — положи туда файл с тем же именем.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private const float MusicVolume = 0.32f;
    private const float DuckedMusicVolume = 0.06f;
    private const float MinRepeatInterval = 0.05f; // один и тот же звук не чаще, чем раз в 50 мс
    private const int SfxVoices = 6;

    private static readonly string[] ClipNames =
    {
        "meow_1", "meow_2", "meow_3", "meow_4", "meow_sad", "meow_happy",
        "pop", "drop", "land", "click", "open", "close", "gameover", "legendary"
    };

    private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, float> lastPlayed = new Dictionary<string, float>();
    private AudioSource[] voices;
    private int nextVoice;
    private AudioSource music;
    private Coroutine musicFade;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        // На телефоне Unity по умолчанию держит 30 FPS: просим 60 для всех сцен (меню и игра)
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        if (Instance != null) return;
        new GameObject("AudioManager").AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (string clipName in ClipNames)
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + clipName);
            if (clip != null) clips[clipName] = clip;
            else Debug.LogWarning("AudioManager: не найден звук Resources/Audio/" + clipName);
        }

        voices = new AudioSource[SfxVoices];
        for (int i = 0; i < SfxVoices; i++)
        {
            voices[i] = gameObject.AddComponent<AudioSource>();
            voices[i].playOnAwake = false;
        }

        music = gameObject.AddComponent<AudioSource>();
        music.clip = Resources.Load<AudioClip>("Audio/music");
        music.loop = true;
        music.playOnAwake = false;
        music.volume = MusicVolume;
        music.ignoreListenerPause = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        ApplyMusicSetting();
        HookButtons();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DuckMusic(false);
        HookButtons();
    }

    // ---------------- Эффекты ----------------

    /// <summary>Проиграть эффект по имени файла. Безопасно вызывать откуда угодно.</summary>
    public static void Play(string clipName, float volume = 1f, float pitch = 1f)
    {
        if (Instance != null) Instance.PlayInternal(clipName, volume, pitch);
    }

    /// <summary>Мяуканье при слиянии: чем крупнее кот, тем ниже голос.</summary>
    public static void PlayMeow(int catTier)
    {
        float pitch = Mathf.Lerp(1.25f, 0.78f, Mathf.Clamp01(catTier / 6f)) * Random.Range(0.96f, 1.04f);
        Play("meow_" + Random.Range(1, 5), 0.9f, pitch);
    }

    private void PlayInternal(string clipName, float volume, float pitch)
    {
        if (!SaveData.SoundOn) return;
        if (!clips.TryGetValue(clipName, out AudioClip clip)) return;

        // Защита от дублей: два кота сталкиваются в одном кадре — звук играет один раз
        float now = Time.unscaledTime;
        if (lastPlayed.TryGetValue(clipName, out float last) && now - last < MinRepeatInterval) return;
        lastPlayed[clipName] = now;

        AudioSource voice = voices[nextVoice];
        nextVoice = (nextVoice + 1) % voices.Length;
        voice.Stop();
        voice.clip = clip;
        voice.volume = Mathf.Clamp01(volume);
        voice.pitch = pitch;
        voice.Play();
    }

    // ---------------- Музыка ----------------

    /// <summary>Приглушить музыку (например, на время Game Over) или вернуть обратно.</summary>
    public static void DuckMusic(bool ducked)
    {
        if (Instance == null) return;
        Instance.FadeMusicTo(ducked ? DuckedMusicVolume : MusicVolume);
    }

    private void FadeMusicTo(float target)
    {
        if (musicFade != null) StopCoroutine(musicFade);
        musicFade = StartCoroutine(FadeMusic(target));
    }

    private IEnumerator FadeMusic(float target)
    {
        float start = music.volume;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.35f)
        {
            music.volume = Mathf.Lerp(start, target, t);
            yield return null;
        }
        music.volume = target;
        musicFade = null;
    }

    private void ApplyMusicSetting()
    {
        if (music.clip == null) return;
        if (SaveData.MusicOn && !music.isPlaying) music.Play();
        else if (!SaveData.MusicOn && music.isPlaying) music.Stop();
    }

    // ---------------- Настройки (для будущего экрана Settings) ----------------

    public static bool SoundOn
    {
        get => SaveData.SoundOn;
        set => SaveData.SoundOn = value;
    }

    public static bool MusicOn
    {
        get => SaveData.MusicOn;
        set
        {
            SaveData.MusicOn = value;
            if (Instance != null) Instance.ApplyMusicSetting();
        }
    }

    // ---------------- Клики по кнопкам ----------------

    /// <summary>Вешает звук клика на все кнопки сцены (включая выключенные панели).</summary>
    private void HookButtons()
    {
        foreach (Button button in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (!button.gameObject.scene.IsValid()) continue;            // пропускаем префабы/ассеты
            if (button.GetComponent<UIClickSound>() != null) continue;   // уже подключена
            button.gameObject.AddComponent<UIClickSound>();
        }
    }
}
