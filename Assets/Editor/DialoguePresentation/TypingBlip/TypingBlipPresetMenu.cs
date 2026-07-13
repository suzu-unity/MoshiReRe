#if UNITY_EDITOR
using MoshiReRe.DialoguePresentation.TypingBlip;
using UnityEditor;
using UnityEngine;

internal static class TypingBlipPresetMenu
{
    private const string PresetFolder = "Assets/Scripts/DialoguePresentation/TypingBlip/Presets";

    [MenuItem("Tools/MoshiReRe/Typing Blip/Create Male Profile")]
    private static void CreateMaleProfile() => CreateProfile("TypingBlip_Male", 0.98f, 0.035f, 0.55f, 0.045f, 1);

    [MenuItem("Tools/MoshiReRe/Typing Blip/Create Female Profile")]
    private static void CreateFemaleProfile() => CreateProfile("TypingBlip_Female", 1.08f, 0.05f, 0.52f, 0.045f, 1);

    [MenuItem("Tools/MoshiReRe/Typing Blip/Create ReRe Electronic Profile")]
    private static void CreateReReProfile() => CreateProfile("TypingBlip_ReRe_Electronic", 1.35f, 0.08f, 0.7f, 0.035f, 1);

    private static void CreateProfile(string assetName, float pitch, float pitchRandomness, float volume,
        float minimumInterval, int charactersPerBlip)
    {
        EnsureFolder();
        var assetPath = $"{PresetFolder}/{assetName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<TypingBlipProfile>(assetPath);
        if (existing)
        {
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            return;
        }

        var profile = ScriptableObject.CreateInstance<TypingBlipProfile>();
        profile.ReplaceEntries(new[]
        {
            new TypingBlipProfileEntry(string.Empty, null, pitch, pitchRandomness, volume, minimumInterval, charactersPerBlip)
        });
        AssetDatabase.CreateAsset(profile, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = profile;
        EditorGUIUtility.PingObject(profile);
    }

    private static void EnsureFolder()
    {
        if (AssetDatabase.IsValidFolder(PresetFolder)) return;
        AssetDatabase.CreateFolder("Assets/Scripts/DialoguePresentation/TypingBlip", "Presets");
    }
}
#endif
