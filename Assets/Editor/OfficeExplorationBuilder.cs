using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoshiReRe.Exploration;
using MoshiReRe.Exploration.State;
using Naninovel;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Creates the office exploration scene from the tested prototype scene.</summary>
public static class OfficeExplorationBuilder
{
    private const string PrototypeScenePath = "Assets/Scenes/ExplorationPrototype.unity";
    private const string OfficeScenePath = "Assets/Scenes/OfficeExploration.unity";
    private const string BackgroundRoot = "Assets/Art/ScenarioExploration/Backgrounds";
    private const string OfficeMapId = "office";
    private const string InventoryDatabasePath = "Assets/Database/Items/InventoryDatabase.asset";
    private const string DeskItemPath = "Assets/Database/Items/ItemData/Key.asset";
    private const string PlayerPortraitStripPath = "Assets/Art/ExplorationPrototype/player_casual_strip.png";
    private const string NpcPortraitStripPath = "Assets/Art/ExplorationPrototype/npc_strip.png";
    private const string PapaCafeBackgroundPath = BackgroundRoot + "/02_bakery_cafe.png";
    private const string CompanySeatedCgPath = "Assets/Art/ScenarioCG/PLACEHOLDER_REPLACE_ME_company_seated.png";
    private const string PapaCafeKeyCgPath = "Assets/Art/ScenarioCG/PLACEHOLDER_REPLACE_ME_papa_cafe_key.png";

    private static readonly string[] BackgroundPaths =
    {
        BackgroundRoot + "/11-1_office_morning.png",
        BackgroundRoot + "/11-2_office_daytime.png",
        BackgroundRoot + "/11-3_office_evening.png",
        BackgroundRoot + "/11-4_office_night&light.png",
        BackgroundRoot + "/11-5_office_night.png"
    };

    [MenuItem("Tools/MoshiReRe/Build Office Exploration")]
    public static void Build()
    {
        var backgrounds = BackgroundPaths.Select(ConfigureBackground).ToArray();
        var papaCafeBackground = ConfigureBackground(PapaCafeBackgroundPath);
        var inventoryDatabase = AssetDatabase.LoadAssetAtPath<InventoryDatabase>(InventoryDatabasePath);
        var deskItem = AssetDatabase.LoadAssetAtPath<InventoryItem>(DeskItemPath);
        if (backgrounds.Any(background => background == null) || papaCafeBackground == null)
            throw new InvalidOperationException("One or more office backgrounds could not be imported as sprites.");
        if (inventoryDatabase == null || deskItem == null)
            throw new InvalidOperationException("The office desk pickup inventory assets are missing.");

        if (!File.Exists(PrototypeScenePath))
            throw new InvalidOperationException($"Prototype scene not found: {PrototypeScenePath}");

        File.Copy(PrototypeScenePath, OfficeScenePath, true);
        AssetDatabase.ImportAsset(OfficeScenePath, ImportAssetOptions.ForceUpdate);
        var scene = EditorSceneManager.OpenScene(OfficeScenePath, OpenSceneMode.Single);

        var background = GameObject.Find("RoomBackground")?.GetComponent<SpriteRenderer>();
        var player = GameObject.Find("Player");
        var npc = GameObject.Find("PrototypeNPC");
        var stateRoot = GameObject.Find("ExplorationState");
        var overlay = GameObject.Find("DialogueOverlayController")?.GetComponent<ExplorationDialogueOverlay>();

        if (background == null || player == null || npc == null || stateRoot == null || overlay == null)
            throw new InvalidOperationException("The prototype scene is missing a required office object.");

        background.sprite = backgrounds[0];
        background.sortingOrder = -20;

        player.transform.position = new Vector3(-5.6f, -2.65f, 0f);
        npc.transform.position = new Vector3(8.25f, -2.65f, 0f);

        var map = stateRoot.GetComponent<ExplorationMapStateController>();
        SetString(map, "mapId", OfficeMapId);
        SetObject(map, "player", player.transform);
        SetObject(map, "spriteAnimator", player.GetComponent<ExplorationSpriteAnimator>());
        var spawn = GameObject.Find("DefaultSpawn");
        if (spawn != null)
            SetString(spawn.GetComponent<ExplorationSpawnPoint>(), "spawnId", "entrance");

        var officeController = stateRoot.GetComponent<OfficeExplorationController>()
            ?? stateRoot.AddComponent<OfficeExplorationController>();
        SetObject(officeController, "arrivingNpc", npc.transform);
        SetObject(officeController, "backgroundRenderer", background);
        SetObjectArray(officeController, "timeOfDayBackgrounds", backgrounds);
        SetObject(officeController, "papaCafeBackground", papaCafeBackground);
        SetInt(officeController, "initialBackgroundIndex", 0);
        SetFloat(officeController, "npcArrivalDelay", 2f);
        SetFloat(officeController, "npcSlideDuration", 1.2f);
        SetVector3(officeController, "npcStartPosition", npc.transform.position);
        SetVector3(officeController, "npcArrivalPosition", new Vector3(1.45f, -2.65f, 0f));

        foreach (var stateful in UnityEngine.Object.FindObjectsByType<ExplorationStatefulObject>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
            SetString(stateful, "mapId", OfficeMapId);

        // The prototype wardrobe and pickup are not part of the first office beat.
        GameObject.Find("WallSuitInteraction")?.SetActive(false);
        GameObject.Find("DummyItemPickup")?.SetActive(false);

        var door = GameObject.Find("LeftDoorInteraction");
        if (door != null)
            SetBool(door.GetComponent<NaninovelDialogueInteractable>(), "requireOutfit", false);

        ConfigureDialogueFeedback(overlay);
        var npcDialogue = npc.GetComponent<NaninovelDialogueInteractable>();
        SetString(npcDialogue, "protagonistPortraitVariant", "player_default");
        SetString(npcDialogue, "npcPortraitVariant", "npc_default");
        CreateDesks(overlay, inventoryDatabase, deskItem);
        ConfigureCamera(player.transform);
        RegisterScenarioResources();
        AddToBuildSettings(OfficeScenePath);

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, OfficeScenePath))
            throw new InvalidOperationException($"Failed to save office scene at '{OfficeScenePath}'.");

