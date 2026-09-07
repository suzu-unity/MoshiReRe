using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Imports the supplied artwork unchanged; rectangles only select atlas regions.</summary>
public static class SuppliedMenuAssetLibrary
{
    private const string Destination = "Assets/Art/UI/SuppliedMenu";
    private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    private static bool force;
    private sealed class Region
    {
        public string name;
        public Rect topRect;
        public bool trim;
        public Region(string name, Rect rect, bool trim = true) { this.name = name; topRect = rect; this.trim = trim; }
    }

    public static Sprite Get(string key)
    {
        if (!Sprites.TryGetValue(key, out var sprite)) throw new InvalidOperationException("Missing supplied menu sprite: " + key);
        return sprite;
    }

    public static Sprite Slice(string atlas, string key, Rect topRect)
    {
        if (Sprites.TryGetValue(key, out var cached)) return cached;
        var texture = Get(atlas).texture;
        var rect = new Rect(topRect.x, texture.height - topRect.yMax, topRect.width, topRect.height);
        var path = Destination + "/" + key + ".asset";
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (!sprite)
        {
            sprite = Sprite.Create(texture, rect, new Vector2(.5f, .5f), 100f, 0, SpriteMeshType.FullRect);
            sprite.name = key;
            AssetDatabase.CreateAsset(sprite, path);
        }
        Sprites[key] = sprite;
        return sprite;
    }

    public static void Import(bool forceMetadata = false)
    {
        force = forceMetadata;
        Sprites.Clear();
        Directory.CreateDirectory(Destination);
        Single("home", "menu_top", "ChatGPT Image 2026年9月7日 16_32_31.png", false);
        Single("event", "menu_top", "eventbar.png");
        Single("hand", "menu_top", "hand.png");
        var normal = new[] { "20_59_52", "20_59_58", "21_00_04", "21_00_06", "21_00_09", "21_00_13", "21_00_16" };
        var hover = new[] { "21_07_47", "21_07_52", "21_07_56", "21_08_00", "21_08_05", "21_08_09", "21_08_14" };
        var actions = new[] { "dress", "items", "characters", "quest", "map", "save", "settings" };
        for (var i = 0; i < actions.Length; i++)
        {
            Single("home_" + actions[i], "menu_top", "ChatGPT Image 2026年9月7日 " + normal[i] + ".png");
            Single("hover_" + actions[i], "menu_top", "ChatGPT Image 2026年9月7日 " + hover[i] + ".png");
        }
        Sheet("home_parts", "menu_top", "ChatGPT Image 2026年9月7日 16_32_47.png",
            R("deadline", 35, 35, 1078, 350), R("stamina", 40, 365, 1025, 235),
            R("money", 40, 598, 1040, 195), R("bubble", 40, 812, 1040, 256), R("badge", 1120, 350, 320, 354));
        var nav = new List<Region>();
        var navNames = new[] { "home", "dress", "items", "characters", "quest", "map", "save", "settings" };
        for (var i = 0; i < 8; i++) nav.Add(R(navNames[i], 192, 40 + i * 130, 168, i == 7 ? 118 : 134));
        nav.Add(R("selected", 914, 772, 355, 195));
        nav.Add(R("previous", 430, 772, 210, 200));
        nav.Add(R("next", 669, 772, 207, 200));
        nav.Add(R("badge", 454, 564, 130, 132));
        Sheet("nav", "menu_clothes", "ChatGPT Image 2026年9月7日 16_44_59.png", nav.ToArray());
        Single("dress_background", "menu_clothes", "ChatGPT Image 2026年9月7日 16_41_39.png", false);
        Single("dress_mock", "menu_clothes", "closes_mok.png", false);
        var clothes = new[] { "16_44_10", "16_44_17", "16_44_22", "16_44_28", "16_44_34", "16_44_40" };
        for (var i = 0; i < clothes.Length; i++) Single("outfit_" + i, "menu_clothes", "ChatGPT Image 2026年9月7日 " + clothes[i] + ".png");
        Single("dress_parts", "menu_clothes", "ChatGPT Image 2026年9月7日 16_44_47.png", false);
        Single("dress_status_parts", "menu_clothes", "ChatGPT Image 2026年9月7日 16_44_53.png", false);
        Single("items_background", "menu_item", "ChatGPT Image 2026年9月7日 17_02_38.png", false);
        Single("items_mock", "menu_item", "item_mok.png", false);
        Single("items_parts", "menu_item", "ChatGPT Image 2026年9月7日 17_02_31.png", false);
        Single("characters_background", "menu_character", "ChatGPT Image 2026年9月7日 18_09_48.png", false);
        Single("characters_parts", "menu_character", "ChatGPT Image 2026年9月7日 18_09_29.png", false);
        Single("rere", "menu_character", "ChatGPT Image 2026年9月7日 18_00_33.png");
        var portraits = new[] { "18_13_26", "18_13_31", "18_13_34", "18_13_42" };
        for (var i = 0; i < portraits.Length; i++) Single("portrait_" + i, "menu_character", "ChatGPT Image 2026年9月7日 " + portraits[i] + ".png");
        var thumbs = new[] { "18_17_32 (1)", "18_17_33 (2)", "18_17_33 (3)", "18_17_33 (4)", "18_17_34 (5)" };
        for (var i = 0; i < thumbs.Length; i++) Single("thumb_" + i, "menu_character", "ChatGPT Image 2026年9月7日 " + thumbs[i] + ".png");
        Single("quest_background", "menu_quest", "ChatGPT Image 2026年9月7日 19_04_40.png", false);
        Single("quest_parts", "menu_quest", "ChatGPT Image 2026年9月7日 19_05_01.png", false);
        Single("quest_tabs", "menu_quest", "ChatGPT Image 2026年9月7日 19_04_55.png", false);
        Single("map_background", "menu_map", "ChatGPT Image 2026年9月7日 20_02_36.png", false);
        Single("map", "menu_map", "ChatGPT Image 2026年9月7日 19_50_37.png", false);
        Single("map_parts", "menu_map", "ChatGPT Image 2026年9月7日 19_41_12.png", false);
        Single("map_controls", "menu_map", "ChatGPT Image 2026年9月7日 19_41_06.png", false);
        Single("save_background", "menu_save&load", "ChatGPT Image 2026年9月7日 20_30_33.png", false);
        Single("save_parts", "menu_save&load", "ChatGPT Image 2026年9月7日 20_30_38.png", false);
        Single("settings_parts", "menu_option", "ChatGPT Image 2026年9月7日 20_39_35.png", false);
        Single("settings_layout", "menu_option", "ChatGPT Image 2026年9月7日 20_39_45.png", false);
        AssetDatabase.SaveAssets();
    }

