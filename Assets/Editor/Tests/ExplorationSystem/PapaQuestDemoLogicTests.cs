using MoshiReRe.Exploration;
using Naninovel;
using NUnit.Framework;
using UnityEditor;

namespace MoshiReRe.EditorTests.ExplorationSystem
{
    public sealed class PapaQuestDemoLogicTests
    {
        [TestCase("papa_cafe", true)]
        [TestCase(" PAPA_CAFE ", true)]
        [TestCase("office", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsPapaCafeMap_MatchesOnlyConfiguredVariant(string mapId, bool expected)
        {
            Assert.That(OfficeExplorationController.IsPapaCafeMap(mapId), Is.EqualTo(expected));
        }

        [Test]
        public void IsPapaCafeMap_AcceptsFutureConfiguredMapId()
        {
            Assert.That(OfficeExplorationController.IsPapaCafeMap("night_demo", "night_demo"), Is.True);
            Assert.That(OfficeExplorationController.IsPapaCafeMap("papa_cafe", "night_demo"), Is.False);
        }

        [TestCase("Scripts", "Scenario/PapaCafeExploration", "Assets/Scenario/PapaCafeExploration.nani")]
        [TestCase("Scripts", "Scenario/PapaQuestDemo", "Assets/Scenario/PapaQuestDemo.nani")]
        [TestCase("Backgrounds/MainBackground", "ScenarioExploration/Backgrounds/02_bakery_cafe", "Assets/Art/ScenarioExploration/Backgrounds/02_bakery_cafe.png")]
        [TestCase("Backgrounds/MainBackground", "ScenarioCG/company_seated_demo", "Assets/Art/ScenarioCG/PLACEHOLDER_REPLACE_ME_company_seated.png")]
        [TestCase("Backgrounds/MainBackground", "ScenarioCG/papa_cafe_key_demo", "Assets/Art/ScenarioCG/PLACEHOLDER_REPLACE_ME_papa_cafe_key.png")]
        public void PapaDemoResources_AreRegistered(string pathPrefix, string resourcePath, string assetPath)
        {
            var expectedGuid = AssetDatabase.AssetPathToGUID(assetPath);
            Assert.That(expectedGuid, Is.Not.Empty, $"Missing asset at '{assetPath}'.");

            var actualGuid = EditorResources.LoadOrDefault().GetGuidByPath($"{pathPrefix}/{resourcePath}");
            Assert.That(actualGuid, Is.EqualTo(expectedGuid));
        }
    }
}
