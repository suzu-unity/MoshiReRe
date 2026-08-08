using Naninovel;
using NUnit.Framework;
using UnityEngine;
using MoshiReRe.Exploration;
using MoshiReRe.Exploration.State;

namespace MoshiReRe.Editor.Tests.ExplorationSystem
{
    public sealed class ExplorationStateDataTests
    {
        [Test]
        public void SaveState_RoundTripsAllSerializableMapData()
        {
            var source = new ExplorationSaveState();
            source.flow.Begin("apartment", "Apartment", "front-door", "Company", "Day/Return", "resume");
            var map = source.GetOrCreateMap("apartment");
            map.sceneName = "Apartment";
            map.spawnId = "front-door";
            map.playerPosition = new Vector3(4f, 2f, 0f);
            map.outfit = ExplorationOutfit.Wardrobe;
            map.UpsertObject(new ExplorationObjectState {
                objectId = "phone",
                activeSelf = false,
                interacted = true,
                localState = "opened"
            });
            map.SetLocal("message", "read");

            var roundTripped = JsonUtility.FromJson<ExplorationSaveState>(JsonUtility.ToJson(source));

            Assert.That(roundTripped.flow.active, Is.True);
            Assert.That(roundTripped.flow.returnLabel, Is.EqualTo("resume"));
            Assert.That(roundTripped.FindMap("apartment").playerPosition, Is.EqualTo(new Vector3(4f, 2f, 0f)));
            Assert.That(roundTripped.FindMap("apartment").outfit, Is.EqualTo(ExplorationOutfit.Wardrobe));
            Assert.That(roundTripped.FindMap("apartment").FindObject("phone").activeSelf, Is.False);
            Assert.That(roundTripped.FindMap("apartment").TryGetLocal("message", out var value), Is.True);
            Assert.That(value, Is.EqualTo("read"));
        }

        [Test]
        public void MapState_UpsertObjectReplacesMatchingStableId()
        {
            var map = new ExplorationMapState();
            map.UpsertObject(new ExplorationObjectState { objectId = "door", activeSelf = true, localState = "closed" });
            map.UpsertObject(new ExplorationObjectState { objectId = "door", activeSelf = false, interacted = true, localState = "open" });

            Assert.That(map.objects, Has.Count.EqualTo(1));
            Assert.That(map.FindObject("door").activeSelf, Is.False);
            Assert.That(map.FindObject("door").interacted, Is.True);
            Assert.That(map.FindObject("door").localState, Is.EqualTo("open"));
        }

        [Test]
        public void SaveState_SeparatesMapScopedObjectsAndLocals()
        {
            var save = new ExplorationSaveState();
            var apartment = save.GetOrCreateMap("apartment");
            var office = save.GetOrCreateMap("office");
            apartment.SetLocal("phase", "night");
            office.SetLocal("phase", "day");
            apartment.UpsertObject(new ExplorationObjectState { objectId = "door", localState = "locked" });
            office.UpsertObject(new ExplorationObjectState { objectId = "door", localState = "unlocked" });

            Assert.That(apartment.TryGetLocal("phase", out var apartmentPhase), Is.True);
            Assert.That(office.TryGetLocal("phase", out var officePhase), Is.True);
            Assert.That(apartmentPhase, Is.EqualTo("night"));
            Assert.That(officePhase, Is.EqualTo("day"));
            Assert.That(apartment.FindObject("door").localState, Is.EqualTo("locked"));
            Assert.That(office.FindObject("door").localState, Is.EqualTo("unlocked"));
        }

        [Test]
        public void GameStateMap_RestoresExplorationStateByStableInstanceId()
        {
            var state = new ExplorationSaveState();
            state.GetOrCreateMap("apartment").SetLocal("flag", "saved");
            var gameState = new GameStateMap();
            gameState.SetState(state, ExplorationStateCoordinator.StateId);

            var json = JsonUtility.ToJson(gameState);
            var restoredGameState = JsonUtility.FromJson<GameStateMap>(json);
            var restored = restoredGameState.GetState<ExplorationSaveState>(ExplorationStateCoordinator.StateId);

            Assert.That(restored.FindMap("apartment").TryGetLocal("flag", out var value), Is.True);
            Assert.That(value, Is.EqualTo("saved"));
        }

        [Test]
        public void DialogueWithoutReturnCommand_StaysOnExplorationMap()
        {
            Assert.That(
                NaninovelDialogueInteractable.ShouldTransitionToNovel(default(ExplorationReturnRequest)),
                Is.False);
        }

        [Test]
        public void ReturnToNovelRequest_CausesDialogueToExitExploration()
        {
            var request = new ExplorationReturnRequest(null, null, null);

            Assert.That(NaninovelDialogueInteractable.ShouldTransitionToNovel(request), Is.True);
            Assert.That(
                NaninovelDialogueInteractable.ResolveReturnTarget(request, null, null, null).SceneName,
                Is.EqualTo(NaninovelDialogueInteractable.NovelHostSceneName));
        }

        [Test]
        public void ReturnToNovelRequest_CapturesExplicitSceneScriptAndLabel()
        {
            var request = new ExplorationReturnRequest(
                "NovelHost", "Scenario/Return", "Resume");

            Assert.That(request.Requested, Is.True);
            Assert.That(request.SceneName, Is.EqualTo("NovelHost"));
            Assert.That(request.ScriptPath, Is.EqualTo("Scenario/Return"));
            Assert.That(request.Label, Is.EqualTo("Resume"));
        }

        [Test]
        public void ReturnToNovelBranch_OverridesInspectorDefaults()
        {
            var request = new ExplorationReturnRequest(
                "BranchHost", "Scenario/Branch", "BranchLabel");

            var target = NaninovelDialogueInteractable.ResolveReturnTarget(
                request,
                "LegacyHost",
                "Scenario/Legacy",
                "LegacyLabel");

            Assert.That(target.SceneName, Is.EqualTo("BranchHost"));
            Assert.That(target.ScriptPath, Is.EqualTo("Scenario/Branch"));
            Assert.That(target.Label, Is.EqualTo("BranchLabel"));
        }

        [Test]
        public void ReturnTargetResolution_IgnoresLegacyEnterExplorationValues()
        {
            var legacyFlow = new ExplorationFlowContext();
            legacyFlow.Begin(
                "office",
                "OfficeExploration",
                "entrance",
                "LegacyHost",
                "Scenario/Legacy",
                "LegacyLabel");

            var target = NaninovelDialogueInteractable.ResolveReturnTarget(
                new ExplorationReturnRequest(null, null, null),
                null,
                null,
                null);

            Assert.That(legacyFlow.returnScript, Is.EqualTo("Scenario/Legacy"));
            Assert.That(target.SceneName, Is.EqualTo(NaninovelDialogueInteractable.NovelHostSceneName));
            Assert.That(target.ScriptPath, Is.Empty);
            Assert.That(target.Label, Is.Empty);
        }
    }
}
