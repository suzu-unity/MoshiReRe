using System.IO;
using System.Linq;
using MoshiReRe.Exploration;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
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

        [Test]
        public void PrototypeDialogue_UsesSharedNaninovelPrinterWithoutBlockingExplorationHud()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var npc = roots.Single(root => root.name == "PrototypeNPC");
                var dialogue = npc.GetComponent<NaninovelDialogueInteractable>();
                var serializedDialogue = new SerializedObject(dialogue);
                var prompt = roots.Single(root => root.name == "ExplorationHUD")
                    .transform.Find("InteractionPrompt")
                    .GetComponent<Image>();

                Assert.That(dialogue, Is.Not.Null);
                Assert.That(
                    serializedDialogue.FindProperty("textPrinterId").stringValue,
                    Is.EqualTo("Dialogue"));
                Assert.That(
                    serializedDialogue.FindProperty("textPrinterSortingOrder").intValue,
                    Is.GreaterThan(200));
                Assert.That(prompt.raycastTarget, Is.False);

                var script = File.ReadAllText("Assets/Scenario/ExplorationPrototypeNpc.nani");
                StringAssert.Contains("@printer Dialogue", script);
                StringAssert.Contains("@hidePrinter Dialogue wait!", script);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void PrototypePickup_HasPersistentInventoryAndSpriteReferences()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var pickupObject = scene.GetRootGameObjects().Single(root => root.name == "DummyItemPickup");
                var pickup = pickupObject.GetComponent<ExplorationItemPickup>();
                var serializedPickup = new SerializedObject(pickup);

                Assert.That(pickupObject.GetComponent<SpriteRenderer>().sprite, Is.Not.Null);
                Assert.That(serializedPickup.FindProperty("inventoryDatabase").objectReferenceValue, Is.Not.Null);
                Assert.That(serializedPickup.FindProperty("item").objectReferenceValue, Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void CompanyExplorationBackgrounds_AreFiveNamedSingleSprites()
        {
            var backgrounds = new[]
            {
                (Name: "morning", Width: 1916, Height: 821),
                (Name: "daytime", Width: 2172, Height: 724),
                (Name: "evening", Width: 2172, Height: 724),
                (Name: "night_lighton", Width: 2172, Height: 724),
                (Name: "night_lightoff", Width: 2172, Height: 724)
            };

            foreach (var background in backgrounds)
            {
                var path = $"Assets/Art/CompanyExploration/Backgrounds/{background.Name}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                Assert.That(importer, Is.Not.Null, path);
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), path);
                Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single), path);
                Assert.That(importer.mipmapEnabled, Is.False, path);
                Assert.That(sprite, Is.Not.Null, path);
                Assert.That(sprite.texture.width, Is.EqualTo(background.Width), path);
                Assert.That(sprite.texture.height, Is.EqualTo(background.Height), path);
            }
        }
    }
}
