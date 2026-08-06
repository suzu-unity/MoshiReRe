using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MoshiReRe.Exploration
{
    /// <summary>Displays the icon of a newly acquired inventory item at screen center.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationItemAcquisitionPopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Image iconImage;
        [SerializeField, Min(0.05f)] private float displayDuration = 1.25f;
        [SerializeField] private Vector2 iconSize = new Vector2(270f, 270f);
        [SerializeField] private bool createMissingUi = true;

        private Coroutine hideRoutine;

        private void Awake()
        {
            EnsureUi();
            if (popupRoot != null)
                popupRoot.SetActive(false);
        }

        private void OnEnable() => InventoryDatabase.ItemAcquired += HandleItemAcquired;

        private void OnDisable()
        {
            InventoryDatabase.ItemAcquired -= HandleItemAcquired;
            if (hideRoutine != null)
                StopCoroutine(hideRoutine);
            hideRoutine = null;
            if (popupRoot != null)
                popupRoot.SetActive(false);
        }

        public static bool ShouldShow(InventoryItem item)
        {
            return item != null && item.icon != null;
        }

        private void HandleItemAcquired(InventoryItem item)
        {
            if (!ShouldShow(item))
                return;

            EnsureUi();
            if (popupRoot == null || iconImage == null)
                return;

            if (hideRoutine != null)
                StopCoroutine(hideRoutine);
            iconImage.sprite = item.icon;
            popupRoot.SetActive(true);
            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, displayDuration));
            popupRoot.SetActive(false);
            hideRoutine = null;
        }

        private void EnsureUi()
        {
            if (!createMissingUi || popupRoot != null && iconImage != null)
                return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            if (popupRoot == null)
            {
                popupRoot = new GameObject("ItemAcquisitionPopup", typeof(RectTransform));
                var rootRect = popupRoot.GetComponent<RectTransform>();
                rootRect.SetParent(canvas.transform, false);
                rootRect.anchorMin = new Vector2(0.5f, 0.5f);
                rootRect.anchorMax = new Vector2(0.5f, 0.5f);
                rootRect.pivot = new Vector2(0.5f, 0.5f);
                rootRect.anchoredPosition = Vector2.zero;
                rootRect.sizeDelta = iconSize;
                rootRect.SetAsLastSibling();
            }

            if (iconImage == null)
            {
                iconImage = popupRoot.GetComponent<Image>() ?? popupRoot.AddComponent<Image>();
                iconImage.preserveAspect = true;
                iconImage.raycastTarget = false;
            }
        }
    }
}
