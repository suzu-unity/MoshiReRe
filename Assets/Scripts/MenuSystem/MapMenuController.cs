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
    [SerializeField] private MapLocationPolygon[] locationPolygons;
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
    [SerializeField] private GameObject dayMapRoot;
    [SerializeField] private GameObject nightMapRoot;

    [Header("Related character buttons")]
    [SerializeField] private Button[] relatedCharacterButtons;
    [SerializeField] private int[] relatedCharacterLocationIndexes;
    [SerializeField] private int[] goButtonLocationIndexes;

    [Header("Page shortcuts")]
    [SerializeField] private Button[] pageNavigationButtons;

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
    private UnityAction[] navigationActions;
    private int selectedIndex = -1;
    private bool isDay;
    private int lastObservedHour = -1;

    private void Awake()
    {
        BindButtons();
        RefreshTimeAndColors();
        SelectLocation(0);
    }

    private void OnEnable()
    {
        RefreshTimeAndColors();
    }

    private void Update()
    {
        var hour = GetCurrentHour();
        if (hour == lastObservedHour)
            return;

        lastObservedHour = hour;
        RefreshTimeAndColors();
    }

    private void OnDestroy()
    {
        UnbindButtons();
    }

    public void RefreshTimeAndColors()
    {
        lastObservedHour = GetCurrentHour();
        isDay = IsDay(lastObservedHour);
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

        if (pageNavigationButtons != null)
        {
            navigationActions = new UnityAction[pageNavigationButtons.Length];
            for (var i = 0; i < pageNavigationButtons.Length; i++)
            {
                var pageIndex = i;
                navigationActions[i] = () => OpenPage(pageIndex);
                if (pageNavigationButtons[i])
                    pageNavigationButtons[i].onClick.AddListener(navigationActions[i]);
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

        if (pageNavigationButtons != null && navigationActions != null)
            for (var i = 0; i < pageNavigationButtons.Length && i < navigationActions.Length; i++)
                if (pageNavigationButtons[i] && navigationActions[i] != null)
                    pageNavigationButtons[i].onClick.RemoveListener(navigationActions[i]);

        characterActions = null;
        goActions = null;
        navigationActions = null;
    }

    private void OpenPage(int pageIndex)
    {
        var menu = GetComponentInParent<MenuRootV2UI>();
        if (!menu) return;
        switch (pageIndex)
        {
            case 0: menu.ShowTop(); break;
            case 1: menu.ShowStatus(); break;
            case 2: menu.ShowItems(); break;
            case 3: menu.ShowCharacters(); break;
            case 4: menu.ShowQuest(); break;
            default: menu.ShowMap(); break;
        }
    }

    private UnityAction CreateCharacterAction(int locationIndex)
    {
        return () =>
        {
            SelectLocation(locationIndex);
            OnCharacterSelected.Invoke(locationIndex);
            OpenPage(3);
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
        if (locationPolygons != null)
        {
            for (var i = 0; i < locationPolygons.Length; i++)
            {
                if (!locationPolygons[i])
                    continue;

                var color = Color.white;
                if (locations != null && i < locations.Length)
                    color = isDay ? locations[i].dayColor : locations[i].nightColor;
                locationPolygons[i].SetVisual(i == selectedIndex, color);
            }
        }

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

    public void SetLocationHover(int index, bool isHovering)
    {
        if (locationPolygons != null && index >= 0 && index < locationPolygons.Length && locationPolygons[index])
            locationPolygons[index].SetHover(isHovering);
    }

    private void ApplyDayNightBackground()
    {
        if (dayNightBackgroundImage && locations != null && selectedIndex >= 0 && selectedIndex < locations.Length)
            dayNightBackgroundImage.color = isDay ? locations[selectedIndex].dayColor : locations[selectedIndex].nightColor;

        SetText(dayNightLabelText, isDay ? "DAY" : "NIGHT");
        if (dayMapRoot) dayMapRoot.SetActive(isDay);
        if (nightMapRoot) nightMapRoot.SetActive(!isDay);
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

public class MapLocationPointerHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
        controller?.SetLocationHover(locationIndex, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        controller?.SetLocationHover(locationIndex, false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        controller?.SelectLocation(locationIndex);
    }
}

/// <summary>Polygonal map hot spot with a translucent fill and outline.</summary>
public class MapLocationPolygon : Graphic, ICanvasRaycastFilter
{
    [SerializeField] private Vector2[] normalizedPoints;
    [SerializeField] private Color fillColor = new Color(1f, 1f, 1f, 0.12f);
    [SerializeField] private Color outlineColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private Color hoverOutlineColor = new Color(1f, 0.92f, 0.35f, 1f);
    [SerializeField] private Color selectedOutlineColor = new Color(1f, 0.43f, 0.48f, 1f);
    [SerializeField, Min(1f)] private float outlineWidth = 4f;

    private bool isHovered;
    private bool isSelected;
    private Color locationColor = Color.white;

    public void Initialize(Vector2[] points)
    {
        normalizedPoints = points;
        SetVerticesDirty();
    }

    public void SetVisual(bool selected, Color locationTint)
    {
        isSelected = selected;
        locationColor = locationTint;
        SetVerticesDirty();
    }

    public void SetHover(bool hovered)
    {
        if (isHovered == hovered)
            return;
        isHovered = hovered;
        SetVerticesDirty();
    }

    public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        if (normalizedPoints == null || normalizedPoints.Length < 3 || !RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out var local))
            return false;

        var rect = rectTransform.rect;
        var point = new Vector2((local.x - rect.xMin) / rect.width, (local.y - rect.yMin) / rect.height);
        var inside = false;
        for (var i = 0, j = normalizedPoints.Length - 1; i < normalizedPoints.Length; j = i++)
        {
            var a = normalizedPoints[i];
            var b = normalizedPoints[j];
            if ((a.y > point.y) != (b.y > point.y) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }
        return inside;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (normalizedPoints == null || normalizedPoints.Length < 3)
            return;

        var rect = rectTransform.rect;
        var fill = fillColor * locationColor;
        fill.a = isSelected ? 0.28f : isHovered ? 0.20f : 0.10f;
        var outline = isSelected ? selectedOutlineColor : isHovered ? hoverOutlineColor : outlineColor;
        for (var i = 0; i < normalizedPoints.Length; i++)
            vh.AddVert(ToLocal(normalizedPoints[i], rect), fill, Vector2.zero);
        for (var i = 1; i < normalizedPoints.Length - 1; i++)
            vh.AddTriangle(0, i, i + 1);

        for (var i = 0; i < normalizedPoints.Length; i++)
        {
            var a = ToLocal(normalizedPoints[i], rect);
            var b = ToLocal(normalizedPoints[(i + 1) % normalizedPoints.Length], rect);
            var normal = Vector2.Perpendicular((b - a).normalized) * (outlineWidth * 0.5f);
            var index = vh.currentVertCount;
            vh.AddVert(a - normal, outline, Vector2.zero);
            vh.AddVert(a + normal, outline, Vector2.zero);
            vh.AddVert(b + normal, outline, Vector2.zero);
            vh.AddVert(b - normal, outline, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }

    private static Vector2 ToLocal(Vector2 normalized, Rect rect) => new Vector2(Mathf.Lerp(rect.xMin, rect.xMax, normalized.x), Mathf.Lerp(rect.yMin, rect.yMax, normalized.y));
}
