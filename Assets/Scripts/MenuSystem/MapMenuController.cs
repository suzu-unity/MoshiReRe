using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public interface IMapTimeProvider
{
    int CurrentHour { get; }
}

public class MapMenuController : MonoBehaviour
{
    [Serializable]
    public struct LocationDraft
    {
        public string baseName;
        [TextArea] public string description;
        [TextArea] public string rereHint;
        [Range(0f, 1f)] public float safety;
        public string relatedItemName;
        public string relatedCharacterName;
        public Color dayColor;
        public Color nightColor;
    }

    [Header("Location data and cards")]
    [SerializeField] private LocationDraft[] locations;
    [SerializeField] private Button[] locationButtons;
    [SerializeField] private Image[] locationImages;
    [SerializeField] private Button[] goButtons;

    [Header("Selected location details")]
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text rereHintText;
    [SerializeField] private TMP_Text relatedItemText;
    [SerializeField] private TMP_Text relatedCharacterText;
    [SerializeField] private Slider safetySlider;
    [SerializeField] private Image safetyFillImage;
    [SerializeField] private TMP_Text dayNightLabelText;
    [SerializeField] private Image dayNightBackgroundImage;

    [Header("Related character buttons")]
    [SerializeField] private Button[] relatedCharacterButtons;
    [SerializeField] private int[] relatedCharacterLocationIndexes;
    [SerializeField] private int[] goButtonLocationIndexes;

    [Header("Time")]
    [SerializeField] private MonoBehaviour timeProvider;
    [SerializeField, Range(0, 23)] private int fallbackHour = 12;
    [SerializeField, Range(0, 23)] private int dayStartsAt = 6;
    [SerializeField, Range(0, 23)] private int nightStartsAt = 18;

    [Header("Feedback")]
    [SerializeField] private AudioSource hoverAudioSource;
    [SerializeField] private AudioClip hoverAudioClip;
    [SerializeField] private Color unselectedImageTint = Color.white;
    [SerializeField] private Color selectedImageTint = Color.white;

    public UnityEvent<int> OnCharacterSelected = new UnityEvent<int>();
    public UnityEvent<int> OnGoSelected = new UnityEvent<int>();

    private UnityAction[] characterActions;
    private UnityAction[] goActions;
    private int selectedIndex = -1;
    private bool isDay;

