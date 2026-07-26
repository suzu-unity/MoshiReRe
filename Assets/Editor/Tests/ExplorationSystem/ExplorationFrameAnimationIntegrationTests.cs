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
                var renderer = player.GetComponent<SpriteRenderer>();
                var animator = player.GetComponent<ExplorationSpriteAnimator>();
                var serializedAnimator = new SerializedObject(animator);
                var defaultFrames = serializedAnimator.FindProperty("defaultWalkFrames");

                Assert.That(renderer.enabled, Is.True);
                Assert.That(defaultFrames.arraySize, Is.EqualTo(12));
                Assert.That(serializedAnimator.FindProperty("framesPerSecond").floatValue, Is.EqualTo(12f));
                Assert.That(serializedAnimator.FindProperty("cutoutRig").objectReferenceValue, Is.Null);
                Assert.That(player.GetComponentsInChildren<SpriteSkin>(true), Is.Empty);

                var shadow = player.transform.Find("PlayerGroundShadow");
                Assert.That(shadow, Is.Not.Null);
                Assert.That(shadow.GetComponent<SpriteRenderer>().sortingOrder, Is.EqualTo(8));

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
    }
}
