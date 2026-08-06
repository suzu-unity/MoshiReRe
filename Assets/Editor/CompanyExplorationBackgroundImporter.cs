using UnityEditor;

/// <summary>Keeps the time-of-day office panoramas ready for side-scrolling SpriteRenderer use.</summary>
public sealed class CompanyExplorationBackgroundImporter : AssetPostprocessor
{
    private const string BackgroundFolder = "Assets/Art/CompanyExploration/Backgrounds/";
    private const string ScenarioBackgroundFolder = "Assets/Art/ScenarioExploration/Backgrounds/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(BackgroundFolder, System.StringComparison.Ordinal) &&
            !assetPath.StartsWith(ScenarioBackgroundFolder, System.StringComparison.Ordinal))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100f;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = UnityEngine.TextureWrapMode.Clamp;
        importer.filterMode = UnityEngine.FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 4096;
    }

    [MenuItem("Tools/MoshiReRe/Company Exploration/Reimport Backgrounds")]
    public static void ReimportBackgrounds()
    {
        var folders = new[] { BackgroundFolder.TrimEnd('/'), ScenarioBackgroundFolder.TrimEnd('/') };
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", folders))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}