    private void Awake()
    {
        BindButtons();
        RefreshTimeAndColors();
        SelectLocation(0);
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    public void RefreshTimeAndColors()
    {
        isDay = IsDay(GetCurrentHour());
        ApplyLocationColors();
        ApplyDayNightBackground();
    }

    public void SelectLocation(int index)
    {
        if (locations == null || locations.Length == 0)
            return;

        selectedIndex = Mathf.Clamp(index, 0, locations.Length - 1);
        var location = locations[selectedIndex];

        SetText(detailNameText, location.baseName);
        SetText(detailDescriptionText, location.description);
        SetText(rereHintText, location.rereHint);
        SetText(relatedItemText, location.relatedItemName);
        SetText(relatedCharacterText, location.relatedCharacterName);

        if (safetySlider)
        {
            safetySlider.minValue = 0f;
            safetySlider.maxValue = 1f;
            safetySlider.SetValueWithoutNotify(Mathf.Clamp01(location.safety));
        }

        if (safetyFillImage)
            safetyFillImage.fillAmount = Mathf.Clamp01(location.safety);

        ApplyLocationColors();
        ApplyDayNightBackground();
    }

    private void BindButtons()
    {
        UnbindButtons();

        if (locationButtons != null)
        {
            for (var i = 0; i < locationButtons.Length; i++)
            {
                if (!locationButtons[i])
                    continue;

                var hover = locationButtons[i].GetComponent<MapLocationPointerHelper>();
                if (!hover)
                    hover = locationButtons[i].gameObject.AddComponent<MapLocationPointerHelper>();
                hover.Initialize(this, i, hoverAudioSource, hoverAudioClip);
            }
        }

        if (relatedCharacterButtons != null)
        {
            characterActions = new UnityAction[relatedCharacterButtons.Length];
            for (var i = 0; i < relatedCharacterButtons.Length; i++)
            {
                var locationIndex = GetMappedIndex(relatedCharacterLocationIndexes, i);
                var action = CreateCharacterAction(locationIndex);
                characterActions[i] = action;
                if (relatedCharacterButtons[i])
                    relatedCharacterButtons[i].onClick.AddListener(action);
            }
        }

        if (goButtons != null)
        {
            goActions = new UnityAction[goButtons.Length];
            for (var i = 0; i < goButtons.Length; i++)
            {
                var locationIndex = GetMappedIndex(goButtonLocationIndexes, i);
                var action = CreateGoAction(locationIndex);
                goActions[i] = action;
                if (goButtons[i])
                    goButtons[i].onClick.AddListener(action);
            }
        }
    }

    private void UnbindButtons()
    {
        if (relatedCharacterButtons != null && characterActions != null)
            for (var i = 0; i < relatedCharacterButtons.Length && i < characterActions.Length; i++)
                if (relatedCharacterButtons[i] && characterActions[i] != null)
                    relatedCharacterButtons[i].onClick.RemoveListener(characterActions[i]);

        if (goButtons != null && goActions != null)
            for (var i = 0; i < goButtons.Length && i < goActions.Length; i++)
                if (goButtons[i] && goActions[i] != null)
                    goButtons[i].onClick.RemoveListener(goActions[i]);

        characterActions = null;
        goActions = null;
    }

    private UnityAction CreateCharacterAction(int locationIndex)
    {
        return () =>
        {
            SelectLocation(locationIndex);
            OnCharacterSelected.Invoke(locationIndex);
        };
    }

    private UnityAction CreateGoAction(int locationIndex)
    {
        return () =>
        {
            SelectLocation(locationIndex);
            OnGoSelected.Invoke(locationIndex);
        };
    }

    private void ApplyLocationColors()
    {
        if (locationImages == null)
            return;

        for (var i = 0; i < locationImages.Length; i++)
        {
            if (!locationImages[i])
                continue;

            var tint = i == selectedIndex ? selectedImageTint : unselectedImageTint;
            if (locations != null && i < locations.Length)
                tint *= isDay ? locations[i].dayColor : locations[i].nightColor;
            locationImages[i].color = tint;
        }
    }

    private void ApplyDayNightBackground()
    {
        if (dayNightBackgroundImage && locations != null && selectedIndex >= 0 && selectedIndex < locations.Length)
            dayNightBackgroundImage.color = isDay ? locations[selectedIndex].dayColor : locations[selectedIndex].nightColor;

        SetText(dayNightLabelText, isDay ? "DAY" : "NIGHT");
    }

    private int GetCurrentHour()
    {
        if (timeProvider is IMapTimeProvider typedProvider)
            return NormalizeHour(typedProvider.CurrentHour);

        if (timeProvider)
        {
            var type = timeProvider.GetType();
            var property = type.GetProperty("CurrentHour", BindingFlags.Instance | BindingFlags.Public)
                ?? type.GetProperty("Hour", BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.PropertyType == typeof(int))
                return NormalizeHour((int)property.GetValue(timeProvider, null));
        }

        return NormalizeHour(fallbackHour);
    }

    private bool IsDay(int hour)
    {
        if (dayStartsAt <= nightStartsAt)
            return hour >= dayStartsAt && hour < nightStartsAt;
        return hour >= dayStartsAt || hour < nightStartsAt;
    }

    private static int NormalizeHour(int hour)
    {
        return ((hour % 24) + 24) % 24;
    }

    private static int GetMappedIndex(int[] indexes, int buttonIndex)
    {
        return indexes != null && buttonIndex < indexes.Length ? indexes[buttonIndex] : buttonIndex;
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text)
            text.text = value ?? string.Empty;
    }
}

public class MapLocationPointerHelper : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private MapMenuController controller;
    private int locationIndex;
    private AudioSource audioSource;
    private AudioClip audioClip;

    public void Initialize(MapMenuController owner, int index, AudioSource source, AudioClip clip)
    {
        controller = owner;
        locationIndex = index;
        audioSource = source;
        audioClip = clip;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (audioSource && audioClip)
            audioSource.PlayOneShot(audioClip);
        controller?.SelectLocation(locationIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        controller?.SelectLocation(locationIndex);
    }
}
