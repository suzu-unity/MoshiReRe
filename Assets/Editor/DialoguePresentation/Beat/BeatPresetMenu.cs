using System.Collections.Generic;
using MoshiReRe.DialoguePresentation.Beat;
using UnityEditor;
using UnityEngine;

public static class BeatPresetMenu
{
    private const string LibraryPath = "Assets/Resources/DialoguePresentation/Beat/DefaultBeatLibrary.asset";

    [MenuItem("MoshiReRe/Dialogue Presentation/Beat/Create Default Library")]
    private static void CreateDefaultLibrary()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/DialoguePresentation");
        EnsureFolder("Assets/Resources/DialoguePresentation/Beat");

        var library = AssetDatabase.LoadAssetAtPath<BeatLibrary>(LibraryPath);
        if (!library)
        {
            library = ScriptableObject.CreateInstance<BeatLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }

        var presets = new List<BeatPreset>(library.Presets ?? new List<BeatPreset>());
        AddIfMissing(presets, new BeatPreset("awkward", .35f, BeatTimeMode.Unscaled, null,
            1f, Color.white, .12f, 0f, false, 0f, 18f, false));
        AddIfMissing(presets, new BeatPreset("shock", .55f, BeatTimeMode.Unscaled, null,
            1f, Color.white, .5f, .12f, true, .04f, 22f, false));
        AddIfMissing(presets, new BeatPreset("realize", .45f, BeatTimeMode.Unscaled, null,
            1f, new Color(1f, .82f, .25f), .2f, 0f, false, .02f, 16f, false));
        AddIfMissing(presets, new BeatPreset("punchline", .25f, BeatTimeMode.Unscaled, null,
            1f, Color.white, .08f, 0f, false, .015f, 20f, true));

        library.ReplacePresets(presets);
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        Selection.activeObject = library;
    }

    private static void AddIfMissing(List<BeatPreset> presets, BeatPreset value)
    {
        for (var i = 0; i < presets.Count; i++)
        {
            if (presets[i] != null && BeatTypeUtility.Normalize(presets[i].Type) == BeatTypeUtility.Normalize(value.Type))
                return;
        }

        presets.Add(value);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = path.Substring(0, path.LastIndexOf('/'));
        var name = path.Substring(path.LastIndexOf('/') + 1);
        AssetDatabase.CreateFolder(parent, name);
    }
}
