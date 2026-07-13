using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform), typeof(Canvas))]
public sealed class ComicPanelController : MonoBehaviour
{
    public static ComicPanelController Instance { get; private set; }

    [SerializeField] private ComicPanelLayout layout;
    [SerializeField] private int selectedPanel;
    [SerializeField] private bool buildInEditMode = true;

    private readonly List<ComicPanelGraphic> graphics = new List<ComicPanelGraphic>();
    private Coroutine transitionRoutine;
    private bool isVisible;
    private int focusIndex = -1;
    private ComicPanelFocusMode focusMode = ComicPanelFocusMode.All;

    public ComicPanelLayout Layout => layout;
    public int SelectedPanel { get => selectedPanel; set => selectedPanel = Mathf.Max(0, value); }
    public bool IsVisible => isVisible;
    public int FocusIndex => focusIndex;
    public ComicPanelFocusMode FocusMode => focusMode;
    public IReadOnlyList<ComicPanelGraphic> Graphics => graphics;

#if UNITY_EDITOR
    public void SetLayoutForEditor(ComicPanelLayout value)
    {
        layout = value;
        RebuildVisuals();
    }
#endif

    private void OnEnable()
    {
        if (Application.isPlaying)
            Instance = this;

        RebuildVisuals();
        if (Application.isPlaying)
            HideInstant();
        else if (buildInEditMode)
            ApplyEditorPreview();
    }

    private void Awake()
    {
        if (Application.isPlaying)
            Instance = this;
    }

    private void OnDisable()
    {
        if (Application.isPlaying && Instance == this)
            Instance = null;
    }

    private void OnValidate()
    {
        selectedPanel = Mathf.Max(0, selectedPanel);
        RebuildVisuals();
        if (!Application.isPlaying && buildInEditMode)
            ApplyEditorPreview();
    }

    public bool ShowById(string layoutId, int panelIndex = -1, ComicPanelFocusMode mode = ComicPanelFocusMode.All, float transitionOverride = -1f)
    {
        if (!TryResolveLayout(layoutId))
        {
            Debug.LogWarning($"[ComicPanel] Layout not found: {layoutId}");
            return false;
        }

        if (panelIndex >= 0 && layout.Panels.Count > 0)
        {
            panelIndex = Mathf.Clamp(panelIndex, 0, layout.Panels.Count - 1);
            return ApplyVisibleState(panelIndex, mode, transitionOverride);
        }

        return ShowAll(transitionOverride);
    }

    public bool ShowAll(float transitionOverride = -1f)
    {
        return ApplyVisibleState(-1, ComicPanelFocusMode.All, transitionOverride);
    }

    public bool FocusThrough(int panelIndex, float transitionOverride = -1f)
    {
        return ApplyVisibleState(panelIndex, ComicPanelFocusMode.Through, transitionOverride);
    }

    public bool FocusOnly(int panelIndex, float transitionOverride = -1f)
    {
        return ApplyVisibleState(panelIndex, ComicPanelFocusMode.Only, transitionOverride);
    }

    public void Hide(float transitionOverride = -1f)
    {
        EnsureVisuals();
        isVisible = false;
        focusIndex = -1;
        focusMode = ComicPanelFocusMode.All;
        var targets = new List<Color>(graphics.Count);
        for (var i = 0; i < graphics.Count; i++)
            targets.Add(Color.clear);
        TransitionTo(targets, transitionOverride >= 0f ? transitionOverride : GetDefaultTransition());
    }