        AssetDatabase.SaveAssets();
        Debug.Log($"[OfficeExplorationBuilder] Built {OfficeScenePath}");
    }

    private static void ConfigureDialogueFeedback(ExplorationDialogueOverlay overlay)
    {
        if (overlay == null)
            return;
        var portraits = overlay.GetComponent<ExplorationDialoguePortraits>();
        if (portraits == null)
            portraits = overlay.gameObject.AddComponent<ExplorationDialoguePortraits>();
        if (overlay.GetComponent<ExplorationItemAcquisitionPopup>() == null)
            overlay.gameObject.AddComponent<ExplorationItemAcquisitionPopup>();

        SetPortraitVariants(portraits, new[]
        {
            ("player_default", LoadSpriteFrame(PlayerPortraitStripPath, "PlayerCasual_01")),
            ("player_alt", LoadSpriteFrame(PlayerPortraitStripPath, "PlayerCasual_02")),
            ("npc_default", LoadSpriteFrame(NpcPortraitStripPath, "Npc_01")),
            ("npc_alt", LoadSpriteFrame(NpcPortraitStripPath, "Npc_02"))
        });
    }

    private static void CreateDesks(
        ExplorationDialogueOverlay overlay,
        InventoryDatabase inventoryDatabase,
        InventoryItem deskItem)
    {
        var deskPositions = new[]
        {
            new Vector3(-5.55f, -2.65f, 0f),
            new Vector3(-2.55f, -2.65f, 0f),
            new Vector3(0.15f, -2.65f, 0f),
            new Vector3(2.85f, -2.65f, 0f),
            new Vector3(5.45f, -2.65f, 0f)
        };

        for (var i = 0; i < deskPositions.Length; i++)
        {
            var desk = new GameObject($"OfficeDesk_{i + 1:00}");
            desk.transform.position = deskPositions[i];

            var collider = desk.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.7f, 1.6f);

            var dialogue = i == 1
                ? desk.AddComponent<ExplorationItemPickup>()
                : desk.AddComponent<NaninovelDialogueInteractable>();
            SetString(dialogue, "promptText", i == 0 ? "机に座る" : "机を調べる");
            SetString(dialogue, "naninovelScriptPath", "Scenario/OfficeExploration");
            SetString(dialogue, "naninovelScriptLabel", i == 0 ? "Desk01" : i == 1 ? "Desk02" : "DeskGeneric");
            SetFloat(dialogue, "initializationTimeout", 2f);
            SetObject(dialogue, "fallbackOverlay", overlay);
            SetBool(dialogue, "showNpcPortrait", i == 0);
            SetString(dialogue, "protagonistPortraitVariant", "player_default");
            SetString(dialogue, "npcPortraitVariant", "npc_default");
            SetString(dialogue, "fallbackSpeaker", "Wanabi");
            SetStringArray(dialogue, "fallbackLines", new[]
            {
                i == 0 ? "机に座るか、もう少し歩き回るか選べそうだ。" : "特に変わったものはない。"
            });

            if (dialogue is ExplorationItemPickup pickup)
                pickup.Configure(inventoryDatabase, deskItem);

            var stateful = desk.AddComponent<ExplorationStatefulObject>();
            SetString(stateful, "objectId", $"office-desk-{i + 1:00}");
            SetString(stateful, "mapId", OfficeMapId);
        }
    }

    private static void ConfigureCamera(Transform player)
    {
        var camera = GameObject.Find("Main Camera");
        if (camera == null)
            return;

        camera.transform.position = new Vector3(-3.6f, 0f, -10f);
        var follow = camera.GetComponent<SideScrollCamera>();
        if (follow == null)
            return;

        SetObject(follow, "target", player);
        // The 2172px backgrounds are 21.72 world units wide at 100 PPU.
        // With the prototype camera's 3.15 orthographic size, these limits keep
        // the camera inside the artwork instead of revealing a clear-color strip.
        SetFloat(follow, "minX", -5.25f);
        SetFloat(follow, "maxX", 5.25f);
    }

    private static Sprite ConfigureBackground(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            return null;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite LoadSpriteFrame(string path, string spriteName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .FirstOrDefault(sprite => sprite.name == spriteName);
    }

    private static void SetPortraitVariants(
        ExplorationDialoguePortraits target,
        IReadOnlyList<(string id, Sprite sprite)> values)
    {
        if (target == null)
            return;
        var so = new SerializedObject(target);
        var property = so.FindProperty("variants");
        property.arraySize = values.Count;
        for (var i = 0; i < values.Count; i++)
        {
            var element = property.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("id").stringValue = values[i].id;
            element.FindPropertyRelative("sprite").objectReferenceValue = values[i].sprite;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddToBuildSettings(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        var index = scenes.FindIndex(scene => scene.path == scenePath);
        if (index >= 0)
            scenes[index] = new EditorBuildSettingsScene(scenePath, true);
        else
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void RegisterScenarioResources()
    {
        var resources = EditorResources.LoadOrDefault();

        RegisterResource(resources, "Scripts", "Scenario/OfficeExploration", "Assets/Scenario/OfficeExploration.nani");
        RegisterResource(resources, "Scripts", "Scenario/PapaCafeExploration", "Assets/Scenario/PapaCafeExploration.nani");
        RegisterResource(resources, "Scripts", "Scenario/PapaQuestDemo", "Assets/Scenario/PapaQuestDemo.nani");
        RegisterResource(resources, "Backgrounds/MainBackground", "ScenarioExploration/Backgrounds/02_bakery_cafe", PapaCafeBackgroundPath);
        RegisterResource(resources, "Backgrounds/MainBackground", "ScenarioCG/company_seated_demo", CompanySeatedCgPath);
        RegisterResource(resources, "Backgrounds/MainBackground", "ScenarioCG/papa_cafe_key_demo", PapaCafeKeyCgPath);

        resources.RemoveAllRecordsWithPath("Scripts", "Scenario/OfficeDeskLeft");
        resources.RemoveAllRecordsWithPath("Scripts", "Scenario/OfficeDeskGeneric");
        EditorUtility.SetDirty(resources);
        AssetDatabase.SaveAssets();
    }

    private static void RegisterResource(EditorResources resources, string pathPrefix, string resourcePath, string assetPath)
    {
        var guid = AssetDatabase.AssetPathToGUID(assetPath);
        if (string.IsNullOrWhiteSpace(guid))
            throw new InvalidOperationException($"Scenario resource was not imported: {assetPath}");

        resources.RemoveAllRecordsWithPath(pathPrefix, resourcePath);
        resources.AddRecord(pathPrefix, pathPrefix, resourcePath, guid);
    }

    private static void SetObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value)
    {
        if (target == null)
            return;
        var so = new SerializedObject(target);
        so.FindProperty(propertyName).objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetObjectArray(UnityEngine.Object target, string propertyName, IReadOnlyList<UnityEngine.Object> values)
    {
        if (target == null)
            return;
        var so = new SerializedObject(target);
        var property = so.FindProperty(propertyName);
        property.arraySize = values.Count;
        for (var i = 0; i < values.Count; i++)
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(UnityEngine.Object target, string propertyName, string value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(propertyName).stringValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetStringArray(UnityEngine.Object target, string propertyName, IReadOnlyList<string> values)
    {
        var so = new SerializedObject(target);
        var property = so.FindProperty(propertyName);
        property.arraySize = values.Count;
        for (var i = 0; i < values.Count; i++)
            property.GetArrayElementAtIndex(i).stringValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetBool(UnityEngine.Object target, string propertyName, bool value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(propertyName).boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetInt(UnityEngine.Object target, string propertyName, int value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(propertyName).intValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetFloat(UnityEngine.Object target, string propertyName, float value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(propertyName).floatValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetVector3(UnityEngine.Object target, string propertyName, Vector3 value)
    {
        var so = new SerializedObject(target);
        so.FindProperty(propertyName).vector3Value = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
