using System.IO;
using System.Linq;
using MoshiReRe.Exploration;
using MoshiReRe.Exploration.State;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Naninovel.UI;

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
                StringAssert.Contains("@showPrinter Dialogue", script);
                StringAssert.Contains("@hidePrinter Dialogue wait!", script);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void DialoguePrinterPanel_ResolvesThePrefabRootCanvas()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NaninovelData/Resources/TextPrinters/Dialogue.prefab");
            var panel = prefab.GetComponentInChildren<UITextPrinterPanel>(true);

            Assert.That(prefab, Is.Not.Null);
            Assert.That(panel, Is.Not.Null);
            Assert.That(panel.GetComponentInParent<Canvas>(true), Is.Not.Null);
        }

        [Test]
        public void ConfigurePrinterCanvas_UsesOverlayCoordinatesAndSetsDialogueSorting()
        {
            var uiRoot = new GameObject("PersistentNaninovelUI");
            var canvasRoot = new GameObject("DialogueCanvas", typeof(RectTransform), typeof(Canvas));
            var panel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(CanvasGroup));
            canvasRoot.transform.SetParent(uiRoot.transform, false);
            panel.transform.SetParent(canvasRoot.transform, false);
            canvasRoot.transform.localScale = Vector3.zero;
            canvasRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
            uiRoot.SetActive(false);

            try
            {
                var canvas = NaninovelDialogueInteractable.ConfigurePrinterCanvas(
                    panel.GetComponent<CanvasGroup>(), 300);

                Assert.That(uiRoot.activeSelf, Is.False, "persistent UI root");
                Assert.That(canvasRoot.activeSelf, Is.True, "printer canvas root");
                Assert.That(panel.activeInHierarchy, Is.False, "printer panel hierarchy");
                Assert.That(canvas, Is.SameAs(canvasRoot.GetComponent<Canvas>()));
                Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.That(canvas.worldCamera, Is.Null);
                Assert.That(canvas.sortingOrder, Is.EqualTo(300));
                Assert.That(canvas.transform.localScale, Is.EqualTo(Vector3.one));
            }
            finally
            {
                Object.DestroyImmediate(uiRoot);
            }
        }

        [Test]
        public void ConfigureChoiceHandlerCanvas_UsesOverlayCoordinatesAboveDialogue()
        {
            var canvasRoot = new GameObject("ChoiceCanvas", typeof(RectTransform), typeof(Canvas));
            var panel = new GameObject("ChoicePanel", typeof(RectTransform), typeof(CanvasGroup));
            panel.transform.SetParent(canvasRoot.transform, false);
            canvasRoot.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;

            try
            {
                var canvas = NaninovelDialogueInteractable.ConfigureChoiceHandlerCanvas(
                    panel.GetComponent<CanvasGroup>(), 310);

                Assert.That(canvas.renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));
                Assert.That(canvas.worldCamera, Is.Null);
                Assert.That(canvas.sortingOrder, Is.EqualTo(310));
            }
            finally
            {
                Object.DestroyImmediate(canvasRoot);
            }
        }

        [Test]
        public void NormalizePrinterContentDepth_RemovesStaleCameraSpaceDepth()
        {
            var content = new GameObject("PrinterContent", typeof(RectTransform)).transform;
            content.localPosition = new Vector3(120f, -32f, -9720f);

            try
            {
                NaninovelDialogueInteractable.NormalizePrinterContentDepth(content);

                Assert.That(content.localPosition, Is.EqualTo(new Vector3(120f, -32f, 0f)));
            }
            finally
            {
                Object.DestroyImmediate(content.gameObject);
            }
        }

        [Test]
        public void CreateBackgroundSnapshot_CopiesBackdropPresentationToIndependentRenderer()
        {
            var sourceObject = new GameObject("RoomBackground");
            Material snapshotMaterial = null;
            Texture2D texture = null;
            Sprite sprite = null;
            GameObject snapshotObject = null;
            try
            {
                texture = new Texture2D(4, 4);
                sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), Vector2.one * 0.5f);
                var source = sourceObject.AddComponent<SpriteRenderer>();
                source.sprite = sprite;
                source.color = new Color(0.8f, 0.7f, 0.6f, 1f);
                source.flipX = true;
                source.sortingOrder = -20;

                var snapshot = NaninovelDialogueInteractable.CreateBackgroundSnapshot(
                    source,
                    out snapshotMaterial);
                snapshotObject = snapshot.gameObject;

                Assert.That(snapshot, Is.Not.SameAs(source));
                Assert.That(snapshot.transform.parent, Is.EqualTo(source.transform));
                Assert.That(snapshot.sprite, Is.SameAs(source.sprite));
                Assert.That(snapshot.color, Is.EqualTo(source.color));
                Assert.That(snapshot.flipX, Is.True);
                Assert.That(snapshot.sortingOrder, Is.EqualTo(source.sortingOrder));
                Assert.That(snapshot.sharedMaterial, Is.Not.Null);
            }
            finally
            {
                if (snapshotObject != null)
                    Object.DestroyImmediate(snapshotObject);
                if (snapshotMaterial != null)
                    Object.DestroyImmediate(snapshotMaterial);
                if (sprite != null)
                    Object.DestroyImmediate(sprite);
                if (texture != null)
                    Object.DestroyImmediate(texture);
                Object.DestroyImmediate(sourceObject);
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
        public void PrototypeScene_WiresEscapeMenuAndExplicitDialoguePrinters()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var guard = scene.GetRootGameObjects().Single(root => root.name == "ExplorationNaninovelUiGuard");
                var menuEsc = guard.GetComponent<MenuEsc>();
                var menuSerialized = new SerializedObject(menuEsc);

                Assert.That(menuEsc, Is.Not.Null);
                Assert.That(guard.GetComponent<ExplorationMenuInputBridge>(), Is.Not.Null);
                Assert.That(menuSerialized.FindProperty("externalInputBridge").boolValue, Is.True);
                Assert.That(menuSerialized.FindProperty("explorationPlayer").objectReferenceValue, Is.Not.Null);

                foreach (var scriptPath in new[]
                {
                    "Assets/Scenario/ExplorationPrototypeNpc.nani",
                    "Assets/Scenario/ExplorationDummyItem.nani",
                    "Assets/Scenario/ExplorationDoorDefault.nani",
                    "Assets/Scenario/ExplorationDoorWardrobe.nani"
                })
                    StringAssert.Contains("@showPrinter Dialogue", File.ReadAllText(scriptPath), scriptPath);

                var itemScript = File.ReadAllText("Assets/Scenario/ExplorationDummyItem.nani");
                StringAssert.Contains(
                    "@choice \"いいえ\" goto:.Leave\n@stop",
                    itemScript.Replace("\r\n", "\n"));

                var inputConfig = File.ReadAllText("Assets/NaninovelData/Resources/Naninovel/Configuration/InputConfiguration.asset");
                StringAssert.Contains("Keys: 0d0000000f0100004a01000020000000", inputConfig);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void LeftDoor_UsesConditionalReturnCommandThroughEnabledNovelHost()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var door = scene.GetRootGameObjects().Single(root => root.name == "LeftDoorInteraction");
                var dialogue = new SerializedObject(door.GetComponent<NaninovelDialogueInteractable>());
                var hostScene = EditorBuildSettings.scenes.SingleOrDefault(
                    candidate => candidate.path == "Assets/Scenes/CommonUIHub.unity");

                Assert.That(
                    dialogue.FindProperty("naninovelScriptPath").stringValue,
                    Is.EqualTo("Scenario/ExplorationDoorWardrobe"));
                var script = File.ReadAllText("Assets/Scenario/ExplorationDoorWardrobe.nani");
                StringAssert.Contains(
                    "@returnToNovel scene:CommonUIHub script:Scenario/scene02 label:OfficeAfterAdv",
                    script);
                Assert.That(hostScene, Is.Not.Null);
                Assert.That(hostScene.enabled, Is.True);
                Assert.That(
                    NaninovelDialogueInteractable.NovelHostSceneName,
                    Is.EqualTo("CommonUIHub"));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void PrototypeScene_HasStableMapAndObjectSaveIdentifiers()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                var roots = scene.GetRootGameObjects();
                var stateRoot = roots.Single(root => root.name == "ExplorationState");
                var map = stateRoot.GetComponent<ExplorationMapStateController>();
                var mapSerialized = new SerializedObject(map);

                Assert.That(mapSerialized.FindProperty("mapId").stringValue, Is.EqualTo("prototype-room"));
                Assert.That(stateRoot.GetComponentInChildren<ExplorationSpawnPoint>(true), Is.Not.Null);

                var expectedIds = new[] { "npc", "wardrobe", "old-key", "left-door" };
                var actualIds = roots
                    .Select(root => root.GetComponent<ExplorationStatefulObject>())
                    .Where(component => component != null)
                    .Select(component => new SerializedObject(component)
                        .FindProperty("objectId").stringValue)
                    .OrderBy(value => value)
                    .ToArray();

                Assert.That(actualIds, Is.EqualTo(expectedIds.OrderBy(value => value).ToArray()));
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
