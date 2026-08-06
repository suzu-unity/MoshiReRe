using MoshiReRe.Exploration;
using NUnit.Framework;
using UnityEngine;

namespace MoshiReRe.EditorTests.ExplorationSystem
{
    public sealed class ExplorationDialoguePresentationLogicTests
    {
        [TestCase("left", ExplorationPortraitSide.Npc)]
        [TestCase("NPC", ExplorationPortraitSide.Npc)]
        [TestCase("right", ExplorationPortraitSide.Protagonist)]
        [TestCase("player", ExplorationPortraitSide.Protagonist)]
        public void TryParseSide_AcceptsNaninovelAliases(
            string value,
            ExplorationPortraitSide expected)
        {
            Assert.That(ExplorationDialoguePortraits.TryParseSide(value, out var actual), Is.True);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void FindVariant_PrefersInteractionVariantAndFallsBackToDirectSprite()
        {
            var interactionSprite = CreateSprite();
            var sharedSprite = CreateSprite();
            var fallbackSprite = CreateSprite();
            try
            {
                var interaction = new[]
                {
                    new ExplorationPortraitVariant { id = "smile", sprite = interactionSprite }
                };
                var shared = new[]
                {
                    new ExplorationPortraitVariant { id = "smile", sprite = sharedSprite }
                };

                Assert.That(
                    ExplorationDialoguePortraits.FindVariant("smile", interaction, shared, fallbackSprite),
                    Is.SameAs(interactionSprite));
                Assert.That(
                    ExplorationDialoguePortraits.FindVariant("missing", interaction, shared, fallbackSprite),
                    Is.SameAs(fallbackSprite));
            }
            finally
            {
                DestroySprite(interactionSprite);
                DestroySprite(sharedSprite);
                DestroySprite(fallbackSprite);
            }
        }

        [Test]
        public void ItemPopup_RequiresAnItemIcon()
        {
            var item = ScriptableObject.CreateInstance<InventoryItem>();
            var sprite = CreateSprite();
            try
            {
                Assert.That(ExplorationItemAcquisitionPopup.ShouldShow(null), Is.False);
                Assert.That(ExplorationItemAcquisitionPopup.ShouldShow(item), Is.False);
                item.icon = sprite;
                Assert.That(ExplorationItemAcquisitionPopup.ShouldShow(item), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(item);
                DestroySprite(sprite);
            }
        }

        private static Sprite CreateSprite()
        {
            var texture = new Texture2D(2, 2);
            return Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), Vector2.one * 0.5f);
        }

        private static void DestroySprite(Sprite sprite)
        {
            if (sprite == null)
                return;
            var texture = sprite.texture;
            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(texture);
        }
    }
}
