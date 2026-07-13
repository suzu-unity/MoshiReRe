using System.Collections.Generic;
using MoshiReRe.DialoguePresentation.CharacterMotion;
using Naninovel;
using UnityEditor;
using UnityEngine;

public static class CharacterMotionPresetMenu
{
    private const string LibraryPath = "Assets/Resources/DialoguePresentation/CharacterMotion/DefaultCharacterMotionLibrary.asset";

    [MenuItem("MoshiReRe/Dialogue Presentation/Character Motion/Create Default Library")]
    private static void CreateDefaultLibrary()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/DialoguePresentation");
        EnsureFolder("Assets/Resources/DialoguePresentation/CharacterMotion");

        var library = AssetDatabase.LoadAssetAtPath<CharacterMotionLibrary>(LibraryPath);
        if (!library)
        {
            library = ScriptableObject.CreateInstance<CharacterMotionLibrary>();
            AssetDatabase.CreateAsset(library, LibraryPath);
        }

        var presets = new List<CharacterMotionPreset>(library.Presets ?? new List<CharacterMotionPreset>());
        AddIfMissing(presets, new CharacterMotionPreset("shock", new Vector3(0f, .08f, 0f), Vector3.one,
            Vector3.zero, .28f, EasingType.EaseOutBounce, true, 2, true, 0));
        AddIfMissing(presets, new CharacterMotionPreset("nervous", new Vector3(.04f, 0f, 0f), Vector3.one,
            Vector3.zero, .42f, EasingType.EaseInOutSine, true, 3, true, 4));
        AddIfMissing(presets, new CharacterMotionPreset("pressure", new Vector3(0f, 0f, -.04f),
            new Vector3(1.06f, 1.06f, 1f), Vector3.zero, .24f, EasingType.EaseOutQuad, false, 1, true, 0));
        AddIfMissing(presets, new CharacterMotionPreset("withdraw", new Vector3(0f, -.05f, .03f),
            new Vector3(.94f, .94f, 1f), Vector3.zero, .26f, EasingType.EaseInOutQuad, false, 1, true, 0));
        AddIfMissing(presets, new CharacterMotionPreset("awkwardGap", new Vector3(.12f, 0f, 0f), Vector3.one,
            Vector3.zero, .3f, EasingType.EaseInOutSine, false, 1, true, 0));

        library.ReplacePresets(presets);
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        Selection.activeObject = library;
    }

    private static void AddIfMissing(List<CharacterMotionPreset> presets, CharacterMotionPreset value)
    {
        for (var i = 0; i < presets.Count; i++)
        {
            if (presets[i] != null && CharacterMotionTypeUtility.Normalize(presets[i].MotionName) ==
                CharacterMotionTypeUtility.Normalize(value.MotionName)) return;
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
