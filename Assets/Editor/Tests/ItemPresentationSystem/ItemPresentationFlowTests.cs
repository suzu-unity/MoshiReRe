using System.IO;
using MoshiReRe.ItemPresentation;
using NUnit.Framework;
using UnityEngine;

namespace MoshiReRe.EditorTests.ItemPresentationSystem
{
    public sealed class ItemPresentationFlowTests
    {
        [Test]
        public void GetCandidates_ReturnsOnlyAcquiredDatabaseItemsInDatabaseOrder()
        {
            var database = ScriptableObject.CreateInstance<InventoryDatabase>();
            var first = ScriptableObject.CreateInstance<InventoryItem>();
            var second = ScriptableObject.CreateInstance<InventoryItem>();
            var missing = ScriptableObject.CreateInstance<InventoryItem>();
            first.id = "old_key";
            second.id = "cafe_ticket";
            missing.id = "not_acquired";
            database.items.Add(first);
            database.items.Add(second);
            database.items.Add(missing);

            try
            {
                InventoryDatabase.ClearAcquired();
                Assert.That(database.Acquire(second), Is.True);
                Assert.That(database.Acquire(first), Is.True);

                var candidates = ItemPresentationFlow.GetCandidates(database);
                Assert.That(candidates.Count, Is.EqualTo(2));
                Assert.That(candidates[0], Is.SameAs(first));
                Assert.That(candidates[1], Is.SameAs(second));
            }
            finally
            {
                InventoryDatabase.ClearAcquired();
                Object.DestroyImmediate(database);
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
                Object.DestroyImmediate(missing);
            }
        }

        [Test]
        public void ResolveResultCode_OnlyReturnsSuccessWhenItemAndConditionMatch()
        {
            var item = ScriptableObject.CreateInstance<InventoryItem>();
            item.id = "old_key";
            try
            {
                Assert.That(
                    ItemPresentationFlow.ResolveResultCode(
                        ItemPresentationOutcome.Presented(item), "old_key", true),
                    Is.EqualTo("old_key"));
                Assert.That(
                    ItemPresentationFlow.ResolveResultCode(
                        ItemPresentationOutcome.Presented(item), "old_key", false),
                    Is.EqualTo(ItemPresentationFlow.NormalResult));
                Assert.That(
                    ItemPresentationFlow.ResolveResultCode(
                        ItemPresentationOutcome.Presented(item), "cafe_ticket", true),
                    Is.EqualTo(ItemPresentationFlow.NormalResult));
            }
            finally
            {
                Object.DestroyImmediate(item);
            }
        }

        [Test]
        public void ResolveResultCode_DistinguishesCancelAndNoItems()
        {
            Assert.That(
                ItemPresentationFlow.ResolveResultCode(
                    ItemPresentationOutcome.Cancelled(), "old_key", true),
                Is.EqualTo(ItemPresentationFlow.CancelledResult));
            Assert.That(
                ItemPresentationFlow.ResolveResultCode(
                    ItemPresentationOutcome.NoItems(), "old_key", true),
                Is.EqualTo(ItemPresentationFlow.NoItemsResult));
        }

        [Test]
        public void PapaQuestDemo_UsesGuardedInventoryPresentationBranch()
        {
            var scenario = File.ReadAllText("Assets/Scenario/PapaQuestDemo.nani");
            StringAssert.Contains(
                "@presentInventory result:papaPresentation success:old_key requiredVariable:papaCafeKeyFound requiredValue:true",
                scenario);
            StringAssert.Contains("@if papaPresentation==\"old_key\"", scenario);
            StringAssert.Contains("@if papaPresentation==\"cancelled\"", scenario);
        }

        [TestCase("old_key", " OLD_KEY ", true)]
        [TestCase("old_key", "cafe_ticket", false)]
        [TestCase(null, "old_key", false)]
        public void ItemIdsMatch_IsCaseInsensitiveButRequiresBothIds(string left, string right, bool expected)
        {
            Assert.That(ItemPresentationFlow.ItemIdsMatch(left, right), Is.EqualTo(expected));
        }
    }
}
