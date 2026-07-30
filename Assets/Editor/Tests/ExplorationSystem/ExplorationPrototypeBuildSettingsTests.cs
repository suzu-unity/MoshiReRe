using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace MoshiReRe.EditorTests.ExplorationSystem
{
    public sealed class ExplorationPrototypeBuildSettingsTests
    {
        private const string ScenePath = "Assets/Scenes/ExplorationPrototype.unity";

        [Test]
        public void ExplorationPrototype_IsAnEnabledBuildScene()
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath), Is.Not.Null);

            var matchingScenes = EditorBuildSettings.scenes
                .Where(scene => scene.path == ScenePath)
                .ToArray();

            Assert.That(matchingScenes, Has.Length.EqualTo(1));
            Assert.That(matchingScenes[0].enabled, Is.True);
        }
    }
}
