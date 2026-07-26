using Naninovel;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Applies MenuRootV2 settings directly to Naninovel services and persists them.</summary>
public sealed class MenuSettingsController : MonoBehaviour
{
    private const string FullscreenKey = "MoshiReRe.Menu.Fullscreen";

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Slider textSpeedSlider;
    [SerializeField] private Slider autoSpeedSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button backButton;

    private MenuRootV2UI menuRoot;
    private bool syncing;
    private bool settingsDirty;

    public void Configure(Slider bgm, Slider se, Slider voice, Slider textSpeed, Slider autoSpeed,
        Toggle fullscreen, Button reset, Button back)
    {
        bgmSlider = bgm;
        seSlider = se;
        voiceSlider = voice;
        textSpeedSlider = textSpeed;
        autoSpeedSlider = autoSpeed;
        fullscreenToggle = fullscreen;
        resetButton = reset;
        backButton = back;
    }

    private void Awake()
    {
        menuRoot = GetComponentInParent<MenuRootV2UI>(true);
        if (bgmSlider) bgmSlider.onValueChanged.AddListener(SetBgm);
        if (seSlider) seSlider.onValueChanged.AddListener(SetSe);
        if (voiceSlider) voiceSlider.onValueChanged.AddListener(SetVoice);
        if (textSpeedSlider) textSpeedSlider.onValueChanged.AddListener(SetTextSpeed);
        if (autoSpeedSlider) autoSpeedSlider.onValueChanged.AddListener(SetAutoSpeed);
        if (fullscreenToggle) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (resetButton) resetButton.onClick.AddListener(ResetSettings);
        if (backButton) backButton.onClick.AddListener(Back);
    }

    private void OnEnable() => SyncFromServices();
    private void OnDisable()
    {
        if (settingsDirty)
            Persist().Forget();
    }

    private void OnDestroy()
    {
        if (bgmSlider) bgmSlider.onValueChanged.RemoveListener(SetBgm);
        if (seSlider) seSlider.onValueChanged.RemoveListener(SetSe);
        if (voiceSlider) voiceSlider.onValueChanged.RemoveListener(SetVoice);
        if (textSpeedSlider) textSpeedSlider.onValueChanged.RemoveListener(SetTextSpeed);
        if (autoSpeedSlider) autoSpeedSlider.onValueChanged.RemoveListener(SetAutoSpeed);
        if (fullscreenToggle) fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
        if (resetButton) resetButton.onClick.RemoveListener(ResetSettings);
        if (backButton) backButton.onClick.RemoveListener(Back);
    }

    private void SyncFromServices()
    {
        syncing = true;
        if (Engine.Initialized && Engine.TryGetService<IAudioManager>(out var audio))
        {
            if (bgmSlider) bgmSlider.SetValueWithoutNotify(audio.BgmVolume);
            if (seSlider) seSlider.SetValueWithoutNotify(audio.SfxVolume);
            if (voiceSlider) voiceSlider.SetValueWithoutNotify(audio.VoiceVolume);
        }
        if (Engine.Initialized && Engine.TryGetService<ITextPrinterManager>(out var printer))
        {
            if (textSpeedSlider) textSpeedSlider.SetValueWithoutNotify(printer.BaseRevealSpeed);
            if (autoSpeedSlider) autoSpeedSlider.SetValueWithoutNotify(printer.BaseAutoDelay);
        }
        if (fullscreenToggle) fullscreenToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1);
        syncing = false;
    }

    private void SetBgm(float value) { if (!syncing && Engine.TryGetService<IAudioManager>(out var audio)) { audio.BgmVolume = value; settingsDirty = true; } }
    private void SetSe(float value) { if (!syncing && Engine.TryGetService<IAudioManager>(out var audio)) { audio.SfxVolume = value; settingsDirty = true; } }
    private void SetVoice(float value) { if (!syncing && Engine.TryGetService<IAudioManager>(out var audio)) { audio.VoiceVolume = value; settingsDirty = true; } }
    private void SetTextSpeed(float value) { if (!syncing && Engine.TryGetService<ITextPrinterManager>(out var printer)) { printer.BaseRevealSpeed = value; settingsDirty = true; } }
    private void SetAutoSpeed(float value) { if (!syncing && Engine.TryGetService<ITextPrinterManager>(out var printer)) { printer.BaseAutoDelay = value; settingsDirty = true; } }
    private void SetFullscreen(bool value) { if (!syncing) { Screen.fullScreen = value; PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0); PlayerPrefs.Save(); } }

    private void ResetSettings()
    {
        if (bgmSlider) bgmSlider.value = 1f;
        if (seSlider) seSlider.value = 1f;
        if (voiceSlider) voiceSlider.value = 1f;
        if (textSpeedSlider) textSpeedSlider.value = .5f;
        if (autoSpeedSlider) autoSpeedSlider.value = .5f;
        if (fullscreenToggle) fullscreenToggle.isOn = true;
    }

    private void Back() => PersistAndBack().Forget();

    private async UniTask PersistAndBack()
    {
        await Persist();
        menuRoot?.ShowTop();
    }

    private async UniTask Persist()
    {
        if (!settingsDirty)
            return;
        if (Engine.Initialized && Engine.TryGetService<IStateManager>(out var state))
            await state.SaveSettings();
        settingsDirty = false;
    }
}
