using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ComicPanelDemoCreator
{
    private const string RootFolder = "Assets/Resources/ComicPanelSystem";
    private const string LayoutPath = RootFolder + "/ComicPanelDemoLayout.asset";
    private const string PrefabPath = RootFolder + "/ComicPanelDemo.prefab";

    [MenuItem("Tools/Comic Panel System/Create Demo Prefab")]
    public static void CreateDemoPrefab()
    {
        EnsureFolder(RootFolder);
        var layout = CreateOrLoadLayout();
        var root = new GameObject("ComicPanelDemo", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(ComicPanelController));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 500;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        root.GetComponent<ComicPanelController>().SetLayoutForEditor(layout);

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log($"[ComicPanel] Demo created: {PrefabPath}");
    }

    private static ComicPanelLayout CreateOrLoadLayout()
    {
        var layout = AssetDatabase.LoadAssetAtPath<ComicPanelLayout>(LayoutPath);
        if (layout != null)
            return layout;

        var sprites = new[]
        {
            CreateSprite("DemoPanelA", new Color(0.92f, 0.28f, 0.30f, 1f)),
            CreateSprite("DemoPanelB", new Color(0.20f, 0.55f, 0.92f, 1f)),
            CreateSprite("DemoPanelC", new Color(0.28f, 0.78f, 0.48f, 1f))
        };
        layout = ScriptableObject.CreateInstance<ComicPanelLayout>();
        layout.SetLayoutIdForEditor("ComicPanelDemo");
        layout.Panels.Add(new ComicPanelDefinition
        {
            id = "panelA",
            image = sprites[0],
            vertices = new System.Collections.Generic.List<Vector2>
            {
                new Vector2(0.03f, 0.05f), new Vector2(0.48f, 0.03f), new Vector2(0.44f, 0.96f), new Vector2(0.02f, 0.90f)
            },
            transitionSeconds = 0.18f
        });
        layout.Panels.Add(new ComicPanelDefinition
        {
            id = "panelB",
            image = sprites[1],
            vertices = new System.Collections.Generic.List<Vector2>
            {
                new Vector2(0.50f, 0.04f), new Vector2(0.97f, 0.10f), new Vector2(0.95f, 0.58f), new Vector2(0.47f, 0.52f)
            },
            transitionSeconds = 0.18f
        });
        layout.Panels.Add(new ComicPanelDefinition
        {
            id = "panelC",
            image = sprites[2],
            vertices = new System.Collections.Generic.List<Vector2>
            {
                new Vector2(0.48f, 0.56f), new Vector2(0.95f, 0.62f), new Vector2(0.98f, 0.97f), new Vector2(0.42f, 0.94f)
            },
            transitionSeconds = 0.18f
        });
        AssetDatabase.CreateAsset(layout, LayoutPath);
        AssetDatabase.SaveAssets();
        return layout;
    }

    private static Sprite CreateSprite(string name, Color baseColor)
    {
        var texturePath = RootFolder + "/" + name + ".asset";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            texture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            var pixels = new Color[256 * 256];
            for (var y = 0; y < 256; y++)
            {
                for (var x = 0; x < 256; x++)
                {
                    var stripe = ((x / 32) + (y / 32)) % 2 == 0 ? 1f : 0.82f;
                    pixels[y * 256 + x] = new Color(baseColor.r * stripe, baseColor.g * stripe, baseColor.b * stripe, 1f);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();
            texture.name = name;
            AssetDatabase.CreateAsset(texture, texturePath);
            AssetDatabase.SaveAssets();
        }

        var spritePath = RootFolder + "/" + name + ".sprite.asset";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite == null)
        {
            sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            sprite.name = name;
            AssetDatabase.CreateAsset(sprite, spritePath);
            AssetDatabase.SaveAssets();
        }
        return sprite;
    }

    private static void EnsureFolder(string path)
    {
        var parts = path.Split('/');
        var current = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
