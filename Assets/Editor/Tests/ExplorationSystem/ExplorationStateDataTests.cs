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
    }
}
