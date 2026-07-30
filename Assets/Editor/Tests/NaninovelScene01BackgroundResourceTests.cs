using Naninovel;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class NaninovelScene01BackgroundResourceTests
{
    private const string MainBackgroundPathPrefix = "Backgrounds/MainBackground";

    [TestCase("1-1-1")]
    [TestCase("1-10")]
    [TestCase("1-11")]
    [TestCase("1-12")]
    public void MainBackgroundScene01Resources_AreRegisteredAsTextures(string resourceName)
    {
        var assetPath = $"Assets/BackGround/scene01/{resourceName}.png";
        var expectedGuid = AssetDatabase.AssetPathToGUID(assetPath);

        Assert.That(expectedGuid, Is.Not.Empty, $"Missing background asset at '{assetPath}'.");
        Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath), Is.Not.Null,
            $"Naninovel SpriteBackground requires a Texture2D for '{assetPath}'.");

        var registeredGuid = EditorResources.LoadOrDefault()
            .GetGuidByPath($"{MainBackgroundPathPrefix}/scene01/{resourceName}");
        Assert.That(registeredGuid, Is.EqualTo(expectedGuid),
            "The default @back command resolves appearances through the MainBackground resource path.");
    }
}