    private bool ApplyVisibleState(int newFocusIndex, ComicPanelFocusMode newMode, float transitionOverride)
    {
        if (layout == null || layout.Panels == null || layout.Panels.Count == 0)
        {
            Debug.LogWarning("[ComicPanel] No panels are configured.");
            return false;
        }

        EnsureVisuals();
        isVisible = true;
        focusIndex = newFocusIndex;
        focusMode = newMode;
        var targets = new List<Color>(layout.Panels.Count);
        for (var i = 0; i < layout.Panels.Count; i++)
        {
            var panel = layout.Panels[i];
            var emphasized = ComicPanelFocusState.IsEmphasized(i, newFocusIndex, newMode);
            targets.Add(panel.GetColor(emphasized));
        }

        var duration = transitionOverride >= 0f ? transitionOverride : GetDefaultTransition();
        TransitionTo(targets, duration);
        return true;
    }

    private bool TryResolveLayout(string layoutId)
    {
        if (layout != null && layout.MatchesId(layoutId))
            return true;

        if (string.IsNullOrWhiteSpace(layoutId))
            return layout != null;

        var loaded = Resources.Load<ComicPanelLayout>(layoutId);
        if (loaded == null)
            return false;

        layout = loaded;
        RebuildVisuals();
        return true;
    }

    private float GetDefaultTransition()
    {
        if (layout == null || layout.Panels == null || layout.Panels.Count == 0)
            return 0f;

        var duration = 0f;
        for (var i = 0; i < layout.Panels.Count; i++)
            duration = Mathf.Max(duration, Mathf.Max(0f, layout.Panels[i].transitionSeconds));
        return duration;
    }

    private void RebuildVisuals()
    {
        EnsureVisuals();
        for (var i = 0; i < graphics.Count; i++)
        {
            var panel = layout.Panels[i];
            graphics[i].Configure(panel.image, panel.SafeVertices);
            graphics[i].raycastTarget = false;
            graphics[i].gameObject.name = $"ComicPanel_{i + 1}_{panel.Id}";
        }
    }

    private void EnsureVisuals()
    {
        var count = layout != null && layout.Panels != null ? layout.Panels.Count : 0;
        while (graphics.Count < count)
        {
            var index = graphics.Count;
            var child = new GameObject($"ComicPanel_{index + 1}", typeof(RectTransform), typeof(ComicPanelGraphic));
            child.transform.SetParent(transform, false);
            var rect = child.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            graphics.Add(child.GetComponent<ComicPanelGraphic>());
        }

        while (graphics.Count > count)
        {
            var last = graphics[graphics.Count - 1];
            graphics.RemoveAt(graphics.Count - 1);
            if (last == null)
                continue;
            if (Application.isPlaying)
                Destroy(last.gameObject);
            else
                DestroyImmediate(last.gameObject);
        }
    }

    private void TransitionTo(IReadOnlyList<Color> targets, float duration)
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (!Application.isPlaying || duration <= 0f)
        {
            ApplyColors(targets);
            return;
        }

        transitionRoutine = StartCoroutine(AnimateColors(targets, duration));
    }

    private IEnumerator AnimateColors(IReadOnlyList<Color> targets, float duration)
    {
        var starts = new Color[graphics.Count];
        for (var i = 0; i < graphics.Count; i++)
            starts[i] = graphics[i].color;

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            var progress = Mathf.Clamp01(elapsed / duration);
            for (var i = 0; i < graphics.Count; i++)
                graphics[i].color = Color.Lerp(starts[i], targets[i], progress);
            yield return null;
        }

        ApplyColors(targets);
        transitionRoutine = null;
    }

    private void ApplyColors(IReadOnlyList<Color> colors)
    {
        for (var i = 0; i < graphics.Count && i < colors.Count; i++)
            graphics[i].color = colors[i];
    }

    private void HideInstant()
    {
        EnsureVisuals();
        isVisible = false;
        focusIndex = -1;
        ApplyColors(new Color[graphics.Count]);
    }

    private void ApplyEditorPreview()
    {
        if (layout == null || layout.Panels == null)
            return;

        var colors = new List<Color>(layout.Panels.Count);
        for (var i = 0; i < layout.Panels.Count; i++)
            colors.Add(layout.Panels[i].GetColor(true));
        ApplyColors(colors);
    }
}
