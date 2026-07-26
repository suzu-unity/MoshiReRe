using System.Linq;
using MoshiReRe.Exploration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.SceneManagement;

namespace MoshiReRe.EditorTests.ExplorationSystem
{
    public sealed class ExplorationFrameAnimationIntegrationTests
    {
        private const string ScenePath = "Assets/Scenes/ExplorationPrototype.unity";

        [Test]
        public void PrototypePlayer_UsesTwelveFixedCanvasFramesWithoutRig()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var player = scene.GetRootGameObjects().Single(root => root.name == "Player");
                var visual = player.transform.Find("PlayerVisual");
                var renderer = visual != null ? visual.GetComponent<SpriteRenderer>() : null;
                var animator = player.GetComponent<ExplorationSpriteAnimator>();
                var serializedAnimator = new SerializedObject(animator);
                var defaultFrames = serializedAnimator.FindProperty("defaultWalkFrames");

                Assert.That(visual, Is.Not.Null);
                Assert.That(renderer, Is.Not.Null);
                Assert.That(renderer.enabled, Is.True);
                Assert.That(defaultFrames.arraySize, Is.EqualTo(12));
                Assert.That(serializedAnimator.FindProperty("framesPerSecond").floatValue, Is.EqualTo(12f));
                Assert.That(serializedAnimator.FindProperty("defaultIdleFrameIndex").intValue, Is.EqualTo(2));
                Assert.That(serializedAnimator.FindProperty("cutoutRig").objectReferenceValue, Is.Null);
                Assert.That(player.GetComponentsInChildren<SpriteSkin>(true), Is.Empty);

                var shadow = player.transform.Find("PlayerGroundShadow");
                Assert.That(shadow, Is.Not.Null);
                var shadowRenderer = shadow.GetComponent<SpriteRenderer>();
                Assert.That(shadowRenderer.sortingOrder, Is.EqualTo(8));
                Assert.That(shadowRenderer.sprite, Is.Not.Null);
                Assert.That(shadowRenderer.sprite.texture.width, Is.EqualTo(256));

                for (var index = 0; index < defaultFrames.arraySize; index++)
                {
                    var sprite = defaultFrames.GetArrayElementAtIndex(index).objectReferenceValue as Sprite;
                    Assert.That(sprite, Is.Not.Null);
                    Assert.That(sprite.rect.size, Is.EqualTo(new Vector2(480f, 624f)));
                    Assert.That(sprite.pivot.y, Is.EqualTo(0f).Within(0.001f));
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void PrototypeScene_UsesReplacementBackgroundAndNpcArtwork()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var background = roots.Single(root => root.name == "RoomBackground")
                    .GetComponent<SpriteRenderer>().sprite;
                var npc = roots.Single(root => root.name == "PrototypeNPC")
                    .GetComponent<SpriteRenderer>().sprite;

                Assert.That(background.texture.width, Is.EqualTo(2004));
                Assert.That(background.texture.height, Is.EqualTo(785));
                Assert.That(npc.texture.width, Is.EqualTo(896));
                Assert.That(npc.texture.height, Is.EqualTo(1152));
                Assert.That(npc.pivot.y, Is.EqualTo(0f).Within(0.001f));
                Assert.That(
                    AssetDatabase.GetAssetPath(npc),
                    Is.EqualTo("Assets/Art/ExplorationPrototype/npc_idle_source.png"));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
