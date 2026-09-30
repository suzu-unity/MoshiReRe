using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MoshiReRe.Exploration
{
    /// <summary>
    /// Gives the control hint a readable backdrop and hides it once the player has started walking,
    /// bringing it back after a long idle so it never competes with the scene art.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationControlsHint : MonoBehaviour
    {
        [SerializeField] private ExplorationPlayerController player;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Color backdropColor = new Color(0.03f, 0.07f, 0.15f, 0.72f);
        [SerializeField] private Vector2 backdropPadding = new Vector2(22f, 10f);
        [SerializeField, Min(0f), Tooltip("Seconds of walking before the hint fades out.")]
        private float walkSecondsBeforeHide = 1.2f;
        [SerializeField, Min(0f), Tooltip("Idle seconds before the hint returns. 0 = never return.")]
        private float idleSecondsBeforeReturn = 12f;
        [SerializeField, Min(0f)] private float fadeDuration = 0.35f;

        private CanvasGroup group;
        private Tween fadeTween;
        private float walkedSeconds;
        private float idleSeconds;
        private bool shown = true;

        private void Reset()
        {
            hintText = GetComponent<TMP_Text>();
            player = FindFirstObjectByType<ExplorationPlayerController>();
        }

        private void Awake()
        {
            if (hintText == null)
                hintText = GetComponent<TMP_Text>();
            if (player == null)
                player = FindFirstObjectByType<ExplorationPlayerController>();
            if (hintText != null)
                BuildBackdrop();
        }

        private void OnDestroy() => fadeTween?.Kill();

        private void Update()
        {
            if (group == null)
                return;

            var walking = player != null && player.MovementEnabled && Mathf.Abs(player.VelocityX) > 0.05f;
            if (walking)
            {
                walkedSeconds += Time.deltaTime;
                idleSeconds = 0f;
            }
            else
            {
                idleSeconds += Time.deltaTime;
            }

            var returnAfterIdle = idleSecondsBeforeReturn > 0f && idleSeconds >= idleSecondsBeforeReturn;
            if (returnAfterIdle)
                walkedSeconds = 0f;

            var movementAvailable = player == null || player.MovementEnabled;
            SetShown(movementAvailable && walkedSeconds < walkSecondsBeforeHide);
        }

        private void SetShown(bool value)
        {
            if (shown == value)
                return;

            shown = value;
            fadeTween?.Kill();
            fadeTween = group.DOFade(value ? 1f : 0f, fadeDuration).SetEase(Ease.OutQuad).SetLink(gameObject);
        }

        /// <summary>Wraps the hint text in a padded, semi-transparent plate sized to the text.</summary>
        private void BuildBackdrop()
        {
            var textRect = hintText.rectTransform;
            var plate = new GameObject("ControlsHintPlate", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            var plateRect = (RectTransform)plate.transform;
            plateRect.SetParent(textRect.parent, false);
            plateRect.SetSiblingIndex(textRect.GetSiblingIndex());
            plateRect.anchorMin = textRect.anchorMin;
            plateRect.anchorMax = textRect.anchorMax;
            plateRect.pivot = textRect.pivot;
            plateRect.anchoredPosition = textRect.anchoredPosition;

            var preferred = hintText.GetPreferredValues(hintText.text);
            plateRect.sizeDelta = new Vector2(preferred.x + backdropPadding.x * 2f, preferred.y + backdropPadding.y * 2f);

            var image = plate.GetComponent<Image>();
            image.color = backdropColor;
            image.raycastTarget = false;
            group = plate.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            textRect.SetParent(plateRect, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = backdropPadding;
            textRect.offsetMax = -backdropPadding;
            hintText.alignment = TextAlignmentOptions.Center;
            hintText.raycastTarget = false;
        }
    }
}
