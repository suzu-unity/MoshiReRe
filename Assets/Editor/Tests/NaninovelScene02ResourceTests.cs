using Naninovel;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class NaninovelScene02ResourceTests
{
    private const string AudioPathPrefix = "Audio";
    private const string MainBackgroundPathPrefix = "Backgrounds/MainBackground";

    [TestCase("BGM/title_bgm", "Assets/Audio/BGM/title_bgm.mp3")]
    [TestCase("SFX/タイプライター（キー）", "Assets/Audio/SFX/タイプライター（キー）.mp3")]
    [TestCase("SFX/電車停車", "Assets/Audio/SFX/電車停車.mp3")]
    [TestCase("SFX/電車蒸気噴出", "Assets/Audio/SFX/電車蒸気噴出.mp3")]
    [TestCase("SFX/電車走行中2", "Assets/Audio/SFX/電車走行中2.mp3")]
    [TestCase("SFX/革靴で歩く", "Assets/Audio/SFX/革靴で歩く.mp3")]
    public void AudioResources_AreRegisteredAsAudioClips(string resourceName, string assetPath)
    {
        var expectedGuid = AssetDatabase.AssetPathToGUID(assetPath);

        Assert.That(expectedGuid, Is.Not.Empty, $"Missing audio asset at '{assetPath}'.");
        Assert.That(AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath), Is.Not.Null,
            $"Naninovel audio resource requires an AudioClip for '{assetPath}'.");

        var registeredGuid = EditorResources.LoadOrDefault()
            .GetGuidByPath($"{AudioPathPrefix}/{resourceName}");
        Assert.That(registeredGuid, Is.EqualTo(expectedGuid));
    }

    [TestCase("1920x1080-px-Blue-Screen-of-Death-BSOD-Microsoft-Windows-1328311", ".jpg")]
    [TestCase("2-1", ".png")]
    [TestCase("2-2_1", ".png")]
    [TestCase("2-2_2", ".png")]
    [TestCase("2-2_3", ".png")]
    [TestCase("2-2_4", ".png")]
    [TestCase("Adverpng6", ".png")]
    [TestCase("Adverpng8", ".png")]
    [TestCase("Adverpng9", ".png")]
    [TestCase("BGsmartphone01", ".png")]
    [TestCase("BGsmartphone02", ".png")]
    [TestCase("BGsmartphone03", ".png")]
    [TestCase("ComfyUI_00283_", ".png")]
    [TestCase("Webpage1", ".png")]
    public void MainBackgroundScene02Resources_AreRegisteredAsTextures(string resourceName, string extension)
    {
        var assetPath = $"Assets/BackGround/scene02/{resourceName}{extension}";
        var expectedGuid = AssetDatabase.AssetPathToGUID(assetPath);

        Assert.That(expectedGuid, Is.Not.Empty, $"Missing background asset at '{assetPath}'.");
        Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath), Is.Not.Null,
            $"Naninovel SpriteBackground requires a Texture2D for '{assetPath}'.");

        var registeredGuid = EditorResources.LoadOrDefault()
            .GetGuidByPath($"{MainBackgroundPathPrefix}/scene02/{resourceName}");
        Assert.That(registeredGuid, Is.EqualTo(expectedGuid),
            "The default @back command resolves appearances through the MainBackground resource path.");
    }
}
