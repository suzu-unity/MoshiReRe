using UnityEditor;
using UnityEngine;

public class MenuReReAnimationImporter : AssetPostprocessor
{
    private const string AnimationPath = "Assets/Resources/MenuReReAnimations/";

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(AnimationPath)) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Default;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
    }
}
