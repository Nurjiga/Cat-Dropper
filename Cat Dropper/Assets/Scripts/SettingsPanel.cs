using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Экран Settings: переключатели SFX и Music (On / Off) и кнопка Help (инструкция по игре).
/// Значения хранятся в SaveData, звук применяет AudioManager.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("SFX")]
    public Button sfxOnButton;
    public Button sfxOffButton;
    public RectTransform sfxKnob;
    public Image sfxIcon;

    [Header("Music")]
    public Button musicOnButton;
    public Button musicOffButton;
    public RectTransform musicKnob;
    public Image musicIcon;

    [Header("Help")]
    public Button helpButton;
    public MainMenuScript menu;

    private const float KnobSlideTime = 0.12f;
    private const float DisabledIconAlpha = 0.35f;

    private Coroutine sfxSlide, musicSlide;
    private bool listenersAdded;

    private void Awake()
    {
        AddListeners();
    }

    private void OnEnable()
    {
        AddListeners();
        ShowState(SaveData.SoundOn, SaveData.MusicOn, true);
    }

    private void AddListeners()
    {
        if (listenersAdded) return;
        listenersAdded = true;

        if (sfxOnButton != null) sfxOnButton.onClick.AddListener(() => SetSound(true));
        if (sfxOffButton != null) sfxOffButton.onClick.AddListener(() => SetSound(false));
        if (musicOnButton != null) musicOnButton.onClick.AddListener(() => SetMusic(true));
        if (musicOffButton != null) musicOffButton.onClick.AddListener(() => SetMusic(false));
        if (helpButton != null) helpButton.onClick.AddListener(OnHelp);
    }

    public void SetSound(bool isOn)
    {
        if (AudioManager.SoundOn == isOn) return;
        AudioManager.SoundOn = isOn;
        ShowState(SaveData.SoundOn, SaveData.MusicOn, false);
        if (isOn) AudioManager.Play("click", 0.8f); // подтверждающий звук при включении
    }

    public void SetMusic(bool isOn)
    {
        if (AudioManager.MusicOn == isOn) return;
        AudioManager.MusicOn = isOn;
        ShowState(SaveData.SoundOn, SaveData.MusicOn, false);
    }

    private void OnHelp()
    {
        if (menu != null) menu.OpenHelp();
    }

    /// <summary>Рисует состояние переключателей (без записи в сохранения). snap = без анимации.</summary>
    public void ShowState(bool sfxOn, bool musicOn, bool snap)
    {
        MoveKnob(sfxKnob, sfxOn, snap, ref sfxSlide);
        MoveKnob(musicKnob, musicOn, snap, ref musicSlide);
        SetIconAlpha(sfxIcon, sfxOn);
        SetIconAlpha(musicIcon, musicOn);
    }

    private void SetIconAlpha(Image icon, bool isOn)
    {
        if (icon == null) return;
        Color c = icon.color;
        c.a = isOn ? 1f : DisabledIconAlpha;
        icon.color = c;
    }

    private void MoveKnob(RectTransform knob, bool isOn, bool snap, ref Coroutine running)
    {
        if (knob == null) return;

        RectTransform pill = (RectTransform)knob.parent;
        float half = knob.rect.width * 0.5f;
        float target = isOn ? half : pill.rect.width - half; // On — слева, Off — справа

        if (running != null) StopCoroutine(running);

        if (snap || !isActiveAndEnabled)
        {
            SetKnobX(knob, target);
            running = null;
        }
        else
        {
            RectTransform k = knob;
            running = StartCoroutine(SlideKnob(k, target));
        }
    }

    private IEnumerator SlideKnob(RectTransform knob, float target)
    {
        float start = knob.anchoredPosition.x;
        for (float t = 0f; t < KnobSlideTime; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / KnobSlideTime);
            SetKnobX(knob, Mathf.Lerp(start, target, k));
            yield return null;
        }
        SetKnobX(knob, target);
    }

    private static void SetKnobX(RectTransform knob, float x)
    {
        knob.anchoredPosition = new Vector2(x, 0f);
    }
}
