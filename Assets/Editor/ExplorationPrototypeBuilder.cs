using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoshiReRe.Exploration;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ExplorationPrototypeBuilder
{
    private const string ArtRoot = "Assets/Art/ExplorationPrototype";
    private const string GeneratedRoot = ArtRoot + "/Generated";
    private const string RigRoot = ArtRoot + "/Rig";
    private const string CasualRigPartsRoot = RigRoot + "/CasualParts";
    private const string SuitRigPartsRoot = RigRoot + "/SuitParts";
    private const string ScenePath = "Assets/Scenes/ExplorationPrototype.unity";
    private const string RestoreStartSceneKey = "MoshiReRe.ExplorationPrototype.RestoreStartScene";
    private const string PreviousStartSceneKey = "MoshiReRe.ExplorationPrototype.PreviousStartScene";

    private static readonly Slice[] CasualSlices =
    {
        new("PlayerCasualWalk_01", 0, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerCasualWalk_02", 222, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerCasualWalk_03", 444, 0, 221, 887, 0.5f, 0.206f),
        new("PlayerCasualWalk_04", 665, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerCasualWalk_05", 887, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerCasualWalk_06", 1109, 0, 221, 887, 0.5f, 0.206f),
        new("PlayerCasualWalk_07", 1330, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerCasualWalk_08", 1552, 0, 222, 887, 0.5f, 0.206f)
    };

    private static readonly Slice[] SuitSlices =
    {
        new("PlayerSuitWalk_01", 0, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerSuitWalk_02", 222, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerSuitWalk_03", 444, 0, 221, 887, 0.5f, 0.206f),
        new("PlayerSuitWalk_04", 665, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerSuitWalk_05", 887, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerSuitWalk_06", 1109, 0, 221, 887, 0.5f, 0.206f),
        new("PlayerSuitWalk_07", 1330, 0, 222, 887, 0.5f, 0.206f),
        new("PlayerSuitWalk_08", 1552, 0, 222, 887, 0.5f, 0.206f)
    };

    private static readonly Slice[] NpcSlices =
    {
        new("Npc_01", 77, 159, 185, 694),
        new("Npc_02", 392, 150, 235, 692),
        new("Npc_03", 684, 139, 271, 697),
        new("Npc_04", 1019, 142, 242, 691),
        new("Npc_05", 1341, 137, 276, 687)
    };

    [MenuItem("Tools/MoshiReRe/Build Exploration Prototype")]
    public static void Build()
    {
        EnsureFolder(GeneratedRoot);

        var background = ConfigureSingleSprite(ArtRoot + "/exploration_room_background.png", 100f);
        var casualPath = ArtRoot + "/player_casual_walk_v2.png";
        var suitPath = ArtRoot + "/player_suit_walk_v2.png";
        var npcPath = GenerateTransparentCopy(
            ArtRoot + "/npc_strip.png", GeneratedRoot + "/npc_transparent.png");
        var casualFrames = ConfigureSpriteStrip(casualPath, CasualSlices, 110f);
        var suitFrames = ConfigureSpriteStrip(suitPath, SuitSlices, 110f);
        var npcFrames = ConfigureSpriteStrip(npcPath, NpcSlices, 150f);
        var casualRigSprites = LoadRigSpriteSet(CasualRigPartsRoot);
        var suitRigSprites = LoadRigSpriteSet(SuitRigPartsRoot);

        if (background == null || casualFrames.Length != 8 || suitFrames.Length != 8 ||
            npcFrames.Length != 5 || !casualRigSprites.IsComplete || !suitRigSprites.IsComplete)
            throw new InvalidOperationException("Exploration prototype sprites were not imported as expected.");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "ExplorationPrototype";

        CreateBackground(background);
        var player = CreatePlayer(casualFrames, suitFrames, casualRigSprites, suitRigSprites);
        var dialogueOverlay = CreateHud(player.GetComponent<ExplorationInteractionController>());
        CreateNpc(npcFrames[0], dialogueOverlay);
        CreateWardrobe(player.GetComponent<ExplorationSpriteAnimator>());
        CreateCamera(player.transform);
        CreateLight();
        CreateNaninovelUiGuard();

        EditorSceneManager.MarkSceneDirty(scene);
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
            throw new InvalidOperationException($"Failed to save prototype scene at '{ScenePath}'.");

        Selection.activeGameObject = player;
        AssetDatabase.SaveAssets();
        Debug.Log($"[ExplorationPrototypeBuilder] Built {ScenePath}");
    }

    [InitializeOnLoadMethod]
    private static void RegisterPlayModeRestore()
    {
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    [MenuItem("Tools/MoshiReRe/Play Exploration Prototype")]
    public static void PlayPrototype()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        var prototype = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (prototype == null)
            throw new InvalidOperationException($"Build the prototype scene first: {ScenePath}");

        var previous = EditorSceneManager.playModeStartScene;
        SessionState.SetString(PreviousStartSceneKey, previous == null ? string.Empty : AssetDatabase.GetAssetPath(previous));
        SessionState.SetBool(RestoreStartSceneKey, true);
        EditorSceneManager.playModeStartScene = prototype;
        EditorApplication.isPlaying = true;
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(RestoreStartSceneKey, false))
            return;

        var previousPath = SessionState.GetString(PreviousStartSceneKey, string.Empty);
        EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previousPath)
            ? null
            : AssetDatabase.LoadAssetAtPath<SceneAsset>(previousPath);
        SessionState.EraseBool(RestoreStartSceneKey);
        SessionState.EraseString(PreviousStartSceneKey);
    }

    private static void CreateBackground(Sprite sprite)
    {
        var root = new GameObject("RoomBackground");
        var renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = -20;
    }

    private static GameObject CreatePlayer(
        Sprite[] casualFrames,
        Sprite[] suitFrames,
        RigSpriteSet casualRigSprites,
        RigSpriteSet suitRigSprites)
    {
        var player = new GameObject("Player");
        player.transform.position = new Vector3(-5.7f, -2.82f, 0f);

        var renderer = player.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = 10;
        renderer.enabled = false;

        var cutoutRig = CreateCutoutRig(player.transform, casualRigSprites, suitRigSprites);

        var animator = player.AddComponent<ExplorationSpriteAnimator>();
        SetObject(animator, "spriteRenderer", renderer);
        SetFloat(animator, "framesPerSecond", 9f);
        SetObjectArray(animator, "defaultWalkFrames", casualFrames);
        SetObjectArray(animator, "wardrobeWalkFrames", suitFrames);
        SetObject(animator, "defaultIdleSprite", casualFrames[0]);
        SetObject(animator, "wardrobeIdleSprite", suitFrames[0]);
        SetObject(animator, "cutoutRig", cutoutRig);

        var controller = player.AddComponent<ExplorationPlayerController>();
        SetFloat(controller, "movementSpeed", 3.4f);
        SetBool(controller, "clampHorizontalPosition", true);
        SetFloat(controller, "minX", -8.1f);
        SetFloat(controller, "maxX", 8.1f);
        SetObject(controller, "spriteAnimator", animator);

        var interactions = player.AddComponent<ExplorationInteractionController>();
        SetObject(interactions, "player", controller);
        SetFloat(interactions, "interactionRadius", 1.55f);
        return player;
    }

    private static ExplorationCutoutRigController CreateCutoutRig(
        Transform player,
        RigSpriteSet casual,
        RigSpriteSet suit)
    {
        var rigRoot = new GameObject("CutoutRig").transform;
        rigRoot.SetParent(player, false);
        rigRoot.localScale = new Vector3(0.42f, 0.42f, 1f);

        var torso = CreateRigBone("Torso", rigRoot, new Vector3(0f, 5.75f, 0f), casual.Torso, 10);
        var backHair = CreateRigBone("BackHair", torso, new Vector3(0f, 3.08f, 0f), casual.BackHair, 6);
        var head = CreateRigBone("Head", torso, new Vector3(0.02f, 3.08f, 0f), casual.Head, 12);

        var leftUpperArm = CreateRigBone(
            "BackUpperArm", torso, new Vector3(-0.38f, 2.58f, 0f), casual.UpperArm, 7);
        leftUpperArm.localScale = new Vector3(0.72f, 0.72f, 1f);
        var leftForearm = CreateRigBone(
            "BackForearm", leftUpperArm, new Vector3(0f, -2.52f, 0f), casual.Forearm, 7);
        var rightUpperArm = CreateRigBone(
            "FrontUpperArm", torso, new Vector3(0.52f, 2.58f, 0f), casual.UpperArm, 15);
        rightUpperArm.localScale = new Vector3(0.72f, 0.72f, 1f);
        var rightForearm = CreateRigBone(
            "FrontForearm", rightUpperArm, new Vector3(0f, -2.52f, 0f), casual.Forearm, 15);

        var leftThigh = CreateRigBone(
            "BackThigh", torso, new Vector3(-0.22f, 0.18f, 0f), casual.Thigh, 5);
        var leftCalf = CreateRigBone(
            "BackCalf", leftThigh, new Vector3(0f, -2.55f, 0f), casual.Calf, 5);
        var leftFoot = CreateRigBone(
            "BackFoot", leftCalf, new Vector3(0f, -2.4f, 0f), casual.Foot, 5);

        var rightThigh = CreateRigBone(
            "FrontThigh", torso, new Vector3(0.22f, 0.18f, 0f), casual.Thigh, 13);
        var rightCalf = CreateRigBone(
            "FrontCalf", rightThigh, new Vector3(0f, -2.55f, 0f), casual.Calf, 13);
        var rightFoot = CreateRigBone(
            "FrontFoot", rightCalf, new Vector3(0f, -2.4f, 0f), casual.Foot, 13);

        var controller = rigRoot.gameObject.AddComponent<ExplorationCutoutRigController>();
        SetObject(controller, "torso", torso);
        SetObject(controller, "head", head);
        SetObject(controller, "backHair", backHair);
        SetObject(controller, "leftUpperArm", leftUpperArm);
        SetObject(controller, "rightUpperArm", rightUpperArm);
        SetObject(controller, "leftForearm", leftForearm);
        SetObject(controller, "rightForearm", rightForearm);
        SetObject(controller, "leftThigh", leftThigh);
        SetObject(controller, "rightThigh", rightThigh);
        SetObject(controller, "leftCalf", leftCalf);
        SetObject(controller, "rightCalf", rightCalf);
        SetObject(controller, "leftFoot", leftFoot);
        SetObject(controller, "rightFoot", rightFoot);
        SetObject(controller, "mirrorRoot", rigRoot);

        SetObject(controller, "torsoRenderer", torso.GetComponent<SpriteRenderer>());
        SetObject(controller, "headRenderer", head.GetComponent<SpriteRenderer>());
        SetObject(controller, "backHairRenderer", backHair.GetComponent<SpriteRenderer>());
        SetObject(controller, "leftUpperArmRenderer", leftUpperArm.GetComponent<SpriteRenderer>());
        SetObject(controller, "rightUpperArmRenderer", rightUpperArm.GetComponent<SpriteRenderer>());
        SetObject(controller, "leftForearmRenderer", leftForearm.GetComponent<SpriteRenderer>());
        SetObject(controller, "rightForearmRenderer", rightForearm.GetComponent<SpriteRenderer>());
        SetObject(controller, "leftThighRenderer", leftThigh.GetComponent<SpriteRenderer>());
        SetObject(controller, "rightThighRenderer", rightThigh.GetComponent<SpriteRenderer>());
        SetObject(controller, "leftCalfRenderer", leftCalf.GetComponent<SpriteRenderer>());
        SetObject(controller, "rightCalfRenderer", rightCalf.GetComponent<SpriteRenderer>());
        SetObject(controller, "leftFootRenderer", leftFoot.GetComponent<SpriteRenderer>());
        SetObject(controller, "rightFootRenderer", rightFoot.GetComponent<SpriteRenderer>());
        SetFloat(controller, "walkPosesPerSecond", 12f);
        SetBool(controller, "showBackLimbsInBlack", false);
        SetEnum(controller, "backLimbSide", (int)ExplorationCutoutRigSide.Left);
        SetRigOutfit(controller, "defaultOutfit", casual);
        SetRigOutfit(controller, "wardrobeOutfit", suit);
        controller.CaptureRestPose();
        return controller;
    }

    private static Transform CreateRigBone(
        string name,
        Transform parent,
        Vector3 localPosition,
        Sprite sprite,
        int sortingOrder)
    {
        var bone = new GameObject(name).transform;
        bone.SetParent(parent, false);
        bone.localPosition = localPosition;
        var renderer = bone.gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
        return bone;
    }

    private static void CreateNpc(Sprite idleSprite, ExplorationDialogueOverlay fallbackOverlay)
    {
        var npc = new GameObject("PrototypeNPC");
        npc.transform.position = new Vector3(3.7f, -2.82f, 0f);

        var renderer = npc.AddComponent<SpriteRenderer>();
        renderer.sprite = idleSprite;
        renderer.flipX = true;
        renderer.sortingOrder = 9;

        var collider = npc.AddComponent<CapsuleCollider2D>();
        collider.isTrigger = true;
        collider.direction = CapsuleDirection2D.Vertical;
        collider.size = new Vector2(1.15f, 4.55f);
        collider.offset = new Vector2(0f, 2.25f);

        var dialogue = npc.AddComponent<NaninovelDialogueInteractable>();
        SetString(dialogue, "promptText", "話す");
        SetString(dialogue, "naninovelScriptPath", "Scenario/ExplorationPrototypeNpc");
        SetFloat(dialogue, "initializationTimeout", 2f);
        SetObject(dialogue, "fallbackOverlay", fallbackOverlay);
        SetString(dialogue, "fallbackSpeaker", "仮置きのNPC");
        SetStringArray(dialogue, "fallbackLines", new[]
        {
            "こんばんは。部屋の中を歩いて、私に話しかけられたみたいね。",
            "壁のスーツを調べると、主人公の服装も変えられるはずよ。",
            "これは探索ADV用のダミー会話です。"
        });
    }

    private static void CreateWardrobe(ExplorationSpriteAnimator playerAnimator)
    {
        var wardrobe = new GameObject("WallSuitInteraction");
        wardrobe.transform.position = new Vector3(7.05f, -2.82f, 0f);

        var collider = wardrobe.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.45f, 2.1f);
        collider.offset = new Vector2(0f, 1.05f);

        var outfit = wardrobe.AddComponent<OutfitInteractable>();
        SetString(outfit, "promptText", "調べる：壁のスーツ");
        SetObject(outfit, "targetAnimator", playerAnimator);
        SetEnum(outfit, "outfit", (int)ExplorationOutfit.Wardrobe);
    }

    private static void CreateCamera(Transform target)
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(-2.15f, 0f, -10f);

        var camera = cameraObject.AddComponent<Camera>();
        camera.depth = 0.5f;
        camera.orthographic = true;
        camera.orthographicSize = 3.15f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(9, 18, 35, 255);
        cameraObject.AddComponent<UniversalAdditionalCameraData>();
        cameraObject.AddComponent<AudioListener>();

        var follow = cameraObject.AddComponent<SideScrollCamera>();
        SetObject(follow, "target", target);
        SetFloat(follow, "smoothTime", 0.16f);
        SetFloat(follow, "horizontalDeadZone", 0.8f);
        SetBool(follow, "clampHorizontalPosition", true);
        SetFloat(follow, "minX", -3.95f);
        SetFloat(follow, "maxX", 3.95f);
    }

    private static void CreateLight()
    {
        var lightObject = new GameObject("Directional Light");
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
    }

    private static void CreateNaninovelUiGuard()
    {
        var guardObject = new GameObject("ExplorationNaninovelUiGuard");
        guardObject.AddComponent<ExplorationNaninovelUiGuard>();
    }

    private static ExplorationDialogueOverlay CreateHud(ExplorationInteractionController interactionController)
    {
        var canvasObject = new GameObject("ExplorationHUD");
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Font/PixelMplus12-Regular SDF.asset");

        var instructions = CreateText("Instructions", canvasObject.transform, font, 27f);
        instructions.text = "A / D・← →：移動　　E / Space：話す・調べる";
        instructions.alignment = TextAlignmentOptions.Left;
        instructions.color = new Color32(239, 242, 248, 255);
        SetRect(instructions.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(38f, -35f), new Vector2(850f, 56f), new Vector2(0f, 1f));

        var panelObject = new GameObject("InteractionPrompt", typeof(RectTransform), typeof(Image));
        panelObject.transform.SetParent(canvasObject.transform, false);
        var panelRect = panelObject.GetComponent<RectTransform>();
        SetRect(panelRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 48f), new Vector2(620f, 82f), new Vector2(0.5f, 0f));
        var panelImage = panelObject.GetComponent<Image>();
        panelImage.color = new Color32(8, 18, 36, 225);

        var promptText = CreateText("PromptText", panelObject.transform, font, 32f);
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = Color.white;
        SetStretch(promptText.rectTransform, new Vector2(22f, 10f), new Vector2(-22f, -10f));

        var promptControllerObject = new GameObject("InteractionPromptController");
        promptControllerObject.transform.SetParent(canvasObject.transform, false);
        var prompt = promptControllerObject.AddComponent<InteractionPromptUI>();
        SetObject(prompt, "interactionController", interactionController);
        SetObject(prompt, "promptRoot", panelObject);
        SetObject(prompt, "promptText", promptText);
        SetString(prompt, "promptFormat", "E：{0}");

        panelObject.SetActive(false);

        var dialoguePanel = new GameObject("DialoguePanel", typeof(RectTransform), typeof(Image));
        dialoguePanel.transform.SetParent(canvasObject.transform, false);
        var dialogueRect = dialoguePanel.GetComponent<RectTransform>();
        SetRect(dialogueRect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(0f, 26f), new Vector2(1510f, 245f), new Vector2(0.5f, 0f));
        dialoguePanel.GetComponent<Image>().color = new Color32(8, 18, 36, 242);

        var speakerText = CreateText("Speaker", dialoguePanel.transform, font, 29f);
        speakerText.alignment = TextAlignmentOptions.Left;
        speakerText.color = new Color32(116, 211, 224, 255);
        SetRect(speakerText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(34f, -24f), new Vector2(900f, 44f), new Vector2(0f, 1f));

        var bodyText = CreateText("Body", dialoguePanel.transform, font, 31f);
        bodyText.alignment = TextAlignmentOptions.TopLeft;
        bodyText.color = Color.white;
        bodyText.textWrappingMode = TextWrappingModes.Normal;
        SetRect(bodyText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(34f, -78f), new Vector2(1420f, 130f), new Vector2(0f, 1f));

        var continueText = CreateText("ContinueHint", dialoguePanel.transform, font, 22f);
        continueText.text = "E / Space：次へ";
        continueText.alignment = TextAlignmentOptions.BottomRight;
        continueText.color = new Color32(190, 202, 218, 255);
        SetRect(continueText.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-28f, 18f), new Vector2(320f, 38f), new Vector2(1f, 0f));

        var overlayController = new GameObject("DialogueOverlayController");
        overlayController.transform.SetParent(canvasObject.transform, false);
        var overlay = overlayController.AddComponent<ExplorationDialogueOverlay>();
        SetObject(overlay, "panelRoot", dialoguePanel);
        SetObject(overlay, "speakerText", speakerText);
        SetObject(overlay, "bodyText", bodyText);
        SetString(overlay, "defaultSpeaker", "仮置きのNPC");
        dialoguePanel.SetActive(false);
        return overlay;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, TMP_FontAsset font, float size)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        var text = textObject.GetComponent<TextMeshProUGUI>();
        text.font = font;
        text.fontSize = size;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static Sprite ConfigureSingleSprite(string path, float pixelsPerUnit)
    {
        var importer = GetTextureImporter(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static RigSpriteSet LoadRigSpriteSet(string root)
    {
        const float pixelsPerUnit = 100f;
        return new RigSpriteSet(
            ConfigureRigSprite(root + "/torso.png", pixelsPerUnit, new Vector2(0.5f, 0.05f)),
            ConfigureRigSprite(root + "/head.png", pixelsPerUnit, new Vector2(0.5f, 0.05f)),
            ConfigureRigSprite(root + "/back_hair.png", pixelsPerUnit, new Vector2(0.5f, 0.28f)),
            ConfigureRigSprite(root + "/upper_arm.png", pixelsPerUnit, new Vector2(0.5f, 0.92f)),
            ConfigureRigSprite(root + "/forearm.png", pixelsPerUnit, new Vector2(0.5f, 0.92f)),
            ConfigureRigSprite(root + "/thigh.png", pixelsPerUnit, new Vector2(0.5f, 0.93f)),
            ConfigureRigSprite(root + "/calf.png", pixelsPerUnit, new Vector2(0.5f, 0.94f)),
            ConfigureRigSprite(root + "/foot.png", pixelsPerUnit, new Vector2(0.3f, 0.72f)));
    }

    private static Sprite ConfigureRigSprite(string path, float pixelsPerUnit, Vector2 pivot)
    {
        var importer = GetTextureImporter(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 1024;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot = pivot;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite[] ConfigureSpriteStrip(string path, IReadOnlyList<Slice> slices, float pixelsPerUnit)
    {
        var importer = GetTextureImporter(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();

        var factories = new SpriteDataProviderFactories();
        factories.Init();
        var provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();
        var previousIds = provider.GetSpriteRects()
            .Where(rect => !string.IsNullOrEmpty(rect.name))
            .ToDictionary(rect => rect.name, rect => rect.spriteID);

        var spriteRects = new SpriteRect[slices.Count];
        for (var i = 0; i < slices.Count; i++)
        {
            var slice = slices[i];
            spriteRects[i] = new SpriteRect
            {
                name = slice.Name,
                rect = new Rect(slice.X, slice.Y, slice.Width, slice.Height),
                alignment = SpriteAlignment.Custom,
                pivot = new Vector2(slice.PivotX, slice.PivotY),
                spriteID = previousIds.TryGetValue(slice.Name, out var existingId) ? existingId : GUID.Generate()
            };
        }

        provider.SetSpriteRects(spriteRects);
        var nameProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameProvider?.SetNameFileIdPairs(spriteRects.Select(rect => new SpriteNameFileIdPair(rect.name, rect.spriteID)));
        provider.Apply();
        importer.SaveAndReimport();

        var byName = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
            .ToDictionary(sprite => sprite.name, sprite => sprite);
        return slices.Select(slice => byName.TryGetValue(slice.Name, out var sprite) ? sprite : null).ToArray();
    }

    private static string GenerateTransparentCopy(string sourcePath, string destinationPath)
    {
        var importer = GetTextureImporter(sourcePath);
        importer.isReadable = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();

        var source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
        if (source == null)
            throw new InvalidOperationException($"Source texture was not found at '{sourcePath}'.");

        var width = source.width;
        var height = source.height;
        var pixels = source.GetPixels32();
        var background = new bool[pixels.Length];
        var queue = new Queue<int>(width * 2 + height * 2);

        void TryEnqueue(int index)
        {
            if (background[index] || !IsConnectedBackgroundColor(pixels[index]))
                return;
            background[index] = true;
            queue.Enqueue(index);
        }

        for (var x = 0; x < width; x++)
        {
            TryEnqueue(x);
            TryEnqueue((height - 1) * width + x);
        }
        for (var y = 0; y < height; y++)
        {
            TryEnqueue(y * width);
            TryEnqueue(y * width + width - 1);
        }

        while (queue.Count > 0)
        {
            var index = queue.Dequeue();
            var x = index % width;
            var y = index / width;
            if (x > 0) TryEnqueue(index - 1);
            if (x + 1 < width) TryEnqueue(index + 1);
            if (y > 0) TryEnqueue(index - width);
            if (y + 1 < height) TryEnqueue(index + width);
        }

        for (var i = 0; i < pixels.Length; i++)
            if (background[i])
                pixels[i].a = 0;

        var output = new Texture2D(width, height, TextureFormat.RGBA32, false);
        output.SetPixels32(pixels);
        output.Apply(false, false);
        File.WriteAllBytes(Path.GetFullPath(destinationPath), output.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(output);
        AssetDatabase.ImportAsset(destinationPath, ImportAssetOptions.ForceSynchronousImport);
        return destinationPath;
    }

    private static bool IsConnectedBackgroundColor(Color32 color)
    {
        var max = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        var min = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        var bright = color.r >= 185 && color.g >= 168 && color.b >= 150;
        var warmOrNeutral = color.r + 5 >= color.g && color.g + 8 >= color.b;
        return bright && warmOrNeutral && max - min <= 82;
    }

    private static TextureImporter GetTextureImporter(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
            throw new InvalidOperationException($"Texture importer was not found for '{path}'.");
        return importer;
    }

    private static void EnsureFolder(string folder)
    {
        var parts = folder.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
        rect.pivot = pivot;
    }

    private static void SetStretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private static void SetObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value) =>
        SetProperty(target, propertyName, property => property.objectReferenceValue = value);

    private static void SetObjectArray(UnityEngine.Object target, string propertyName, IReadOnlyList<Sprite> values) =>
        SetProperty(target, propertyName, property =>
        {
            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        });

    private static void SetRigOutfit(
        ExplorationCutoutRigController target,
        string propertyName,
        RigSpriteSet sprites)
    {
        var serialized = new SerializedObject(target);
        var outfit = serialized.FindProperty(propertyName);
        if (outfit == null)
            throw new InvalidOperationException(
                $"Serialized property '{propertyName}' was not found on {target.GetType().Name}.");

        outfit.FindPropertyRelative("torso").objectReferenceValue = sprites.Torso;
        outfit.FindPropertyRelative("head").objectReferenceValue = sprites.Head;
        outfit.FindPropertyRelative("backHair").objectReferenceValue = sprites.BackHair;
        outfit.FindPropertyRelative("upperArm").objectReferenceValue = sprites.UpperArm;
        outfit.FindPropertyRelative("forearm").objectReferenceValue = sprites.Forearm;
        outfit.FindPropertyRelative("thigh").objectReferenceValue = sprites.Thigh;
        outfit.FindPropertyRelative("calf").objectReferenceValue = sprites.Calf;
        outfit.FindPropertyRelative("foot").objectReferenceValue = sprites.Foot;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetString(UnityEngine.Object target, string propertyName, string value) =>
        SetProperty(target, propertyName, property => property.stringValue = value);

    private static void SetStringArray(UnityEngine.Object target, string propertyName, IReadOnlyList<string> values) =>
        SetProperty(target, propertyName, property =>
        {
            property.arraySize = values.Count;
            for (var i = 0; i < values.Count; i++)
                property.GetArrayElementAtIndex(i).stringValue = values[i];
        });

    private static void SetFloat(UnityEngine.Object target, string propertyName, float value) =>
        SetProperty(target, propertyName, property => property.floatValue = value);

    private static void SetBool(UnityEngine.Object target, string propertyName, bool value) =>
        SetProperty(target, propertyName, property => property.boolValue = value);

    private static void SetEnum(UnityEngine.Object target, string propertyName, int value) =>
        SetProperty(target, propertyName, property => property.enumValueIndex = value);

    private static void SetProperty(UnityEngine.Object target, string propertyName, Action<SerializedProperty> setter)
    {
        var serialized = new SerializedObject(target);
        var property = serialized.FindProperty(propertyName);
        if (property == null)
            throw new InvalidOperationException($"Serialized property '{propertyName}' was not found on {target.GetType().Name}.");
        setter(property);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private readonly struct Slice
    {
        public readonly string Name;
        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;
        public readonly float PivotX;
        public readonly float PivotY;

        public Slice(string name, int x, int y, int width, int height, float pivotX = 0.5f, float pivotY = 0f)
        {
            Name = name;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            PivotX = pivotX;
            PivotY = pivotY;
        }
    }

    private readonly struct RigSpriteSet
    {
        public readonly Sprite Torso;
        public readonly Sprite Head;
        public readonly Sprite BackHair;
        public readonly Sprite UpperArm;
        public readonly Sprite Forearm;
        public readonly Sprite Thigh;
        public readonly Sprite Calf;
        public readonly Sprite Foot;

        public bool IsComplete =>
            Torso != null && Head != null && BackHair != null && UpperArm != null &&
            Forearm != null && Thigh != null && Calf != null && Foot != null;

        public RigSpriteSet(
            Sprite torso,
            Sprite head,
            Sprite backHair,
            Sprite upperArm,
            Sprite forearm,
            Sprite thigh,
            Sprite calf,
            Sprite foot)
        {
            Torso = torso;
            Head = head;
            BackHair = backHair;
            UpperArm = upperArm;
            Forearm = forearm;
            Thigh = thigh;
            Calf = calf;
            Foot = foot;
        }
    }
}