    private static Region R(string name, float x, float y, float width, float height) => new Region(name, new Rect(x, y, width, height));
    private static void Single(string key, string folder, string name, bool trim = true) => Sheet(key, folder, name, new Region("full", Rect.zero, trim));

    private static void Sheet(string key, string folder, string name, params Region[] regions)
    {
        var source = Path.Combine("素材候補", folder, name);
        var path = Destination + "/" + key + ".png";
        var changed = !File.Exists(path) || File.GetLastWriteTimeUtc(source) > File.GetLastWriteTimeUtc(path);
        if (changed) File.Copy(source, path, true);
        if (!changed && !force)
        {
            var existing = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            if (existing.Length == regions.Length)
            {
                foreach (var entry in existing) Sprites[entry.name] = entry;
                return;
            }
        }
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.maxTextureSize = 4096;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.filterMode = FilterMode.Bilinear;
        importer.SaveAndReimport();
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var pixels = texture.GetPixels32();
        var metadata = new List<SpriteMetaData>();
        foreach (var region in regions)
        {
            var rect = region.topRect == Rect.zero ? new Rect(0, 0, texture.width, texture.height)
                : new Rect(region.topRect.x, texture.height - region.topRect.yMax, region.topRect.width, region.topRect.height);
            if (region.trim) rect = Trim(rect, pixels, texture.width, texture.height);
            metadata.Add(new SpriteMetaData { name = region.name == "full" ? key : key + "." + region.name, rect = rect, alignment = 0, pivot = new Vector2(.5f, .5f) });
        }
#pragma warning disable 618
        importer.spritesheet = metadata.ToArray();
#pragma warning restore 618
        importer.isReadable = false;
        importer.SaveAndReimport();
        foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()) Sprites[sprite.name] = sprite;
    }

    private static Rect Trim(Rect source, Color32[] pixels, int width, int height)
    {
        var minX = width; var minY = height; var maxX = -1; var maxY = -1;
        for (var y = Mathf.Max(0, (int)source.yMin); y < Mathf.Min(height, source.yMax); y++)
        for (var x = Mathf.Max(0, (int)source.xMin); x < Mathf.Min(width, source.xMax); x++)
        {
            if (pixels[y * width + x].a < 4) continue;
            minX = Math.Min(minX, x); minY = Math.Min(minY, y); maxX = Math.Max(maxX, x); maxY = Math.Max(maxY, y);
        }
        return maxX < minX ? source : new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}
