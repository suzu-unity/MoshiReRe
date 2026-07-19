using System;
using System.IO;
using System.Linq;
using MoshiReRe.DialoguePresentation.CharacterStamp;
using Naninovel;
using Naninovel.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MoshiReRe.Editor.DialoguePresentation
{
    /// <summary>
    /// Creates the project-side Dialogue prefab override without editing Naninovel package assets.
    /// </summary>
    public static class CharacterStampDialogueBuilder
    {
        private const string SourcePath = "Packages/com.elringus.naninovel/Prefabs/TextPrinters/Dialogue.prefab";
        private const string DestinationFolder = "Assets/NaninovelData/Resources/TextPrinters";
        private const string DestinationPath = DestinationFolder + "/Dialogue.prefab";
        private const string FontPath = "Assets/Font/PixelMplus12-Regular SDF.asset";
        private const string CharacterDatabasePath = "Assets/Database/Characters/CharacterDatabase.asset";
        private const string NarrationFrameFolder = "Assets/Art/UI/Dialogue/NarrationBookFrames";

        private static readonly Color PanelColor = new(0.024f, 0.067f, 0.145f, 0.91f);
        private static readonly Color BorderColor = new(0.25f, 0.69f, 0.92f, 0.9f);
        private static readonly Color TextColor = new(0.88f, 0.95f, 1f, 1f);

        [MenuItem("MoshiReRe/Dialogue Presentation/Build Character Stamp Dialogue")]
        public static void Build ()
        {
            EnsureFolder(DestinationFolder);

            var source = AssetDatabase.LoadAssetAtPath<GameObject>(SourcePath);
            if (!source) throw new InvalidOperationException($"Naninovel Dialogue prefab was not found at '{SourcePath}'.");

            var instance = PrefabUtility.InstantiatePrefab(source) as GameObject;
            if (!instance) throw new InvalidOperationException("Failed to instantiate the Naninovel Dialogue prefab.");

            try
            {
                // The stock prefab contains nested UI prefabs; unpack the temporary copy so the
                // stamp can safely reposition its author label before saving the project override.
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                Configure(instance);
                PrefabUtility.SaveAsPrefabAsset(instance, DestinationPath);
                RegisterNaninovelOverride();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"Created Character Stamp Dialogue override: {DestinationPath}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static void Configure (GameObject root)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            var database = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(CharacterDatabasePath);
            if (!font) throw new InvalidOperationException($"PixelMplus12 TMP font was not found at '{FontPath}'.");
            if (!database) throw new InvalidOperationException($"Character database was not found at '{CharacterDatabasePath}'.");

            var basePanel = root.GetComponentInChildren<RevealableTextPrinterPanel>(true);
            if (!basePanel) throw new InvalidOperationException("The source Dialogue prefab has no RevealableTextPrinterPanel.");
            ReplaceScript<CharacterStampDialoguePanel>(basePanel);
            var panel = root.GetComponentInChildren<CharacterStampDialoguePanel>(true);

            var textPanel = Find(root.transform, "TextPanel");
            var dialogueText = Find(root.transform, "DialogueText").GetComponent<TextMeshProUGUI>();
            var authorPanel = Find(root.transform, "AuthorNamePanel");
            var authorText = Find(root.transform, "AuthorNameText").GetComponent<TextMeshProUGUI>();
            if (!textPanel || !dialogueText || !authorPanel || !authorText)
                throw new InvalidOperationException("The source Dialogue prefab hierarchy does not match Naninovel's expected layout.");

            StylePanel(root.transform, textPanel, dialogueText, font);
            var stamp = CreateStamp(textPanel, authorPanel, authorText, font, database);
            CreateInputDiamond(root, textPanel, font, panel);
            AssignPanelStamp(panel, stamp);
        }

        private static void StylePanel (Transform root, Transform textPanel, TextMeshProUGUI dialogueText, TMP_FontAsset font)
        {
            var printerPanel = Find(root, "PrinterPanel");
            if (!printerPanel) throw new InvalidOperationException("The source Dialogue prefab has no PrinterPanel.");
            var printerWidth = printerPanel.GetComponent<LayoutElement>();
            printerWidth.minWidth = 0f;
            printerWidth.preferredWidth = 1680f;
            printerWidth.flexibleWidth = -1f;
            var printer = Find(root, "Printer");
            if (printer) (printer as RectTransform).anchoredPosition = new Vector2(0f, 26f);

            var background = textPanel.GetComponent<Image>();
            background.sprite = null;
            background.type = Image.Type.Simple;
            background.color = PanelColor;
            var border = textPanel.GetComponent<Outline>() ?? textPanel.gameObject.AddComponent<Outline>();
            border.effectColor = BorderColor;
            border.effectDistance = new Vector2(1f, -1f);

            var layout = textPanel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(224, 74, 30, 36);
            layout.childAlignment = TextAnchor.UpperLeft;

            var element = textPanel.GetComponent<LayoutElement>();
            element.minHeight = 224f;

            dialogueText.font = font;
            dialogueText.fontSize = 30f;
            dialogueText.color = TextColor;
            dialogueText.margin = Vector4.zero;
            dialogueText.enableWordWrapping = true;
        }

        private static CharacterStampPresenter CreateStamp (Transform textPanel, Transform authorPanel, TextMeshProUGUI authorText, TMP_FontAsset font, CharacterDatabase database)
        {
            var stampRoot = CreateUIObject("CharacterStamp", textPanel);
            SetRect(stampRoot, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(190f, 68f), new Vector2(0f, 1f));
            stampRoot.GetComponent<LayoutElement>().ignoreLayout = true;
            var stampImage = stampRoot.AddComponent<Image>();
            stampImage.color = new Color(0.035f, 0.11f, 0.22f, 0.98f);
            var stampOutline = stampRoot.AddComponent<Outline>();
            stampOutline.effectColor = BorderColor;
            stampOutline.effectDistance = new Vector2(1f, -1f);

            var iconFrame = CreateUIObject("IconFrame", stampRoot.transform);
            SetRect(iconFrame, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(52f, 52f), new Vector2(0f, 0.5f));
            var frameImage = iconFrame.AddComponent<Image>();
            frameImage.color = new Color(0.12f, 0.31f, 0.47f, 1f);

            var icon = CreateUIObject("Icon", iconFrame.transform);
            SetRect(icon, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-4f, -4f), new Vector2(0.5f, 0.5f));
            var iconImage = icon.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.enabled = false;

            var narrationBook = CreateUIObject("NarrationBook", iconFrame.transform);
            SetRect(narrationBook, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-4f, -4f), new Vector2(0.5f, 0.5f));
            var narrationImage = narrationBook.AddComponent<Image>();
            narrationImage.preserveAspect = true;
            narrationImage.enabled = false;
            var narrationFlipbook = narrationBook.AddComponent<NarrationFlipbook>();
            ConfigureNarrationFrames(narrationFlipbook, narrationImage);
            narrationBook.SetActive(false);

            var tag = CreateLabel("StampNumber", stampRoot.transform, font, "01", 15f, TextAnchor.MiddleLeft);
            SetRect(tag.gameObject, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -10f), new Vector2(44f, 18f), new Vector2(0f, 1f));
            tag.color = BorderColor;

            authorPanel.SetParent(stampRoot.transform, false);
            SetRect(authorPanel.gameObject, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -31f), new Vector2(112f, 26f), new Vector2(0f, 1f));
            var authorLayout = authorPanel.GetComponent<LayoutElement>() ?? authorPanel.gameObject.AddComponent<LayoutElement>();
            authorLayout.ignoreLayout = true;
            var authorBackground = authorPanel.GetComponent<Image>();
            if (authorBackground) UnityEngine.Object.DestroyImmediate(authorBackground);
            authorText.font = font;
            authorText.fontSize = 20f;
            authorText.alignment = TextAlignmentOptions.MidlineLeft;
            authorText.color = TextColor;
            ReplaceScript<StampAuthorNamePanel>(authorPanel.GetComponent<AuthorNameTMProPanel>());
            var stampAuthor = authorPanel.GetComponent<StampAuthorNamePanel>();
            var authorSerialized = new SerializedObject(stampAuthor);
            authorSerialized.FindProperty("text").objectReferenceValue = authorText;
            authorSerialized.ApplyModifiedPropertiesWithoutUndo();

            var presenter = stampRoot.AddComponent<CharacterStampPresenter>();
            var presenterSerialized = new SerializedObject(presenter);
            presenterSerialized.FindProperty("characterDatabase").objectReferenceValue = database;
            presenterSerialized.FindProperty("iconImage").objectReferenceValue = iconImage;
            presenterSerialized.FindProperty("numberLabel").objectReferenceValue = tag;
            presenterSerialized.FindProperty("authorNamePanel").objectReferenceValue = authorPanel.gameObject;
            presenterSerialized.FindProperty("narrationFlipbook").objectReferenceValue = narrationFlipbook;
            presenterSerialized.ApplyModifiedPropertiesWithoutUndo();
            return presenter;
        }

        private static void ConfigureNarrationFrames (NarrationFlipbook flipbook, Image image)
        {
            var frames = new Sprite[4];
            for (var index = 0; index < frames.Length; index++)
            {
                var path = $"{NarrationFrameFolder}/NarrationBook_{index + 1:00}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (!importer) throw new InvalidOperationException($"Narration book frame was not found at '{path}'.");

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                frames[index] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            }

            var serialized = new SerializedObject(flipbook);
            serialized.FindProperty("image").objectReferenceValue = image;
            var frameProperty = serialized.FindProperty("frames");
            frameProperty.arraySize = frames.Length;
            for (var index = 0; index < frames.Length; index++)
                frameProperty.GetArrayElementAtIndex(index).objectReferenceValue = frames[index];
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateInputDiamond (GameObject root, Transform textPanel, TMP_FontAsset font, CharacterStampDialoguePanel panel)
        {
            foreach (var existingIndicator in root.GetComponentsInChildren<MonoBehaviour>(true).OfType<IInputIndicator>())
                ((MonoBehaviour)existingIndicator).gameObject.SetActive(false);

            var diamondRoot = CreateUIObject("InputDiamond", textPanel);
            SetRect(diamondRoot, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-18f, 17f), new Vector2(28f, 28f), new Vector2(1f, 0f));
            diamondRoot.GetComponent<LayoutElement>().ignoreLayout = true;
            var rawImage = diamondRoot.AddComponent<RawImage>();
            rawImage.color = Color.clear;
            var indicator = diamondRoot.AddComponent<PingPongInputIndicator>();
            var indicatorSerialized = new SerializedObject(indicator);
            indicatorSerialized.FindProperty("tintPingPong").boolValue = false;
            indicatorSerialized.ApplyModifiedPropertiesWithoutUndo();

            var diamond = CreateLabel("DiamondGlyph", diamondRoot.transform, font, "\u25C7", 22f, TextAnchor.MiddleCenter);
            SetRect(diamond.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            diamond.color = BorderColor;
            diamond.raycastTarget = false;

            var panelSerialized = new SerializedObject(panel);
            panelSerialized.FindProperty("inputIndicator").objectReferenceValue = indicator;
            panelSerialized.FindProperty("positionIndicatorOverText").boolValue = false;
            panelSerialized.FindProperty("authorAvatarImage").objectReferenceValue = null;
            panelSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignPanelStamp (CharacterStampDialoguePanel panel, CharacterStampPresenter stamp)
        {
            var serialized = new SerializedObject(panel);
            serialized.FindProperty("characterStamp").objectReferenceValue = stamp;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RegisterNaninovelOverride ()
        {
            var configuration = ConfigurationSettings.LoadOrDefaultAndSave<TextPrintersConfiguration>();
            var metadata = configuration.GetMetadataOrDefault("Dialogue");
            var overrideGuid = AssetDatabase.AssetPathToGUID(DestinationPath);
            if (string.IsNullOrEmpty(overrideGuid))
                throw new InvalidOperationException($"Could not resolve the GUID for '{DestinationPath}'.");

            var resources = EditorResources.LoadOrDefault();
            var category = metadata.GetResourceCategoryId();
            resources.RemoveAllRecordsWithPath(metadata.Loader.PathPrefix, "Dialogue", category);
            resources.AddRecord(category, metadata.Loader.PathPrefix, "Dialogue", overrideGuid);
            EditorUtility.SetDirty(resources);
        }

        private static GameObject CreateUIObject (string name, Transform parent)
        {
            var result = new GameObject(name, typeof(RectTransform), typeof(LayoutElement));
            result.layer = parent.gameObject.layer;
            result.transform.SetParent(parent, false);
            return result;
        }

        private static TextMeshProUGUI CreateLabel (string name, Transform parent, TMP_FontAsset font, string value, float size, TextAnchor alignment)
        {
            var label = CreateUIObject(name, parent).AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = size;
            label.text = value;
            label.alignment = alignment switch
            {
                TextAnchor.MiddleRight => TextAlignmentOptions.MidlineRight,
                TextAnchor.MiddleCenter => TextAlignmentOptions.Midline,
                _ => TextAlignmentOptions.MidlineLeft
            };
            label.raycastTarget = false;
            return label;
        }

        private static void ReplaceScript<T> (MonoBehaviour component) where T : MonoBehaviour
        {
            if (!component) throw new InvalidOperationException($"Cannot replace a missing component with {typeof(T).Name}.");
            var probe = new GameObject("ScriptProbe");
            var targetScript = MonoScript.FromMonoBehaviour(probe.AddComponent<T>());
            UnityEngine.Object.DestroyImmediate(probe);
            var serialized = new SerializedObject(component);
            serialized.FindProperty("m_Script").objectReferenceValue = targetScript;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Transform Find (Transform root, string name)
        {
            return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
        }

        private static void SetRect (GameObject gameObject, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 pivot)
        {
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            rect.pivot = pivot;
        }

        private static void EnsureFolder (string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            Directory.CreateDirectory(folder);
            AssetDatabase.Refresh();
        }
    }
}
