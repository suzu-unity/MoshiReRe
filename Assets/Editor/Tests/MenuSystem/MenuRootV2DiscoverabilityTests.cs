using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuRootV2DiscoverabilityTests
{
    private const string PrefabPath = "Assets/NaninovelData/Resources/UI/MenuRootV2.prefab";

    [Test]
    public void TopPage_OffersOneActionMapEntryForFirstTarget()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null, "MenuRootV2 prefab should be generated before running this test.");

        var top = FindChild(prefab.transform, "PageTop");
        var mapButton = FindChild(top, "HudMapButton");
        Assert.That(mapButton, Is.Not.Null);
        var mapButtonComponent = mapButton.GetComponent<Button>();
        Assert.That(mapButtonComponent, Is.Not.Null);
        Assert.That(mapButton.GetComponent<MenuUIButtonHover>(), Is.Not.Null);
        Assert.That(mapButtonComponent.colors.pressedColor, Is.Not.EqualTo(mapButtonComponent.colors.normalColor));
        Assert.That(FindText(mapButton), Does.Contain("初回ターゲット").And.Contain("カフェ下調べ"));

        var ui = prefab.GetComponent<MenuRootV2UI>();
        var serialized = new SerializedObject(ui);
        Assert.That(serialized.FindProperty("mapTileButton").objectReferenceValue, Is.EqualTo(mapButtonComponent));
    }

    [Test]
    public void MapPage_DescribesCafeRouteAndGoCondition()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var map = FindChild(prefab.transform, "PageMap");
        Assert.That(FindChild(map, "MapDemoBanner"), Is.Not.Null);
        var targetCard = FindChild(map, "MapDemoTargetCard");
        Assert.That(targetCard, Is.Not.Null);
        Assert.That(FindText(targetCard), Does.Contain("初回ターゲット").And.Contain("カフェ下調べ"));

        var goButton = FindChild(map, "MapGoButton");
        Assert.That(goButton, Is.Not.Null);
        Assert.That(goButton.GetComponent<MenuUIButtonHover>(), Is.Not.Null);
        Assert.That(FindText(goButton), Does.Contain("GO").And.Contain("カフェ下調べ"));

        var controller = map.GetComponent<MapMenuController>();
        var controllerSerialized = new SerializedObject(controller);
        Assert.That(controllerSerialized.FindProperty("initialLocationIndex").intValue, Is.EqualTo(3));
        var locations = controllerSerialized.FindProperty("locations");
        Assert.That(locations.arraySize, Is.EqualTo(6));
        Assert.That(locations.GetArrayElementAtIndex(3).FindPropertyRelative("baseName").stringValue, Is.EqualTo("カフェ下調べ"));

        var launcher = map.GetComponent<MapRouteLauncher>();
        var launcherSerialized = new SerializedObject(launcher);
        var routes = launcherSerialized.FindProperty("routes");
        Assert.That(routes.arraySize, Is.EqualTo(1));
        var route = routes.GetArrayElementAtIndex(0);
        Assert.That(route.FindPropertyRelative("enabled").boolValue, Is.True);
        Assert.That(route.FindPropertyRelative("locationIndex").intValue, Is.EqualTo(3));
        Assert.That(route.FindPropertyRelative("entryScriptPath").stringValue, Is.EqualTo("Scenario/PapaQuestDemo"));
    }

    [Test]
    public void DashboardAndConversationStayInsideReferenceFrame()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var frame = FindChild(prefab.transform, "PortraitPhonePresentation").GetComponent<RectTransform>();
        var demo = FindChild(prefab.transform, "FirstTargetDemoPanel").GetComponent<RectTransform>();
        var conversation = FindChild(prefab.transform, "ReReConversation").GetComponent<RectTransform>();
        Assert.That(frame.sizeDelta.x, Is.LessThanOrEqualTo(1760f));
        Assert.That(frame.sizeDelta.y, Is.LessThanOrEqualTo(980f));
        Assert.That(demo.sizeDelta.x, Is.LessThanOrEqualTo(frame.sizeDelta.x));
        Assert.That(conversation.sizeDelta.x, Is.EqualTo(500f));
        Assert.That(conversation.anchoredPosition.x, Is.EqualTo(-112f));
    }

    [Test]
    public void QuestPage_OffersDemoStartAndCompanyRoute()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var quest = FindChild(prefab.transform, "PageQuest");
        var text = FindText(quest);
        Assert.That(text, Does.Contain("初回ターゲット"));
        Assert.That(text, Does.Contain("カフェ下調べ"));
        Assert.That(text, Does.Contain("条件: なし"));
        Assert.That(text, Does.Contain("会社パート"));
        Assert.That(text, Does.Not.Contain("メインクエストはありません"));

        var goButton = FindChild(quest, "GoToAreaButton");
        Assert.That(goButton, Is.Not.Null);
        Assert.That(FindText(goButton), Does.Contain("GO").And.Contain("CAFE"));
        Assert.That(goButton.GetComponent<MenuUIButtonHover>(), Is.Not.Null);
    }

    [Test]
    public void CharactersPage_UsesDemoNamesAndVisibleIntelSnapshot()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var characters = FindChild(prefab.transform, "PageCharacters");
        var text = FindText(characters);
        Assert.That(text, Does.Contain("初回ターゲット"));
        Assert.That(text, Does.Contain("元取引先担当"));
        Assert.That(text, Does.Contain("呼称").And.Contain("行きつけ").And.Contain("承認欲求"));
        Assert.That(text, Does.Contain("17番席"));
        Assert.That(text, Does.Not.Contain("仮:").And.Not.Contain("仮：").And.Not.Contain("???"));
        Assert.That(FindChild(characters, "DemoInformationNodeRow3"), Is.Not.Null);

        var panel = characters.GetComponent<CharacterInformationNodePanel>();
        Assert.That(panel, Is.Not.Null);
        var serialized = new SerializedObject(panel);
        Assert.That(serialized.FindProperty("characterRowButtons").arraySize, Is.EqualTo(12));
        Assert.That(serialized.FindProperty("characterRowIndexes").arraySize, Is.EqualTo(12));
    }

    [Test]
    public void ItemsPage_LoadsInventoryDatabaseAndShowsRouteReadyCards()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        Assert.That(prefab, Is.Not.Null);

        var items = FindChild(prefab.transform, "PageItems");
        var text = FindText(items);
        Assert.That(text, Does.Contain("カフェ回数券"));
        Assert.That(text, Does.Contain("古びた鍵"));
        Assert.That(text, Does.Contain("免罪符"));
        Assert.That(text, Does.Contain("初回ターゲット準備"));

        var controller = items.GetComponent<ItemMenuController>();
        Assert.That(controller, Is.Not.Null);
        var serialized = new SerializedObject(controller);
        Assert.That(serialized.FindProperty("inventoryDatabase").objectReferenceValue, Is.Not.Null);
        var firstCard = FindChild(items, "ItemCard0");
        Assert.That(firstCard, Is.Not.Null);
        Assert.That(firstCard.GetComponent<MenuUIButtonHover>(), Is.Not.Null);
        Assert.That(firstCard.GetComponent<Button>().colors.pressedColor,
            Is.Not.EqualTo(firstCard.GetComponent<Button>().colors.normalColor));
    }

    private static Transform FindChild(Transform root, string name)
    {
        if (!root)
            return null;

        foreach (var child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == name)
                return child;
        }

        return null;
    }

    private static string FindText(Transform root)
    {
        if (!root)
            return string.Empty;

        var texts = root.GetComponentsInChildren<TMP_Text>(true);
        var values = new string[texts.Length];
        for (var i = 0; i < texts.Length; i++)
            values[i] = texts[i].text;
        return string.Join(" ", values);
    }
}
