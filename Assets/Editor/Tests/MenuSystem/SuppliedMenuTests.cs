using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SuppliedMenuTests
{
    [Test]
    public void UnknownNodeDoesNotRevealItsImage()
    {
        var database=ScriptableObject.CreateInstance<CharacterDatabase>();
        var character=ScriptableObject.CreateInstance<CharacterInfo>();
        var picture=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,1,1),Vector2.one*.5f);
        try
        {
            character.id="image_test";
            character.nodes.Add(new CharacterInformationNodeDefinition { id="clue", image=picture, initialConfidence=CharacterInformationConfidence.Unknown });
            database.characters.Add(character);
            var state=new CharacterInformationNodeState(database);
            Assert.That(state.GetNodes(character.id)[0].Image, Is.Null);
            state.TrySetConfidence(character.id,"clue",CharacterInformationConfidence.Confirmed);
            Assert.That(state.GetNodes(character.id)[0].Image, Is.SameAs(picture));
        }
        finally { Object.DestroyImmediate(database);Object.DestroyImmediate(character);Object.DestroyImmediate(picture); }
    }
    [Test]
    public void SaveDayMetadataRoundTripsAndOldSavesRemainUnknown()
    {
        var map=new Naninovel.GameStateMap();
        Assert.That(MenuDaySaveBridge.FormatDay(map), Is.EqualTo("DAY —"));
        map.SetState(new MenuDaySaveBridge.DayState { day=12 }, MenuDaySaveBridge.StateKey);
        Assert.That(MenuDaySaveBridge.FormatDay(map), Is.EqualTo("DAY 12"));
    }
    private const string Path = "Assets/NaninovelData/Resources/UI/MenuRootV2.prefab";
    [Test]
    public void AllPagesAndCommonShellAreAssigned()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Path).GetComponent<MenuRootV2UI>();
        var so = new SerializedObject(root);
        foreach (var field in new[] { "pageTop", "pageStatus", "pageItems", "pageCharacters", "pageQuest", "pageMap", "pageSave", "pageSettings", "standardPhoneLayer" })
            Assert.That(so.FindProperty(field).objectReferenceValue, Is.Not.Null, field);
        Assert.That(root.GetComponentsInChildren<TMPro.TMP_InputField>(true), Is.Empty);
    }
    [Test]
    public void HomeHasSevenSuppliedHoverPairs()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Path).GetComponent<MenuRootV2UI>();
        var home = (GameObject)new SerializedObject(root).FindProperty("pageTop").objectReferenceValue;
        var pairs = 0;
        foreach (var button in home.GetComponentsInChildren<Button>(true))
            if (button.spriteState.highlightedSprite) { Assert.That(button.spriteState.highlightedSprite, Is.Not.EqualTo(button.image.sprite)); pairs++; }
        Assert.That(pairs, Is.EqualTo(7));
    }
    [Test]
    public void DeleteSelectionIsHiddenAndClearedOnCancel()
    {
        var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Path));
        try
        {
            var save = instance.GetComponentInChildren<MenuSaveLoadController>(true);
            save.EnterDeleteMode(); save.SetDeleteSelected(0, true); save.SetDeleteSelected(2, true);
            Assert.That(save.IsDeleteMode, Is.True); Assert.That(save.SelectedDeleteCount, Is.EqualTo(2));
            save.ExitDeleteMode();
            Assert.That(save.IsDeleteMode, Is.False); Assert.That(save.SelectedDeleteCount, Is.Zero);
            var selections = new SerializedObject(save).FindProperty("deleteSelections");
            for (var i=0;i<selections.arraySize;i++) Assert.That(((Toggle)selections.GetArrayElementAtIndex(i).objectReferenceValue).gameObject.activeSelf, Is.False);
        }
        finally { Object.DestroyImmediate(instance); }
    }
    [Test]
    public void MapIsMaskedAndBiggerThanViewport()
    {
        var root = AssetDatabase.LoadAssetAtPath<GameObject>(Path);
        var map = root.GetComponentInChildren<MapMenuController>(true);
        var scroll = map.GetComponentInChildren<ScrollRect>(true);
        Assert.That(scroll.viewport.GetComponent<RectMask2D>(), Is.Not.Null);
        Assert.That(scroll.content.rect.width, Is.GreaterThan(scroll.viewport.rect.width));
        Assert.That(scroll.content.rect.height, Is.GreaterThan(scroll.viewport.rect.height));
    }
    [Test]
    public void QuestStepIsClamped()
    {
        var go = new GameObject();
        try { var route = go.AddComponent<MenuQuestRouteController>(); route.SetStep((MenuQuestRouteController.Step)99); Assert.That(route.CurrentStep, Is.EqualTo(MenuQuestRouteController.Step.Conversation)); }
        finally { Object.DestroyImmediate(go); }
    }
}
