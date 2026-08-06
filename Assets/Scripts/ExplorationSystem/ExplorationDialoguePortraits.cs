using System;
using Naninovel;
using UnityEngine;
using UnityEngine.UI;

namespace MoshiReRe.Exploration
{
    public enum ExplorationPortraitSide
    {
        Npc,
        Protagonist
    }

    [Serializable]
    public struct ExplorationPortraitVariant
    {
        public string id;
        public Sprite sprite;
    }

    /// <summary>Shows temporary ADV portraits over an exploration scene.</summary>
    [DisallowMultipleComponent]
    public sealed class ExplorationDialoguePortraits : MonoBehaviour
    {
        [Header("Optional prebuilt UI")]
        [SerializeField] private RectTransform portraitRoot;
        [SerializeField] private Image npcImage;
        [SerializeField] private Image protagonistImage;

        [Header("Runtime-created UI fallback")]
        [SerializeField] private bool createMissingUi = true;
        [SerializeField] private Vector2 portraitSize = new Vector2(430f, 720f);
        [SerializeField] private Vector2 npcOffset = new Vector2(34f, 225f);
        [SerializeField] private Vector2 protagonistOffset = new Vector2(-34f, 225f);
        [SerializeField] private bool preserveAspect = true;

        [Header("Shared expression variants")]
        [SerializeField] private ExplorationPortraitVariant[] variants = Array.Empty<ExplorationPortraitVariant>();

        private static ExplorationDialoguePortraits activePresenter;

        private ExplorationPortraitVariant[] interactionVariants = Array.Empty<ExplorationPortraitVariant>();
        private Sprite previousNpcSprite;
        private Sprite previousProtagonistSprite;
        private bool previousNpcActive;
        private bool previousProtagonistActive;
        private bool presenting;
        private int presentationId;

        private void Awake()
        {
            EnsureUi();
            HideImages();
        }

        private void OnDisable()
        {
            if (activePresenter == this)
                activePresenter = null;
            if (presenting)
                EndPresentation(presentationId);
        }

        /// <summary>Begins a restorable portrait presentation and returns its ownership token.</summary>
        public int BeginPresentation(
            Sprite protagonist,
            Sprite npc,
            string protagonistVariantId,
            string npcVariantId,
            ExplorationPortraitVariant[] localVariants)
        {
            EnsureUi();
            if (presenting)
                EndPresentation(presentationId);

            previousNpcSprite = npcImage != null ? npcImage.sprite : null;
            previousProtagonistSprite = protagonistImage != null ? protagonistImage.sprite : null;
            previousNpcActive = npcImage != null && npcImage.gameObject.activeSelf;
            previousProtagonistActive = protagonistImage != null && protagonistImage.gameObject.activeSelf;
            interactionVariants = localVariants ?? Array.Empty<ExplorationPortraitVariant>();
            presenting = true;
            presentationId++;
            activePresenter = this;

            SetPortrait(ExplorationPortraitSide.Protagonist,
                ResolvePortrait(protagonistVariantId, protagonist));
            SetPortrait(ExplorationPortraitSide.Npc,
                ResolvePortrait(npcVariantId, npc));
            return presentationId;
        }

        /// <summary>Restores the portrait state that existed before the matching presentation.</summary>
        public void EndPresentation(int ownerId)
        {
            if (!presenting || ownerId != presentationId)
                return;

            RestoreImage(npcImage, previousNpcSprite, previousNpcActive);
            RestoreImage(protagonistImage, previousProtagonistSprite, previousProtagonistActive);
            interactionVariants = Array.Empty<ExplorationPortraitVariant>();
            presenting = false;
            if (activePresenter == this)
                activePresenter = null;
        }

        public bool TrySetVariant(ExplorationPortraitSide side, string variantId)
        {
            var sprite = ResolvePortrait(variantId, null);
            if (sprite == null)
                return false;
            SetPortrait(side, sprite);
            return true;
        }

        public static bool TrySetActiveVariant(string side, string variantId)
        {
            if (activePresenter == null || !TryParseSide(side, out var parsedSide))
                return false;
            return activePresenter.TrySetVariant(parsedSide, variantId);
        }

