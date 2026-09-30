using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MoshiReRe.Exploration
{
    /// <summary>Binds a TextMesh Pro prompt to the currently selected interactable.</summary>
    [DisallowMultipleComponent]
    public sealed class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private ExplorationInteractionController interactionController;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private string promptFormat = "E：{0}";

        [Header("Placement")]
        [SerializeField, Tooltip("Float the prompt above the selected object instead of keeping its authored position.")]
        private bool anchorToTarget = true;
        [SerializeField, Tooltip("Camera that renders the exploration map. Defaults to the first enabled camera in this scene.")]
        private Camera worldCamera;
        [SerializeField, Tooltip("Canvas-unit gap between the object's top and the prompt's bottom edge.")]
        private float verticalGap = 18f;
        [SerializeField, Min(0f)] private float screenEdgeMargin = 24f;
        [SerializeField] private bool autoSizeWidth = true;
        [SerializeField, Min(0f)] private float horizontalPadding = 34f;
        [SerializeField, Min(0f)] private float minimumWidth = 150f;

        [Header("Motion")]
        [SerializeField, Min(0f)] private float popDuration = 0.2f;
        [SerializeField, Min(0f)] private float bobAmplitude = 5f;
        [SerializeField, Min(0f)] private float bobCyclesPerSecond = 1.1f;

        private ExplorationInteractable current;
        private RectTransform promptRect;
        private Vector3 authoredLocalPosition;
        private CanvasGroup promptGroup;
        private Sequence popSequence;
        private float shownTime;

        private void Reset()
        {
            promptText = GetComponentInChildren<TMP_Text>(true);
            promptRoot = promptText == null ? null : promptText.gameObject;
        }

        private void Awake()
        {
            promptRect = promptRoot != null ? promptRoot.transform as RectTransform : null;
            if (promptRect != null)
            {
                authoredLocalPosition = promptRect.localPosition;
                promptGroup = promptRoot.GetComponent<CanvasGroup>();
                if (promptGroup == null)
                    promptGroup = promptRoot.AddComponent<CanvasGroup>();
                promptGroup.blocksRaycasts = false;
                promptGroup.interactable = false;
            }
        }

        private void OnEnable()
        {
            if (interactionController != null)
                interactionController.NearestChanged += Refresh;

            Refresh(interactionController == null ? null : interactionController.Nearest);
        }

        private void OnDisable()
        {
            if (interactionController != null)
                interactionController.NearestChanged -= Refresh;
            popSequence?.Kill();
        }

        public void Refresh(ExplorationInteractable interactable)
        {
            var wasVisible = current != null;
            current = interactable;
            var visible = interactable != null;
            if (promptRoot != null)
                promptRoot.SetActive(visible);
            else if (promptText != null)
                promptText.enabled = visible;

            if (!visible)
            {
                popSequence?.Kill();
                return;
            }

            if (promptText != null)
            {
                promptText.text = string.Format(promptFormat, interactable.PromptText);
                FitWidth();
            }

            UpdatePlacement();
            if (!wasVisible)
                PlayPop();
        }

        private void LateUpdate()
        {
            if (current != null)
                UpdatePlacement();
        }

        private void FitWidth()
        {
            if (!autoSizeWidth || promptRect == null || promptText == null)
                return;

            var textWidth = promptText.GetPreferredValues(promptText.text).x;
            var size = promptRect.sizeDelta;
            size.x = Mathf.Max(minimumWidth, textWidth + horizontalPadding * 2f);
            promptRect.sizeDelta = size;
        }

        private void UpdatePlacement()
        {
            if (promptRect == null)
                return;

            var bob = Mathf.Sin((Time.unscaledTime - shownTime) * bobCyclesPerSecond * Mathf.PI * 2f) * bobAmplitude;
            if (!anchorToTarget || current == null || !TryGetLocalAnchor(CalculateWorldAnchor(current), out var local))
            {
                promptRect.localPosition = authoredLocalPosition + Vector3.up * bob;
                return;
            }

            promptRect.pivot = new Vector2(0.5f, 0f);
            var parent = (RectTransform)promptRect.parent;
            var half = promptRect.rect.size * 0.5f;
            var bounds = parent.rect;
            local.x = Mathf.Clamp(local.x, bounds.xMin + screenEdgeMargin + half.x, bounds.xMax - screenEdgeMargin - half.x);
            local.y = Mathf.Clamp(local.y + verticalGap, bounds.yMin + screenEdgeMargin,
                bounds.yMax - screenEdgeMargin - promptRect.rect.height);
            promptRect.localPosition = new Vector3(local.x, local.y + bob, 0f);
        }

        /// <summary>
        /// Uses the object's own top, but never lets the prompt sink below the player's head so
        /// background-painted props (desks, doors) do not cover the character.
        /// </summary>
        private Vector3 CalculateWorldAnchor(ExplorationInteractable interactable)
        {
            var anchor = interactable.GetPromptAnchorPosition();
            var player = interactionController != null ? interactionController.Player : null;
            if (player == null)
                return anchor;

            var hasBounds = false;
            var playerBounds = default(Bounds);
            foreach (var spriteRenderer in player.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!spriteRenderer.enabled || spriteRenderer.sprite == null)
                    continue;
                if (hasBounds) playerBounds.Encapsulate(spriteRenderer.bounds);
                else { playerBounds = spriteRenderer.bounds; hasBounds = true; }
            }

            if (hasBounds)
                anchor.y = Mathf.Max(anchor.y, playerBounds.max.y);
            return anchor;
        }

        private bool TryGetLocalAnchor(Vector3 worldPosition, out Vector2 local)
        {
            local = default;
            var parent = promptRect.parent as RectTransform;
            var camera = ResolveCamera();
            if (parent == null || camera == null)
                return false;

            var screen = camera.WorldToScreenPoint(worldPosition);
            if (screen.z < 0f)
                return false;

            var canvas = promptRect.GetComponentInParent<Canvas>();
            var uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCamera, out local);
        }

        private Camera ResolveCamera()
        {
            if (worldCamera != null && worldCamera.isActiveAndEnabled)
                return worldCamera;

            foreach (var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                if (camera.isActiveAndEnabled && camera.gameObject.scene == gameObject.scene)
                {
                    worldCamera = camera;
                    return camera;
                }
            }

            return Camera.main;
        }

        private void PlayPop()
        {
            if (promptRect == null || popDuration <= 0f)
                return;

            shownTime = Time.unscaledTime;
            popSequence?.Kill();
            promptRect.localScale = new Vector3(0.82f, 0.82f, 1f);
            if (promptGroup != null)
                promptGroup.alpha = 0f;

            popSequence = DOTween.Sequence().SetUpdate(true).SetLink(promptRoot);
            popSequence.Join(promptRect.DOScale(1f, popDuration).SetEase(Ease.OutBack, 2.2f));
            if (promptGroup != null)
                popSequence.Join(promptGroup.DOFade(1f, popDuration * 0.7f));
        }
    }
}
