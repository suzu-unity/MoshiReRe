using NUnit.Framework;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace MoshiReRe.EditorTests.ExplorationSystem
{
    public sealed class ExplorationSkinnedSpriteConfiguratorTests
    {
        [Test]
        public void RigProfiles_KeepAnIdenticalBoneOrderAndHierarchy()
        {
            var defaultBones = ExplorationSkinnedSpriteConfigurator.CreateRigDefinition(
                ExplorationSkinnedSpriteConfigurator.RigProfile.Default,
                ExplorationSkinnedSpriteConfigurator.ReferenceWidth,
                ExplorationSkinnedSpriteConfigurator.ReferenceHeight);
            var suitBones = ExplorationSkinnedSpriteConfigurator.CreateRigDefinition(
                ExplorationSkinnedSpriteConfigurator.RigProfile.Suit,
                ExplorationSkinnedSpriteConfigurator.ReferenceWidth,
                ExplorationSkinnedSpriteConfigurator.ReferenceHeight);

            Assert.That(defaultBones, Has.Length.EqualTo(ExplorationSkinnedSpriteConfigurator.BoneCount));
            Assert.That(suitBones, Has.Length.EqualTo(defaultBones.Length));
            for (var i = 0; i < defaultBones.Length; i++)
            {
                Assert.That(suitBones[i].Name, Is.EqualTo(defaultBones[i].Name));
                Assert.That(suitBones[i].ParentIndex, Is.EqualTo(defaultBones[i].ParentIndex));
            }
        }

        [Test]
        public void GeneratedMesh_HasSafeNormalizedWeightsAndValidIndices()
        {
            var mesh = ExplorationSkinnedSpriteConfigurator.CreateMeshData(
                ExplorationSkinnedSpriteConfigurator.RigProfile.Default,
                ExplorationSkinnedSpriteConfigurator.ReferenceWidth,
                ExplorationSkinnedSpriteConfigurator.ReferenceHeight);

            foreach (var vertex in mesh.Vertices)
            {
                Assert.That(float.IsFinite(vertex.position.x), Is.True);
                Assert.That(float.IsFinite(vertex.position.y), Is.True);
                Assert.That(ExplorationSkinnedSpriteConfigurator.HasValidBoneWeight(vertex.boneWeight), Is.True);
                Assert.That(vertex.boneWeight.boneIndex0, Is.InRange(0, ExplorationSkinnedSpriteConfigurator.BoneCount - 1));
                Assert.That(vertex.boneWeight.boneIndex1, Is.InRange(0, ExplorationSkinnedSpriteConfigurator.BoneCount - 1));
            }

            foreach (var index in mesh.Indices)
                Assert.That(index, Is.InRange(0, mesh.Vertices.Length - 1));
        }

        [Test]
        public void InfluenceRegions_KeepLeftAndRightLimbsSeparated()
        {
            var bounds = ExplorationSkinnedSpriteConfigurator.GetAlphaBounds(
                ExplorationSkinnedSpriteConfigurator.RigProfile.Default);
            Vector2 Point(float normalizedX, float normalizedY) =>
                bounds.Min + Vector2.Scale(bounds.Size, new Vector2(normalizedX, normalizedY));

            Assert.That(ExplorationSkinnedSpriteConfigurator.GetInfluenceRegion(
                ExplorationSkinnedSpriteConfigurator.RigProfile.Default, Point(0.15f, 0.65f), 1024, 1536),
                Is.EqualTo(ExplorationSkinnedSpriteConfigurator.InfluenceRegion.LeftArm));
            Assert.That(ExplorationSkinnedSpriteConfigurator.GetInfluenceRegion(
                ExplorationSkinnedSpriteConfigurator.RigProfile.Default, Point(0.85f, 0.65f), 1024, 1536),
                Is.EqualTo(ExplorationSkinnedSpriteConfigurator.InfluenceRegion.RightArm));
            Assert.That(ExplorationSkinnedSpriteConfigurator.GetInfluenceRegion(
                ExplorationSkinnedSpriteConfigurator.RigProfile.Default, Point(0.35f, 0.18f), 1024, 1536),
                Is.EqualTo(ExplorationSkinnedSpriteConfigurator.InfluenceRegion.LeftLeg));
            Assert.That(ExplorationSkinnedSpriteConfigurator.GetInfluenceRegion(
                ExplorationSkinnedSpriteConfigurator.RigProfile.Default, Point(0.65f, 0.18f), 1024, 1536),
                Is.EqualTo(ExplorationSkinnedSpriteConfigurator.InfluenceRegion.RightLeg));
        }
    }
}
