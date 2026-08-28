using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoshiReRe.ItemPresentation
{
    /// <summary>Describes how the item-presentation modal was closed.</summary>
    public enum ItemPresentationOutcomeType
    {
        Cancelled,
        NoItems,
        Presented
    }

    /// <summary>Small value returned by the runtime item-presentation UI.</summary>
    public readonly struct ItemPresentationOutcome
    {
        public ItemPresentationOutcome(ItemPresentationOutcomeType type, InventoryItem item)
        {
            Type = type;
            Item = item;
        }

        public ItemPresentationOutcomeType Type { get; }
        public InventoryItem Item { get; }

        public static ItemPresentationOutcome Cancelled() =>
            new ItemPresentationOutcome(ItemPresentationOutcomeType.Cancelled, null);

        public static ItemPresentationOutcome NoItems() =>
            new ItemPresentationOutcome(ItemPresentationOutcomeType.NoItems, null);

        public static ItemPresentationOutcome Presented(InventoryItem item) =>
            new ItemPresentationOutcome(ItemPresentationOutcomeType.Presented, item);
    }

    /// <summary>Pure routing helpers shared by the modal and EditMode tests.</summary>
    public static class ItemPresentationFlow
    {
        public const string CancelledResult = "cancelled";
        public const string NoItemsResult = "no_items";
        public const string NormalResult = "normal";

        public static IReadOnlyList<InventoryItem> GetCandidates(InventoryDatabase database)
        {
            if (database == null)
                return Array.Empty<InventoryItem>();

            var acquired = database.GetAcquired();
            if (acquired == null || acquired.Count == 0)
                return Array.Empty<InventoryItem>();

            var candidates = new List<InventoryItem>(acquired.Count);
            for (var i = 0; i < acquired.Count; i++)
            {
                var item = acquired[i];
                if (item != null)
                    candidates.Add(item);
            }

            return candidates;
        }

        public static string ResolveResultCode(
            ItemPresentationOutcome outcome,
            string successItemId,
            bool requiredConditionMet)
        {
            if (outcome.Type == ItemPresentationOutcomeType.Cancelled)
                return CancelledResult;

            if (outcome.Type == ItemPresentationOutcomeType.NoItems || outcome.Item == null)
                return NoItemsResult;

            if (!requiredConditionMet || !ItemIdsMatch(outcome.Item.id, successItemId))
                return NormalResult;

            return NormalizeId(successItemId);
        }

        public static bool ItemIdsMatch(string left, string right)
        {
            return !string.IsNullOrWhiteSpace(left) &&
                   !string.IsNullOrWhiteSpace(right) &&
                   string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        public static string NormalizeId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        public static Color GetPlaceholderColor(string itemId)
        {
            unchecked
            {
                var hash = string.IsNullOrEmpty(itemId) ? 17 : itemId.GetHashCode();
                var hue = (hash & 0x7fffffff) % 360 / 360f;
                return Color.HSVToRGB(hue, 0.42f, 0.92f);
            }
        }
    }
}