        public static bool TryParseSide(string value, out ExplorationPortraitSide side)
        {
            if (string.Equals(value, "right", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "protagonist", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "player", StringComparison.OrdinalIgnoreCase))
            {
                side = ExplorationPortraitSide.Protagonist;
                return true;
            }

            if (string.Equals(value, "left", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "npc", StringComparison.OrdinalIgnoreCase))
            {
                side = ExplorationPortraitSide.Npc;
                return true;
            }

            side = default;
            return false;
        }

        public static Sprite FindVariant(
            string variantId,
            ExplorationPortraitVariant[] primary,
            ExplorationPortraitVariant[] secondary,
            Sprite fallback)
        {
            if (string.IsNullOrWhiteSpace(variantId))
                return fallback;

            var sprite = FindVariantIn(variantId, primary);
            return sprite != null ? sprite : FindVariantIn(variantId, secondary) ?? fallback;
        }

        private Sprite ResolvePortrait(string variantId, Sprite fallback)
        {
            return FindVariant(variantId, interactionVariants, variants, fallback);
        }

        private static Sprite FindVariantIn(string variantId, ExplorationPortraitVariant[] source)
        {
            if (source == null)
                return null;
            for (var i = 0; i < source.Length; i++)
                if (string.Equals(source[i].id, variantId, StringComparison.OrdinalIgnoreCase))
                    return source[i].sprite;
            return null;
        }

        private void EnsureUi()
        {
            if ((!createMissingUi || npcImage != null && protagonistImage != null))
                return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return;

            if (portraitRoot == null)
            {
                var rootObject = new GameObject("DialoguePortraits", typeof(RectTransform));
                portraitRoot = rootObject.GetComponent<RectTransform>();
                portraitRoot.SetParent(canvas.transform, false);
                portraitRoot.anchorMin = Vector2.zero;
                portraitRoot.anchorMax = Vector2.one;
                portraitRoot.offsetMin = Vector2.zero;
                portraitRoot.offsetMax = Vector2.zero;
                portraitRoot.SetAsFirstSibling();
            }

            if (npcImage == null)
                npcImage = CreateImage("NpcPortrait", false, npcOffset);
            if (protagonistImage == null)
                protagonistImage = CreateImage("ProtagonistPortrait", true, protagonistOffset);
        }

        private Image CreateImage(string objectName, bool right, Vector2 offset)
        {
            var imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(portraitRoot, false);
            var anchor = right ? new Vector2(1f, 0f) : new Vector2(0f, 0f);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = portraitSize;

            var image = imageObject.GetComponent<Image>();
            image.preserveAspect = preserveAspect;
            image.raycastTarget = false;
            return image;
        }

        private void SetPortrait(ExplorationPortraitSide side, Sprite sprite)
        {
            var target = side == ExplorationPortraitSide.Npc ? npcImage : protagonistImage;
            if (target == null)
                return;
            target.sprite = sprite;
            target.gameObject.SetActive(sprite != null);
        }

        private void HideImages()
        {
            if (npcImage != null)
                npcImage.gameObject.SetActive(false);
            if (protagonistImage != null)
                protagonistImage.gameObject.SetActive(false);
        }

        private static void RestoreImage(Image image, Sprite sprite, bool active)
        {
            if (image == null)
                return;
            image.sprite = sprite;
            image.gameObject.SetActive(active);
        }
    }

    /// <summary>Changes the active exploration portrait from a .nani line.</summary>
    [Command.CommandAlias("explorationPortrait")]
    public sealed class ExplorationPortraitCommand : Command
    {
        [Command.ParameterAlias("side")]
        public StringParameter Side;

        [Command.ParameterAlias("id")]
        public StringParameter VariantId;

        public override UniTask Execute(AsyncToken asyncToken = default)
        {
            if (!ExplorationDialoguePortraits.TrySetActiveVariant(Side?.Value, VariantId?.Value))
                Debug.LogWarning($"[explorationPortrait] Could not apply '{VariantId?.Value}' to '{Side?.Value}'.");
            return UniTask.CompletedTask;
        }
    }
}
