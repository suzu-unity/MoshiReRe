using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuRootV2DiscoverabilityTests
{
    private static GameObject Prefab => AssetDatabase.LoadAssetAtPath<GameObject>("Assets/NaninovelData/Resources/UI/MenuRootV2.prefab");
    [Test]
    public void MapCafeRouteRetainsScenarioAndSelection()
    {
        var controller=Prefab.GetComponentInChildren<MapMenuController>(true);
        var so=new SerializedObject(controller);
        Assert.That(so.FindProperty("initialLocationIndex").intValue,Is.EqualTo(3));
        Assert.That(so.FindProperty("onlyAllowConfiguredRoutes").boolValue,Is.True);
        Assert.That(controller.GetComponent<MapRouteLauncher>().TryGetRoute(3,out var route),Is.True);
        Assert.That(route.entryScriptPath,Is.EqualTo("Scenario/PapaQuestDemo"));
    }
    [Test]
    public void LandscapeNavigationDoesNotStartTransition()
    {
        var host=new GameObject(); var first=new GameObject(); var next=new GameObject();
        try
        {
            var transition=host.AddComponent<MenuRootV2OrientationTransition>(); transition.SetInitialPage(first,false);
            var applied=false;
            Assert.That(transition.RequestPage(next,false,()=>applied=true),Is.True);
            Assert.That(applied,Is.True); Assert.That(transition.IsTransitioning,Is.False);
        }
        finally { Object.DestroyImmediate(host); Object.DestroyImmediate(first); Object.DestroyImmediate(next); }
    }
    [Test]
    public void NavigationExpandsOnlyFivePrimaryPages()
    {
        var instance=Object.Instantiate(Prefab);
        try
        {
            var navigation=instance.GetComponent<MenuPhoneNavigation>();
            var labels=new SerializedObject(navigation).FindProperty("selectedLabels");
            for(var page=0;page<8;page++)
            {
                navigation.Select(page);
                for(var i=0;i<8;i++) Assert.That(((GameObject)labels.GetArrayElementAtIndex(i).objectReferenceValue).activeSelf,Is.EqualTo(page==i && i>=1 && i<=5));
            }
        }
        finally { Object.DestroyImmediate(instance); }
    }
    [Test]
    public void QuestGoRoutesByPreparationStep()
    {
        var instance=Object.Instantiate(Prefab);
        try
        {
            Object.DestroyImmediate(instance.GetComponent<MenuRootV2OrientationTransition>());
            var route=instance.GetComponentInChildren<MenuQuestRouteController>(true);
            var so=new SerializedObject(instance.GetComponent<MenuRootV2UI>());
            route.SetStep(MenuQuestRouteController.Step.Map); route.Go();
            Assert.That(((GameObject)so.FindProperty("pageMap").objectReferenceValue).activeSelf,Is.True);
            route.SetStep(MenuQuestRouteController.Step.Items); route.Go();
            Assert.That(((GameObject)so.FindProperty("pageItems").objectReferenceValue).activeSelf,Is.True);
        }
        finally { Object.DestroyImmediate(instance); }
    }
    [Test]
    public void CharactersHaveRuntimeNodePrefabAndDatabase()
    {
        var panel=Prefab.GetComponentInChildren<CharacterInformationNodePanel>(true); var so=new SerializedObject(panel);
        Assert.That(so.FindProperty("characterDatabase").objectReferenceValue,Is.Not.Null);
        Assert.That(((GameObject)so.FindProperty("nodeRowPrefab").objectReferenceValue).GetComponent<MenuInformationNodeCard>(),Is.Not.Null);
    }
    [Test]
    public void ItemsRetainInventoryAndIndividualBagSlots()
    {
        var so=new SerializedObject(Prefab.GetComponentInChildren<ItemMenuController>(true));
        Assert.That(so.FindProperty("inventoryDatabase").objectReferenceValue,Is.Not.Null);
        Assert.That(so.FindProperty("bagSlotImages").arraySize,Is.EqualTo(8));
        Assert.That(so.FindProperty("bagDropArea").objectReferenceValue,Is.Not.Null);
    }
}
